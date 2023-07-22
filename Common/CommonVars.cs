using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Common
{
    public static class CommonVars
    {
        public enum MessageResults
        {
            [Display(Name = "Successfully saved!")]
            [Description("Successfully saved!")]
            SuccessSave,
            [Display(Name = "Successfully updated!")]
            [Description("Successfully updated!")]
            SuccessUpdate,
            [Display(Name = "Successfully get!")]
            [Description("Successfully get!")]
            SuccessGet,
            [Display(Name = "Error while saving!")]
            [Description("Error while saving!")]
            ErrorSave,
            [Display(Name = "Successfully deleted!")]
            [Description("Successfully deleted!")]
            SuccessDelete,
            [Display(Name = "Error while deleting!")]
            [Description("Error while deleting!")]
            ErrorDelete,
            [Display(Name = "Error while geting data!")]
            [Description("Error while geting data!")]
            ErrorGet,
            [Display(Name = "User with this email already exists!")]
            [Description("User with this email already exists!")]
            UserDuplicateEmail,
            [Display(Name = "User with this code already exists!")]
            [Description("User with this code already exists!")]
            UserDuplicateCode,
            [Display(Name = "Invalid login details!")]
            [Description("Invalid login details!")]
            InvalidLogin,
            [Display(Name = "No such user found!")]
            [Description("No such user found!")]
            UserNotFound,
            [Display(Name = "No company found for selected user!")]
            [Description("No company found for selected user!")]
            UserCompanyNotFound,
            [Display(Name = "No such Record found!")]
            [Description("No such Record found!")]
            RecordNotFound,
            [Display(Name = "Authentication failed!")]
            [Description("Authentication failed!")]
            AuthenticationFailed,
            [Display(Name = "Duplicate Record Found!")]
            [Description("Duplicate Record Found!")]
            DuplicateRecordFound,
        }

    }
}
