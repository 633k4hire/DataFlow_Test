using SensorFusion;
namespace DataFlow_Test
{
    internal class Program
    {
        public static async Task Main()
        {
            // Create a new pipeline
            //var pipeline = new ProcessingPipeline();

            //// Post some numbers to the pipeline

            //PipeStage ps = new PipeStage(10);

            //pipeline.PostData(ps);


            //// Tell the pipeline that there's no more data coming, and wait for all data to be processed
            //await pipeline.CompleteAndWaitAsync();

            var pipeline = new AudioProcessingPipelinePro();

            // Feed the buffer with 10 seconds of audio data
            await pipeline.FeedDataAsync();

            //// Now you can continue to feed the pipeline as needed
            //// var feedDataTask = pipeline.FeedDataAsync();

            //// After some time, complete the pipeline and wait for all data to be processed
            //await Task.Delay(TimeSpan.FromMinutes(1));

            //// feedDataTask.Dispose();

            //await pipeline.CompleteAndWaitAsync();

            Console.WriteLine("All data has been processed");
        }
    }
}