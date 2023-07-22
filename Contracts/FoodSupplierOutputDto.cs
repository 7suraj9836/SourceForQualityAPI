using SourceforqualityAPI.Common;
using SourceforqualityAPI.Model;
using System.Collections.Generic;

namespace SourceforqualityAPI.Contracts
{
    public class FoodSupplierOutputTest
    {
        public List<Products> SupplierInfo { get; set; }
        public List<Products> Product { get; set; }
        public List<FoodSupplierAward> Awards { get; set; }
        public List<Qualifications> Qualification { get; set; }
        public List<Service> Service { get; set; }
        public List<UploadImages> UploadImage { get; set; }
        public List<Wallpapers> Wallpaper { get; set; }
        public List<Videos> Video { get; set; }
    }

    public class FoodSupplierOutputDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public string ProductCatergoryName { get; set; }
        public string RegistrationNumber { get; set; }
        public string? Description { get; set; }
        public string PhoneNumber { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Instagram { get; set; }
     
        public string Email { get; set; }
        public string LinkedIn { get; set; }
        public string CountryName { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Address { get; set; }
        public string? Logo { get; set; }


        public List<Products> Product { get; set; }
        public List<FoodSupplierAward> Awards { get; set; }
        public List<Qualifications> Qualification { get; set; }
        public List<Service> Service { get; set; }
        public List<UploadImages> UploadImage { get; set; }
        public List<Wallpapers> Wallpaper { get; set; }
        public List<Videos> Video { get; set; }
    }

    public class SPDetail_FoodSupplierAwardTables
    {

        public int AwardId { get; set; }
        public string Title { get; set; }
        public string AwardFileStorageName { get; set; }
        public int AwardFileId { get; set; }
        public string AwardFileName { get; set; }
        public string AwardFileType { get; set; }
    }
    public class FoodSupplierAward
    {
        public int AwardId { get; set; }
        public string Title { get; set; }
        public List<FileOutputDto> Files { get; set; }
    }

    public class SPDetail_ProductTables
    {

        public int ProductId { get; set; }
        public string Title { get; set; }
        public int? ProductCategoryId { get; set; }
        public string? ProductCategoryName { get; set; }
        public string? OtherProductCategoryName { get; set; }
        public string ProductFileStorageName { get; set; }
        public object ProductFileId { get; internal set; }
        public object ProductFileName { get; internal set; }
        public object ProductFileType { get; internal set; }
    }
    public class Products
    {

        public int ProductId { get; set; }
        public string Title { get; set; }
        public int? ProductCategoryId { get; set; }
        public string? ProductCategoryName { get; set; }
        public string? OtherProductCategoryName { get; set; }
        public List<FileOutputDto> Files { get; set; }
    }

    public class SPDetail_QualificationTables
    {

        public int QualificationId { get; set; }
        public string Title { get; set; }
        public int? CertificationsCategoryId { get; set; }
        public string? CertificationsCategoryName { get; set; }
        public string? OtherCertificationsCategoryName { get; set; }
        public string ProductFileStorageName { get; set; }
        public int QualificationFileId { get; set; }
        public string QualificationFileName { get; internal set; }
        public string QualificationFileStorageName { get; internal set; }
        public string QualificationFileType { get; set; }
    }
    public class Qualifications
    {

        public int QualificationId { get; set; }
        public string Title { get; set; }
        public int? CertificationsCategoryId { get; set; }
        public string? CertificationsCategoryName { get; set; }
        public string? OtherCertificationsCategoryName { get; set; }
        public List<FileOutputDto> Files { get; set; }
    }

    public class SPDetail_ServiceTables
    {

        public int ServiceId { get; set; }
        public string Title { get; set; }
        public string Descriptions { get; set; }
        public int? BusinessActivityCategoryId { get; set; }
        public string? BusinessActivityCategoryName { get; set; }
        public string? OtherBusinessActivityCategoryName { get; set; }
        public string ProductFileStorageName { get; set; }
        public int ServiceFileId { get; internal set; }
        public string ServiceFileName { get; internal set; }
        public string ServiceFileStorageName { get; internal set; }
        public string ServiceFileType { get; internal set; }
    }
    public class Service
    {

        public int ServiceId { get; set; }
        public string Title { get; set; }
        public string Descriptions { get; set; }
        public int? BusinessActivityCategoryId { get; set; }
        public string? BusinessActivityCategoryName { get; set; }
        public string? OtherBusinessActivityCategoryName { get; set; }
        public List<FileOutputDto> Files { get; set; }
    }

    public class SPDetail_UploadImagesables
    {
        public int UploadImageId { get; set; }
        public string UploadImageTitle { get; set; }
        public int UploadImageFileId { get; set; }
        public string UploadImageFileName { get; set; }
        public string Description { get; set; }
        public string UploadImageFileStorageName { get; set; }
        public string UploadImageFileType { get; set; }

    }
    public class UploadImages
    {
        public int UploadImageId { get; set; }
        public string Description { get; set; }
        public List<FileOutputDto> Files { get; set; }
    }
    public class SPDetail_WallpapersTables
    {

        public int WallPaperId { get; set; }
        public string WallpaperTitle { get; set; }
        public int WallpaperFileId { get; set; }
        public string WallpaperFileName { get; set; }
        public string WallpaperFileStorageName { get; set; }
        public string WallpaperFileType { get; set; }

    }
    public class Wallpapers
    {
        public int WallPaperId { get; set; }
        public string Description { get; set; }
        public List<FileOutputDto> Files { get; set; }
    }

    public class Videos
    {
        public int VideoId { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
    }
}
