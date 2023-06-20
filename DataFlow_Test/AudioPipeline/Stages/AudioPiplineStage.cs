using DataFlow_Test.AudioPipeline.Stages.Data;
using Microsoft.Extensions.Logging;
using SensorFusion.AudioPipeline.Data;
using SensorFusion.AudioPipeline.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SensorFusion.AudioPipeline.Stages
{

    /// <summary>
    /// SensorFusionPipelineStage represents a stage in a sensor fusion pipeline.
    /// </summary>
    public class AudioPipelineStage : IAudioPipelineStage<AudioPipeLineData, AudioPipelineStage>
    {
        private readonly ILogger _logger;
        public ExecutionDataflowBlockOptions _ExecutionDataflowBlockOptions { get; set; }
        public AudioPipeLineData PipeData { get; set; }
        public AudioStageData StageData { get; set; }
        public long ExecutionTime { get; set; }
        public TransformBlock<AudioPipeLineData, AudioPipeLineData>? AudioTransformBlock { get; set; }
        public CancellationTokenSource _cancellationTokenSource { get; set; }
        public Func<AudioPipeLineData, Task<AudioPipeLineData>> ProcessFunction { get; set; }

        /// <summary>
        /// Constructor for SensorFusionPipelineStage.
        /// </summary>
        public AudioPipelineStage(ILogger logger, Func<AudioPipeLineData, Task<AudioPipeLineData>> processFunction, AudioStageData stageData, CancellationTokenSource tokenSource)
        {
            _logger = logger;
            PipeData = new AudioPipeLineData();
            ProcessFunction = WrapProcessFunction(processFunction);
            StageData = stageData;
            _cancellationTokenSource = tokenSource;
            _ExecutionDataflowBlockOptions = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, EnsureOrdered = true, CancellationToken = _cancellationTokenSource.Token };
            AudioTransformBlock = new TransformBlock<AudioPipeLineData, AudioPipeLineData>(processFunction, _ExecutionDataflowBlockOptions);
        }

        /// <summary>
        /// Assigns a new processing function to the pipeline stage.
        /// </summary>
        public void AssignProcessingFunction(Func<AudioPipeLineData, Task<AudioPipeLineData>> processFunction)
        {
            ProcessFunction = processFunction;
        }

        /// <summary>
        /// Links this pipeline stage to another stage.
        /// </summary>
        public void LinkTo(AudioPipelineStage stage)
        {
            if (AudioTransformBlock == null)
            {
                _logger.LogError("AudioTransformBlock is null");
                throw new Exception("AudioTransformBlock is null");
            }
            if (stage.AudioTransformBlock == null)
            {
                _logger.LogError("stage.AudioTransformBlock is null");
                throw new Exception("stage.AudioTransformBlock is null");
            }
            AudioTransformBlock.LinkTo(stage.AudioTransformBlock, new DataflowLinkOptions() { PropagateCompletion = true });
        }

        /// <summary>
        /// Wraps the process function with a stopwatch for timing and logging purposes.
        /// </summary>
        private Func<AudioPipeLineData, Task<AudioPipeLineData>> WrapProcessFunction(Func<AudioPipeLineData, Task<AudioPipeLineData>> processFunction)
        {
            var wrappedFunction = new Func<AudioPipeLineData, Task<AudioPipeLineData>>(async data =>
            {
                Stopwatch stopWatch = Stopwatch.StartNew();

                try
                {
                    // We call the passed processFunction here
                    var result = await processFunction(data);

                    stopWatch.Stop();
                    _logger.LogInformation($"Execution Time: {stopWatch.ElapsedMilliseconds} ms");
                    this.ExecutionTime = stopWatch.ElapsedMilliseconds;
                    data.RaiseStageCompleteEvent();
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred in WrapProcessFunction: {ex.Message}");
                    throw;
                }
            });

            return wrappedFunction;
        }
    }

}
