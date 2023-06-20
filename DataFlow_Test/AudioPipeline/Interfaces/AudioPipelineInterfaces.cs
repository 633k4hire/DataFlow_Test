using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SensorFusion.AudioPipeline.Interfaces
{
    public interface IAudioPipelineStage<T, TT>
    {
        T PipeData { get; set; }
        long ExecutionTime { get; set; }
        TransformBlock<T, T>? AudioTransformBlock { get; set; }
        void LinkTo(TT stage);
        CancellationTokenSource _cancellationTokenSource { get; set; }
        Func<T, Task<T>> ProcessFunction { get; set; }
        void AssignProcessingFunction(Func<T, Task<T>> processFunction);
    }

}
