namespace SourceforqualityAPI.Contracts
{
    public class FoodSupplierIntermediateDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ProductCatergoryName { get; set; }
        public string RegistrationNumber { get; set; }
        public string? Description { get; set; }
        public string? Logo { get; set; }
        public string PhoneNumber { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Instagram { get; set; }
        public string Country { get; set; }
        public string AwardId{ get; set; }
        public string AwardTitle { get; set; }
        public int AwardFileId { get; set; }
        public string AwardFileName { get; set; }
        public string AwardFileStorageName { get; set; }
        public string AwardFileType { get; set; }

        public string QualificationId { get; set; }
        public int CertificationsCategoryId { get; set; }
        public string CertificationsCategoryName { get; set; }
        public string QualificationTitle { get; set; }
        public int QualificationFileId { get; set; }
        public string QualificationFileName { get; set; }
        public string QualificationFileStorageName { get; set; }
        public string QualificationFileType { get; set; }


        public string ProductId { get; set; }
        public int ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }
        public string ProductTitle { get; set; }
        public int ProductFileId { get; set; }
        public string ProductFileName { get; set; }
        public string ProductFileStorageName { get; set; }
        public string ProductFileType { get; set; }

        public string ServiceId { get; set; }
        public int BusinessActivityCategoryId { get; set; }
        public string BusinessActivitysCategoryName { get; set; }
        public string ServiceTitle { get; set; }
        public string ServiceDescription { get; set; }
        public int ServiceFileId { get; set; }
        public string ServiceFileName { get; set; }
        public string ServiceFileStorageName { get; set; }
        public string ServiceFileType { get; set; }

        public string UploadImageId { get; set; }
        public string UploadImageDescription { get; set; }
        public int UploadImageFileId { get; set; }
        public string UploadImageFileName { get; set; }
        public string UploadImageFileStorageName { get; set; }
        public string UploadImageFileType { get; set; }

        public string WallpaperId { get; set; }
        public string WallpaperDescriptions { get; set; }
        public int WallpaperFileId { get; set; }
        public string WallpaperFileName { get; set; }
        public string WallpaperFileStorageName { get; set; }
        public string WallpaperFileType { get; set; }

        public string VideoId { get; set; }
        public string VideoDescription { get; set; }
        public string VideoLink { get; set; }



    }
}
