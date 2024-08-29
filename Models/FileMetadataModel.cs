namespace SYNCFUSION_TRIAL.Models
{
    public class FileMetadata
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
        //public string UploaderId { get; set; }

        // Navigation property
        public FileData FileData { get; set; }

        // Navigation property
        public OcrResult OcrResult { get; set; }
    }
}
