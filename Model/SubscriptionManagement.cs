namespace SourceforqualityAPI.Model
{
    public class SubscriptionManagement
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Validity { get; set; }
        public decimal Price { get; set; }
        public string BenefitsString { get; set; }
        public string[] Benefits { get; set; }
        public int NumberOfPurchasers { get; set; }
        public bool Enable { get; set; }

        public string PopularRating { get; set; }

        public string Description { get; set; }

    }
}
