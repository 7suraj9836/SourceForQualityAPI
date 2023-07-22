namespace SourceforqualityAPI.Model
{
    public class FavouriteSupplierSearchModel: RecordFilterBase
    {
        public int UserId { get; set; }
        public int SupplierId { get; set; }
        public bool IsFavourite { get; set; }
        public int TotalCount { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }

    }
}
