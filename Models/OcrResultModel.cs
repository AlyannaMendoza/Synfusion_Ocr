namespace SYNCFUSION_TRIAL.Models
{
    public class OcrResult
    {
        public int Id { get; set; }
        public int FileMetadataId { get; set; }
        public string? ExtractedText { get; set; }

        public virtual FileMetadata FileMetadata { get; set; }
    }
}
