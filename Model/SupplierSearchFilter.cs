namespace SourceforqualityAPI.Model
{
    public class SupplierSearchFilter : RecordFilterBase
    {
        public int SearchedBy { get; set; } = 0;
        public int? ProductCategoryId { get; set; } = null;
        public int? CountryId { get; set; } = null;
        public string? SearchValue { get; set; } = null;

        public int? BusinessActivityCategoryId { get; set; } = null;

        public int? CertificationCategoryId { get; set; } = null;
    }
}
