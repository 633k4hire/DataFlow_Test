using SensorFusion.AudioPipeline;
using SensorFusion.AudioPipeline.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataFlow_Test.AudioPipeline.Transports
{

    public class AudioCaptureClient<T>
    {
        public AudioCaptureClient(AudioPipeLine pipeline)
        {
            LongBufferSize = SampleRate * SecondsToBuffer;
            Pipeline = pipeline;
            AudioBufferBlock = Pipeline.AudioBufferBlock;

        }
        public AudioPipeLine Pipeline { get; set; }
        public int SampleRate { get; set; } = 48000;// 48 kHz
        public int PacketSize { get; set; } = 512;
        public int LongBufferSize { get; }
        public int SecondsToBuffer { get; set; } = 10;
        public object? Tag { get; set; }
        public CancellationToken? CancelToken { get; set; }
        public AudioPipeLineData? PipeData { get; set; }
        public T? SocketClient { get; set; }
        public BufferBlock<AudioPipeLineData> AudioBufferBlock { get; set; }

    }

}
