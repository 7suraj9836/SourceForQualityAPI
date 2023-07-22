namespace SourceforqualityAPI.Contracts
{
    public class SearchInputDTO
    {
       
        public int? ProductCategoryId { get; set; } = null;
        public int? CountryId { get; set; } = null;
        public string? SearchValue { get; set; } = null;
     
        public int? BusinessActivityCategoryId { get; set; } = null;
      
        public int? CertificationCategoryId { get; set; } = null;
    }
}