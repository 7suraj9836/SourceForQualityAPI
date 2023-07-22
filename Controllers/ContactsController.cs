using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SourceforqualityAPI.Interfaces;
using SourceforqualityAPI.Model;
using SourceforqualityAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : Controller
    {
        private readonly IContactService _contactServices;

        public ContactsController(IContactService contactServices)
        {
            _contactServices = contactServices;
        }


      //  [Authorize(policy: "RequireAdminRole")]
        //[Authorize(policy: "RequireEndUserRole")]
        [HttpGet("GetContacts")]
        public async Task<ResponseModel<List<Contacts>>> GetContacts(int pageNumber, int pageSize)
        {
            var res = new ResponseModel<List<Contacts>>();
            try
            {
                res.Data = _contactServices.GetContacts(pageNumber,pageSize);
                res.StatusCode = 200;
                res.Message = "Contact Data Fetched Successfully";
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

       // [Authorize(policy: "RequireEndUserFoodSupplier")]
        [HttpPost("SaveContact")]
        public async Task<ResponseModel<string>> SaveContact(Contacts contact)
        {
            var res = new ResponseModel<string>();
            try
            {
                res.Data = _contactServices.SaveContact(contact);
                res.StatusCode = 200;
                res.Message = "Contact Saved Successfully";
            }
            catch (Exception ex)
            {
                res.Data = null;
                res.Message = ex.Message;
                res.StatusCode = 400;
            }
            return res;

            //return Ok(_contactServices.SaveContact(contact));
        }
    }
}
