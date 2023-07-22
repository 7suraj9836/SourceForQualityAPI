namespace SourceforqualityAPI.Common
{
    public class FileDto
    {
        public string File { get; set; }
        public string FileName { get; set; }
    }

    public class FileOutputDto : FileDto
    {
        public int? Id { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string FileStorageName { get; set; }
    }
}
