//using DataFlow_Test;
//using System;
//using System.Net.WebSockets;
//using System.Threading.Tasks;
//using System.Threading.Tasks.Dataflow;

//namespace SensorFusion
//{
    
//    public interface IAudioPipelineStageData
//    {
//        BufferBlock<AudioPipeineStageData> Buffer_Block { get; set; }
//        TransformBlock<AudioPipeineStageData, AudioPipeineStageData> Transform_Block { get; set; }
//        ActionBlock<AudioPipeineStageData> Action_Block { get; set; }
//        StageBlockType Stage_BlockType { get; set; }
//        void LinkHead(AudioPipeineStageData stage);
//        void LinkStage(AudioPipeineStageData stage);
//        void LinkTail(AudioPipeineStageData stage);
//        CancellationTokenSource _cancellationTokenSource { get; set; }
//        short[] Buffer { get; set; }
//    }

//    public class AudioPipeineStageData: IAudioPipelineStageData
//    {
        
//        public AudioPipeineStageData(CancellationTokenSource cts, StageBlockType blockType, int samplerate = 48000, int secondsToBuffer=10)
//        {
//            SecondsToBuffer = secondsToBuffer;
            
//            SampleRate = samplerate;
//            _cancellationTokenSource = cts;
//            Stage_BlockType = blockType;
//            Buffer = new short[SampleRate * SecondsToBuffer];
//            switch (Stage_BlockType)
//            {
//                case StageBlockType.NotSet:
//                    break;
//                case StageBlockType.StartBlock:
//                    Buffer_Block = new BufferBlock<AudioPipeineStageData>( new DataflowBlockOptions() { CancellationToken = _cancellationTokenSource.Token });
//                    break;
//                case StageBlockType.ProcessingBlock:
                    
//                    break;
//                case StageBlockType.EndBlock:

//                    break;
//                default:
//                    break;
//            }
//        }

//        public int SecondsToBuffer { get; set; }
//        public int SampleRate { get; set; }
//        public short[] ?ProcessedSamples { get; set; }
//        public byte[] ?RawAudioBytes { get; set; }
//        public short[] ?RawAudioSamples { get; set; }
//        public dynamic ?Data { get; set; }
//        public Type ?DataType { get; set; }
//        public string ?Name { get; set; } = "AudioProcessingStage";
//        public BufferBlock<AudioPipeineStageData> ?Buffer_Block { get; set; }
//        public TransformBlock<AudioPipeineStageData, AudioPipeineStageData> ?Transform_Block { get; set; }
//        public ActionBlock<AudioPipeineStageData> ?Action_Block { get; set; }
//        public StageBlockType Stage_BlockType {get;set; }
//        public CancellationTokenSource _cancellationTokenSource { get; set; } 
//        public short[] Buffer { get; set; }

//        public void LinkHead(AudioPipeineStageData stage)
//        {
//            Buffer_Block.LinkTo(stage.Transform_Block, new DataflowLinkOptions() { PropagateCompletion = true });
//        }
//        public void LinkStage(AudioPipeineStageData stage)
//        {
//            Transform_Block.LinkTo(stage.Transform_Block, new DataflowLinkOptions() { PropagateCompletion = true });
//        }
//        public void LinkTail(AudioPipeineStageData stage)
//        {
//            Transform_Block.LinkTo(stage.Action_Block, new DataflowLinkOptions() { PropagateCompletion = true });
//        }
//        public void LinkTo(AudioPipeineStageData stage)
//        {
//            switch (Stage_BlockType)
//            {
//                case StageBlockType.NotSet:
//                    throw new ArgumentException("stage.Stage_BlockType is NotSet");                   
//                case StageBlockType.StartBlock:
//                    Buffer_Block.LinkTo(stage.Transform_Block, new DataflowLinkOptions() { PropagateCompletion = true });
//                    break;
//                case StageBlockType.ProcessingBlock:
//                    Transform_Block.LinkTo(stage.Action_Block, new DataflowLinkOptions() { PropagateCompletion = true });
//                    break;
//                case StageBlockType.EndBlock:
//                    Transform_Block.LinkTo(stage.Action_Block, new DataflowLinkOptions() { PropagateCompletion = true });
//                    break;
//                default:
//                    break;
//            }           
//        }
//    }


//    public class AudioProcessingPipeline
//    {

//        public BufferBlock<AudioPipeineStageData> HeadBlock { get; set; }
//        public ActionBlock<AudioPipeineStageData> TailBlock { get; set; }
//        public List<IAudioPipelineStageData> Stages { get; set; }
//        public int StageCount { get; set; } = 0;
//        public event Action<AudioPipeineStageData> ProcessingComplete;
//        public AudioPipeineStageData ProcessedData { get; internal set; }
//        private readonly object pdlock  = new object();

//        public AudioProcessingPipeline()
//        {
//            //set initial capture data when initializing an instance
//            Stages = new List<IAudioPipelineStageData>();       
//        }

//        public void AddHeadStage(IAudioPipelineStageData stage)
//        {            
//            HeadBlock = stage.Buffer_Block;
//            Stages.Add(stage);
//            StageCount++;
//        }

//        public void AddProcessStage(IAudioPipelineStageData process)
//        {
//            if (StageCount == 1)
//            {
//                HeadBlock.LinkTo(process.Transform_Block);
//            }
//            else
//            {
//                var lastStep = Stages[StageCount].Transform_Block as TransformBlock<AudioPipeineStageData, AudioPipeineStageData>;
//                if (lastStep == null)
//                {
//                    throw new Exception("No Blocks to link");
//                }
//                lastStep.LinkTo(process.Transform_Block);
//                Stages.Add(process);
//            }
          
//            StageCount++;
//        }

//        public void AddTailStage(IAudioPipelineStageData tail)
//        {
//            if (tail.Action_Block == null)
//            {
//                throw new ArgumentException("Tail.Action_BLock is null");
//            }
//            TailBlock = tail.Action_Block;
//            var last = Stages.Last().Transform_Block as TransformBlock<AudioPipeineStageData, AudioPipeineStageData>;
//            if (last != null)
//            {
//                last.LinkTo(tail.Action_Block);
//                Stages.Add(tail);
//            }            
//        }

//        public void Post(AudioPipeineStageData input)
//        {
//            if (HeadBlock != null)
//            {
//                if (!HeadBlock.Post(input))
//                {
//                    throw new Exception("Post failed to post data to first block");
//                }
//                HeadBlock?.Complete(); //stop pipeline accepting data
//            }
//            else
//            {
//                throw new Exception("HeadBlock is null");
//            }
//        }       

//        public async Task AwaitCompletion()
//        {
//            await TailBlock.Completion;
//            lock (pdlock)
//            {
//                ProcessingComplete?.Invoke(ProcessedData);
//            }           
//        }


            

//    }
//}


////(Verse 1)
////Yo, here’s a tale 'bout a pipeline so divine,
////Three blocks in a row, they’re all set to align.
////First block takes the short, doubles it in time,
////TransformBlock be the class, code's lookin’ fine.

////Data streaming in, like a steady pour of wine,
////UDP packets caught, no need for sunshine.
////Our pipeline’s always ready, don't even need a sign,
////Operates in real time, and works by design.

////(Chorus)
////We're building up a pipeline, steady flow design,
////Eminem-style rap, with a code rhythm so fine,
////TransformBlock is our tool, no data left behind,
////Making sure every bit falls in line.

////(Verse 2)
////Second block, you see, adds a dime,
////Then the third subtracts five, it ain’t a crime.
////Data moves from block to block, just like a rhyme,
////TPL Dataflow, efficiency prime.

////Got a task on the side, data intake, it's prime,
////Continuous feed, no packet gets overtime.
////Minimal latency, performance sublime,
////In this world of code, that's the lifeline.

////(Chorus)
////We're building up a pipeline, the logic's intertwine,
////Eminem-style rap, with a code story line,
////Data in, result out, everything's in line,
////In the world of programming, it's a gold mine.

////(Bridge)
////In thread safety we trust, we lock it down,
////A single object to guard, wears the crown.
////Critical sections safe, no reason to frown,
////Even in concurrency, we won't drown.

////(Chorus)
////We're building up a pipeline, straight from the divine,
////Eminem-style rap, in the code we dine,
////Efficiency, safety, everything's fine,
////Welcome to the world where tech and rap combine.

////(Outro)
////In the end, this pipeline’s a treat,
////Data processed in rhythm, can't be beat.
////Thread safety assured, that's neat,
////In the world of code, that's quite a feat.