using SourceforqualityAPI.Common;
using SourceforqualityAPI.Model;
using System;
using System.Collections.Generic;

namespace SourceforqualityAPI.Contracts
{
    public class FoodSupplierUpdateInputDTO 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ModifiedBy { get; set; }
        public string RegistrationNumber { get; set; }
        public string  Description { get; set; }
        public string  Logo { get; set; }
        public string PhoneNumber { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Instagram { get; set; }
        public string LinkedIn { get; set; }
        public int CountryId { get; set; }
        public int CreatedBy { get; set; }
        public int ApprovalStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string Email { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Address { get; set; }
        public List<ProductInputModel> Products { get; set; }
        public List<AwardUpdateModel> Awards { get; set; }
        public List<QualificationUpdateModel> Qualifications { get; set; }
        public List<ServiceInputModel> Services { get; set; }
        public List<UploadImageInputModel> UploadImages { get; set; }
        public List<WallpaperInputModel> Wallpapers { get; set; }
        public List<VideoInputModel> Videos { get; set; }


    }

    public class AwardUpdateModel 
    {
        public int? Id { get; set; }
        public bool IsDeleted { get; set; }
        public string Title { get; set; }
        public List<FileOutputDto> Files { get; set; }
    }

    public class QualificationUpdateModel
    {
        public int Id { get; set; }
        public int? CertificationsCategoryId { get; set; }
        public string? OtherCertificationsCategoryName { get; set; }
        public bool IsDeleted { get; set; }
        public string? Title { get; set; }
        public List<FileOutputDto> Files { get; set; }
    }

    public class ProductInputModel
    {
        public int Id { get; set; }
        public int? ProductCategoryId { get; set; }
        public string? OtherProductCategoryName { get; set; }
        public bool IsDeleted { get; set; }
        public string ?Title { get; set; }
        public List<FileOutputDto> Files { get; set; }
    }

    public class ServiceInputModel
    {
        public int Id { get; set; }
        public string ?Title { get; set; }
        public string Descriptions { get; set; }
        public int? BusinessActivityCategoryId { get; set; }
        public string? OtherBusinessActivityCategoryName { get; set; }
        public bool IsDeleted { get; set; }
        public List<FileOutputDto> Files { get; set; }
    }

    public class UploadImageInputModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public List<FileOutputDto> Files { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class WallpaperInputModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public List<FileOutputDto> Files { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class VideoInputModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public bool IsDeleted { get; set; }

    }



    public class FileUpdateDTO : FileOutputDto
    {
        public bool IsDeleted { get; set; }
    }
}
