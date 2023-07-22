using System;

namespace SourceforqualityAPI.Model
{
    public class UserFavouriteSupplierDataModel
    {
        
        public int UserId { get; set; }
        public int SupplierId { get; set; }
        public bool IsFavourite { get; set; }
        public DateTime CreatedOn { get; set; }= DateTime.Now;
        
        
    }
}
