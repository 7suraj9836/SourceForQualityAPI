using Microsoft.AspNetCore.Mvc;
using SourceforqualityAPI.Model;
using System.Threading.Tasks;
using System;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Services;
using System.Collections.Generic;

namespace SourceforqualityAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserFavouriteSupplierController : Controller
    {

        private readonly IUserFavouriteSupplierServices _userFavouriteSupplierServices;

        public UserFavouriteSupplierController(IUserFavouriteSupplierServices userFavouriteSupplierServices)
        {
            _userFavouriteSupplierServices = userFavouriteSupplierServices;
        }



        [HttpPost("GetAllFavouriteSuppliers")]
        public async Task<ResponseModel<List<FavouriteSuppliersList>>> GetAllFavouriteSuppliers(SortFavSupplierByCategory favSupplier)
        {
            var res = new ResponseModel<List<FavouriteSuppliersList>>();
            try
            {
                res.Data = _userFavouriteSupplierServices.GetAllFavouriteSuppliers(favSupplier);

                res.StatusCode = 200;
                res.Message = "Favourite Supplier Data Fetched Successfully";
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;
            //return Ok(_userManagementServices.GetUserManagement());
        }



        [HttpPost("UserFavouriteSupplier")]

        public async Task<ResponseModel<string>> UserFavouriteSupplier(UserFavouriteSupplierDataModel userFavouriteSupplier)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = await _userFavouriteSupplierServices.UserFavouriteSupplier(userFavouriteSupplier);
                res.StatusCode = 200;
                res.Message = "Data Saved Successfully";

            }
            catch (Exception ex)
            {
                res.Data = null;
                res.StatusCode = 400;
                res.Message = ex.Message;
            }
            return res;
        }

        //[HttpPost("GetAllFavouriteSuppliersBySortCategory")]
        //public async Task<ResponseModel<List<SortFavSupplierByCategoryOutputModel>>> GetAllFavouriteSuppliersBySortCategory(SortFavSupplierByCategory favSupplier)
        //{
        //    var res = new ResponseModel<List<SortFavSupplierByCategoryOutputModel>>();
        //    try
        //    {
        //        res.Data = _userFavouriteSupplierServices.GetAllFavouriteSuppliersBySortCategory( favSupplier);

        //        res.StatusCode = 200;
        //        res.Message = "Favourite Supplier Data Successfully Sorted By Category";
        //    }
        //    catch (Exception ex)
        //    {
        //        res.Data = null;
        //        res.Message = ex.Message;
        //        res.StatusCode = 400;
        //    }
        //    return res;
        //    //return Ok(_userManagementServices.GetUserManagement());
        //}
    }
}
