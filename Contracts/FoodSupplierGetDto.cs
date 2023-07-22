namespace SourceforqualityAPI.Contracts
{
    public class FoodSupplierGetDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string AwardTitle { get; set; }
        public bool SubscriptionStatus { get; set; }
        public bool IsDeleted { get; set; }
        public int CountryId { get; set; }
        public string Location { get; set; }
        public int CreatedBy { get; set; }
        public int BusinessActivityCategoryId { get; set; }
        public string BusinessActivityCategoryName { get; set; }
        public string CertificationCategoryName { get; set; }
        public int CertificationsCategoryId  { get; set; }
        public bool AccountStatus { get; set; }
    }
}
