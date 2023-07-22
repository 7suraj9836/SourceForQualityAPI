using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SourceforqualityAPI.Contracts;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DropdownController : ControllerBase
    {
        private readonly IDropdown _dropdownService;

        public DropdownController(IDropdown dropdownService)
        {
            _dropdownService = dropdownService;
        }

        [HttpGet("GetProductCategories")]
        public async Task<ResponseModel<List<DropdownResponse>>> GetProductCategories()
        {
            var result = new ResponseModel<List<DropdownResponse>>();
            try
            {
                result.Data = await _dropdownService.GetProductCategories();
                result.Message = "Data fetch successful";
                result.StatusCode = 200;
            }
            catch (System.Exception ex)
            {
                result.Data = null;
                result.Message = ex.Message;
                result.StatusCode = 400;
            }

            return result;
        }


        [HttpGet("GetBusinessActivityCategories")]
        public async Task<ResponseModel<List<DropdownResponse>>> GetBusinessActivityCategories()
        {
            var result = new ResponseModel<List<DropdownResponse>>();
            try
            {
                result.Data = await _dropdownService.GetBusinessActivityCategories();
                result.Message = "Data fetch successful";
                result.StatusCode = 200;
            }
            catch (System.Exception ex)
            {
                result.Data = null;
                result.Message = ex.Message;
                result.StatusCode = 400;
            }

            return result;
        }


        [HttpGet("GetCertificationCategories")]
        public async Task<ResponseModel<List<DropdownResponse>>> GetCertificationCategories()
        {
            var result = new ResponseModel<List<DropdownResponse>>();
            try
            {
                result.Data = await _dropdownService.GetCertificationCategories();
                result.Message = "Data fetch successful";
                result.StatusCode = 200;
            }
            catch (System.Exception ex)
            {
                result.Data = null;
                result.Message = ex.Message;
                result.StatusCode = 400;
            }

            return result;
        }

        [HttpGet("GetCountries")]
        public async Task<ResponseModel<List<DropdownResponse>>> GetCountries()
        {
            var result = new ResponseModel<List<DropdownResponse>>();
            try
            {
                result.Data = await _dropdownService.GetCountries();
                result.Message = "Data fetch successful";
                result.StatusCode = 200;
            }
            catch (System.Exception ex)
            {
                result.Data = null;
                result.Message = ex.Message;
                result.StatusCode = 400;
            }

            return result;
        }

        [HttpGet("GetFaqPages")]
        public async Task<ResponseModel<List<DropdownResponse>>> GetFaqPages()
        {
            var result = new ResponseModel<List<DropdownResponse>>();
            try
            {
                result.Data = await _dropdownService.GetFaqPages();
                result.Message = "Data fetch successful";
                result.StatusCode = 200;
            }
            catch (System.Exception ex)
            {
                result.Data = null;
                result.Message = ex.Message;
                result.StatusCode = 400;
            }

            return result;
        }



    }
}
