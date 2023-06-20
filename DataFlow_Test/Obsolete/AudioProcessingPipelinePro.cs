//using System;
//using System.Diagnostics;
//using System.Threading.Tasks;
//using System.Threading.Tasks.Dataflow;

//namespace SensorFusion
//{
//    /// <summary>
//    /// Class AudioProcessingPipelinePro, simulates an audio processing pipeline
//    /// that buffers 10 seconds of audio and processes it in two stages.
//    /// </summary>
//    public class AudioProcessingPipelinePro
//    {
//        // Constants for sample rate, buffer size and amount of data to buffer
//        private const int SampleRate = 48000; // 48 kHz
//        private const int BufferSize = 512;
//        private const int TenSecondsBuffer = SampleRate * 10; // amount of samples to buffer

//        // Dataflow blocks for the audio processing pipeline
//        private BufferBlock<short[]> bufferBlock;
//        private TransformBlock<short[], short[]> transformBlock1;
//        private TransformBlock<short[], short[]> transformBlock2;
//        private ActionBlock<short[]> actionBlock;

//        public AudioProcessingPipelinePro()
//        {
//            Console.WriteLine("Initializing the pipeline...");

//            // Initialize the BufferBlock
//            bufferBlock = new BufferBlock<short[]>(new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, EnsureOrdered = true });
//            Console.WriteLine("BufferBlock initialized.");

//            // Initialize the TransformBlock for the first stage of processing
//            transformBlock1 = new TransformBlock<short[], short[]>(async data =>
//            {
//                Console.WriteLine("Processing stage 1 started...");
//                await Task.Delay(TimeSpan.FromSeconds(4)); // simulate processing delay
//                Console.WriteLine("Processing stage 1 completed.");
//                return data; // return the processed data
//            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, EnsureOrdered = true });
//            Console.WriteLine("TransformBlock for stage 1 initialized.");

//            // Initialize the TransformBlock for the second stage of processing
//            transformBlock2 = new TransformBlock<short[], short[]>(async data =>
//            {
//                Console.WriteLine("Processing stage 2 started...");
//                await Task.Delay(TimeSpan.FromSeconds(4)); // simulate processing delay
//                Console.WriteLine("Processing stage 2 completed.");
//                return data; // return the processed data
//            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, EnsureOrdered = true });
//            Console.WriteLine("TransformBlock for stage 2 initialized.");

//            // Initialize the ActionBlock to consume the processed data
//            actionBlock = new ActionBlock<short[]>(data =>
//            {
//                Console.WriteLine($"Processed {data.Length} samples. Consuming the processed data...");
//                // Consume the processed data (In a real application, you might do something with the data here)
//            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, EnsureOrdered = true });
//            Console.WriteLine("ActionBlock for consuming data initialized.");

//            // Link the blocks
//            bufferBlock.LinkTo(transformBlock1, new DataflowLinkOptions { PropagateCompletion = true });
//            transformBlock1.LinkTo(transformBlock2, new DataflowLinkOptions { PropagateCompletion = true });
//            transformBlock2.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

//            Console.WriteLine("Blocks linked successfully. The pipeline is ready for processing.");
//        }

//        /// <summary>
//        /// Posts the data to the BufferBlock.
//        /// </summary>
//        /// <param name="data">The data.</param>
//        /// <returns></returns>
//        public async Task PostDataAsync(short[] data)
//        {
//            // Console.WriteLine("Posting data to the buffer...");
//            await bufferBlock.SendAsync(data);
//            //Console.WriteLine("Data posted successfully.");
//        }

//        /// <summary>
//        /// Completes the pipeline and waits for all data to be processed.
//        /// </summary>
//        /// <returns></returns>
//        public async Task CompleteAndWaitAsync()
//        {
//            Console.WriteLine("Completing the pipeline...");
//            bufferBlock.Complete();
//            await actionBlock.Completion;
//            Console.WriteLine("Pipeline completed successfully. All data has been processed.");
//        }

//        /// <summary>
//        /// Feeds the BufferBlock with simulated raw audio samples.
//        /// </summary>
//        /// <returns></returns>
//        public async Task FeedDataAsync()
//        {
//            Console.WriteLine("Feeding data to the buffer...");

//            double frequency = 440.0; // Frequency of the sine wave
//            double amplitude = short.MaxValue / 2; // Amplitude of the wave (half of the maximum to avoid overflow)

//            // Create a new Stopwatch instance
//            var stopwatch = new System.Diagnostics.Stopwatch();

//            while (true) // This will run indefinitely
//            {
//                stopwatch.Start(); // Start the stopwatch

//                // This array will hold all the samples for 10 seconds of audio
//                short[] data = new short[TenSecondsBuffer];

//                for (int i = 0; i < TenSecondsBuffer; i++)
//                {
//                    // Compute the position in the wave (from 0 to 2π) for the current sample
//                    double positionInWave = (i / (double)SampleRate) * frequency * 2 * Math.PI;
//                    // Compute the sample value based on the sine of the position in the wave
//                    data[i] = (short)(amplitude * Math.Sin(positionInWave));
//                }

//                // Post the entire 10 seconds of audio to the BufferBlock
//                await PostDataAsync(data);

//                stopwatch.Stop(); // Stop the stopwatch

//                double elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
//                if (elapsedMilliseconds < 10000)
//                {
//                    // If less than 10 seconds (10000 milliseconds) have passed, wait the remaining time
//                    await Task.Delay(TimeSpan.FromMilliseconds(10000 - elapsedMilliseconds));
//                }

//                Console.WriteLine($"Ten seconds of data feeding completed in {stopwatch.Elapsed.TotalMilliseconds} milliseconds. Continuing to feed the buffer...");

//                stopwatch.Reset(); // Reset the stopwatch for the next loop iteration
//            }
//        }

//    }
//}