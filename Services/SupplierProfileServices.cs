using Dapper;
using Microsoft.AspNetCore.Mvc;
using SourceforqualityAPI.Common;
using SourceforqualityAPI.Model;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System;
using SourceforqualityAPI.Interfaces;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Data.Common;
using Microsoft.AspNetCore.Http;
using SourceforqualityAPI.Contracts;
using System.Drawing;
using Microsoft.IdentityModel.Tokens;
using SourceforqualityAPI.Model.UpdateSupplierTempModels;
using Microsoft.AspNetCore.Http.HttpResults;

namespace SourceforqualityAPI.Services
{

    public class SupplierProfileServices : ISupplierProfileServices
    {
        private readonly IFileUpload _fileUploadService;
        private readonly IFileFormatConverter _fileFormatConverterService;

        public SupplierProfileServices(IFileUpload fileUploadService, IFileFormatConverter fileFormatConverterService)
        {
            _fileUploadService = fileUploadService;
            _fileFormatConverterService = fileFormatConverterService;
        }

        

        public List<FoodSupplierGetDto> GetFoodSupplier(int pageNumber, int pageSize)
        {
            var Ins = new List<FoodSupplierGetDto>();
            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }

                // Calculate the number of rows to skip and the number of rows to take based on the page size and number
                var offset = (pageNumber - 1) * pageSize;

                // Build the SQL query with ORDER BY and OFFSET-FETCH clauses based on the sorting parameters and pagination

                string sqlQuery = $@"SELECT DISTINCT  c.Name as Location,sp.Id,cc.Name as CertificationCategoryName,sp.CreatedBy,bac.Name as BusinessActivityCategoryName, sp.Name,a.Title as AwardTitle, sp.PhoneNumber, sp.Email, sp.SubscriptionStatus, sp.AccountStatus, ns.BusinessActivityCategoryId, a.Title, q.CertificationsCategoryId FROM SupplierProfiles sp LEFT JOIN Countries c ON sp.CountryId=c.Id
                  LEFT JOIN NewService ns ON sp.Id = ns.SupplierProfileId 
                  LEFT JOIN BusinessActivityCategories as bac ON ns.BusinessActivityCategoryId=bac.Id
                  LEFT JOIN Awards a ON sp.Id = a.SupplierProfileId 
                  LEFT JOIN Qualification q ON sp.Id = q.SupplierProfileId 
                  LEFT JOIN CertificationsCategories cc ON q.CertificationsCategoryId=cc.Id
                  WHERE sp.IsDeleted = 0 GROUP BY sp.Id,cc.Name,c.Name,sp.CreatedBy, sp.Name, sp.PhoneNumber, sp.Email, sp.SubscriptionStatus, sp.AccountStatus, ns.BusinessActivityCategoryId, a.Title, q.CertificationsCategoryId,bac.Name";

                Ins = con.Query<FoodSupplierGetDto>(sqlQuery).ToList();
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return Ins;
        }



        public async Task<string> SaveSupplierProfile(FoodSupplierProfile foodSupplier)
        {
            //int ImageId = 0;
            string result = "Success";
            try
            {
                // ...

                if (foodSupplier.CreatedBy <= 0)
                {
                    return "CreatedBy is required";
                }
                if (string.IsNullOrEmpty(foodSupplier.RegistrationNumber))
                {
                    return "RegistrationNumber is required";
                }

                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    using (var transaction = con.BeginTransaction()) // Start a transaction
                    {
                        try
                        {
                            var checkUserId = con.ExecuteScalar<int>("select Id from [dbo].[Users] where Id=@CreatedBy and RoleId=3", new { CreatedBy = foodSupplier.CreatedBy }, transaction: transaction);

                            if (!(checkUserId == foodSupplier.CreatedBy))
                            {
                                return "There is no User with this Id ";
                            }
                            string ExistingNumber = con.ExecuteScalar<string>("select RegistrationNumber from [dbo].[SupplierProfiles] where CreatedBy=@CreatedBy and IsDeleted=0", foodSupplier, transaction: transaction);

                            if (!string.IsNullOrEmpty(ExistingNumber))
                            {
                                return "The user is already registered as supplier";
                            }

                            ExistingNumber = con.ExecuteScalar<string>("select RegistrationNumber from [dbo].[SupplierProfiles] where RegistrationNumber=@RegistrationNumber and IsDeleted=0", foodSupplier, transaction: transaction);

                            if (!string.IsNullOrEmpty(ExistingNumber))
                            {
                                return "The Registration number is already used";
                            }
                            int SupplierID = foodSupplier.Id;
                            bool bIsUpdate = false;

                            if (SupplierID == 0)
                            {

                                foodSupplier.CreatedOn = DateTime.UtcNow;
                                foodSupplier.ModifiedBy = foodSupplier.CreatedBy;
                                foodSupplier.ModifiedOn = DateTime.UtcNow;


                                SupplierID = con.ExecuteScalar<int>("insert into [dbo].[SupplierProfiles] (Name, RegistrationNumber, Description, Logo, PhoneNumber,Facebook, Twitter, Instagram,LinkedIn, CountryId,Email, CreatedBy, CreatedOn,ModifiedBy,ModifiedOn,IsDeleted,Street,PostalCode,City,State,Address,ApprovalStatus) values(@Name, @RegistrationNumber, @Description, @Logo, @PhoneNumber, @Facebook, @Twitter, @Instagram,@LinkedIn,@CountryId,@Email, @CreatedBy, @CreatedOn,@ModifiedBy,@ModifiedOn,0,@Street,@PostalCode,@City,@State,@Address,@ApprovalStatus); select SCOPE_IDENTITY();", foodSupplier, transaction: transaction);

                                //for inserting multiple products
                                if (foodSupplier.Products != null)
                                    if (foodSupplier.Products.Count > 0)
                                    {
                                        foreach (var product in foodSupplier.Products)
                                        {
                                            if (product.ProductCategoryId == 0 || product.ProductCategoryId == null &&
                                            product.OtherProductCatergoryName == null)
                                            {
                                                throw new Exception("Either pass ProductCatergoryId or OtherProductCatergoryName");
                                            }
                                            else if (product.ProductCategoryId > 0)
                                            {
                                                product.OtherProductCatergoryName = null;
                                            }

                                            var ProductId = con.ExecuteScalar<int>(@"Insert into Products(SupplierProfileId,ProductCategoryId,OtherProductCategoryName,Title,CreatedOn,UpdatedOn)
                                          values (@SupplierProfileId,@ProductCategoryId,@OtherProductCatergoryName,@Title,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = SupplierID, ProductCategoryId = product.ProductCategoryId, Title = product.Title , OtherProductCatergoryName=product.OtherProductCatergoryName }, transaction: transaction);

                                            if (product.Files != null)
                                            {
                                                foreach (var file in product.Files)
                                                {
                                                    IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                    var fileId = await _fileUploadService.UploadFile(data);
                                                    con.Execute($"Insert into [dbo].[ProductFileMapper](ProductId,FileId) values({ProductId},{fileId})", transaction: transaction);
                                                }
                                            }


                                        }
                                    }




                                // for inserting multiple awards
                                if (foodSupplier.Awards != null)
                                    if (foodSupplier.Awards.Count > 0)
                                    {
                                        foreach (var award in foodSupplier.Awards)
                                        {

                                            var AwardId = con.ExecuteScalar<int>(@"Insert into Awards(SupplierProfileId,Title,CreatedOn,UpdatedOn)
                                          values (@SupplierProfileId,@Title,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = SupplierID, Title = award.Title }, transaction: transaction);

                                            if (award.Files != null)
                                            {
                                                foreach (var file in award.Files)
                                                {
                                                    IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                    var fileId = await _fileUploadService.UploadFile(data);
                                                    con.Execute($"Insert into AwardFileMapper(AwardId,FileId) values({AwardId},{fileId})", transaction: transaction);
                                                }
                                            }


                                        }
                                    }

                                // for inserting multiple qualifications

                                if (foodSupplier.Qualifications != null)
                                    if (foodSupplier.Qualifications.Count > 0)
                                    {
                                        foreach (var qualification in foodSupplier.Qualifications)
                                        {
                                            if (qualification.CertificationsCategoryId == 0 || qualification.CertificationsCategoryId == null &&
                                 qualification.OtherCertificationsCategoryName == null)
                                            {
                                                throw new Exception("Either pass CertificationsCategoryId or CertificationsCategoryName");
                                            }
                                            else if (qualification.CertificationsCategoryId > 0)
                                            {
                                                qualification.OtherCertificationsCategoryName = null;
                                            }


                                            var QualificationId = con.ExecuteScalar<int>(@"Insert into [dbo].[Qualification](SupplierProfileId,Title,CreatedOn,UpdatedOn,CertificationsCategoryId,OtherCertificationsCategoryName)
                                              values (@SupplierProfileId,@Title,GETUTCDATE(),GETUTCDATE(),@CertificationsCategoryId,@OtherCertificationsCategoryName);select SCOPE_IDENTITY();", new
                                            {
                                                SupplierProfileId = SupplierID,
                                                Title = qualification.Title,
                                                CertificationsCategoryId = qualification.CertificationsCategoryId,
                                                OtherCertificationsCategoryName = qualification.OtherCertificationsCategoryName,
                                            }, transaction: transaction);


                                            if (qualification.Files != null)
                                            {
                                                foreach (var file in qualification.Files)
                                                {
                                                    IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                    var fileId = await _fileUploadService.UploadFile(data);
                                                    con.Execute($"Insert into QualificationFileMapper(QualificationId,FileId) values({QualificationId},{fileId})", transaction: transaction);
                                                }
                                            }
                                        }
                                    }


                                //for inserting multiple services
                                if (foodSupplier.Services != null)
                                    if (foodSupplier.Services.Count > 0)
                                    {
                                        foreach (var service in foodSupplier.Services)
                                        {
                                            if (service.BusinessActivityCategoryId == 0 || service.BusinessActivityCategoryId == null &&
                                                service.OtherBusinessActivityCategoryName == null)
                                            {
                                                throw new Exception("Either pass BusinessActivityCategoryId or OtherBusinessActivityCategoryName");
                                            }
                                            else if (service.BusinessActivityCategoryId > 0)
                                            {
                                                service.OtherBusinessActivityCategoryName = null;
                                            }

                                            var ServiceId = con.ExecuteScalar<int>(@"Insert into [dbo].[NewService](SupplierProfileId,Title,Descriptions,CreatedOn,UpdatedOn,BusinessActivityCategoryId,OtherBusinessActivityCategoryName)
                                         values (@SupplierProfileId,@Title,@Descriptions,GETUTCDATE(),GETUTCDATE(),@BusinessActivityCategoryId,@OtherBusinessActivityCategoryName);select SCOPE_IDENTITY();", new
                                            {
                                                SupplierProfileId = SupplierID,
                                                Title = service.Title,
                                                Descriptions = service.Descriptions,
                                                BusinessActivityCategoryId = service.BusinessActivityCategoryId,
                                                OtherBusinessActivityCategoryName = service.OtherBusinessActivityCategoryName,
                                            }, transaction: transaction);

                                            if (service.Files != null)
                                            {
                                                foreach (var file in service.Files)
                                                {
                                                    IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                    var fileId = await _fileUploadService.UploadFile(data);
                                                    con.Execute($"Insert into ServiceFileMapper(ServiceId,FileId) values({ServiceId},{fileId})", transaction: transaction);
                                                }
                                            }

                                        }
                                    }
                                //for Uploading new Images
                                if (foodSupplier.UploadImages != null)
                                    if (foodSupplier.UploadImages.Count > 0)
                                    {
                                        foreach (var uploadImage in foodSupplier.UploadImages)
                                        {

                                            var UploadImageId = con.ExecuteScalar<int>(@"Insert into [dbo].[UploadsImages](SupplierProfileId,Descriptions,CreatedOn,UpdatedOn)
                                         values (@SupplierProfileId,@Descriptions,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = SupplierID, Descriptions = uploadImage.Description }, transaction: transaction);

                                            if (uploadImage.Files != null)
                                            {
                                                foreach (var file in uploadImage.Files)
                                                {
                                                    IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                    var fileId = await _fileUploadService.UploadFile(data);
                                                    con.Execute($"Insert into UploadImagesFileMapper(UploadImageId,FileId) values({UploadImageId},{fileId})", transaction: transaction);
                                                }
                                            }
                                        }
                                    }
                                //for Uploading WallPaper
                                if (foodSupplier.Wallpapers != null)
                                    if (foodSupplier.Wallpapers.Count > 0)
                                    {
                                        foreach (var wallpaper in foodSupplier.Wallpapers)
                                        {

                                            var WallpaperId = con.ExecuteScalar<int>(@"Insert into [dbo].[Wallpapers](SupplierProfileId,Descriptions,CreatedOn,UpdatedOn)
                                         values (@SupplierProfileId,@Descriptions,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = SupplierID, Descriptions = wallpaper.Description }, transaction: transaction);

                                            if (wallpaper.Files != null)
                                            {
                                                foreach (var file in wallpaper.Files)
                                                {
                                                    IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                    var fileId = await _fileUploadService.UploadFile(data);
                                                    con.Execute($"Insert into WallPaperFileMapper(WallpaperId,FileId) values({WallpaperId},{fileId})", transaction: transaction);
                                                }
                                            }
                                        }
                                    }

                                //  for Uploading Videos
                                if (foodSupplier.Videos != null)
                                    if (foodSupplier.Videos.Count > 0)
                                    {
                                        foreach (var video in foodSupplier.Videos)
                                        {
                                            var VideoId = con.ExecuteScalar<int>(@"Insert into [dbo].[UploadVideo](SupplierProfileId,Description,CreatedOn,UpdatedOn,Link)
                                         values (@SupplierProfileId,@Description,GETUTCDATE(),GETUTCDATE(),@Link);select SCOPE_IDENTITY();", new { SupplierProfileId = SupplierID, Description = video.Description, Link = video.Link }, transaction: transaction);
                                        }
                                    }

                                // Commit the transaction if successful
                                transaction.Commit();

                                

                            }
                        }
                        catch (Exception)
                        {
                            // Handle exception or rollback the transaction
                            transaction.Rollback();
                            throw;
                        }
                    }

                    // ...
                }
            }
            catch (Exception ex)
            {
                result = "Failed";
            }

            return result;
        }


        //Save data temporarily before Admin Approval for Updation
        //public async Task<string> TempSaveSupplierProfile(FoodSupplierProfile foodSupplier)
        //{
        //    int ImageId = 0;
        //    string result = "Success";
        //    try
        //    {
        //        // ...

        //        if (foodSupplier.CreatedBy <= 0)
        //        {
        //            return "CreatedBy is required";
        //        }
        //        if (string.IsNullOrEmpty(foodSupplier.RegistrationNumber))
        //        {
        //            return "RegistrationNumber is required";
        //        }

        //        using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
        //        {
        //            if (con.State == System.Data.ConnectionState.Closed)
        //            {
        //                con.Open();
        //            }

        //            using (var transaction = con.BeginTransaction()) // Start a transaction
        //            {
        //                try
        //                {
        //                    var checkUserId = con.ExecuteScalar<int>("select Id from [dbo].[Users] where Id=@CreatedBy and RoleId=3", new { CreatedBy = foodSupplier.CreatedBy }, transaction: transaction);

        //                    if (!(checkUserId == foodSupplier.CreatedBy))
        //                    {
        //                        return "There is no User with this Id ";
        //                    }
        //                    string ExistingNumber = con.ExecuteScalar<string>("select RegistrationNumber from [dbo].[Temp_SupplierProfiles] where CreatedBy=@CreatedBy and IsDeleted=0", foodSupplier, transaction: transaction);

        //                    if (!string.IsNullOrEmpty(ExistingNumber))
        //                    {
        //                        return "The user is already registered as supplier";
        //                    }

        //                    ExistingNumber = con.ExecuteScalar<string>("select RegistrationNumber from [dbo].[Temp_SupplierProfiles] where RegistrationNumber=@RegistrationNumber and IsDeleted=0", foodSupplier, transaction: transaction);

        //                    if (!string.IsNullOrEmpty(ExistingNumber))
        //                    {
        //                        return "The Registration number is already used";
        //                    }
        //                    int SupplierID = foodSupplier.Id;
        //                    bool bIsUpdate = false;

        //                    if (SupplierID == 0)
        //                    {

        //                        foodSupplier.CreatedOn = DateTime.UtcNow;
        //                        foodSupplier.ModifiedBy = foodSupplier.CreatedBy;
        //                        foodSupplier.ModifiedOn = DateTime.UtcNow;


        //                        SupplierID = con.ExecuteScalar<int>("insert into [dbo].[Temp_SupplierProfiles] (Name, RegistrationNumber, Description, Logo, PhoneNumber,Facebook, Twitter, Instagram,LinkedIn, CountryId,Email, CreatedBy, CreatedOn,ModifiedBy,ModifiedOn,IsDeleted,Street,PostalCode,City,State,Address,ApprovalStatus) values(@Name, @RegistrationNumber, @Description, @Logo, @PhoneNumber, @Facebook, @Twitter, @Instagram,@LinkedIn,@CountryId,@Email, @CreatedBy, @CreatedOn,@ModifiedBy,@ModifiedOn,0,@Street,@PostalCode,@City,@State,@Address,@ApprovalStatus); select SCOPE_IDENTITY();", foodSupplier, transaction: transaction);

        //                        //for inserting multiple products
        //                        if (foodSupplier.Products != null)
        //                            if (foodSupplier.Products.Count > 0)
        //                            {
        //                                foreach (var product in foodSupplier.Products)
        //                                {
        //                                    if (product.ProductCategoryId == 0 || product.ProductCategoryId == null &&
        //                                    product.OtherProductCatergoryName == null)
        //                                    {
        //                                        throw new Exception("Either pass ProductCatergoryId or OtherProductCatergoryName");
        //                                    }
        //                                    else if (product.ProductCategoryId > 0)
        //                                    {
        //                                        product.OtherProductCatergoryName = null;
        //                                    }

        //                                    var ProductId = con.ExecuteScalar<int>(@"Insert into [dbo].[Temp_Products](SupplierProfileId,ProductCategoryId,OtherProductCategoryName,Title,CreatedOn,UpdatedOn)
        //                                  values (@SupplierProfileId,@ProductCategoryId,@OtherProductCatergoryName,@Title,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = SupplierID, ProductCategoryId = product.ProductCategoryId, Title = product.Title, OtherProductCatergoryName = product.OtherProductCatergoryName }, transaction: transaction);

        //                                    if (product.Files != null)
        //                                    {
        //                                        foreach (var file in product.Files)
        //                                        {
        //                                            IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                            var fileId = await _fileUploadService.UploadFile(data);
        //                                            con.Execute($"Insert into [dbo].[Temp_ProductFileMapper](ProductId,FileId) values({ProductId},{fileId})", transaction: transaction);
        //                                        }
        //                                    }


        //                                }
        //                            }




        //                        // for inserting multiple awards
        //                        if (foodSupplier.Awards != null)
        //                            if (foodSupplier.Awards.Count > 0)
        //                            {
        //                                foreach (var award in foodSupplier.Awards)
        //                                {

        //                                    var AwardId = con.ExecuteScalar<int>(@"Insert into [dbo].[Temp_Awards](SupplierProfileId,Title,CreatedOn,UpdatedOn)
        //                                  values (@SupplierProfileId,@Title,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = SupplierID, Title = award.Title }, transaction: transaction);

        //                                    if (award.Files != null)
        //                                    {
        //                                        foreach (var file in award.Files)
        //                                        {
        //                                            IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                            var fileId = await _fileUploadService.UploadFile(data);
        //                                            con.Execute($"Insert into [dbo].[Temp_AwardFileMapper](AwardId,FileId) values({AwardId},{fileId})", transaction: transaction);
        //                                        }
        //                                    }


        //                                }
        //                            }

        //                        // for inserting multiple qualifications

        //                        if (foodSupplier.Qualifications != null)
        //                            if (foodSupplier.Qualifications.Count > 0)
        //                            {
        //                                foreach (var qualification in foodSupplier.Qualifications)
        //                                {
        //                                    if (qualification.CertificationsCategoryId == 0 || qualification.CertificationsCategoryId == null &&
        //                         qualification.OtherCertificationsCategoryName == null)
        //                                    {
        //                                        throw new Exception("Either pass CertificationsCategoryId or CertificationsCategoryName");
        //                                    }
        //                                    else if (qualification.CertificationsCategoryId > 0)
        //                                    {
        //                                        qualification.OtherCertificationsCategoryName = null;
        //                                    }


        //                                    var QualificationId = con.ExecuteScalar<int>(@"Insert into [dbo].[Temp_Qualification](SupplierProfileId,Title,CreatedOn,UpdatedOn,CertificationsCategoryId,OtherCertificationsCategoryName)
        //                                      values (@SupplierProfileId,@Title,GETUTCDATE(),GETUTCDATE(),@CertificationsCategoryId,@OtherCertificationsCategoryName);select SCOPE_IDENTITY();", new
        //                                    {
        //                                        SupplierProfileId = SupplierID,
        //                                        Title = qualification.Title,
        //                                        CertificationsCategoryId = qualification.CertificationsCategoryId,
        //                                        OtherCertificationsCategoryName = qualification.OtherCertificationsCategoryName,
        //                                    }, transaction: transaction);


        //                                    if (qualification.Files != null)
        //                                    {
        //                                        foreach (var file in qualification.Files)
        //                                        {
        //                                            IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                            var fileId = await _fileUploadService.UploadFile(data);
        //                                            con.Execute($"Insert into [dbo].[Temp_QualificationFileMapper](QualificationId,FileId) values({QualificationId},{fileId})", transaction: transaction);
        //                                        }
        //                                    }
        //                                }
        //                            }


        //                        //for inserting multiple services
        //                        if (foodSupplier.Services != null)
        //                            if (foodSupplier.Services.Count > 0)
        //                            {
        //                                foreach (var service in foodSupplier.Services)
        //                                {
        //                                    if (service.BusinessActivityCategoryId == 0 || service.BusinessActivityCategoryId == null &&
        //                                        service.OtherBusinessActivityCategoryName == null)
        //                                    {
        //                                        throw new Exception("Either pass BusinessActivityCategoryId or OtherBusinessActivityCategoryName");
        //                                    }
        //                                    else if (service.BusinessActivityCategoryId > 0)
        //                                    {
        //                                        service.OtherBusinessActivityCategoryName = null;
        //                                    }

        //                                    var ServiceId = con.ExecuteScalar<int>(@"Insert into [dbo].[Temp_Service](SupplierProfileId,Title,Descriptions,CreatedOn,UpdatedOn,BusinessActivityCategoryId,OtherBusinessActivityCategoryName)
        //                                 values (@SupplierProfileId,@Title,@Descriptions,GETUTCDATE(),GETUTCDATE(),@BusinessActivityCategoryId,@OtherBusinessActivityCategoryName);select SCOPE_IDENTITY();", new
        //                                    {
        //                                        SupplierProfileId = SupplierID,
        //                                        Title = service.Title,
        //                                        Descriptions = service.Descriptions,
        //                                        BusinessActivityCategoryId = service.BusinessActivityCategoryId,
        //                                        OtherBusinessActivityCategoryName = service.OtherBusinessActivityCategoryName,
        //                                    }, transaction: transaction);

        //                                    if (service.Files != null)
        //                                    {
        //                                        foreach (var file in service.Files)
        //                                        {
        //                                            IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                            var fileId = await _fileUploadService.UploadFile(data);
        //                                            con.Execute($"Insert into [dbo].[Temp_ServiceFileMapper](ServiceId,FileId) values({ServiceId},{fileId})", transaction: transaction);
        //                                        }
        //                                    }

        //                                }
        //                            }
        //                        //for Uploading new Images
        //                        if (foodSupplier.UploadImages != null)
        //                            if (foodSupplier.UploadImages.Count > 0)
        //                            {
        //                                foreach (var uploadImage in foodSupplier.UploadImages)
        //                                {

        //                                    var UploadImageId = con.ExecuteScalar<int>(@"Insert into [dbo].[Temp_UploadsImages](SupplierProfileId,Descriptions,CreatedOn,UpdatedOn)
        //                                 values (@SupplierProfileId,@Descriptions,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = SupplierID, Descriptions = uploadImage.Description }, transaction: transaction);

        //                                    if (uploadImage.Files != null)
        //                                    {
        //                                        foreach (var file in uploadImage.Files)
        //                                        {
        //                                            IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                            var fileId = await _fileUploadService.UploadFile(data);
        //                                            con.Execute($"Insert into [dbo].[Temp_UploadImagesFileMapper](UploadImageId,FileId) values({UploadImageId},{fileId})", transaction: transaction);
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        //for Uploading WallPaper
        //                        if (foodSupplier.Wallpapers != null)
        //                            if (foodSupplier.Wallpapers.Count > 0)
        //                            {
        //                                foreach (var wallpaper in foodSupplier.Wallpapers)
        //                                {

        //                                    var WallpaperId = con.ExecuteScalar<int>(@"Insert into [dbo].[Temp_Wallpapers](SupplierProfileId,Descriptions,CreatedOn,UpdatedOn)
        //                                 values (@SupplierProfileId,@Descriptions,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = SupplierID, Descriptions = wallpaper.Description }, transaction: transaction);

        //                                    if (wallpaper.Files != null)
        //                                    {
        //                                        foreach (var file in wallpaper.Files)
        //                                        {
        //                                            IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                            var fileId = await _fileUploadService.UploadFile(data);
        //                                            con.Execute($"Insert into [dbo].[Temp_WallPaperFileMapper](WallpaperId,FileId) values({WallpaperId},{fileId})", transaction: transaction);
        //                                        }
        //                                    }
        //                                }
        //                            }

        //                        //  for Uploading Videos
        //                        if (foodSupplier.Videos != null)
        //                            if (foodSupplier.Videos.Count > 0)
        //                            {
        //                                foreach (var video in foodSupplier.Videos)
        //                                {
        //                                    var VideoId = con.ExecuteScalar<int>(@"Insert into Temp_UploadVideo(SupplierProfileId,Description,CreatedOn,UpdatedOn,Link)
        //                                 values (@SupplierProfileId,@Description,GETUTCDATE(),GETUTCDATE(),@Link);select SCOPE_IDENTITY();", new { SupplierProfileId = SupplierID, Description = video.Description, Link = video.Link }, transaction: transaction);
        //                                }
        //                            }

        //                        // Commit the transaction if successful
        //                        transaction.Commit();



        //                    }
        //                }
        //                catch (Exception)
        //                {
        //                    // Handle exception or rollback the transaction
        //                    transaction.Rollback();
        //                    throw;
        //                }
        //            }

        //            // ...
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result = "Failed";
        //    }

        //    return result;
        //}





        public FoodSupplierOutputDto GetFoodSupplierById(int id,int currentUserID)
        {
            var foodSupplierProfile = new FoodSupplierOutputDto();

            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }
                // check if the id and userId are same or the role is admin
                var userRole = con.QuerySingle<string>("select r.RoleName from Users u left join Roles r on u.RoleId=r.RoleId where u.Id=@CurrentUserID", new { CurrentUserID =currentUserID});
                var supplierCount = con.QuerySingle<int>("select count(*) from Temp1_SupplierProfiles where CreatedBy=@CreatedBy;", new { CreatedBy = currentUserID });
                if ((id == currentUserID || userRole.ToLower() == "admin") && supplierCount>0)
                {
                    foodSupplierProfile = TempGetFoodSupplierById(id);
                }
                else
                {
                    var data = con.Query<FoodSupplierOutputDto>(@"EXEC GetFoodSupplierById @Id, @Type", new { Id = id, Type = 1 }).ToList();
                    var data2 = con.Query<SPDetail_ProductTables>(@"EXEC GetFoodSupplierById @Id, @Type", new { Id = id, Type = 2 }).ToList();
                    var data3 = con.Query<SPDetail_ServiceTables>(@"EXEC GetFoodSupplierById @Id, @Type", new { Id = id, Type = 3 }).ToList();
                    var data4 = con.Query<SPDetail_FoodSupplierAwardTables>(@"EXEC GetFoodSupplierById @Id, @Type", new { Id = id, Type = 4 }).ToList();
                    var data5 = con.Query<SPDetail_QualificationTables>(@"EXEC GetFoodSupplierById @Id, @Type", new { Id = id, Type = 5 }).ToList();
                    var data6 = con.Query<SPDetail_UploadImagesables>(@"EXEC GetFoodSupplierById @Id, @Type", new { Id = id, Type = 6 }).ToList();
                    var data7 = con.Query<SPDetail_WallpapersTables>(@"EXEC GetFoodSupplierById @Id, @Type", new { Id = id, Type = 7 }).ToList();
                    var data8 = con.Query<Videos>(@"EXEC GetFoodSupplierById @Id, @Type", new { Id = id, Type = 8 }).ToList();

                    //foodSupplierProfile.Id = id.ToString();
                    foodSupplierProfile.Id = data.First().Id;
                    foodSupplierProfile.Name = data.First().Name;
                    //foodSupplierProfile.ProductCatergoryName = data.First().ProductCatergoryName;
                    foodSupplierProfile.RegistrationNumber = data.First().RegistrationNumber;
                    foodSupplierProfile.Description = data.First().Description;
                    foodSupplierProfile.Logo = data.First().Logo;
                    foodSupplierProfile.PhoneNumber = data.First().PhoneNumber;
                    foodSupplierProfile.Facebook = data.First().Facebook;
                    foodSupplierProfile.Twitter = data.First().Twitter;
                    foodSupplierProfile.Instagram = data.First().Instagram;

                    foodSupplierProfile.Email = data.First().Email;
                    foodSupplierProfile.LinkedIn = data.First().LinkedIn;
                    foodSupplierProfile.Street = data.First().Street;
                    foodSupplierProfile.State = data.First().State;
                    foodSupplierProfile.City = data.First().City;
                    foodSupplierProfile.PostalCode = data.First().PostalCode;
                    foodSupplierProfile.Address = data.First().Address;
                    foodSupplierProfile.CountryName = data.First().CountryName;


                    //for retrieving ProductData

                    foodSupplierProfile.Product = data2.GroupBy(x => new
                    {
                        x.Title,
                        x.ProductId,
                        x.ProductCategoryId,
                        x.ProductCategoryName
                    }).Select(x => new Products()
                    {
                        ProductId = x.Key.ProductId,
                        Title = x.Key.Title,
                        ProductCategoryId = x.Key.ProductCategoryId,
                        ProductCategoryName = x.Key.ProductCategoryName,
                        Files = x.Select(y => new FileOutputDto()
                        {
                            Id = (int)y.ProductFileId,
                            FileName = y.ProductFileName.ToString(),
                            FileStorageName = y.ProductFileStorageName,
                            File = _fileUploadService.GetFile(y.ProductFileStorageName, y.ProductFileType.ToString()).Result
                        }).ToList()
                    }).ToList();

                    foodSupplierProfile.Service = data3.GroupBy(x => new
                    {
                        x.Title,
                        x.ServiceId,
                        x.Descriptions,
                        x.BusinessActivityCategoryId,
                        x.BusinessActivityCategoryName
                    }).Select(x => new Service()
                    {
                        ServiceId = x.Key.ServiceId,
                        Title = x.Key.Title,
                        BusinessActivityCategoryId = x.Key.BusinessActivityCategoryId,
                        BusinessActivityCategoryName = x.Key.BusinessActivityCategoryName,
                        Files = x.Select(y => new FileOutputDto()
                        {
                            Id = y.ServiceFileId,
                            FileName = y.ServiceFileName,
                            FileStorageName = y.ServiceFileStorageName,
                            File = string.IsNullOrEmpty(y.ServiceFileStorageName) ? null : _fileUploadService.GetFile(y.ServiceFileStorageName, y.ServiceFileType.ToString()).Result
                        }).ToList()
                    }).ToList();

                    foodSupplierProfile.Awards = data4.GroupBy(x => new
                    {
                        x.Title,
                        x.AwardId,
                    }).Select(x => new FoodSupplierAward()
                    {
                        AwardId = x.Key.AwardId,
                        Title = x.Key.Title,
                        Files = x.Select(y => new FileOutputDto()
                        {
                            Id = y.AwardFileId,
                            FileName = y.AwardFileName,
                            FileStorageName = y.AwardFileStorageName,
                            File = _fileUploadService.GetFile(y.AwardFileStorageName, y.AwardFileType).Result
                        }).ToList()
                    }).ToList();

                    ////for retrieving QualificationData

                    foodSupplierProfile.Qualification = data5.GroupBy(x => new
                    {
                        x.Title,
                        x.QualificationId,
                        x.CertificationsCategoryId,
                        x.CertificationsCategoryName
                    }).Select(x => new Qualifications()
                    {
                        QualificationId = x.Key.QualificationId,
                        Title = x.Key.Title,
                        CertificationsCategoryId = x.Key.CertificationsCategoryId,
                        CertificationsCategoryName = x.Key.CertificationsCategoryName,
                        Files = x.Select(y => new FileOutputDto()
                        {
                            Id = y.QualificationFileId,
                            FileName = y.QualificationFileName,
                            FileStorageName = y.QualificationFileStorageName,
                            File = _fileUploadService.GetFile(y.QualificationFileStorageName, y.QualificationFileType).Result
                        }).ToList()
                    }).ToList();

                    ////for retrieving upload Image data
                    ///

                    foodSupplierProfile.UploadImage = data6.GroupBy(x => new
                    {
                        x.UploadImageTitle,
                        x.UploadImageId
                    }).Select(x => new UploadImages()
                    {
                        UploadImageId = x.Key.UploadImageId,
                        Description = x.Key.UploadImageTitle,
                        Files = x.Select(y => new FileOutputDto()
                        {
                            Id = y.UploadImageFileId,
                            FileName = y.UploadImageFileName,
                            FileStorageName = y.UploadImageFileStorageName,
                            File = _fileUploadService.GetFile(y.UploadImageFileStorageName, y.UploadImageFileType).Result
                        }).ToList()
                    }).ToList();

                    ////For Retrieving Wallpaper details

                    foodSupplierProfile.Wallpaper = data7.GroupBy(x => new
                    {
                        x.WallpaperTitle,
                        x.WallPaperId,
                    }).Select(x => new Wallpapers()
                    {
                        WallPaperId = x.Key.WallPaperId,
                        Description = x.Key.WallpaperTitle,
                        Files = x.Select(y => new FileOutputDto()
                        {
                            Id = y.WallpaperFileId,
                            FileName = y.WallpaperFileName,
                            FileStorageName = y.WallpaperFileStorageName,
                            File = _fileUploadService.GetFile(y.WallpaperFileStorageName, y.WallpaperFileType).Result
                        }).ToList()
                    }).ToList();

                    //for retrieving videos
                    foodSupplierProfile.Video = data8.Select(r => new Videos() { Description = r.Description, Link = r.Link, VideoId = r.VideoId }).ToList();
                }


                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return foodSupplierProfile;
        }

       
        //Method to get the data stored in the temporary table for particular Supplier

        public FoodSupplierOutputDto TempGetFoodSupplierById(int id)
        {
            var foodSupplierProfile = new FoodSupplierOutputDto();

            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }



                var data = con.Query<FoodSupplierOutputDto>(@"EXEC Temp1_GetFoodSupplierById @Id, @Type", new { Id = id, Type = 1 }).ToList();
                var data2 = con.Query<SPDetail_ProductTables>(@"EXEC Temp1_GetFoodSupplierById @Id, @Type", new { Id = id, Type = 2 }).ToList();
                var data3 = con.Query<SPDetail_ServiceTables>(@"EXEC Temp1_GetFoodSupplierById @Id, @Type", new { Id = id, Type = 3 }).ToList();
                var data4 = con.Query<SPDetail_FoodSupplierAwardTables>(@"EXEC Temp1_GetFoodSupplierById @Id, @Type", new { Id = id, Type = 4 }).ToList();
                var data5 = con.Query<SPDetail_QualificationTables>(@"EXEC Temp1_GetFoodSupplierById @Id, @Type", new { Id = id, Type = 5 }).ToList();
                var data6 = con.Query<SPDetail_UploadImagesables>(@"EXEC Temp1_GetFoodSupplierById @Id, @Type", new { Id = id, Type = 6 }).ToList();
                var data7 = con.Query<SPDetail_WallpapersTables>(@"EXEC Temp1_GetFoodSupplierById @Id, @Type", new { Id = id, Type = 7 }).ToList();
                var data8 = con.Query<Videos>(@"EXEC Temp1_GetFoodSupplierById @Id, @Type", new { Id = id, Type = 8 }).ToList();

                //foodSupplierProfile.Id = id.ToString();
                foodSupplierProfile.Id = data.First().Id;
                foodSupplierProfile.Name = data.First().Name;
                //foodSupplierProfile.ProductCatergoryName = data.First().ProductCatergoryName;
                foodSupplierProfile.RegistrationNumber = data.First().RegistrationNumber;
                foodSupplierProfile.Description = data.First().Description;
                foodSupplierProfile.Logo = data.First().Logo;
                foodSupplierProfile.PhoneNumber = data.First().PhoneNumber;
                foodSupplierProfile.Facebook = data.First().Facebook;
                foodSupplierProfile.Twitter = data.First().Twitter;
                foodSupplierProfile.Instagram = data.First().Instagram;

                foodSupplierProfile.Email = data.First().Email;
                foodSupplierProfile.LinkedIn = data.First().LinkedIn;
                foodSupplierProfile.Street = data.First().Street;
                foodSupplierProfile.State = data.First().State;
                foodSupplierProfile.City = data.First().City;
                foodSupplierProfile.PostalCode = data.First().PostalCode;
                foodSupplierProfile.Address = data.First().Address;
                foodSupplierProfile.CountryName = data.First().CountryName;


                //for retrieving ProductData

                foodSupplierProfile.Product = data2.GroupBy(x => new
                {
                    x.Title,
                    x.ProductId,
                    x.ProductCategoryId,
                    x.ProductCategoryName,
                    x.OtherProductCategoryName,
                }).Select(x => new Products()
                {
                    ProductId = x.Key.ProductId,
                    Title = x.Key.Title,
                    ProductCategoryId = x.Key.ProductCategoryId,
                    ProductCategoryName = x.Key.ProductCategoryName,
                    OtherProductCategoryName=x.Key.OtherProductCategoryName,
                    // do this null check for each of the module
                    Files = x.Select(y => y.ProductFileName==null? new FileOutputDto() : new FileOutputDto()
                    {
                        Id = (int)y.ProductFileId,
                        FileName = y.ProductFileName.ToString(),
                        FileStorageName = y.ProductFileStorageName,
                        File = _fileUploadService.GetFile(y.ProductFileStorageName, y.ProductFileType.ToString()).Result
                    }).ToList()
                }).ToList();

                foodSupplierProfile.Service = data3.GroupBy(x => new
                {
                    x.Title,
                    x.ServiceId,
                    x.Descriptions,
                    x.BusinessActivityCategoryId,
                    x.BusinessActivityCategoryName,
                    x.OtherBusinessActivityCategoryName,
                }).Select(x => new Service()
                {
                    ServiceId = x.Key.ServiceId,
                    Title = x.Key.Title,
                    BusinessActivityCategoryId = x.Key.BusinessActivityCategoryId,
                    BusinessActivityCategoryName = x.Key.BusinessActivityCategoryName,
                    OtherBusinessActivityCategoryName= x.Key.OtherBusinessActivityCategoryName,
                    Files = x.Select(y => y.ServiceFileName == null ? new FileOutputDto() : new FileOutputDto()
                    {
                        Id = y.ServiceFileId,
                        FileName = y.ServiceFileName,
                        FileStorageName = y.ServiceFileStorageName,
                        File = string.IsNullOrEmpty(y.ServiceFileStorageName) ? null : _fileUploadService.GetFile(y.ServiceFileStorageName, y.ServiceFileType.ToString()).Result
                    }).ToList()
                }).ToList();

                foodSupplierProfile.Awards = data4.GroupBy(x => new
                {
                    x.Title,
                    x.AwardId,
                }).Select(x => new FoodSupplierAward()
                {
                    AwardId = x.Key.AwardId,
                    Title = x.Key.Title,
                    Files = x.Select(y => y.AwardFileName == null ? new FileOutputDto() : new FileOutputDto()
                    {
                        Id = y.AwardFileId,
                        FileName = y.AwardFileName,
                        FileStorageName = y.AwardFileStorageName,
                        File = _fileUploadService.GetFile(y.AwardFileStorageName, y.AwardFileType).Result
                    }).ToList()
                }).ToList();

                ////for retrieving QualificationData

                foodSupplierProfile.Qualification = data5.GroupBy(x => new
                {
                    x.Title,
                    x.QualificationId,
                    x.CertificationsCategoryId,
                    x.CertificationsCategoryName,
                    x.OtherCertificationsCategoryName,
                }).Select(x => new Qualifications()
                {
                    QualificationId = x.Key.QualificationId,
                    Title = x.Key.Title,
                    CertificationsCategoryId = x.Key.CertificationsCategoryId,
                    CertificationsCategoryName = x.Key.CertificationsCategoryName,
                    OtherCertificationsCategoryName= x.Key.OtherCertificationsCategoryName,
                    Files = x.Select(y => y.QualificationFileName == null ? new FileOutputDto() : new FileOutputDto()
                    {
                        Id = y.QualificationFileId,
                        FileName = y.QualificationFileName,
                        FileStorageName = y.QualificationFileStorageName,
                        File = _fileUploadService.GetFile(y.QualificationFileStorageName, y.QualificationFileType).Result
                    }).ToList()
                }).ToList();

                ////for retrieving upload Image data
                ///

                foodSupplierProfile.UploadImage = data6.GroupBy(x => new
                {
                    x.UploadImageTitle,
                    x.UploadImageId
                }).Select(x => new UploadImages()
                {
                    UploadImageId = x.Key.UploadImageId,
                    Description = x.Key.UploadImageTitle,
                    Files = x.Select(y => y.UploadImageFileName == null ? new FileOutputDto() : new FileOutputDto()
                    {
                        Id = y.UploadImageFileId,
                        FileName = y.UploadImageFileName,
                        FileStorageName = y.UploadImageFileStorageName,
                        File = _fileUploadService.GetFile(y.UploadImageFileStorageName, y.UploadImageFileType).Result
                    }).ToList()
                }).ToList();

                ////For Retrieving Wallpaper details

                foodSupplierProfile.Wallpaper = data7.GroupBy(x => new
                {
                    x.WallpaperTitle,
                    x.WallPaperId,
                }).Select(x => new Wallpapers()
                {
                    WallPaperId = x.Key.WallPaperId,
                    Description = x.Key.WallpaperTitle,
                    Files = x.Select(y => y.WallpaperFileName == null ? new FileOutputDto() : new FileOutputDto()
                    {
                        Id = y.WallpaperFileId,
                        FileName = y.WallpaperFileName,
                        FileStorageName = y.WallpaperFileStorageName,
                        File = _fileUploadService.GetFile(y.WallpaperFileStorageName, y.WallpaperFileType).Result
                    }).ToList()
                }).ToList();

                //for retrieving videos
                foodSupplierProfile.Video = data8.Select(r => new Videos() { Description = r.Description, Link = r.Link, VideoId = r.VideoId }).ToList();

                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return foodSupplierProfile;
        }



        public async Task<string> UpdateUserFavouriteSupplier(UserFavouriteSupplierDataModel inputModel)
        {
            /* List<FoodSupplierProfile>*/
            string output = "";

            using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }

                output = con.Query<string>($"exec UserSuplierFavMarker  @UserId,  @SupplierId, @IsFavourite ",inputModel).FirstOrDefault();

                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            return output;
        }

        //public async Task<string> UpdateFoodSupplier(FoodSupplierUpdateInputDTO foodSupplier)
        //{
        //    String result = "Success";
        //    try
        //    {
        //        using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
        //        {
        //            if (con.State == System.Data.ConnectionState.Closed)
        //            {
        //                con.Open();
        //            }
                   
        //            // Start a transaction
        //            using (var transaction = con.BeginTransaction())
        //            {
        //                try
        //                {
        //                    string ExistingNumber = con.ExecuteScalar<string>("select RegistrationNumber from [dbo].[SupplierProfiles] where RegistrationNumber=@RegistrationNumber and IsDeleted=0 and Id<>@Id", foodSupplier, transaction: transaction);

        //                    if (!string.IsNullOrEmpty(ExistingNumber))
        //                    {
        //                        return "The Registration number is already used";
        //                    }


        //                    con.ExecuteScalar<int>("UPDATE [dbo].[SupplierProfiles] SET Name=@Name, RegistrationNumber = @RegistrationNumber, Description = @Description, Logo = @Logo, PhoneNumber = @PhoneNumber,Facebook = @Facebook, Twitter = @Twitter, ModifiedBy=@ModifiedBy, Instagram = @Instagram, ModifiedOn=Getutcdate() , CountryId = @CountryId,Street=@Street,PostalCode=@PostalCode,City=@City,State=@State,Address=@Address  WHERE Id = @Id; ", foodSupplier, transaction: transaction);

        //                    //update foodSupplier Awards


        //                    if (foodSupplier.Awards != null)
        //                    {
        //                        foreach (var award in foodSupplier.Awards)
        //                        {
        //                            if (award == null)
        //                            {
        //                                continue;
        //                            }
        //                            if (award.Id == 0 || award.Id == null)
        //                            {
        //                                var AwardId = con.ExecuteScalar<int>(@"Insert into Awards(SupplierProfileId,Title,CreatedOn,UpdatedOn)
        //                                  values (@SupplierProfileId,@Title,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Title = award.Title }, transaction: transaction);

        //                                if (award.Files != null)
        //                                {
        //                                    foreach (var file in award.Files)
        //                                    {
        //                                        IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                        var fileId = await _fileUploadService.UploadFile(data);
        //                                        con.Execute($"Insert into AwardFileMapper(AwardId,FileId) values({AwardId},{fileId})", transaction: transaction);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (award.IsDeleted == true)
        //                                {
        //                                    var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from AwardFileMapper afm 
        //                                                        inner join FileUploadTable ft on afm.FileId=ft.Id where afm.AwardId=@AwardId ", new { AwardId = award.Id }, transaction: transaction);
        //                                    foreach (var item in FilesAssociated)
        //                                    {
        //                                        _fileUploadService.DeleteFile(item);
        //                                    }
        //                                    con.Execute("delete from AwardFileMapper where AwardId=@awardId;delete from Awards where Id=@awardId;", new { awardId = award.Id }, transaction: transaction);
        //                                }
        //                                else
        //                                {
        //                                    con.Execute("update Awards set Title=@Title where Id=@AwardID ", new { Title = award.Title, AwardId = award.Id }, transaction: transaction);
        //                                    foreach (var file in award.Files)
        //                                    {
        //                                        if (file.IsDeleted && file.Id > 0)
        //                                        {
        //                                            var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id }, transaction: transaction);
        //                                            _fileUploadService.DeleteFile(fileStorageName);
        //                                            con.Execute("delete from AwardFileMapper where AwardId=@AwardId;", new { AwardId = award.Id }, transaction: transaction);
        //                                        }
        //                                        else if (file.Id == 0 || file.Id == null)
        //                                        {
        //                                            IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                            var fileId = _fileUploadService.UploadFile(data);
        //                                            con.Execute($"Insert into AwardFileMapper(AwardId,FileId) values({award.Id},{fileId})", transaction: transaction);
        //                                        }
        //                                    }
        //                                }

        //                            }
        //                        }
        //                    }

        //                    //update Qualification

        //                    if (foodSupplier.Qualifications != null)
        //                    {
        //                        foreach (var qualification in foodSupplier.Qualifications)
        //                        {
        //                            if (qualification.CertificationsCategoryId == 0 || qualification.CertificationsCategoryId == null &&
        //                            qualification.OtherCertificationsCategoryName == null)
        //                            {
        //                                throw new Exception("Either pass CertificationsCategoryId or OtherCertificationsCategoryName");
        //                            }
        //                            else if (qualification.CertificationsCategoryId > 0)
        //                            {
        //                                qualification.OtherCertificationsCategoryName = null;
        //                            }
        //                            if (qualification == null)
        //                            {
        //                                continue;
        //                            }
        //                            if (qualification.Id == 0 || qualification.Id == null)
        //                            {
        //                                var QualificationId = con.ExecuteScalar<int>(@"Insert into [dbo].[Qualification](SupplierProfileId,Title,CreatedOn,UpdatedOn,CertificationsCategoryId,OtherCertificationsCategoryName)
        //                                      values (@SupplierProfileId,@Title,GETUTCDATE(),GETUTCDATE(),@CertificationsCategoryId,@OtherCertificationsCategoryName);select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Title = qualification.Title, CertificationsCategoryId = qualification.CertificationsCategoryId, OtherCertificationsCategoryName = qualification.OtherCertificationsCategoryName }, transaction: transaction);

        //                                if (qualification.Files != null)
        //                                {
        //                                    foreach (var file in qualification.Files)
        //                                    {
        //                                        IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                        var fileId = await _fileUploadService.UploadFile(data);
        //                                        con.Execute($"Insert into QualificationFileMapper(QualificationId,FileId) values({QualificationId},{fileId})", transaction: transaction);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (qualification.IsDeleted == true)
        //                                {
        //                                    var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from QualificationFileMapper afm 
        //                                                        inner join FileUploadTable ft on afm.FileId=ft.Id where afm.QualificationId=@QualificationId ", new { QualificationId = qualification.Id }, transaction: transaction);
        //                                    foreach (var item in FilesAssociated)
        //                                    {
        //                                        _fileUploadService.DeleteFile(item);
        //                                    }
        //                                    con.Execute("delete from QualificationFileMapper where QualificationId=@qualificationId;delete from Qualification where Id=@qualificationId;", new { qualificationId = qualification.Id }, transaction: transaction);
        //                                }
        //                                else
        //                                {
        //                                    con.Execute("update Qualification set Title=@Title,CertificationsCategoryId=@CertificationsCategoryId,OtherCertificationsCategoryName=@OtherCertificationsCategoryName where Id=@QualificationId ", new { Title = qualification.Title, QualificationId = qualification.Id, CertificationsCategoryId = qualification.CertificationsCategoryId, OtherCertificationsCategoryName = qualification.OtherCertificationsCategoryName }, transaction: transaction);
        //                                    foreach (var file in qualification.Files)
        //                                    {
        //                                        if (file.IsDeleted && file.Id > 0)
        //                                        {
        //                                            var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id }, transaction: transaction);
        //                                            _fileUploadService.DeleteFile(fileStorageName);
        //                                            con.Execute("delete from QualificationFileMapper where FileId=@FileId;", new { qualificationId = qualification.Id, FileId = file.Id }, transaction: transaction);
        //                                        }
        //                                        else if (file.Id == 0 || file.Id == null)
        //                                        {
        //                                            IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                            var fileId = _fileUploadService.UploadFile(data);
        //                                            con.Execute($"Insert into QualificationFileMapper(QualificationId,FileId) values({qualification.Id},{fileId})", transaction: transaction);
        //                                        }
        //                                    }
        //                                }

        //                            }
        //                        }
        //                    }

        //                    //ut 
        //                    if (foodSupplier.Products != null)
        //                    {
        //                        foreach (var product in foodSupplier.Products)
        //                        {
        //                            if (product.ProductCategoryId == 0 || product.ProductCategoryId == null &&
        //                            product.OtherProductCategoryName == null)
        //                            {
        //                                throw new Exception("Either pass ProductCatergoryId or OtherProductCatergoryName");
        //                            }
        //                            else if (product.ProductCategoryId > 0)
        //                            {
        //                                product.OtherProductCategoryName = null;
        //                            }
        //                            if (product == null)
        //                            {
        //                                continue;
        //                            }
        //                            if (product.Id == 0 || product.Id == null)
        //                            {
        //                                var ProductId = con.ExecuteScalar<int>(@"Insert into [Products](SupplierProfileId,Title,ProductCategoryId,OtherProductCategoryName,CreatedOn,UpdatedOn)
        //                                  values (@SupplierProfileId,@Title,@ProductCategoryId,@OtherProductCategoryName,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Title = product.Title, ProductCategoryId = product.ProductCategoryId, OtherProductCategoryName = product.OtherProductCategoryName }, transaction: transaction);

        //                                if (product.Files != null)
        //                                {
        //                                    foreach (var file in product.Files)
        //                                    {
        //                                        IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                        var fileId = await _fileUploadService.UploadFile(data);
        //                                        con.Execute($"Insert into [ProductFileMapper](ProductId,FileId) values({ProductId},{fileId})", transaction: transaction);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (product.IsDeleted == true)
        //                                {
        //                                    var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from ProductFileMapper afm inner join FileUploadTable ft on afm.FileId=ft.Id where afm.ProductId=@ProductId ", new { ProductId = product.Id }, transaction: transaction);
        //                                    foreach (var item in FilesAssociated)
        //                                    {
        //                                        _fileUploadService.DeleteFile(item);
        //                                    }
        //                                    con.Execute("delete from ProductFileMapper where ProductId=@productId;delete from Products where Id=@productId;", new { productId = product.Id }, transaction: transaction);
        //                                }
        //                                else
        //                                {
        //                                    con.Execute("update Products set Title=@Title,ProductCategoryId=@ProductCategoryId,OtherProductCategoryName=@OtherProductCategoryName where Id=@ProductId ", new { Title = product.Title, ProductId = product.Id, ProductCategoryId = product.ProductCategoryId, OtherProductCategoryName = product.OtherProductCategoryName }, transaction: transaction);

        //                                    foreach (var file in product.Files)
        //                                    {
        //                                        if (file.IsDeleted && file.Id > 0)
        //                                        {
        //                                            var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id }, transaction: transaction);
        //                                            _fileUploadService.DeleteFile(fileStorageName);
        //                                            con.Execute("delete from ProductFileMapper where FileId=@FileId;", new { FileId = file.Id }, transaction: transaction);
        //                                        }
        //                                        else if (file.Id == 0 || file.Id == null)
        //                                        {
        //                                            IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                            var fileId = _fileUploadService.UploadFile(data);
        //                                            con.Execute($"Insert into ProductFileMapper(ProductId,FileId) values({product.Id},{fileId})", transaction: transaction);
        //                                        }
        //                                    }
        //                                }

        //                            }
        //                        }
        //                    }
        //                    //ut 
        //                    if (foodSupplier.Services != null)
        //                    {
        //                        foreach (var service in foodSupplier.Services)
        //                        {
        //                            if (service.BusinessActivityCategoryId == 0 || service.BusinessActivityCategoryId == null &&
        //                            service.OtherBusinessActivityCategoryName == null)
        //                            {
        //                                throw new Exception("Either pass BusinessActivityCategoryId or OtherBusinessActivityCategoryName");
        //                            }
        //                            else if (service.BusinessActivityCategoryId > 0)
        //                            {
        //                                service.OtherBusinessActivityCategoryName = null;
        //                            }
        //                            if (service == null)
        //                            {
        //                                continue;
        //                            }
        //                            if (service.Id == 0)
        //                            {
        //                                /// add missing fields
        //                                var ServiceId = con.ExecuteScalar<int>(@"Insert into [NewService](SupplierProfileId,Title,Descriptions,BusinessActivityCategoryId,OtherBusinessActivityCategoryName,CreatedOn,UpdatedOn)values(@SupplierProfileId,@Title,@Descriptions,@BusinessActivityCategoryId,@OtherBusinessActivityCategoryName,GETUTCDATE(),GETUTCDATE()); select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Title = service.Title, Descriptions = service.Descriptions, BusinessActivityCategoryId = service.BusinessActivityCategoryId, OtherBusinessActivityCategoryName = service.OtherBusinessActivityCategoryName }, transaction: transaction);

        //                                if (service.Files != null)
        //                                {
        //                                    foreach (var file in service.Files)
        //                                    {
        //                                        IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                        var fileId = await _fileUploadService.UploadFile(data);
        //                                        con.Execute($"Insert into [ServiceFileMapper] (ServiceId,FileId) values({ServiceId},{fileId})", transaction: transaction);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (service.IsDeleted == true)
        //                                {
        //                                    var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from ServiceFileMapper afm inner join FileUploadTable ft on afm.FileId=ft.Id where afm.ServiceId=@ServiceId ", new { ServiceId = service.Id }, transaction: transaction);
        //                                    foreach (var item in FilesAssociated)
        //                                    {
        //                                        _fileUploadService.DeleteFile(item);
        //                                    }
        //                                    con.Execute("delete from ServiceFileMapper where ServiceId=@serviceId;delete from NewService where Id=@serviceId;", new { serviceId = service.Id }, transaction: transaction);
        //                                }
        //                                else
        //                                {
        //                                    con.Execute("update NewService set Title=@Title,Descriptions=@Descriptions,BusinessActivityCategoryId=@BusinessActivityCategoryId,OtherBusinessActivityCategoryName=@OtherBusinessActivityCategoryName where Id=@ServiceId ", new { Title = service.Title, ServiceId = service.Id, Descriptions = service.Descriptions, BusinessActivityCategoryId = service.BusinessActivityCategoryId, OtherBusinessActivityCategoryName = service.OtherBusinessActivityCategoryName }, transaction: transaction);
        //                                    foreach (var file in service.Files)
        //                                    {
        //                                        if (file.IsDeleted && file.Id > 0)
        //                                        {
        //                                            var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id }, transaction: transaction);
        //                                            _fileUploadService.DeleteFile(fileStorageName);
        //                                            con.Execute("delete from ServiceFileMapper where FileId=@FileId;", new { FileId = file.Id }, transaction: transaction);
        //                                        }
        //                                        else if (file.Id == 0 || file.Id == null)
        //                                        {
        //                                            IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                            var fileId = await _fileUploadService.UploadFile(data);
        //                                            con.Execute($"Insert into ServiceFileMapper(ServiceId,FileId) values({service.Id},{fileId})", transaction: transaction);
        //                                            //   con.Execute($"Insert into ProductFileMapper(ProductId,FileId) values({product.Id},{fileId})");
        //                                        }
        //                                    }
        //                                }

        //                            }
        //                        }
        //                    }

        //                    //update UploadImages

        //                    if (foodSupplier.UploadImages != null)
        //                    {
        //                        foreach (var uploadImage in foodSupplier.UploadImages)
        //                        {
        //                            if (uploadImage == null)
        //                            {
        //                                continue;
        //                            }
        //                            if (uploadImage.Id == 0 || uploadImage.Id == null)
        //                            {
        //                                var UploadImageId = con.ExecuteScalar<int>(@"Insert into [UploadsImages](SupplierProfileId,Descriptions,CreatedOn,UpdatedOn)
        //                                  values (@SupplierProfileId,@Description,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Description = uploadImage.Description }, transaction: transaction);

        //                                if (uploadImage.Files != null)
        //                                {
        //                                    foreach (var file in uploadImage.Files)
        //                                    {
        //                                        IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                        var fileId = await _fileUploadService.UploadFile(data);
        //                                        con.Execute($"Insert into [UploadImagesFileMapper](UploadImageId,FileId) values({UploadImageId},{fileId})", transaction: transaction);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (uploadImage.IsDeleted == true)
        //                                {
        //                                    var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from UploadImagesFileMapper afm 
        //                                                        inner join FileUploadTable ft on afm.FileId=ft.Id where afm.UploadImageId=@UploadImageId ", new { UploadImageId = uploadImage.Id }, transaction: transaction);
        //                                    foreach (var item in FilesAssociated)
        //                                    {
        //                                        _fileUploadService.DeleteFile(item);
        //                                    }
        //                                    con.Execute("delete from UploadImagesFileMapper where UploadImageId=@uploadImageId;delete from UploadsImages where Id=@uploadImageId;", new { uploadImageId = uploadImage.Id }, transaction: transaction);
        //                                }
        //                                else
        //                                {
        //                                    con.Execute("update [UploadsImages] set Descriptions=@Description where Id=@UploadImageId ", new { Description = uploadImage.Description, UploadImageId = uploadImage.Id }, transaction: transaction);
        //                                    foreach (var file in uploadImage.Files)
        //                                    {
        //                                        if (file.IsDeleted && file.Id > 0)
        //                                        {
        //                                            var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id }, transaction: transaction);
        //                                            _fileUploadService.DeleteFile(fileStorageName);
        //                                            con.Execute("delete from UploadImagesFileMapper where FileId=@FileId;", new { FileId = file.Id }, transaction: transaction);
        //                                        }
        //                                        else if (file.Id == 0 || file.Id == null)
        //                                        {
        //                                            var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id }, transaction: transaction);
        //                                            _fileUploadService.DeleteFile(fileStorageName);
        //                                            IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                            var fileId = _fileUploadService.UploadFile(data);
        //                                            con.Execute($"Insert into UploadImagesFileMapper(UploadImageId,FileId) values({uploadImage.Id},{fileId})", transaction: transaction);
        //                                        }
        //                                    }
        //                                }

        //                            }
        //                        }
        //                    }

        //                    //update wallPapers

        //                    if (foodSupplier.Wallpapers != null)
        //                    {
        //                        foreach (var wallpaper in foodSupplier.Wallpapers)
        //                        {
        //                            if (wallpaper == null)
        //                            {
        //                                continue;
        //                            }
        //                            if (wallpaper.Id == 0 || wallpaper.Id == null)
        //                            {
        //                                var WallpaperId = con.ExecuteScalar<int>(@"Insert into [Wallpapers](SupplierProfileId,Descriptions,CreatedOn,UpdatedOn)
        //                                  values (@SupplierProfileId,@Descriptions,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Descriptions = wallpaper.Description }, transaction: transaction);

        //                                if (wallpaper.Files != null)
        //                                {
        //                                    foreach (var file in wallpaper.Files)
        //                                    {
        //                                        IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                        var fileId = await _fileUploadService.UploadFile(data);
        //                                        con.Execute($"Insert into [WallPaperFileMapper](WallpaperId,FileId) values({WallpaperId},{fileId})", transaction: transaction);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (wallpaper.IsDeleted == true)
        //                                {
        //                                    var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from WallPaperFileMapper afm 
        //                                                        inner join FileUploadTable ft on afm.FileId=ft.Id where afm.WallpaperId=@WallpaperId ", new { WallpaperId = wallpaper.Id }, transaction: transaction);
        //                                    foreach (var item in FilesAssociated)
        //                                    {
        //                                        _fileUploadService.DeleteFile(item);
        //                                    }
        //                                    con.Execute("delete from WallPaperFileMapper where WallpaperId=@wallpaperId;delete from Wallpapers where Id=@wallpaperId;", new { wallpaperId = wallpaper.Id }, transaction: transaction);
        //                                }
        //                                else
        //                                {
        //                                    con.Execute("update [Wallpapers] set Descriptions=@Descriptions where Id=@WallpaperId ", new { Descriptions = wallpaper.Description, WallpaperId = wallpaper.Id }, transaction: transaction);
        //                                    foreach (var file in wallpaper.Files)
        //                                    {
        //                                        if (file.IsDeleted && file.Id > 0)
        //                                        {
        //                                            var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id }, transaction: transaction);
        //                                            _fileUploadService.DeleteFile(fileStorageName);
        //                                            con.Execute("delete from WallPaperFileMapper where FileId=@FileId;", new { FileId = file.Id }, transaction: transaction);
        //                                        }
        //                                        else if (file.Id == 0 || file.Id == null)
        //                                        {
        //                                            IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                            var fileId = _fileUploadService.UploadFile(data);
        //                                            con.Execute($"Insert into WallPaperFileMapper(WallpaperId,FileId) values({wallpaper.Id},{fileId})", transaction: transaction);
        //                                        }
        //                                    }
        //                                }

        //                            }
        //                        }
        //                    }

        //                    //updating Videos

        //                    if (foodSupplier.Videos != null)
        //                        if (foodSupplier.Videos.Count > 0)
        //                        {
        //                            foreach (var video in foodSupplier.Videos)
        //                            {
        //                                con.ExecuteScalar(@"update [dbo].[UploadVideo] set Description=@Description,Link=@Link where Id=@Id;", new { Id = video.Id, Description = video.Description, Link = video.Link }, transaction: transaction);
        //                            }
        //                        }
        //                    transaction.Commit();
        //                }
        //                catch (Exception)
        //                {
        //                    // Handle exception or rollback the transaction
        //                    transaction.Rollback();
        //                    throw;
        //                }
        //            }

                   


        //            if (con.State == System.Data.ConnectionState.Open)
        //            {
        //                con.Close();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result = "Failed";
        //    }
        //    return result;
        //}

        public async Task<string> TempUpdateFoodSupplier(FoodSupplierUpdateInputDTO foodSupplier)
        {
            String result = "Success";
            try
            {
                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    // Start a transaction
                    using (var transaction = con.BeginTransaction())
                    {
                        try
                        {
                            string ExistingNumber = con.ExecuteScalar<string>("select RegistrationNumber from [dbo].[SupplierProfiles] where RegistrationNumber=@RegistrationNumber and IsDeleted=0 and Id<>@Id", foodSupplier, transaction: transaction);
                            foodSupplier.CreatedBy = con.ExecuteScalar<int>("select CreatedBy from [dbo].[SupplierProfiles] where isNull(IsDeleted,0)=0 and Id=@Id", foodSupplier, transaction: transaction);
                            if (!string.IsNullOrEmpty(ExistingNumber))
                            {
                                return "The Registration number is already used";
                            }

                            //con.ExecuteScalar<int>("UPDATE [dbo].[SupplierProfiles] SET Name=@Name, RegistrationNumber = @RegistrationNumber, Description = @Description, Logo = @Logo, PhoneNumber = @PhoneNumber,Facebook = @Facebook, Twitter = @Twitter, ModifiedBy=@ModifiedBy, Instagram = @Instagram, ModifiedOn=Getutcdate() , CountryId = @CountryId,Street=@Street,PostalCode=@PostalCode,City=@City,State=@State,Address=@Address  WHERE Id = @Id; ", foodSupplier, transaction: transaction);

                            foodSupplier.CreatedOn = DateTime.UtcNow;
                            foodSupplier.ModifiedBy = foodSupplier.CreatedBy;
                            foodSupplier.ModifiedOn = DateTime.UtcNow;
                            var createdBy = con.ExecuteScalar<int>("Select top 1 CreatedBy from SupplierProfiles where Id=@SupplierId", new { SupplierId = foodSupplier.Id }, transaction: transaction);
                            var exitsSupplier = con.ExecuteScalar<int>("Select Count(*) from Temp1_SupplierProfiles where Id=@SupplierId", new { SupplierId = foodSupplier.Id }, transaction: transaction);
                            if (exitsSupplier > 0)
                            {
                                con.ExecuteScalar<int>("UPDATE [dbo].[Temp1_SupplierProfiles] SET Name=@Name, RegistrationNumber = @RegistrationNumber, Description = @Description, Logo = @Logo, PhoneNumber = @PhoneNumber,Facebook = @Facebook, Twitter = @Twitter, ModifiedBy=@ModifiedBy, Instagram = @Instagram, ModifiedOn=Getutcdate() , CountryId = @CountryId,Street=@Street,PostalCode=@PostalCode,City=@City,State=@State,Address=@Address,CreatedOn=getutcdate(),ApprovalStatus=0 WHERE Id = @Id; ", foodSupplier, transaction: transaction);
                            }
                            else
                            {
                                var SupplierID = con.ExecuteScalar<int>(@"insert into [dbo].[Temp1_SupplierProfiles] (Id,Name, RegistrationNumber, Description, Logo, PhoneNumber,Facebook, Twitter, Instagram,LinkedIn,CountryId,Email, CreatedBy, CreatedOn,ModifiedBy,IsDeleted,Street,PostalCode,City,State,Address,ApprovalStatus) 
                                                    values(@Id,@Name, @RegistrationNumber, @Description, @Logo, @PhoneNumber, @Facebook, @Twitter, @Instagram,@LinkedIn,@CountryId,@Email, @CreatedBy, getutcdate(),@ModifiedBy,0,@Street,@PostalCode,@City,@State,@Address,0)", foodSupplier, transaction: transaction);
                            }


                            //update foodSupplier Awards


                            if (foodSupplier.Awards != null)
                            {
                                foreach (var award in foodSupplier.Awards)
                                {
                                    if (award == null)
                                    {
                                        continue;
                                    }
                                    if (award.Id == 0 || award.Id == null)
                                    {

                                        //insert into temp1Awards
                                        var AwardId = con.ExecuteScalar<int>(@"Insert into Awards(SupplierProfileId,Title,CreatedOn,UpdatedOn,IsApproved)
                                          values (@SupplierProfileId,@Title,GETUTCDATE(),GETUTCDATE(),0);select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Title = award.Title }, transaction: transaction);
                                        con.Execute($@"insert into Temp1_Awards(Id,SupplierProfileId,Title,UserId,CreatedOn,UpdatedOn)
                                        values(${AwardId},${foodSupplier.Id},@Title,{createdBy},GetUtcdate(),getutcDate());", new { Title = award.Title, }, transaction);
                                        if (award.Files != null)
                                        {
                                            foreach (var file in award.Files)
                                            {
                                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                var fileId = await _fileUploadService.UploadFile(data);
                                                //Insert into Temp1AwardMapper
                                                var afmid = con.ExecuteScalar<int>($"Insert into AwardFileMapper(AwardId,FileId,IsApproved) values({AwardId},{fileId},0); select SCOPE_IDENTITY();", transaction: transaction);
                                                con.Execute($@"insert into Temp1_AwardFileMapper(Id,AwardId,FileId,UserId,CreatedOn)values({afmid},{AwardId},{fileId},{createdBy},GETUTCDATE());", transaction: transaction);

                                            }
                                        }
                                    }
                                    else
                                    {
                                        var countAwards = con.QuerySingle<int>("select Count(*) from Temp1_Awards where Id=@AwardId", new { AwardId = award.Id }, transaction: transaction);
                                        if (countAwards > 0)
                                        {
                                            con.Execute("update Temp1_Awards set Title=@Title,IsDeleted=@IsDeleted where Id=@AwardID ", new { Title = award.Title, AwardId = award.Id, IsDeleted = award.IsDeleted }, transaction: transaction);
                                        }
                                        else
                                        {
                                            con.Execute($@"insert into Temp1_Awards(Id,SupplierProfileId,Title,UserId,CreatedOn,UpdatedOn,IsDeleted)
                                    values(${award.Id},${foodSupplier.Id},@Title,{createdBy},GetUtcdate(),getutcDate(),@IsDeleted);", new { Title = award.Title, IsDeleted = award.IsDeleted }, transaction: transaction);
                                        }

                                        foreach (var file in award.Files)
                                        {
                                            if (file.IsDeleted && file.Id > 0)
                                            {
                                                var afmid = con.ExecuteScalar<int>($"select Id from AwardFileMapper where FileId={file.Id}", transaction: transaction);
                                                var countAfm = con.ExecuteScalar<int>($"select count(*) from Temp1_AwardFileMapper where Id={afmid}", transaction: transaction);
                                                if (countAfm > 0)
                                                {
                                                    con.Execute($"update Temp1_AwardFileMapper set AwardId=@AwardId,FileId=@FileId,UserId=@UserId,CreatedOn=Getutcdate(),IsDeleted=@IsDeleted where Id={afmid};", new { FileId = file.Id, AwardId = award.Id, UserId = createdBy, IsDeleted = file.IsDeleted }, transaction: transaction);
                                                }
                                                else
                                                {
                                                    con.Execute($@"insert into Temp1_AwardFileMapper(Id,AwardId,FileId,UserId,CreatedOn,IsDeleted)values({afmid},{award.Id},{file.Id},{createdBy},GETUTCDATE(),1);", transaction: transaction);
                                                }

                                            }
                                            else if (file.Id == 0 || file.Id == null)
                                            {
                                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                var fileId = await _fileUploadService.UploadFile(data);
                                                var afmId = con.ExecuteScalar<int>($"Insert into AwardFileMapper(AwardId,FileId,IsApproved) values({award.Id},{fileId},0); select SCOPE_IDENTITY();", transaction: transaction);
                                                con.Execute($@"insert into Temp1_AwardFileMapper(Id,AwardId,FileId,UserId,CreatedOn)values({afmId},{award.Id},{fileId},{createdBy},GETUTCDATE());", transaction: transaction);
                                            }
                                        }
                                    }


                                }
                            }


                            //update Qualification

                            if (foodSupplier.Qualifications != null)
                            {
                                foreach (var qualification in foodSupplier.Qualifications)
                                {
                                    if (qualification.CertificationsCategoryId == 0 || qualification.CertificationsCategoryId == null &&
                                    qualification.OtherCertificationsCategoryName == null)
                                    {
                                        throw new Exception("Either pass CertificationsCategoryId or OtherCertificationsCategoryName");
                                    }
                                    else if (qualification.CertificationsCategoryId > 0)
                                    {
                                        qualification.OtherCertificationsCategoryName = null;
                                    }
                                    if (qualification == null)
                                    {
                                        continue;
                                    }
                                    if (qualification.Id == 0 || qualification.Id == null)
                                    {
                                        var QualificationId = con.ExecuteScalar<int>(@"Insert into [dbo].[Qualification](SupplierProfileId,Title,CreatedOn,UpdatedOn,CertificationsCategoryId,OtherCertificationsCategoryName,IsApproved)
                                              values (@SupplierProfileId,@Title,GETUTCDATE(),GETUTCDATE(),@CertificationsCategoryId,@OtherCertificationsCategoryName,0);select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Title = qualification.Title, CertificationsCategoryId = qualification.CertificationsCategoryId, OtherCertificationsCategoryName = qualification.OtherCertificationsCategoryName }, transaction: transaction);

                                        con.Execute($@"insert into Temp1_Qualification(Id,SupplierProfileId,Title,UserId,CertificationsCategoryId,OtherCertificationsCategoryName,CreatedOn,UpdatedOn)
                                    values(${QualificationId},${foodSupplier.Id},@Title,{createdBy},@CertificationsCategoryId,@OtherCertificationsCategoryName,GetUtcdate(),getutcDate());", new { Title = qualification.Title, CertificationsCategoryId = qualification.CertificationsCategoryId, OtherCertificationsCategoryName = qualification.OtherCertificationsCategoryName }, transaction: transaction);

                                        if (qualification.Files != null)
                                        {
                                            foreach (var file in qualification.Files)
                                            {
                                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                var fileId = await _fileUploadService.UploadFile(data);
                                                //Insert into QualificationFileMapper
                                                var qfmid = con.ExecuteScalar<int>($"Insert into QualificationFileMapper(QualificationId,FileId,IsApproved) values({QualificationId},{fileId},0); select SCOPE_IDENTITY();", transaction: transaction);

                                                con.Execute($"Insert into Temp1_QualificationFileMapper(Id,QualificationId,FileId,UserId,CreatedOn) values({qfmid},{QualificationId},{fileId},{createdBy},GETUTCDATE())", transaction: transaction);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var countQualifications = con.QuerySingle<int>("select Count(*) from Temp1_Qualification where Id=@QualificationId", new { QualificationId = qualification.Id }, transaction: transaction);
                                        if (countQualifications > 0)
                                        {
                                            con.Execute("update Temp1_Qualification set Title=@Title,IsDeleted=@IsDeleted,CertificationsCategoryId=@CertificationsCategoryId,OtherCertificationsCategoryName=@OtherCertificationsCategoryName where Id=@QualificationID ", new
                                            {
                                                Title = qualification.Title,
                                                QualificationID = qualification.Id,
                                                IsDeleted = qualification.IsDeleted,
                                                CertificationsCategoryId = qualification.CertificationsCategoryId,
                                                OtherCertificationsCategoryName = qualification.OtherCertificationsCategoryName,
                                            }, transaction: transaction);
                                        }
                                        else
                                        {
                                            con.Execute($@"insert into Temp1_Qualification(Id,SupplierProfileId,Title,UserId,CertificationsCategoryId,OtherCertificationsCategoryName,CreatedOn,UpdatedOn,IsDeleted)
                                    values(${qualification.Id},${foodSupplier.Id},@Title,{createdBy},@CertificationsCategoryId,@OtherCertificationsCategoryName,GetUtcdate(),getutcDate(),@IsDeleted);", new { Title = qualification.Title, IsDeleted = qualification.IsDeleted, CertificationsCategoryId = qualification.CertificationsCategoryId, OtherCertificationsCategoryName = qualification.OtherCertificationsCategoryName }, transaction: transaction);
                                        }

                                        foreach (var file in qualification.Files)
                                        {



                                            if (file.IsDeleted && file.Id > 0)
                                            {
                                                var qfmid = con.ExecuteScalar<int>($"select Id from [dbo].[QualificationFileMapper] where FileId={file.Id}", transaction: transaction);
                                                var countQfm = con.ExecuteScalar<int>($"select count(*) from Temp1_QualificationFileMapper where Id={qfmid}", transaction: transaction);

                                                if (countQfm > 0)
                                                {
                                                    con.Execute($"update Temp1_QualificationFileMapper set QualificationId=@QualificationId,FileId=@FileId,UserId=@UserId,CreatedOn=Getutcdate(),IsDeleted=@IsDeleted where Id={qfmid};", new { FileId = file.Id, QualificationId = qualification.Id, UserId = createdBy, IsDeleted = file.IsDeleted }, transaction: transaction);
                                                }

                                                else
                                                {


                                                    con.Execute($@"insert into Temp1_QualificationFileMapper(Id,QualificationId,FileId,UserId,CreatedOn,IsDeleted)values({qfmid},{qualification.Id},{file.Id},{createdBy},GETUTCDATE(),1);", transaction: transaction);
                                                }

                                            }
                                            else if (file.Id == 0 || file.Id == null)
                                            {
                                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                var fileId = await _fileUploadService.UploadFile(data);
                                                var qfmId = con.ExecuteScalar<int>($"Insert into QualificationFileMapper(QualificationId,FileId,IsApproved) values({qualification.Id},{fileId},0); select SCOPE_IDENTITY();", transaction: transaction);

                                                con.Execute($@"Insert into Temp1_QualificationFileMapper(Id,QualificationId,FileId,UserId,CreatedOn) values({qfmId},{qualification.Id},{fileId},{createdBy},GETUTCDATE())", transaction: transaction);
                                            }
                                        }
                                    }

                                }
                            }

                            //ut 
                            if (foodSupplier.Products != null)
                            {
                                foreach (var product in foodSupplier.Products)
                                {
                                    if (product.ProductCategoryId == 0 || product.ProductCategoryId == null &&
                                    product.OtherProductCategoryName == null)
                                    {
                                        throw new Exception("Either pass ProductCatergoryId or OtherProductCatergoryName");
                                    }
                                    else if (product.ProductCategoryId > 0)
                                    {
                                        product.OtherProductCategoryName = null;
                                    }
                                    if (product == null)
                                    {
                                        continue;
                                    }
                                    if (product.Id == 0 || product.Id == null)
                                    {
                                        //insert into Temp1Products
                                        var ProductId = con.ExecuteScalar<int>(@"Insert into [Products](SupplierProfileId,Title,ProductCategoryId,OtherProductCategoryName,CreatedOn,UpdatedOn)
                                          values (@SupplierProfileId,@Title,@ProductCategoryId,@OtherProductCategoryName,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Title = product.Title, ProductCategoryId = product.ProductCategoryId, OtherProductCategoryName = product.OtherProductCategoryName }, transaction: transaction);

                                        con.Execute($@"insert into [Temp1_Products](Id,SupplierProfileId,Title,UserId,ProductCategoryId,OtherProductCategoryName,CreatedOn,UpdatedOn)
                                    values(${ProductId},${foodSupplier.Id},@Title,{createdBy},@ProductCategoryId,@OtherProductCategoryName,GetUtcdate(),getutcDate());", new { Title = product.Title, ProductCategoryId = product.ProductCategoryId, OtherProductCategoryName = product.OtherProductCategoryName }, transaction: transaction);


                                        if (product.Files != null)
                                        {
                                            foreach (var file in product.Files)
                                            {
                                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                var fileId = await _fileUploadService.UploadFile(data);

                                                //Insert into Temp1ProductMapper
                                                var pfmid = con.ExecuteScalar<int>($"Insert into ProductFileMapper(ProductId,FileId,IsApproved) values({ProductId},{fileId},0); select SCOPE_IDENTITY();", transaction: transaction);

                                                con.Execute($@"insert into [dbo].[Temp1_ProductFileMapper](Id,ProductId,FileId,UserId,CreatedOn)values({pfmid},{ProductId},{fileId},{createdBy},GETUTCDATE());", transaction: transaction);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var countProducts = con.QuerySingle<int>("select Count(*) from Temp1_Products where Id=@ProductId", new { ProductId = product.Id }, transaction: transaction);
                                        if (countProducts > 0)
                                        {
                                            con.Execute("update Temp1_Products set  Title=@Title,ProductCategoryId=@ProductCategoryId,OtherProductCategoryName =@OtherProductCategoryName, IsDeleted = @IsDeleted where Id = @ProductID ", new { Title = product.Title, ProductCategoryId = product.ProductCategoryId, OtherProductCategoryName = product.OtherProductCategoryName, ProductID = product.Id, IsDeleted = product.IsDeleted }, transaction: transaction);
                                        }


                                        else
                                        {
                                            con.Execute($@"insert into [Temp1_Products](Id,SupplierProfileId,Title,UserId,ProductCategoryId,OtherProductCategoryName,CreatedOn,UpdatedOn,IsDeleted)
                                    values(${product.Id},${foodSupplier.Id},@Title,{createdBy},@ProductCategoryId,@OtherProductCategoryName,GetUtcdate(),getutcDate(),@IsDeleted);", new { Title = product.Title, IsDeleted = product.IsDeleted, ProductCategoryId = product.ProductCategoryId, OtherProductCategoryName = product.OtherProductCategoryName }, transaction: transaction);
                                        }


                                        //if (product.IsDeleted == true)
                                        //{
                                        //    var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from ProductFileMapper afm inner join FileUploadTable ft on afm.FileId=ft.Id where afm.ProductId=@ProductId ", new { ProductId = product.Id }, transaction: transaction);
                                        //    foreach (var item in FilesAssociated)
                                        //    {
                                        //        _fileUploadService.DeleteFile(item);
                                        //    }
                                        //    con.Execute("delete from ProductFileMapper where ProductId=@productId;delete from Products where Id=@productId;", new { productId = product.Id }, transaction: transaction);
                                        //}

                                        //{
                                        //    con.Execute("update Products set Title=@Title,ProductCategoryId=@ProductCategoryId,OtherProductCategoryName=@OtherProductCategoryName where Id=@ProductId ", new { Title = product.Title, ProductId = product.Id, ProductCategoryId = product.ProductCategoryId, OtherProductCategoryName = product.OtherProductCategoryName }, transaction: transaction);

                                        foreach (var file in product.Files)
                                        {
                                            if (file.IsDeleted && file.Id > 0)
                                            {

                                                var pfmid = con.ExecuteScalar<int>($"select Id from ProductFileMapper where FileId={file.Id}", transaction: transaction);
                                                var countPfm = con.ExecuteScalar<int>($"select count(*) from Temp1_ProductFileMapper where Id={pfmid}", transaction: transaction);

                                                if (countPfm > 0)
                                                {
                                                    con.Execute($"update Temp1_ProductFileMapper set ProductId=@ProductId,FileId=@FileId,UserId=@UserId,CreatedOn=Getutcdate(),IsDeleted=@IsDeleted where Id={pfmid};", new { FileId = file.Id, ProductId = product.Id, UserId = createdBy, IsDeleted = file.IsDeleted }, transaction: transaction);
                                                }


                                                else
                                                {
                                                    con.Execute($@"insert into Temp1_ProductFileMapper(Id,ProductId,FileId,UserId,CreatedOn,IsDeleted)values({pfmid},{product.Id},{file.Id},{createdBy},GETUTCDATE(),1);", transaction: transaction);
                                                }
                                            }


                                            //    var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id }, transaction: transaction);
                                            //    _fileUploadService.DeleteFile(fileStorageName);
                                            //    con.Execute("delete from ProductFileMapper where FileId=@FileId;", new { FileId = file.Id }, transaction: transaction);
                                            //}
                                            else if (file.Id == 0 || file.Id == null)
                                            {
                                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                var fileId = await _fileUploadService.UploadFile(data);
                                                var pfmId = con.ExecuteScalar<int>($"Insert into ProductFileMapper(AwardId,FileId,IsApproved) values({product.Id},{fileId},0); select SCOPE_IDENTITY();", transaction: transaction);

                                                con.Execute($@"insert into Temp1_ProductFileMapper(Id,ProductId,FileId,UserId,CreatedOn)values({pfmId},{product.Id},{fileId},{createdBy},GETUTCDATE());", transaction: transaction);
                                            }
                                        }
                                    }

                                }
                            }

                            //ut 
                            if (foodSupplier.Services != null)
                            {
                                foreach (var service in foodSupplier.Services)
                                {
                                    if (service.BusinessActivityCategoryId == 0 || service.BusinessActivityCategoryId == null &&
                                    service.OtherBusinessActivityCategoryName == null)
                                    {
                                        throw new Exception("Either pass BusinessActivityCategoryId or OtherBusinessActivityCategoryName");
                                    }
                                    else if (service.BusinessActivityCategoryId > 0)
                                    {
                                        service.OtherBusinessActivityCategoryName = null;
                                    }
                                    if (service == null)
                                    {
                                        continue;
                                    }
                                    if (service.Id == 0 || service.Id == null)
                                    {
                                        /// add missing fields
                                        var ServiceId = con.ExecuteScalar<int>(@"Insert into [NewService](SupplierProfileId,Title,Descriptions,BusinessActivityCategoryId,OtherBusinessActivityCategoryName,CreatedOn,UpdatedOn,IsApproved)values(@SupplierProfileId,@Title,@Descriptions,@BusinessActivityCategoryId,@OtherBusinessActivityCategoryName,GETUTCDATE(),GETUTCDATE(),0); select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Title = service.Title, Descriptions = service.Descriptions, BusinessActivityCategoryId = service.BusinessActivityCategoryId, OtherBusinessActivityCategoryName = service.OtherBusinessActivityCategoryName }, transaction: transaction);

                                        con.Execute($@"insert into [Temp1_Service](Id,SupplierProfileId,Title,Descriptions,UserId,BusinessActivityCategoryId,OtherBusinessActivityCategoryName,CreatedOn,UpdatedOn)
                                    values(${ServiceId},${foodSupplier.Id},@Title,
@Descriptions,{createdBy},@BusinessActivityCategoryId,@OtherBusinessActivityCategoryName,GetUtcdate(),getutcDate());", new { Title = service.Title, Descriptions = service.Descriptions, IsDeleted = service.IsDeleted, BusinessActivityCategoryId = service.BusinessActivityCategoryId, OtherBusinessActivityCategoryName = service.OtherBusinessActivityCategoryName }, transaction: transaction);


                                        if (service.Files != null)
                                        {
                                            foreach (var file in service.Files)
                                            {
                                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                var fileId = await _fileUploadService.UploadFile(data);

                                                var sfmid = con.ExecuteScalar<int>($"Insert into ServiceFileMapper(ServiceId,FileId,IsApproved) values({ServiceId},{fileId},0); select SCOPE_IDENTITY();", transaction: transaction);


                                                con.Execute($"Insert into [Temp1_ServiceFileMapper] (Id,ServiceId,FileId,UserId,CreatedOn) values({sfmid},{ServiceId},{fileId},{createdBy},GETUTCDATE())", transaction: transaction);
                                            }
                                        }
                                    }
                                    else
                                    {

                                        var countServices = con.QuerySingle<int>("select Count(*) from Temp1_Service where Id=@ServiceId", new { ServiceId = service.Id }, transaction: transaction);

                                        if (countServices > 0)
                                        {
                                            con.Execute("update [Temp1_Service] set Descriptions=@Descriptions,Title=@Title,IsDeleted=@IsDeleted,BusinessActivityCategoryId=@BusinessActivityCategoryId,OtherBusinessActivityCategoryName=@OtherBusinessActivityCategoryName where Id=@ServiceID ", new
                                            {
                                                Descriptions = service.Descriptions,
                                                Title = service.Title,
                                                ServiceID = service.Id,
                                                IsDeleted = service.IsDeleted,
                                                BusinessActivityCategoryId = service.BusinessActivityCategoryId,
                                                OtherBusinessActivityCategoryName = service.OtherBusinessActivityCategoryName,
                                            }, transaction: transaction);
                                        }


                                        else
                                        {
                                            con.Execute($@"insert into [Temp1_Service](Id,SupplierProfileId,Title,Descriptions,UserId,BusinessActivityCategoryId,OtherBusinessActivityCategoryName,CreatedOn,UpdatedOn,IsDeleted)
                                    values(${service.Id},${foodSupplier.Id},@Title,
@Descriptions,{createdBy},@BusinessActivityCategoryId,@OtherBusinessActivityCategoryName,GetUtcdate(),getutcDate(),@IsDeleted);", new { Title = service.Title, Descriptions = service.Descriptions, IsDeleted = service.IsDeleted, BusinessActivityCategoryId = service.BusinessActivityCategoryId, OtherBusinessActivityCategoryName = service.OtherBusinessActivityCategoryName }, transaction: transaction);
                                        }




                                        //if (service.IsDeleted == true)
                                        //{
                                        //    var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from ServiceFileMapper afm inner join FileUploadTable ft on afm.FileId=ft.Id where afm.ServiceId=@ServiceId ", new { ServiceId = service.Id }, transaction: transaction);
                                        //    foreach (var item in FilesAssociated)
                                        //    {
                                        //        _fileUploadService.DeleteFile(item);
                                        //    }
                                        //    con.Execute("delete from ServiceFileMapper where ServiceId=@serviceId;delete from NewService where Id=@serviceId;", new { serviceId = service.Id }, transaction: transaction);
                                        //}
                                        //else
                                        //{
                                        //    con.Execute("update NewService set Title=@Title,Descriptions=@Descriptions,BusinessActivityCategoryId=@BusinessActivityCategoryId,OtherBusinessActivityCategoryName=@OtherBusinessActivityCategoryName where Id=@ServiceId ", new { Title = service.Title, ServiceId = service.Id, Descriptions = service.Descriptions, BusinessActivityCategoryId = service.BusinessActivityCategoryId, OtherBusinessActivityCategoryName = service.OtherBusinessActivityCategoryName }, transaction: transaction);
                                        foreach (var file in service.Files)
                                        {
                                            if (file.IsDeleted && file.Id > 0)
                                            {
                                                var sfmid = con.ExecuteScalar<int>($"select Id from ServiceFileMapper where FileId={file.Id}", transaction: transaction);
                                                var countSfm = con.ExecuteScalar<int>($"select count(*) from Temp1_ServiceFileMapper where Id={sfmid}", transaction: transaction);

                                                if (countSfm > 0)
                                                {
                                                    con.Execute($"update Temp1_ServiceFileMapper set ServiceId=@ServiceId,FileId=@FileId,UserId=@UserId,CreatedOn=Getutcdate(),IsDeleted=@IsDeleted where Id={sfmid};", new { FileId = file.Id, ServiceId = service.Id, UserId = createdBy, IsDeleted = file.IsDeleted }, transaction: transaction);
                                                }

                                                else
                                                {



                                                    con.Execute($@"insert into Temp1_ServiceFileMapper(Id,ServiceId,FileId,UserId,CreatedOn,IsDeleted)values({sfmid},{service.Id},{file.Id},{createdBy},GETUTCDATE(),1);", transaction: transaction);
                                                }
                                            }



                                            //var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id }, transaction: transaction);
                                            //    _fileUploadService.DeleteFile(fileStorageName);
                                            //    con.Execute("delete from ServiceFileMapper where FileId=@FileId;", new { FileId = file.Id }, transaction: transaction);
                                            //}
                                            else if (file.Id == 0 || file.Id == null)
                                            {
                                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                var fileId = await _fileUploadService.UploadFile(data);

                                                var sfmId = con.ExecuteScalar<int>($"Insert into ServiceFileMapper(ServiceId,FileId,IsApproved) values({service.Id},{fileId},0); select SCOPE_IDENTITY();", transaction: transaction);




                                                con.Execute($@"Insert into Temp1_ServiceFileMapper(Id,ServiceId,FileId,UserId,CreatedOn) values({sfmId},{service.Id},{fileId},{createdBy},GETUTCDATE())", transaction: transaction);
                                                //   con.Execute($"Insert into ProductFileMapper(ProductId,FileId) values({product.Id},{fileId})");
                                            }
                                        }
                                    }

                                }
                            }


                            //update UploadImages

                            if (foodSupplier.UploadImages != null)
                            {
                                foreach (var uploadImage in foodSupplier.UploadImages)
                                {
                                    if (uploadImage == null)
                                    {
                                        continue;
                                    }
                                    if (uploadImage.Id == 0 || uploadImage.Id == null)
                                    {
                                        var UploadImageId = con.ExecuteScalar<int>(@"Insert into [UploadsImages](SupplierProfileId,Descriptions,CreatedOn,UpdatedOn,IsApproved)
                                          values (@SupplierProfileId,@Description,GETUTCDATE(),GETUTCDATE(),0);select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Description = uploadImage.Description }, transaction: transaction);

                                        con.Execute($@"insert into [dbo].[Temp1_UploadsImages](Id,SupplierProfileId,Descriptions,UserId,CreatedOn,UpdatedOn)
                                    values(${UploadImageId},${foodSupplier.Id},
@Descriptions,{createdBy},GetUtcdate(),getutcDate());", new { Descriptions = uploadImage.Description, }, transaction: transaction);




                                        if (uploadImage.Files != null)
                                        {
                                            foreach (var file in uploadImage.Files)
                                            {
                                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                var fileId = await _fileUploadService.UploadFile(data);

                                                var uifmid = con.ExecuteScalar<int>($"Insert into [UploadImagesFileMapper](UploadImageId,FileId,IsApproved) values({UploadImageId},{fileId},0); select SCOPE_IDENTITY();", transaction: transaction);

                                                con.Execute($@"insert into Temp1_UploadImagesFileMapper(Id,UploadImageId,FileId,UserId,CreatedOn)values({uifmid},{UploadImageId},{fileId},{createdBy},GETUTCDATE());", transaction: transaction);
                                            }
                                        }
                                    }
                                    else
                                    {

                                        var countImages = con.QuerySingle<int>("select Count(*) from Temp1_UploadsImages where Id=@UploadImageId", new { UploadImageId = uploadImage.Id }, transaction: transaction);

                                        if (countImages > 0)
                                        {
                                            con.Execute("update Temp1_UploadsImages set Descriptions=@Descriptions,IsDeleted=@IsDeleted where Id=@UploadImageID ", new
                                            {
                                                Descriptions = uploadImage.Description,

                                                UploadImageID = uploadImage.Id,
                                                IsDeleted = uploadImage.IsDeleted

                                            }, transaction: transaction);

                                        }

                                        else
                                        {
                                            con.Execute($@"insert into [dbo].[Temp1_UploadsImages](Id,SupplierProfileId,Descriptions,UserId,CreatedOn,UpdatedOn,IsDeleted)
                                    values(${uploadImage.Id},${foodSupplier.Id},
@Descriptions,{createdBy},GetUtcdate(),getutcDate(),@IsDeleted);", new { Descriptions = uploadImage.Description, IsDeleted = uploadImage.IsDeleted }, transaction: transaction);
                                        }



                                        //if (uploadImage.IsDeleted == true)
                                        //{
                                        //    var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from UploadImagesFileMapper afm 
                                        //                        inner join FileUploadTable ft on afm.FileId=ft.Id where afm.UploadImageId=@UploadImageId ", new { UploadImageId = uploadImage.Id }, transaction: transaction);
                                        //    foreach (var item in FilesAssociated)
                                        //    {
                                        //        _fileUploadService.DeleteFile(item);
                                        //    }
                                        //    con.Execute("delete from UploadImagesFileMapper where UploadImageId=@uploadImageId;delete from UploadsImages where Id=@uploadImageId;", new { uploadImageId = uploadImage.Id }, transaction: transaction);
                                        //}
                                        //else
                                        //{
                                        //    con.Execute("update [UploadsImages] set Descriptions=@Description where Id=@UploadImageId ", new { Description = uploadImage.Description, UploadImageId = uploadImage.Id }, transaction: transaction);
                                        foreach (var file in uploadImage.Files)
                                        {
                                            if (file.IsDeleted && file.Id > 0)
                                            {
                                                //var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id }, transaction: transaction);
                                                //_fileUploadService.DeleteFile(fileStorageName);
                                                //con.Execute("delete from UploadImagesFileMapper where FileId=@FileId;", new { FileId = file.Id }, transaction: transaction);

                                                var uifmid = con.ExecuteScalar<int>($"select Id from [dbo].[UploadImagesFileMapper] where FileId={file.Id}", transaction: transaction);
                                                var countUifm = con.ExecuteScalar<int>($"select count(*) from [dbo].[Temp1_UploadImagesFileMapper] where Id={uifmid}", transaction: transaction);

                                                if (countUifm > 0)
                                                {
                                                    con.Execute($"update [dbo].[Temp1_UploadImagesFileMapper] set UploadImageId=@UploadImageId,FileId=@FileId,UserId=@UserId,CreatedOn=Getutcdate(),IsDeleted=@IsDeleted where Id={uifmid};", new { FileId = file.Id, UploadImageId = uploadImage.Id, UserId = createdBy, IsDeleted = file.IsDeleted }, transaction: transaction);
                                                }

                                                else
                                                {



                                                    con.Execute($@"insert into Temp1_UploadImagesFileMapper(Id,UploadImageId,FileId,UserId,CreatedOn,IsDeleted)values({uifmid},{uploadImage.Id},{file.Id},{createdBy},GETUTCDATE(),1);", transaction: transaction);

                                                }


                                            }
                                            else if (file.Id == 0 || file.Id == null)
                                            {

                                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                var fileId = await _fileUploadService.UploadFile(data);

                                                var uifmId = con.ExecuteScalar<int>($"Insert into UploadImagesFileMapper(UploadImageId,FileId,IsApproved) values({uploadImage.Id},{fileId},0); select SCOPE_IDENTITY();", transaction: transaction);




                                                con.Execute($@"Insert into [dbo].[Temp1_UploadImagesFileMapper](Id,UploadImageId,FileId,UserId,CreatedOn) values({uifmId},{uploadImage.Id},{fileId},{createdBy},GETUTCDATE())", transaction: transaction);


                                                //var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id }, transaction: transaction);
                                                //    _fileUploadService.DeleteFile(fileStorageName);
                                                //    IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                //    var fileId = _fileUploadService.UploadFile(data);
                                                //    con.Execute($"Insert into UploadImagesFileMapper(UploadImageId,FileId) values({uploadImage.Id},{fileId})", transaction: transaction);
                                            }
                                        }
                                    }

                                }
                            }


                            //update wallPapers

                            if (foodSupplier.Wallpapers != null)
                            {
                                foreach (var wallpaper in foodSupplier.Wallpapers)
                                {
                                    if (wallpaper == null)
                                    {
                                        continue;
                                    }
                                    if (wallpaper.Id == 0 || wallpaper.Id == null)
                                    {
                                        var WallpaperId = con.ExecuteScalar<int>(@"Insert into [Wallpapers](SupplierProfileId,Descriptions,CreatedOn,UpdatedOn)
                                          values (@SupplierProfileId,@Descriptions,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Descriptions = wallpaper.Description }, transaction: transaction);

                                        con.Execute($@"insert into [dbo].[Temp1_Wallpapers](Id,SupplierProfileId,Descriptions,UserId,CreatedOn,UpdatedOn)
                                    values(${WallpaperId},${foodSupplier.Id},@Descriptions,{createdBy},GetUtcdate(),getutcDate());", new { Descriptions = wallpaper.Description, }, transaction: transaction);

                                        if (wallpaper.Files != null)
                                        {
                                            foreach (var file in wallpaper.Files)
                                            {
                                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                                var fileId = await _fileUploadService.UploadFile(data);

                                                var wfmid = con.ExecuteScalar<int>($"Insert into [dbo].[WallPaperFileMapper](WallpaperId,FileId,IsApproved) values({WallpaperId},{fileId},0); select SCOPE_IDENTITY();", transaction: transaction);

                                                con.Execute($@"insert into [dbo].[Temp1_WallPaperFileMapper](Id,WallpaperId,FileId,UserId,CreatedOn)values({wfmid},{WallpaperId},{fileId},{createdBy},GETUTCDATE());", transaction: transaction);
                                            }
                                        }
                                    }

                                    else
                                    {
                                        var countWallpapers = con.QuerySingle<int>("select Count(*) from [dbo].Temp1_Wallpapers where Id=@WallpaperId", new { WallpaperId = wallpaper.Id }, transaction: transaction);

                                        if (countWallpapers > 0)
                                        {
                                            con.Execute("update [dbo].[Temp1_Wallpapers] set [Descriptions]=@Descriptions,IsDeleted=@IsDeleted where Id=@WallpaperId ", new
                                            {
                                                Descriptions = wallpaper.Description,

                                                WallpaperId = wallpaper.Id,
                                                IsDeleted = wallpaper.IsDeleted

                                            }, transaction: transaction);

                                        }

                                        else
                                        {
                                            con.Execute($@"insert into [dbo].[Temp1_Wallpapers](Id,SupplierProfileId,Descriptions,UserId,CreatedOn,UpdatedOn,IsDeleted)
                                    values(${wallpaper.Id},${foodSupplier.Id},
@Descriptions,{createdBy},GetUtcdate(),getutcDate(),@IsDeleted);", new { Descriptions = wallpaper.Description, IsDeleted = wallpaper.IsDeleted }, transaction: transaction);
                                        }



                                    


                                    //new code added
                                    foreach (var file in wallpaper.Files)
                                    {
                                        if (file.IsDeleted && file.Id > 0)
                                        {
                                            //var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id }, transaction: transaction);
                                            //_fileUploadService.DeleteFile(fileStorageName);
                                            //con.Execute("delete from UploadImagesFileMapper where FileId=@FileId;", new { FileId = file.Id }, transaction: transaction);

                                            var wfmid = con.ExecuteScalar<int>($"select Id from [dbo].[WallPaperFileMapper] where FileId={file.Id}", transaction: transaction);
                                            var countWfm = con.ExecuteScalar<int>($"select count(*) from [dbo].[Temp1_WallPaperFileMapper] where Id={wfmid}", transaction: transaction);

                                            if (countWfm > 0)
                                            {
                                                con.Execute($"update [dbo].[Temp1_WallPaperFileMapper] set WallpaperId=@WallpaperId,FileId=@FileId,UserId=@UserId,CreatedOn=Getutcdate(),IsDeleted=@IsDeleted where Id={wfmid};", new { FileId = file.Id, WallpaperId = wallpaper.Id, UserId = createdBy, IsDeleted = file.IsDeleted }, transaction: transaction);
                                            }

                                            else
                                            {



                                                con.Execute($@"insert into [dbo].[Temp1_WallPaperFileMapper](Id,WallpaperId,FileId,UserId,CreatedOn,IsDeleted)values({wfmid},{wallpaper.Id},{file.Id},{createdBy},GETUTCDATE(),1);", transaction: transaction);

                                            }


                                        }
                                        else if (file.Id == 0 || file.Id == null)
                                        {

                                            IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                            var fileId = await _fileUploadService.UploadFile(data);

                                            var wfmId = con.ExecuteScalar<int>($"Insert into [dbo].[WallPaperFileMapper](WallpaperId,FileId,IsApproved) values({wallpaper.Id},{fileId},0); select SCOPE_IDENTITY();", transaction: transaction);




                                            con.Execute($@"Insert into [dbo].[Temp1_WallPaperFileMapper] (Id,WallpaperId,FileId,UserId,CreatedOn) values({wfmId},{wallpaper.Id},{fileId},{createdBy},GETUTCDATE())", transaction: transaction);


                                            //var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id }, transaction: transaction);
                                            //    _fileUploadService.DeleteFile(fileStorageName);
                                            //    IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                                            //    var fileId = _fileUploadService.UploadFile(data);
                                            //    con.Execute($"Insert into UploadImagesFileMapper(UploadImageId,FileId) values({uploadImage.Id},{fileId})", transaction: transaction);
                                        }
                                    }

                                }
                            }

                        }
                        


                            //    if (wallpaper.Files != null)
                            //    {
                            //        foreach (var file in wallpaper.Files)
                            //        {
                            //            IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                            //            var fileId = await _fileUploadService.UploadFile(data);
                            //            con.Execute($"Insert into [WallPaperFileMapper](WallpaperId,FileId) values({WallpaperId},{fileId})", transaction: transaction);
                            //        }
                            //    }
                            //}
                            //else
                            //{
                            //    if (wallpaper.IsDeleted == true)
                            //    {
                            //        var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from WallPaperFileMapper afm 
                            //                            inner join FileUploadTable ft on afm.FileId=ft.Id where afm.WallpaperId=@WallpaperId ", new { WallpaperId = wallpaper.Id }, transaction: transaction);
                            //        foreach (var item in FilesAssociated)
                            //        {
                            //            _fileUploadService.DeleteFile(item);
                            //        }
                            //        con.Execute("delete from WallPaperFileMapper where WallpaperId=@wallpaperId;delete from Wallpapers where Id=@wallpaperId;", new { wallpaperId = wallpaper.Id }, transaction: transaction);
                            //    }
                            //    else
                            //    {
                            //        con.Execute("update [Wallpapers] set Descriptions=@Descriptions where Id=@WallpaperId ", new { Descriptions = wallpaper.Description, WallpaperId = wallpaper.Id }, transaction: transaction);
                            //        foreach (var file in wallpaper.Files)
                            //        {
                            //            if (file.IsDeleted && file.Id > 0)
                            //            {
                            //                var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id }, transaction: transaction);
                            //                _fileUploadService.DeleteFile(fileStorageName);
                            //                con.Execute("delete from WallPaperFileMapper where FileId=@FileId;", new { FileId = file.Id }, transaction: transaction);
                            //            }
                            //            else if (file.Id == 0 || file.Id == null)
                            //            {
                            //                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
                            //                var fileId = _fileUploadService.UploadFile(data);
                            //                con.Execute($"Insert into WallPaperFileMapper(WallpaperId,FileId) values({wallpaper.Id},{fileId})", transaction: transaction);
                            //            }
                            //        }
                            //    }

                            //}

                            //updating Videos

                            //if (foodSupplier.Videos != null)
                            //if (foodSupplier.Videos.Count > 0)
                            //{
                            //    foreach (var video in foodSupplier.Videos)
                            //    {
                            //        con.ExecuteScalar(@"update [dbo].[UploadVideo] set Description=@Description,Link=@Link where Id=@Id;", new { Id = video.Id, Description = video.Description, Link = video.Link }, transaction: transaction);
                            //    }
                            //}

                            // for Updating Videos

                            
                            if (foodSupplier.Videos != null)
                            {
                                foreach (var video in foodSupplier.Videos)
                                {
                                    if (video == null)
                                    {
                                        continue;
                                    }
                                    if (video.Id == 0 || video.Id == null)
                                    {

                                        //insert into temp1Video
                                        var VideoId = con.ExecuteScalar<int>(@"Insert into UploadVideo (SupplierProfileId,Description,Link,CreatedOn,UpdatedOn,IsApproved)
                                          values (@SupplierProfileId,@Description,@Link,GETUTCDATE(),GETUTCDATE(),0);select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Description = video.Description,Link=video.Link }, transaction: transaction);
                                        con.Execute($@"insert into Temp1_UploadVideo(Id,SupplierProfileId,Description,UserId,CreatedOn,UpdatedOn)
                                        values(${VideoId},${foodSupplier.Id},@Description,{createdBy},GetUtcdate(),getutcDate());", new { Description = video.Description, }, transaction);
                                       
                                    }
                                    else
                                    {
                                        var countVideos = con.QuerySingle<int>("select Count(*) from Temp1_UploadVideo where Id=@VideoId", new { VideoId = video.Id }, transaction: transaction);
                                        if (countVideos > 0)
                                        {
                                            con.Execute("update Temp1_UploadVideo set Description=@Description,Link=@Link,IsDeleted=@IsDeleted where Id=@VideoId ", new { Description = video.Description, VideoId = video.Id, IsDeleted =video.IsDeleted,Link=video.Link }, transaction: transaction);
                                        }
                                        else
                                        {
                                            con.Execute($@"insert into Temp1_UploadVideo(Id,SupplierProfileId,Description,Link,UserId,CreatedOn,UpdatedOn,IsDeleted)
                                    values(${video.Id},${foodSupplier.Id},@Description,@Link,{createdBy},GetUtcdate(),getutcDate(),@IsDeleted);", new {Link=video.Link, Description = video.Description, IsDeleted = video.IsDeleted }, transaction: transaction);
                                        }

                                       
                                    }


                                }
                            }


                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            // Handle exception or rollback the transaction
                            transaction.Rollback();
                            throw;
                        }
                    }




                    if (con.State == System.Data.ConnectionState.Open)
                    {
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                result = "Failed";
            }
            return result;
        }

        public async Task<string> UpdateApprovedSupplier(int createdBy)
        {
            string result = "";
            /*
             * 3 steps
             * 1 get that modules data from temporary table
             * update that data in respective modules actual table
             * handle that modules file which are deleted
             */
            //---- supplier

            try
            {
                using (System.Data.IDbConnection con=new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    using (var transaction = con.BeginTransaction())
                    {

                        try
                        {
                            var supplierCount = con.QuerySingle<int>("select count(*) from Temp1_SupplierProfiles where CreatedBy=@CreatedBy;", new { CreatedBy=createdBy }, transaction);
                            if (supplierCount <= 0)
                            {
                                throw new Exception("Supplier update data not found");
                            }
                            var supplierData = (@"
                                         update SupplierProfiles set 
                                         SupplierProfiles.Name=tsp.Name ,
                                         SupplierProfiles.RegistrationNumber=tsp.RegistrationNumber,
                                         SupplierProfiles.Description=tsp.Description,
                                         SupplierProfiles.Logo=tsp.Logo,
                                         SupplierProfiles.PhoneNumber=tsp.PhoneNumber,
                                         SupplierProfiles.Facebook=tsp.Facebook,
                                         SupplierProfiles.Twitter=tsp.Twitter,
                                         SupplierProfiles.Instagram=tsp.Instagram,
                                         SupplierProfiles.LinkedIn=tsp.LinkedIn,
                                         SupplierProfiles.Email=tsp.Email,
                                         SupplierProfiles.ModifiedOn=tsp.CreatedOn,
                                         SupplierProfiles.CountryId=tsp.CountryId,
                                         SupplierProfiles.Street=tsp.Street,
                                         SupplierProfiles.PostalCode=tsp.PostalCode,
                                         SupplierProfiles.City=tsp.City,
                                         SupplierProfiles.State=tsp.State,
                                         SupplierProfiles.Address=tsp.Address
                                         FROM SupplierProfiles sp left JOIN Temp1_SupplierProfiles tsp on  
                                        sp.Id = tsp.Id where tsp.CreatedBy=@CreatedBy;", new { CreatedBy = createdBy },transaction);


                            // for awards
                            var awardsCount = con.QuerySingle<int>("select count(*) from Temp1_Awards where UserId=@CreatedBy;", new { CreatedBy = createdBy }, transaction);
                            if (awardsCount > 0)
                            {
                                var awardsData = con.Query<TempAwardUpdateDTO>("select Id,SupplierProfileId,Title,CreatedOn,UpdatedOn,isNull(IsDeleted,0) IsDeleted from Temp1_Awards where UserId=@CreatedBy;", new { CreatedBy = createdBy },transaction);
                                foreach (var award in awardsData)
                                {
                                    if (award.IsDeleted)
                                    {
                                        con.Execute("delete from AwardFileMapper where AwardId=@Id; delete from Awards where Id=@Id;",award,transaction);
                                    }
                                    else
                                    {
                                        // update awards
                                        con.Execute("update Awards set Title=@Title,UpdatedOn=@UpdatedOn,IsApproved=1 where Id=@Id", award,transaction);
                                        // afm - award file mapper
                                        var afmDataCount = con.QuerySingle<int>("select count(*) from Temp1_AwardFileMapper where AwardId=@AwardId and UserId=@CreatedBy", new { AwardId = award.Id, CreatedBy = createdBy }, transaction);
                                        if (afmDataCount > 0)
                                        {
                                            var afmDatas = con.Query<TempAwardFileMapperUpdateDTO>("select Id,AwardId,FileId,CreatedOn,IsDeleted from Temp1_AwardFileMapper where AwardId=@AwardId and UserId=@CreatedBy", new { AwardId = award.Id, CreatedBy = createdBy }, transaction);
                                            foreach (var afm in afmDatas)
                                            {
                                                if (afm.IsDeleted)
                                                {
                                                    con.Execute("delete from AwardFileMapper where Id=@Id", new { Id = afm.Id }, transaction);
                                                }
                                                else
                                                {
                                                    con.Execute("update AwardFileMapper set AwardId=@AwardId,FileId=@FileId,IsApproved=1 where Id=@Id ", afm, transaction);
                                                }
                                            }
                                        }
                                    }
                                    
                                }
                            }

                            // continue for other modules

                            // for products
                            var productsCount = con.QuerySingle<int>("select count(*) from Temp1_Products where UserId=@CreatedBy;", new { CreatedBy = createdBy }, transaction);
                            if (productsCount > 0)
                            {
                                var productsData = con.Query<TempProductUpdateDTO>("select Id,SupplierProfileId,Title,ProductCategoryId,OtherProductCategoryName,CreatedOn,UpdatedOn,isNull(IsDeleted,0) IsDeleted from Temp1_Products where UserId=@CreatedBy;", new { CreatedBy = createdBy }, transaction);
                                foreach (var product in productsData)
                                {
                                    if (product.IsDeleted)
                                    {
                                        con.Execute("delete from ProductFileMapper where ProductId=@Id; delete from Products where Id=@Id;", product, transaction);
                                    }
                                    else
                                    {
                                        // update products
                                        con.Execute("update Products set Title=@Title,ProductCategoryId=@ProductCategoryId,OtherProductCategoryName=@OtherProductCategoryName,UpdatedOn=@UpdatedOn,IsApproved=1 where Id=@Id", product, transaction);
                                        // afm - products file mapper
                                        var pfmDataCount = con.QuerySingle<int>("select count(*) from Temp1_ProductFileMapper where ProductId=@ProductId and UserId=@CreatedBy", new { ProductId = product.Id, CreatedBy = createdBy }, transaction);
                                        if (pfmDataCount > 0)
                                        {
                                            var pfmDatas = con.Query<TempProductFileMapperUpdateDTO>("select Id,ProductId,FileId,CreatedOn,IsDeleted from Temp1_ProductFileMapper where ProductId=@ProductId and UserId=@CreatedBy", new { ProductId = product.Id, CreatedBy = createdBy }, transaction);
                                            foreach (var pfm in pfmDatas)
                                            {
                                                if (pfm.IsDeleted)
                                                {
                                                    con.Execute("delete from ProductFileMapper where Id=@Id", new { Id = pfm.Id }, transaction);
                                                }
                                                else
                                                {
                                                    con.Execute("update ProductFileMapper set ProductId=@ProductId,FileId=@FileId,IsApproved=1 where Id=@Id ", pfm, transaction);
                                                }
                                            }
                                        }
                                    }

                                }
                            }

                            //for Qualifications
                            var qualificationsCount = con.QuerySingle<int>("select count(*) from Temp1_Qualification where UserId=@CreatedBy;", new { CreatedBy = createdBy }, transaction);
                            if (qualificationsCount > 0)
                            {
                                var qualificationsData = con.Query<TempQualificationUpdateDTO>("select Id,SupplierProfileId,Title,CertificationsCategoryId,OtherCertificationsCategoryName,CreatedOn,UpdatedOn,isNull(IsDeleted,0) IsDeleted from Temp1_Qualification where UserId=@CreatedBy;", new { CreatedBy = createdBy }, transaction);
                                foreach (var qualification in qualificationsData)
                                {
                                    if (qualification.IsDeleted)
                                    {
                                        con.Execute("delete from QualificationFileMapper where QualificationId=@Id; delete from Products where Id=@Id;", qualification, transaction);
                                    }
                                    else
                                    {
                                        // update qualifications
                                        con.Execute("update Qualification set Title=@Title,CertificationsCategoryId=@CertificationsCategoryId,OtherCertificationsCategoryName=@OtherCertificationsCategoryName,UpdatedOn=@UpdatedOn,IsApproved=1 where Id=@Id", qualification, transaction);
                                        // afm - products file mapper
                                        var qfmDataCount = con.QuerySingle<int>("select count(*) from Temp1_QualificationFileMapper where QualificationId=@QualificationId and UserId=@CreatedBy", new { QualificationId = qualification.Id, CreatedBy = createdBy }, transaction);
                                        if (qfmDataCount > 0)
                                        {
                                            var qfmDatas = con.Query<TempQualificationFileMapperUpdateDTO>("select Id,QualificationId,FileId,CreatedOn,IsDeleted from Temp1_QualificationFileMapper where QualificationId=@QualificationId and UserId=@CreatedBy", new { QualificationId = qualification.Id, CreatedBy = createdBy }, transaction);
                                            foreach (var qfm in qfmDatas)
                                            {
                                                if (qfm.IsDeleted)
                                                {
                                                    con.Execute("delete from QualificationFileMapper where Id=@Id", new { Id = qfm.Id }, transaction);
                                                }
                                                else
                                                {
                                                    con.Execute("update QualificationFileMapper set QualificationId=@QualificationId,FileId=@FileId,IsApproved=1 where Id=@Id ", qfm, transaction);
                                                }
                                            }
                                        }
                                    }

                                }
                            }

                            //for Services
                            var servicesCount = con.QuerySingle<int>("select count(*) from [dbo].[Temp1_Service] where UserId=@CreatedBy;", new { CreatedBy = createdBy }, transaction);
                            if (servicesCount > 0)
                            {
                                var servicesData = con.Query<TempServiceUpdateDTO>("select Id,SupplierProfileId,Title,Descriptions,BusinessActivityCategoryId,OtherBusinessActivityCategoryName,CreatedOn,UpdatedOn,isNull(IsDeleted,0) IsDeleted from Temp1_Service where UserId=@CreatedBy;", new { CreatedBy = createdBy }, transaction);
                                foreach (var service in servicesData)
                                {
                                    if (service.IsDeleted)
                                    {
                                        con.Execute("delete from ServiceFileMapper where ServiceId=@Id; delete from NewService where Id=@Id;", service, transaction);
                                    }
                                    else
                                    {
                                        // update services
                                        con.Execute("update NewService set Title=@Title,Descriptions=@Descriptions,BusinessActivityCategoryId=@BusinessActivityCategoryId,OtherBusinessActivityCategoryName=@OtherBusinessActivityCategoryName,UpdatedOn=@UpdatedOn,IsApproved=1 where Id=@Id", service, transaction);
                                        // afm - products file mapper
                                        var sfmDataCount = con.QuerySingle<int>("select count(*) from Temp1_ServiceFileMapper where ServiceId=@ServiceId and UserId=@CreatedBy", new { ServiceId = service.Id, CreatedBy = createdBy }, transaction);
                                        if (sfmDataCount > 0)
                                        {
                                            var sfmDatas = con.Query<TempServiceFileMapperUpdateDTO>("select Id,ServiceId,FileId,CreatedOn,IsDeleted from Temp1_ServiceFileMapper where ServiceId=@ServiceId and UserId=@CreatedBy", new { ServiceId = service.Id, CreatedBy = createdBy }, transaction);
                                            foreach (var sfm in sfmDatas)
                                            {
                                                if (sfm.IsDeleted)
                                                {
                                                    con.Execute("delete from [dbo].[ServiceFileMapper] where Id=@Id", new { Id = sfm.Id }, transaction);
                                                }
                                                else
                                                {
                                                    con.Execute("update ServiceFileMapper set ServiceId=@ServiceId,FileId=@FileId,IsApproved=1 where Id=@Id ", sfm, transaction);
                                                }
                                            }
                                        }
                                    }

                                }
                            }

                            //for UploadImage
                            var uploadImageCount = con.QuerySingle<int>("select count(*) from Temp1_UploadsImages where UserId=@CreatedBy;", new { CreatedBy = createdBy }, transaction);
                            if (uploadImageCount > 0)
                            {
                                var uploadImageData = con.Query<TempUploadImageUpdateDTO>("select Id,SupplierProfileId,Descriptions,CreatedOn,UpdatedOn,isNull(IsDeleted,0) IsDeleted from Temp1_UploadsImages where UserId=@CreatedBy;", new { CreatedBy = createdBy }, transaction);
                                foreach (var uploadImage in uploadImageData)
                                {
                                    if (uploadImage.IsDeleted)
                                    {
                                        con.Execute("delete from UploadImagesFileMapper where UploadImageId=@Id; delete from UploadsImages where Id=@Id;", uploadImage, transaction);
                                    }
                                    else
                                    {
                                        // update uploadImage
                                        con.Execute("update UploadsImages set Descriptions=@Descriptions,UpdatedOn=@UpdatedOn,IsApproved=1 where Id=@Id", uploadImage, transaction);
                                        // afm - products file mapper
                                        var uifmDataCount = con.QuerySingle<int>("select count(*) from Temp1_UploadImagesFileMapper where UploadImageId=@UploadImageId and UserId=@CreatedBy", new { UploadImageId = uploadImage.Id, CreatedBy = createdBy }, transaction);
                                        if (uifmDataCount > 0)
                                        {
                                            var uifmDatas = con.Query<TempUploadImageFileMapperUpdateDTO>("select Id,UploadImageId,FileId,CreatedOn,IsDeleted from Temp1_UploadImagesFileMapper where UploadImageId=@UploadImageId and UserId=@CreatedBy", new { UploadImageId = uploadImage.Id, CreatedBy = createdBy }, transaction);
                                            foreach (var uifm in uifmDatas)
                                            {
                                                if (uifm.IsDeleted)
                                                {
                                                    con.Execute("delete from UploadImagesFileMapper where Id=@Id", new { Id = uifm.Id }, transaction);
                                                }
                                                else
                                                {
                                                    con.Execute("update UploadImagesFileMapper set UploadImageId=@UploadImageId,FileId=@FileId,IsApproved=1 where Id=@Id ", uifm, transaction);
                                                }
                                            }
                                        }
                                    }

                                }
                            }

                            //for Wallpapers
                            var wallpaperCount = con.QuerySingle<int>("select count(*) from Temp1_Wallpapers where UserId=@CreatedBy;", new { CreatedBy = createdBy }, transaction);
                            if (wallpaperCount > 0)
                            {
                                var wallpaperData = con.Query<TempWallpaperUpdateDTO>("select Id,SupplierProfileId,Descriptions,CreatedOn,UpdatedOn,isNull(IsDeleted,0) IsDeleted from Temp1_Wallpapers where UserId=@CreatedBy;", new { CreatedBy = createdBy }, transaction);
                                foreach (var wallpaper in wallpaperData)
                                {
                                    if (wallpaper.IsDeleted)
                                    {
                                        con.Execute("delete from WallPaperFileMapper where WallpaperId=@Id; delete from Wallpapers where Id=@Id;", wallpaper, transaction);
                                    }
                                    else
                                    {
                                        // update wallpapers
                                        con.Execute("update Wallpapers set Descriptions=@Descriptions,UpdatedOn=@UpdatedOn,IsApproved=1 where Id=@Id", wallpaper, transaction);
                                        // afm - products file mapper
                                        var wfmDataCount = con.QuerySingle<int>("select count(*) from Temp1_WallPaperFileMapper where WallpaperId=@WallpaperId and UserId=@CreatedBy", new { WallpaperId = wallpaper.Id, CreatedBy = createdBy }, transaction);
                                        if (wfmDataCount > 0)
                                        {
                                            var wfmDatas = con.Query<TempWallpaperFileMapperUpdateDTO>("select Id,WallpaperId,FileId,CreatedOn,IsDeleted from Temp1_WallPaperFileMapper where WallpaperId=@WallpaperId and UserId=@CreatedBy", new { WallpaperId = wallpaper.Id, CreatedBy = createdBy }, transaction);
                                            foreach (var wfm in wfmDatas)
                                            {
                                                if (wfm.IsDeleted)
                                                {
                                                    con.Execute("delete from WallPaperFileMapper where Id=@Id", new { Id = wfm.Id }, transaction);
                                                }
                                                else
                                                {
                                                    con.Execute("update WallPaperFileMapper set WallpaperId=@WallpaperId,FileId=@FileId,IsApproved=1 where Id=@Id ", wfm, transaction);
                                                }
                                            }
                                        }
                                    }

                                }
                            }

                            //for Videos
                            var videosCount = con.QuerySingle<int>("select count(*) from Temp1_UploadVideo where UserId=@CreatedBy;", new { CreatedBy = createdBy }, transaction);
                            if (videosCount > 0)
                            {
                                var videoData = con.Query<TempVideoUpdateDTO>("select Id,SupplierProfileId,Description,Link,CreatedOn,UpdatedOn,isNull(IsDeleted,0) IsDeleted from Temp1_UploadVideo where UserId=@CreatedBy;", new { CreatedBy = createdBy }, transaction);
                                foreach (var video in videoData)
                                {
                                    if (video.IsDeleted)
                                    {
                                        con.Execute(" delete from [dbo].[UploadVideo] where Id=@Id;", video, transaction);
                                    }
                                    else
                                    {
                                        // update wallpapers
                                        con.Execute("update UploadVideo set Description=@Description,Link=@Link,UpdatedOn=@UpdatedOn,IsApproved=1 where Id=@Id", video, transaction);
                                        // afm - products file mapper

                                    }

                                }
                            }



                            // deletion query
                            con.Execute(@"
                            delete from Temp1_Awards where UserId=@CreatedBy;
                            delete from Temp1_AwardFileMapper where UserId=@CreatedBy;

                            delete from Temp1_Products where UserId=@CreatedBy;
                            delete from  Temp1_ProductFileMapper where UserId=@CreatedBy;
                             
                            delete from Temp1_Qualification where UserId=@CreatedBy;
                            delete from  Temp1_QualificationFileMapper where UserId=@CreatedBy;
                          
                            delete from Temp1_Service where UserId=@CreatedBy;
                            delete from Temp1_ServiceFileMapper where UserId=@CreatedBy;

                             
                            delete from Temp1_UploadsImages where UserId=@CreatedBy;
                            delete from Temp1_UploadImagesFileMapper where UserId=@CreatedBy;

                            delete from Temp1_Wallpapers where UserId=@CreatedBy;
                            delete from  Temp1_WallPaperFileMapper where UserId=@CreatedBy;

                            delete from Temp1_UploadVideo where UserId=@CreatedBy;
                            
                             ", new { CreatedBy =createdBy},transaction);
                            // do commit at last
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {

                            throw ex;
                        }
                        


                    }                       
                    

                    if (con.State == System.Data.ConnectionState.Open)
                    {
                        con.Close();
                    }
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }


            return result;
        }


        //public async Task<string> UpdateFoodSupplier(FoodSupplierUpdateInputDTO foodSupplier)
        //{
        //    String result = "Success";
        //    try
        //    {
        //        using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
        //        {
        //            if (con.State == System.Data.ConnectionState.Closed)
        //            {
        //                con.Open();
        //            }
        //            // Start a transaction
        //            //using (var transaction = con.BeginTransaction())
        //            //{
        //            //    try
        //            //    {
        //            //        transaction.Commit();
        //            //    }
        //            //    catch (Exception)
        //            //    {
        //            //        // Handle exception or rollback the transaction
        //            //        transaction.Rollback();
        //            //        throw;
        //            //    }
        //            //} 

        //            string ExistingNumber = con.ExecuteScalar<string>("select RegistrationNumber from [dbo].[SupplierProfiles] where RegistrationNumber=@RegistrationNumber and IsDeleted=0 and Id<>@Id", foodSupplier);

        //            if (!string.IsNullOrEmpty(ExistingNumber))
        //            {
        //                return "The Registration number is already used";
        //            }

        //            con.ExecuteScalar<int>("UPDATE [dbo].[SupplierProfiles] SET Name=@Name, RegistrationNumber = @RegistrationNumber, Description = @Description, Logo = @Logo, PhoneNumber = @PhoneNumber,Facebook = @Facebook, Twitter = @Twitter, ModifiedBy=@ModifiedBy, Instagram = @Instagram, ModifiedOn=Getutcdate() , CountryId = @CountryId,Street=@Street,PostalCode=@PostalCode,City=@City,State=@State,Address=@Address  WHERE Id = @Id; ", foodSupplier);

        //            //update foodSupplier Awards


        //            if (foodSupplier.Awards != null)
        //            {
        //                foreach (var award in foodSupplier.Awards)
        //                {
        //                    if (award == null)
        //                    {
        //                        continue;
        //                    }
        //                    if (award.Id == 0 || award.Id == null)
        //                    {
        //                        var AwardId = con.ExecuteScalar<int>(@"Insert into Awards(SupplierProfileId,Title,CreatedOn,UpdatedOn)
        //                                  values (@SupplierProfileId,@Title,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Title = award.Title });

        //                        if (award.Files != null)
        //                        {
        //                            foreach (var file in award.Files)
        //                            {
        //                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                var fileId = await _fileUploadService.UploadFile(data);
        //                                con.Execute($"Insert into AwardFileMapper(AwardId,FileId) values({AwardId},{fileId})");
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (award.IsDeleted == true)
        //                        {
        //                            var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from AwardFileMapper afm 
        //                                                        inner join FileUploadTable ft on afm.FileId=ft.Id where afm.AwardId=@AwardId ", new { AwardId = award.Id });
        //                            foreach (var item in FilesAssociated)
        //                            {
        //                                _fileUploadService.DeleteFile(item);
        //                            }
        //                            con.Execute("delete from AwardFileMapper where AwardId=@awardId;delete from Awards where Id=@awardId;", new { awardId = award.Id });
        //                        }
        //                        else
        //                        {
        //                            con.Execute("update Awards set Title=@Title where Id=@AwardID ", new { Title = award.Title, AwardId = award.Id });
        //                            foreach (var file in award.Files)
        //                            {
        //                                if (file.IsDeleted && file.Id > 0)
        //                                {
        //                                    var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id });
        //                                    _fileUploadService.DeleteFile(fileStorageName);
        //                                    con.Execute("delete from AwardFileMapper where AwardId=@AwardId;", new { AwardId = award.Id });
        //                                }
        //                                else if (file.Id == 0 || file.Id == null)
        //                                {
        //                                    IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                    var fileId = _fileUploadService.UploadFile(data);
        //                                    con.Execute($"Insert into AwardFileMapper(AwardId,FileId) values({award.Id},{fileId})");
        //                                }
        //                            }
        //                        }

        //                    }
        //                }
        //            }

        //            //update Qualification

        //            if (foodSupplier.Qualifications != null)
        //            {
        //                foreach (var qualification in foodSupplier.Qualifications)
        //                {
        //                    if (qualification.CertificationsCategoryId == 0 || qualification.CertificationsCategoryId == null &&
        //                    qualification.OtherCertificationsCategoryName == null)
        //                    {
        //                        throw new Exception("Either pass CertificationsCategoryId or OtherCertificationsCategoryName");
        //                    }
        //                    else if (qualification.CertificationsCategoryId > 0)
        //                    {
        //                        qualification.OtherCertificationsCategoryName = null;
        //                    }
        //                    if (qualification == null)
        //                    {
        //                        continue;
        //                    }
        //                    if (qualification.Id == 0 || qualification.Id == null)
        //                    {
        //                        var QualificationId = con.ExecuteScalar<int>(@"Insert into [dbo].[Qualification](SupplierProfileId,Title,CreatedOn,UpdatedOn,CertificationsCategoryId,OtherCertificationsCategoryName)
        //                                      values (@SupplierProfileId,@Title,GETUTCDATE(),GETUTCDATE(),@CertificationsCategoryId,@OtherCertificationsCategoryName);select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Title = qualification.Title, CertificationsCategoryId = qualification.CertificationsCategoryId, OtherCertificationsCategoryName = qualification.OtherCertificationsCategoryName });

        //                        if (qualification.Files != null)
        //                        {
        //                            foreach (var file in qualification.Files)
        //                            {
        //                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                var fileId = await _fileUploadService.UploadFile(data);
        //                                con.Execute($"Insert into QualificationFileMapper(QualificationId,FileId) values({QualificationId},{fileId})");
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (qualification.IsDeleted == true)
        //                        {
        //                            var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from QualificationFileMapper afm 
        //                                                        inner join FileUploadTable ft on afm.FileId=ft.Id where afm.QualificationId=@QualificationId ", new { QualificationId = qualification.Id });
        //                            foreach (var item in FilesAssociated)
        //                            {
        //                                _fileUploadService.DeleteFile(item);
        //                            }
        //                            con.Execute("delete from QualificationFileMapper where QualificationId=@qualificationId;delete from Qualification where Id=@qualificationId;", new { qualificationId = qualification.Id });
        //                        }
        //                        else
        //                        {
        //                            con.Execute("update Qualification set Title=@Title,CertificationsCategoryId=@CertificationsCategoryId,OtherCertificationsCategoryName=@OtherCertificationsCategoryName where Id=@QualificationId ", new { Title = qualification.Title, QualificationId = qualification.Id, CertificationsCategoryId = qualification.CertificationsCategoryId, OtherCertificationsCategoryName = qualification.OtherCertificationsCategoryName });
        //                            foreach (var file in qualification.Files)
        //                            {
        //                                if (file.IsDeleted && file.Id > 0)
        //                                {
        //                                    var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id });
        //                                    _fileUploadService.DeleteFile(fileStorageName);
        //                                    con.Execute("delete from QualificationFileMapper where FileId=@FileId;", new { qualificationId = qualification.Id, FileId = file.Id });
        //                                }
        //                                else if (file.Id == 0 || file.Id == null)
        //                                {
        //                                    IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                    var fileId = _fileUploadService.UploadFile(data);
        //                                    con.Execute($"Insert into QualificationFileMapper(QualificationId,FileId) values({qualification.Id},{fileId})");
        //                                }
        //                            }
        //                        }

        //                    }
        //                }
        //            }

        //            //ut 
        //            if (foodSupplier.Products != null)
        //            {
        //                foreach (var product in foodSupplier.Products)
        //                {
        //                    if (product.ProductCategoryId == 0 || product.ProductCategoryId == null &&
        //                    product.OtherProductCategoryName == null)
        //                    {
        //                        throw new Exception("Either pass ProductCatergoryId or OtherProductCatergoryName");
        //                    }
        //                    else if (product.ProductCategoryId > 0)
        //                    {
        //                        product.OtherProductCategoryName = null;
        //                    }
        //                    if (product == null)
        //                    {
        //                        continue;
        //                    }
        //                    if (product.Id == 0 || product.Id == null)
        //                    {
        //                        var ProductId = con.ExecuteScalar<int>(@"Insert into [Products](SupplierProfileId,Title,ProductCategoryId,OtherProductCategoryName,CreatedOn,UpdatedOn)
        //                                  values (@SupplierProfileId,@Title,@ProductCategoryId,@OtherProductCategoryName,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Title = product.Title, ProductCategoryId = product.ProductCategoryId, OtherProductCategoryName = product.OtherProductCategoryName });

        //                        if (product.Files != null)
        //                        {
        //                            foreach (var file in product.Files)
        //                            {
        //                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                var fileId = await _fileUploadService.UploadFile(data);
        //                                con.Execute($"Insert into [ProductFileMapper](ProductId,FileId) values({ProductId},{fileId})");
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (product.IsDeleted == true)
        //                        {
        //                            var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from ProductFileMapper afm inner join FileUploadTable ft on afm.FileId=ft.Id where afm.ProductId=@ProductId ", new { ProductId = product.Id });
        //                            foreach (var item in FilesAssociated)
        //                            {
        //                                _fileUploadService.DeleteFile(item);
        //                            }
        //                            con.Execute("delete from ProductFileMapper where ProductId=@productId;delete from Products where Id=@productId;", new { productId = product.Id });
        //                        }
        //                        else
        //                        {
        //                            con.Execute("update Products set Title=@Title,ProductCategoryId=@ProductCategoryId,OtherProductCategoryName=@OtherProductCategoryName where Id=@ProductId ", new { Title = product.Title, ProductId = product.Id, ProductCategoryId = product.ProductCategoryId, OtherProductCategoryName = product.OtherProductCategoryName });

        //                            foreach (var file in product.Files)
        //                            {
        //                                if (file.IsDeleted && file.Id > 0)
        //                                {
        //                                    var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id });
        //                                    _fileUploadService.DeleteFile(fileStorageName);
        //                                    con.Execute("delete from ProductFileMapper where FileId=@FileId;", new { FileId = file.Id });
        //                                }
        //                                else if (file.Id == 0 || file.Id == null)
        //                                {
        //                                    IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                    var fileId = _fileUploadService.UploadFile(data);
        //                                    con.Execute($"Insert into ProductFileMapper(ProductId,FileId) values({product.Id},{fileId})");
        //                                }
        //                            }
        //                        }

        //                    }
        //                }
        //            }
        //            //ut 
        //            if (foodSupplier.Services != null)
        //            {
        //                foreach (var service in foodSupplier.Services)
        //                {
        //                    if (service.BusinessActivityCategoryId == 0 || service.BusinessActivityCategoryId == null &&
        //                    service.OtherBusinessActivityCategoryName == null)
        //                    {
        //                        throw new Exception("Either pass BusinessActivityCategoryId or OtherBusinessActivityCategoryName");
        //                    }
        //                    else if (service.BusinessActivityCategoryId > 0)
        //                    {
        //                        service.OtherBusinessActivityCategoryName = null;
        //                    }
        //                    if (service == null)
        //                    {
        //                        continue;
        //                    }
        //                    if (service.Id == 0)
        //                    {
        //                        /// add missing fields
        //                        var ServiceId = con.ExecuteScalar<int>(@"Insert into [NewService](SupplierProfileId,Title,Descriptions,BusinessActivityCategoryId,OtherBusinessActivityCategoryName,CreatedOn,UpdatedOn)values(@SupplierProfileId,@Title,@Descriptions,@BusinessActivityCategoryId,@OtherBusinessActivityCategoryName,GETUTCDATE(),GETUTCDATE()); select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Title = service.Title, Descriptions = service.Descriptions, BusinessActivityCategoryId = service.BusinessActivityCategoryId, OtherBusinessActivityCategoryName = service.OtherBusinessActivityCategoryName });

        //                        if (service.Files != null)
        //                        {
        //                            foreach (var file in service.Files)
        //                            {
        //                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                var fileId = await _fileUploadService.UploadFile(data);
        //                                con.Execute($"Insert into [ServiceFileMapper] (ServiceId,FileId) values({ServiceId},{fileId})");
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (service.IsDeleted == true)
        //                        {
        //                            var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from ServiceFileMapper afm inner join FileUploadTable ft on afm.FileId=ft.Id where afm.ServiceId=@ServiceId ", new { ServiceId = service.Id });
        //                            foreach (var item in FilesAssociated)
        //                            {
        //                                _fileUploadService.DeleteFile(item);
        //                            }
        //                            con.Execute("delete from ServiceFileMapper where ServiceId=@serviceId;delete from NewService where Id=@serviceId;", new { serviceId = service.Id });
        //                        }
        //                        else
        //                        {
        //                            con.Execute("update NewService set Title=@Title,Descriptions=@Descriptions,BusinessActivityCategoryId=@BusinessActivityCategoryId,OtherBusinessActivityCategoryName=@OtherBusinessActivityCategoryName where Id=@ServiceId ", new { Title = service.Title, ServiceId = service.Id, Descriptions = service.Descriptions, BusinessActivityCategoryId = service.BusinessActivityCategoryId, OtherBusinessActivityCategoryName = service.OtherBusinessActivityCategoryName });
        //                            foreach (var file in service.Files)
        //                            {
        //                                if (file.IsDeleted && file.Id > 0)
        //                                {
        //                                    var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id });
        //                                    _fileUploadService.DeleteFile(fileStorageName);
        //                                    con.Execute("delete from ServiceFileMapper where FileId=@FileId;", new { FileId = file.Id });
        //                                }
        //                                else if (file.Id == 0 || file.Id == null)
        //                                {
        //                                    IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                    var fileId = await _fileUploadService.UploadFile(data);
        //                                    con.Execute($"Insert into ServiceFileMapper(ServiceId,FileId) values({service.Id},{fileId})");
        //                                    //   con.Execute($"Insert into ProductFileMapper(ProductId,FileId) values({product.Id},{fileId})");
        //                                }
        //                            }
        //                        }

        //                    }
        //                }
        //            }

        //            //update UploadImages

        //            if (foodSupplier.UploadImages != null)
        //            {
        //                foreach (var uploadImage in foodSupplier.UploadImages)
        //                {
        //                    if (uploadImage == null)
        //                    {
        //                        continue;
        //                    }
        //                    if (uploadImage.Id == 0 || uploadImage.Id == null)
        //                    {
        //                        var UploadImageId = con.ExecuteScalar<int>(@"Insert into [UploadsImages](SupplierProfileId,Descriptions,CreatedOn,UpdatedOn)
        //                                  values (@SupplierProfileId,@Description,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Description = uploadImage.Description });

        //                        if (uploadImage.Files != null)
        //                        {
        //                            foreach (var file in uploadImage.Files)
        //                            {
        //                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                var fileId = await _fileUploadService.UploadFile(data);
        //                                con.Execute($"Insert into [UploadImagesFileMapper](UploadImageId,FileId) values({UploadImageId},{fileId})");
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (uploadImage.IsDeleted == true)
        //                        {
        //                            var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from UploadImagesFileMapper afm 
        //                                                        inner join FileUploadTable ft on afm.FileId=ft.Id where afm.UploadImageId=@UploadImageId ", new { UploadImageId = uploadImage.Id });
        //                            foreach (var item in FilesAssociated)
        //                            {
        //                                _fileUploadService.DeleteFile(item);
        //                            }
        //                            con.Execute("delete from UploadImagesFileMapper where UploadImageId=@uploadImageId;delete from UploadsImages where Id=@uploadImageId;", new { uploadImageId = uploadImage.Id });
        //                        }
        //                        else
        //                        {
        //                            con.Execute("update [UploadsImages] set Description=@Description where Id=@UploadImageId ", new { Description = uploadImage.Description, UploadImageId = uploadImage.Id });
        //                            foreach (var file in uploadImage.Files)
        //                            {
        //                                if (file.IsDeleted && file.Id > 0)
        //                                {
        //                                    var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id });
        //                                    _fileUploadService.DeleteFile(fileStorageName);
        //                                    con.Execute("delete from UploadImagesFileMapper where FileId=@FileId;", new { FileId = file.Id });
        //                                }
        //                                else if (file.Id == 0 || file.Id == null)
        //                                {
        //                                    var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id });
        //                                    _fileUploadService.DeleteFile(fileStorageName);
        //                                    IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                    var fileId = _fileUploadService.UploadFile(data);
        //                                    con.Execute($"Insert into UploadImagesFileMapper(UploadImageId,FileId) values({uploadImage.Id},{fileId})");
        //                                }
        //                            }
        //                        }

        //                    }
        //                }
        //            }

        //            //update wallPapers

        //            if (foodSupplier.Wallpapers != null)
        //            {
        //                foreach (var wallpaper in foodSupplier.Wallpapers)
        //                {
        //                    if (wallpaper == null)
        //                    {
        //                        continue;
        //                    }
        //                    if (wallpaper.Id == 0 || wallpaper.Id == null)
        //                    {
        //                        var WallpaperId = con.ExecuteScalar<int>(@"Insert into [Wallpapers](SupplierProfileId,Descriptions,CreatedOn,UpdatedOn)
        //                                  values (@SupplierProfileId,@Descriptions,GETUTCDATE(),GETUTCDATE());select SCOPE_IDENTITY();", new { SupplierProfileId = foodSupplier.Id, Descriptions = wallpaper.Description });

        //                        if (wallpaper.Files != null)
        //                        {
        //                            foreach (var file in wallpaper.Files)
        //                            {
        //                                IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                var fileId = await _fileUploadService.UploadFile(data);
        //                                con.Execute($"Insert into [WallPaperFileMapper](WallpaperId,FileId) values({WallpaperId},{fileId})");
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (wallpaper.IsDeleted == true)
        //                        {
        //                            var FilesAssociated = con.Query<string>(@"select ft.FileStorageName from WallPaperFileMapper afm 
        //                                                        inner join FileUploadTable ft on afm.FileId=ft.Id where afm.WallpaperId=@WallpaperId ", new { WallpaperId = wallpaper.Id });
        //                            foreach (var item in FilesAssociated)
        //                            {
        //                                _fileUploadService.DeleteFile(item);
        //                            }
        //                            con.Execute("delete from WallPaperFileMapper where WallpaperId=@wallpaperId;delete from Wallpapers where Id=@wallpaperId;", new { wallpaperId = wallpaper.Id });
        //                        }
        //                        else
        //                        {
        //                            con.Execute("update [Wallpapers] set Descriptions=@Descriptions where Id=@WallpaperId ", new { Descriptions = wallpaper.Description, WallpaperId = wallpaper.Id });
        //                            foreach (var file in wallpaper.Files)
        //                            {
        //                                if (file.IsDeleted && file.Id > 0)
        //                                {
        //                                    var fileStorageName = con.QuerySingle<string>("select FileStorageName from FileUploadTable where Id=@Id ", new { Id = file.Id });
        //                                    _fileUploadService.DeleteFile(fileStorageName);
        //                                    con.Execute("delete from WallPaperFileMapper where FileId=@FileId;", new { FileId = file.Id });
        //                                }
        //                                else if (file.Id == 0 || file.Id == null)
        //                                {
        //                                    IFormFile data = _fileFormatConverterService.Base64ToIFormFile(file.File, file.FileName);
        //                                    var fileId = _fileUploadService.UploadFile(data);
        //                                    con.Execute($"Insert into WallPaperFileMapper(WallpaperId,FileId) values({wallpaper.Id},{fileId})");
        //                                }
        //                            }
        //                        }

        //                    }
        //                }
        //            }

        //            //updating Videos

        //            if (foodSupplier.Videos != null)
        //                if (foodSupplier.Videos.Count > 0)
        //                {
        //                    foreach (var video in foodSupplier.Videos)
        //                    {
        //                        con.ExecuteScalar(@"update [dbo].[UploadVideo] set Description=@Description,Link=@Link where Id=@Id;", new { Id = video.Id, Description = video.Description, Link = video.Link });
        //                    }
        //                }


        //            if (con.State == System.Data.ConnectionState.Open)
        //            {
        //                con.Close();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result = "Failed";
        //    }
        //    return result;
        //}
        public string DeleteFoodSupplier(int id)
        {
            try
            {

                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    var rowsAffected = con.Execute("UPDATE [dbo].[SupplierProfiles] SET IsDeleted=1 where Id=@id", new { Id = id });
                }
                return "success";

            }
            catch (Exception ex)
            { return ex.Message; }
        }

        public string SubscriptionStatusActive(int id)
        {

            try
            {

                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    var rowsAffected = con.Execute("UPDATE [dbo].[SupplierProfiles] SET SubscriptionStatus=1,AccountStatus=1 where CreatedBy=@Id", new { Id = id });
                    if(rowsAffected > 0)
                    {
                        con.Execute("UPDATE [dbo].[Users] SET RoleId=3 where Id=@id", new { Id = id });
                    }
                    
                    if (rowsAffected == 0)
                    {
                        return "Supplier Subscription Status is not Active";

                    }
                }
                return "success";

            }
            catch (Exception ex)
            { return ex.Message; }
        }


        public string SubscriptionStatusInActive(int id)
        {

            try
            {

                using (System.Data.IDbConnection con = new SqlConnection(Global.ConnectionString))
                {
                    if (con.State == System.Data.ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    var rowsAffected = con.Execute("UPDATE [dbo].[SupplierProfiles] SET SubscriptionStatus=0,AccountStatus=0 where CreatedBy=@Id", new { Id = id });
                    if (rowsAffected > 0)
                    {
                        con.Execute("UPDATE [dbo].[Users] SET RoleId=2 where Id=@Id", new { Id = id });
                    }
                    if (rowsAffected == 0)
                    {
                        return "Supplier Subscription Status is Active";

                    }
                }
                return "success";

            }
            catch (Exception ex)
            { return ex.Message; }
        }
    }
}
