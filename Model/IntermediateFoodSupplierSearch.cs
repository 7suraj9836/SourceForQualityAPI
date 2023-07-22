using SourceforqualityAPI.Contracts;

namespace SourceforqualityAPI.Model
{
    public class IntermediateFoodSupplierSearch
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public int  CreatedBy{ get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; } 
        public int TotalCount { get; set; }
        public bool IsFavourite { get; set; }
    }
}
