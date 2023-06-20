using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataFlow_Test
{
   public class PipeStage
    {
        public PipeStage(short input = 0)
        {
            Input = input;
        }
        public PipeStage(TransformBlock<PipeStage, PipeStage> block, short input=0)
        {
            Block = block;
            Input = input;
        }
        public TransformBlock<PipeStage, PipeStage> Block;
        public ActionBlock<PipeStage> EndBlock;
        public short FinalAnswer; 
        public short Input;
        public void LinkTo(PipeStage stage)
        {
            Block.LinkTo(stage.Block, new DataflowLinkOptions() { PropagateCompletion = true });
        }
        public void LinkTail(PipeStage stage)
        {
            Block.LinkTo(stage.EndBlock, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    public class ProcessingPipeline
    {
        // Define the blocks
        private PipeStage Stage1;
        private PipeStage Stage2;
        private PipeStage Stage3;
        private PipeStage EndPipe;
        public short Answer;
        private TaskCompletionSource<short> task1;
        

        public ProcessingPipeline()
        {
           
        }

        public void CreatePipeline()
        {
            var t = new ExecutionDataflowBlockOptions();
            t.MaxDegreeOfParallelism = -1;
            t.EnsureOrdered = true;
            // Initialize the blocks


            EndPipe = new PipeStage();
            EndPipe.EndBlock = new ActionBlock<PipeStage>(endStage);

            task1 = new TaskCompletionSource<short>();

            Stage1 = new PipeStage(new TransformBlock<PipeStage, PipeStage>(stage1, t), 10);
            Stage2 = new PipeStage(new TransformBlock<PipeStage, PipeStage>(stage2, t));
            Stage3 = new PipeStage(new TransformBlock<PipeStage, PipeStage>(stage3, t));

            Stage1.LinkTo(Stage2);
            Stage2.LinkTo(Stage3);
            Stage3.LinkTail(EndPipe);

        }

        // A method to post data to the first block
        public void PostData(PipeStage data)
        {
            if (!Stage1.Block.Post(data))
            {
                throw new InvalidOperationException("Failed to post data to the pipeline");
            }
        }

      
        // A method to complete the pipeline and wait for all data to be processed
        public async Task CompleteAndWaitAsync()
        {
            // Stage1.Block.Complete();
           // await task1.Task;
            await EndPipe.EndBlock.Completion;
            var t = new ExecutionDataflowBlockOptions();
            t.MaxDegreeOfParallelism = -1;
            t.EnsureOrdered = true;
            Stage1 = new PipeStage(new TransformBlock<PipeStage, PipeStage>(stage1, t), 10);
            Stage2 = new PipeStage(new TransformBlock<PipeStage, PipeStage>(stage2, t));
            Stage3 = new PipeStage(new TransformBlock<PipeStage, PipeStage>(stage3, t));
            EndPipe = new PipeStage();
            EndPipe.EndBlock = new ActionBlock<PipeStage>(endStage);

            Stage1.LinkTo(Stage2);
            Stage2.LinkTo(Stage3);
            Stage3.LinkTail(EndPipe);
            try
            {
                PostData(Stage1);
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        public void endStage(PipeStage input)
        {           
            input.Input = (short)(input.Input * 2);
            Answer = input.Input;
            //end the pipeline 
        }

        public PipeStage stage1(PipeStage input )
        {
            Stage1.Block.Complete();
            input.Input = (short)(input.Input *2);
            return input;
        }
        public PipeStage stage2(PipeStage input)
        {
            
            input.Input = (short)(input.Input +10 );
            return input;
        }
        public PipeStage stage3(PipeStage input)
        {
           // Stage3.Block.Complete();           
            input.Input = (short)(input.Input - 1);
          
            return input;
        }
    }
}
