namespace SourceforqualityAPI.Model
{
    public class ResponseModel<T>
    {
        public int StatusCode { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
    }

    public class DownloadLinkModel
    {
        public string Pdf { get; set; }
        public string Csv { get; set; }
    }
}
