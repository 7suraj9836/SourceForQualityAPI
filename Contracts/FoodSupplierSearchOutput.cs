using SourceforqualityAPI.Model;
using System.Collections.Generic;

namespace SourceforqualityAPI.Contracts
{
    public class FoodSupplierSearchOutput
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public int UserId { get; set; }
        public Location Locations { get; set; }
        public int TotalCount { get; set; }
        public bool IsFavourite { get; set; }
    }

    public class Location
    {
        public double Latitude { get; set; } 
        public double Longitude { get; set; } 
    }
}
