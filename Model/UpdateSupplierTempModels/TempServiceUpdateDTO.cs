using System;

namespace SourceforqualityAPI.Model.UpdateSupplierTempModels
{
    public class TempServiceUpdateDTO
    {
        public int Id { get; set; }
        public int SupplierProfileId { get; set; }
        public string Title { get; set; }
        public string Descriptions { get; set; }
        public int BusinessActivityCategoryId { get; set; }
        public string OtherBusinessActivityCategoryName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
