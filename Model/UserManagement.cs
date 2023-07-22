using System;

namespace SourceforqualityAPI.Model
{
    public class UserManagement
    {
        
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public string  MobileNo { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public Int32 RoleId { get; set; } = 2;
            public DateTime CreatedOn { get; set; } = DateTime.Now;
            public DateTime UpdatedOn { get; set; } = DateTime.Now;
            public bool IsActive { get; set; } = true;

            //Added in the latest code
            //public IFormFile? ProfileImage { get; set; }
            public string? EPassword { get; set; }
            //  public int ProfileImageId { get; set; }

            //  public bool IsSubscribed { get; set; }
        
    }
}
