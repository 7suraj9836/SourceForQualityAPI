namespace SourceforqualityAPI.Contracts
{
    public class SubcriptionManagementUpdateDTO
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Validity { get; set; }
        public decimal Price { get; set; }
        public string[] Benefits { get; set; }
        public bool Enable { get; set; }

    }
}
