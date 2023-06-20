using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SensorFusion;
using SensorFusion.AudioPipeline;
using SensorFusion.AudioPipeline.Stages;
using Microsoft.Extensions.Logging;
using SensorFusion.AudioPipeline.Data;
using DataFlow_Test.AudioPipeline.Stages.Data;
using System.Net.Sockets;
using System.Threading.Tasks.Dataflow;
using DataFlow_Test.AudioPipeline.Transports;

namespace DataFlow_Test
{
    internal class Program
    {
        private static CancellationTokenSource _cancellationTokenSource;
        private static AudioPipeLine Pipeline;
        public static async Task Main()
        {

            var serviceCollection = new ServiceCollection();

            // Add logging services
            serviceCollection.AddLogging(configure =>
                configure.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "hh:mm:ss ";
                }));

            // Build your service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Get your logger from the service provider
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            _cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += CancelKeyPressHandler;

            //initiate trhe pipeline
            Pipeline = new AudioPipeLine( 
                _cancellationTokenSource,
                sampleRate:48000, 
                packetSize:512,
                secondsToBuffer:10
                );

            //add stages
            var stage1Data = new AudioStageData();
            var stage1 = new AudioPipelineStage(logger, Stage1Process, stage1Data, _cancellationTokenSource);

            var stage2Data = new AudioStageData();
            var stage2 = new AudioPipelineStage(logger, Stage2Process, stage2Data, _cancellationTokenSource);

            Pipeline.AddStage(stage1);
            Pipeline.AddStage(stage2);

            //finish creating the pipeline
            Pipeline.CreatePipeline();

            //feed the pipeline buffer with data with a capture client continuously
            var audioCaptureClient = new AudioCaptureClient<UdpClient>(Pipeline);
            var result = await CaptureAudioTestingAsync(audioCaptureClient);

            //stop pipeline
            StopPipeline();
            

            Console.WriteLine("All data has been processed");
        }

        public static async Task<dynamic> CaptureAudioTestingAsync(AudioCaptureClient<UdpClient> clientData)
        {          
            Console.WriteLine("Feeding data to the buffer...");

            double frequency = 440.0; // Frequency of the sine wave
            double amplitude = short.MaxValue / 2; // Amplitude of the wave (half of the maximum to avoid overflow)

            // Create a new Stopwatch instance
            var stopwatch = new System.Diagnostics.Stopwatch();

            while (!clientData.Pipeline.TokenSource.Token.IsCancellationRequested) // This will run indefinitely
            {
                stopwatch.Start(); // Start the stopwatch

                // This array will hold all the samples for 10 seconds of audio
                short[] data = new short[clientData.LongBufferSize];

                for (int i = 0; i < clientData.LongBufferSize; i++)
                {
                    // Compute the position in the wave (from 0 to 2π) for the current sample
                    double positionInWave = (i / (double)clientData.SampleRate) * frequency * 2 * Math.PI;
                    // Compute the sample value based on the sine of the position in the wave
                    data[i] = (short)(amplitude * Math.Sin(positionInWave));
                }

                // Post the entire 10 seconds of audio to the BufferBlock
                // await PostAudioDataToBufferBlockAsync(new AudioPipeLineData(data, 48000));

                await clientData.Pipeline.AudioBufferBlock.SendAsync(new AudioPipeLineData(data, 48000), clientData.Pipeline.TokenSource.Token);

                stopwatch.Stop(); // Stop the stopwatch

                double elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                if (elapsedMilliseconds < 10000)
                {
                    // If less than 10 seconds (10000 milliseconds) have passed, wait the remaining time
                    await Task.Delay(TimeSpan.FromMilliseconds(10000 - elapsedMilliseconds), clientData.Pipeline.TokenSource.Token);
                }

                Console.WriteLine($"Ten seconds of data feeding completed in {stopwatch.Elapsed.TotalMilliseconds} milliseconds. Continuing to feed the buffer...");

                stopwatch.Reset(); // Reset the stopwatch for the next loop iteration
            }
            return clientData;
        }

        private static void StopPipeline()
        {
            Pipeline.StopPipeline();
            _cancellationTokenSource.Cancel();
        }


        private static async Task<AudioPipeLineData> Stage1Process(AudioPipeLineData data)
        {
            await Task.Delay(1000, _cancellationTokenSource.Token);

            return data;
        }

        private static  async Task<AudioPipeLineData> Stage2Process(AudioPipeLineData data)
        {
            await Task.Delay(1000, _cancellationTokenSource.Token);

            return data;
        }



        private static void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("CTRL+C detected. Attempting to cancel tasks...");

            // Cancel the tasks and prevent the program from terminating immediately
            _cancellationTokenSource.Cancel();
            e.Cancel = true;
        }
    }
}