namespace RagApi.Options;

public class RagOptions
{
    public const string SectionName = "Rag";

    public ulong SearchLimit { get; set; } = 3;
    public float ScoreThreshold { get; set; } = 0.5f;

    public int ChunkSize { get; set; } = 1200;
    public int ChunkOverlap { get; set; } = 200;
}
