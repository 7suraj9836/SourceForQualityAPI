using System;

namespace SourceforqualityAPI.Model.UpdateSupplierTempModels
{
    public class TempWallpaperFileMapperUpdateDTO
    {
        public int Id { get; set; }
        public int WallpaperId { get; set; }
        public int FileId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
