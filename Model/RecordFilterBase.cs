namespace SourceforqualityAPI.Model
{
    public class RecordFilterBase
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SortParam { get; set; }
        public string SortOrder { get; set; }
        public string Keyword { get; set; }

    }
}
