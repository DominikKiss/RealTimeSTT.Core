using Whisper.net.Ggml;

namespace RealTimeSTT.STT.Initialization;

public class RuntimeOptions
{
    public GgmlType ggmltype { get; set; } = GgmlType.Base;
    public string ModelFileName { get; set; } = "ggml-base.bin";
    public string CoreMlModelcName { get; set; } = "ggml-base-encoder.mlmodelc";
}
