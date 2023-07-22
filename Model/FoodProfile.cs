using Microsoft.AspNetCore.Http;
using SourceforqualityAPI.Common;
using System;
using System.Collections.Generic;

namespace SourceforqualityAPI.Model
{
    public class FoodSupplierProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string RegistrationNumber { get; set; }
        public string? Description { get; set; }
        public string? Logo { get; set; }
        public string PhoneNumber { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Instagram { get; set; }
        public string LinkedIn { get; set; }
        public int ?CountryId { get; set; }
        public int ApprovalStatus { get; set; } = 0;
        public string Email { get; set; }
        public bool SubscriptionStatus { get; set; }
        public bool AccountStatus { get; set; }
        public string Street { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        public List<ProductModel> Products { get; set; }
        public List<AwardModel> Awards { get; set; }
        public List<QualificationModel> Qualifications { get; set; }
        public List<ServiceModel> Services { get; set; }
        public List<UploadImageModel> UploadImages { get; set; }
        public List<WallpaperModel> Wallpapers { get; set; }
        public List<VideoModel> Videos { get; set; }
    }

    public class ProductModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? ProductCategoryId { get; set; }
        public string? OtherProductCatergoryName { get; set; }
        public List<FileDto> Files { get; set; }
    }

    public class AwardModel
    {
        public string Title { get; set; }
        public List<FileDto> Files { get; set; }
    }

    public class QualificationModel
    {
        public string Title { get; set; }
        public int? CertificationsCategoryId { get; set; }
        public string? OtherCertificationsCategoryName  { get; set; }
        public List<FileDto> Files { get; set; }
    }

    public class ServiceModel
    {
        public string Title { get; set; }
        public string Descriptions { get; set; }        
        public int? BusinessActivityCategoryId { get; set; }
        public string? OtherBusinessActivityCategoryName  { get; set; }        
        public List<FileDto> Files { get; set; }
    }

    public class UploadImageModel
    {
        public string Description { get; set; }
        public List<FileDto> Files { get; set; }
    }

    public class WallpaperModel
    {
        public string Description { get; set; }
        public List<FileDto> Files { get; set; }
       
    }

    public class VideoModel
    {
        public string Description { get; set; }
        public string Link { get; set; }

     }

 

}
