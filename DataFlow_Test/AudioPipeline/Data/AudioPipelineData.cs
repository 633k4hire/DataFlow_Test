using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DataFlow_Test.AudioPipeline.Stages.Data;

namespace SensorFusion.AudioPipeline.Data
{
    public class AudioPipeLineData
    {
        public AudioPipeLineData()
        {
            Children = new List<AudioPipeLineData>();
            
        }
        public AudioPipeLineData(short[] buffer, int sampleRate = 48000)
        {
            Children = new List<AudioPipeLineData>();
            BufferSize = buffer.Length;
            AudioBuffer = buffer;
            SampleRate = sampleRate;

        }    
        public List<AudioPipeLineData> Children { get; set; }
        public AudioStageData? StageData { get; set; }
        public short[] ?AudioBuffer { get; set; }
        public int BufferSize { get; set; }
        public int SampleRate { get; set; }
        public object? Tag { get; set; }
        public CancellationToken? CancelToken { get; set; }
        //events
        public event EventHandler ?PipelineComplete;
        public event EventHandler ?StageComplete;
        public void RaisePipelineCompleteEvent()
        {
            // The ?. operator is a null-conditional operator that only invokes the event if there are any subscribers.
            // If MyEvent is null (meaning there are no subscribers), then the event won't be invoked and no exception will be thrown.
            PipelineComplete?.Invoke(this, EventArgs.Empty);
        }
        public void RaiseStageCompleteEvent()
        {
            // The ?. operator is a null-conditional operator that only invokes the event if there are any subscribers.
            // If MyEvent is null (meaning there are no subscribers), then the event won't be invoked and no exception will be thrown.
            StageComplete?.Invoke(this, EventArgs.Empty);
        }
    }


}
