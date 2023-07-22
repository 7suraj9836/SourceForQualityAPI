using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SourceforqualityAPI.Contracts;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierSearchController : ControllerBase
    {
        private readonly ISupplierSearchService _searchInputServices;

        public SupplierSearchController(ISupplierSearchService searchInputServices)
        {
            _searchInputServices = searchInputServices;
        }

        //[Authorize(policy: "RequireAnyRole")]
        [HttpPost("GetFoodSupplierByFilter")]
        public async Task<ResponseModel<List<FoodSupplierSearchOutput>>> GetFoodSupplierByFilter(SupplierSearchFilter searchInputDTO)
        {
            var res = new ResponseModel<List<FoodSupplierSearchOutput>>();
            try
            {
                res.Data = await _searchInputServices.GetFoodSupplierByFilter(searchInputDTO);
                res.StatusCode = 200;
                res.Message = "Data Fetched Successfully ";
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;
        }

        //[HttpPost("GetUserFavouriteSupplier")]
        //public async Task<ResponseModel<List<UserFavouriteSupplier>>> GetUserFavouriteSupplier(UserFavouriteSupplier searchInputDTO)
        //{
        //    var res = new ResponseModel<List<UserFavouriteSupplier>>();
        //    try
        //    {
        //        res.Data = await _searchInputServices.GetUserFavouriteSupplier(searchInputDTO);
        //        res.StatusCode = 200;
        //        res.Message = "Data Fetched Successfully ";
        //    }
        //    catch (Exception ex)
        //    {
        //        res.Data = null;
        //        res.Message = ex.Message;
        //        res.StatusCode = 400;
        //    }
        //    return res;
        //}

    }
}
