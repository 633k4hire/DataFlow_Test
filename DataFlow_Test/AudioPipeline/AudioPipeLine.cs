using DataFlow_Test.AudioPipeline.Transports;
using Microsoft.Extensions.Logging;
using SensorFusion.AudioPipeline.Data;
using SensorFusion.AudioPipeline.Interfaces;
using SensorFusion.AudioPipeline.Stages;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SensorFusion.AudioPipeline
{


    /// <summary>
    /// Class AudioProcessingPipelinePro, simulates an audio processing pipeline
    /// that buffers 10 seconds of audio and processes it in two stages.
    /// </summary>
    public class AudioPipeLine
    {
        // Constants for sample rate, buffer size and amount of data to buffer..
        public int SampleRate { get; set; } // 48 kHz
        public int PacketSize { get; set; }
        public  int LongBufferSize {get;set;}
        public  int SecondsToBuffer {get;set;}
        public AudioPipeLineData ?PipeData;
        public ExecutionDataflowBlockOptions _ExecutionDataflowBlockOptions { get; set; }
        public CancellationTokenSource TokenSource;
        public int StageCount { get; set; }
        public Func<AudioCaptureClient<UdpClient>, Task<dynamic>> ?AudioCaptureFunction { get; set; }        
        //public Action<AudioPipeLineData> AudioCapturePipelineCompleteFunction { get; set; }



        // Dataflow blocks for the audio processing pipeline
        public BufferBlock<AudioPipeLineData> AudioBufferBlock;
        public List<AudioPipelineStage> Stages;
        public ActionBlock<AudioPipeLineData> EndOfPipeBlock;

        public AudioPipeLine(CancellationTokenSource tokenSource = null,  int sampleRate = 48000, int packetSize=512, int secondsToBuffer= 10)
        {
            if (tokenSource == null)
            {
                TokenSource = new CancellationTokenSource();
            }
            else
            {
                TokenSource = tokenSource; 
            }
            
            _ExecutionDataflowBlockOptions = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, EnsureOrdered = true, CancellationToken = TokenSource.Token };
            AudioBufferBlock = new BufferBlock<AudioPipeLineData>(_ExecutionDataflowBlockOptions);
            EndOfPipeBlock = new ActionBlock<AudioPipeLineData>(AudioPipelineComplete, _ExecutionDataflowBlockOptions);
            Stages = new List<AudioPipelineStage>();
            SampleRate = sampleRate;
            PacketSize = packetSize;
            SecondsToBuffer = secondsToBuffer;

            Console.WriteLine("Initializing the pipeline...");                   

        }


        public void AddStage(AudioPipelineStage process)
        {
            if (AudioBufferBlock== null)
            {
                throw new ArgumentException("bufferBlock is null");
            }
            if (StageCount == 0)
            {
                AudioBufferBlock.LinkTo(process.AudioTransformBlock);
                Console.WriteLine("BufferBlock Linked to first stage");
                Stages.Add(process);
            }
            else
            {
                var lastStep = Stages[Stages.Count-1].AudioTransformBlock as TransformBlock<AudioPipeLineData, AudioPipeLineData>;
                if (lastStep == null)
                {
                    throw new Exception("No Blocks to link");
                }
                lastStep.LinkTo(process.AudioTransformBlock);
                Stages.Add(process);
                Console.WriteLine("Stage "+ StageCount.ToString() +" Linked Stage "+ (StageCount+1).ToString());
            }

            StageCount++;
        }

        public BufferBlock<AudioPipeLineData> CreatePipeline()
        {
            Console.WriteLine("Creating Pipeline with " + Stages.Count + " stages");
            if (Stages.Count == 0)
            {
                throw new Exception("No stages added to the pipeline");
            }
            var last = Stages.Last();
            last?.AudioTransformBlock?.LinkTo(EndOfPipeBlock, new DataflowLinkOptions { PropagateCompletion = true });
            return AudioBufferBlock;

        }

        //public void AddTailStage(SensorFusionPipelineStage tail)
        //{
        //    if (tail.Action_Block == null)
        //    {
        //        throw new ArgumentException("Tail.Action_Block is null");
        //    }
        //    actionBlock = tail.Action_Block;
        //    var last = Stages.Last().Transform_Block as TransformBlock<AudioPipeLineData, AudioPipeLineData>;
        //    if (last != null)
        //    {
        //        last.LinkTo(tail.Action_Block);
        //        Stages.Add(tail);
        //    }
        //}

        /// <summary>
        /// Posts the data to the BufferBlock.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public async Task<bool> PostAudioDataToBufferBlockAsync(AudioPipeLineData data)
        {
           return await AudioBufferBlock.SendAsync(data);
        }

        /// <summary>
        /// Post Data non async
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  bool PostAudioDataToBufferBlock(AudioPipeLineData data)
        {
            return  AudioBufferBlock.Post(data);
        }

        public void AudioPipelineComplete(AudioPipeLineData data)
        {
            data.RaisePipelineCompleteEvent();
        }

        /// <summary>
        /// Completes the pipeline and waits for all data to be processed.
        /// </summary>
        /// <returns></returns>
        public async Task CompleteAndWaitAsync()
        {
            Console.WriteLine("Completing the pipeline...");
            AudioBufferBlock.Complete();
            await EndOfPipeBlock.Completion;
            Console.WriteLine("Pipeline completed successfully. All data has been processed.");
        }

        public  void StopPipeline()
        {
            TokenSource.Cancel();
        }

        
           

    }
}