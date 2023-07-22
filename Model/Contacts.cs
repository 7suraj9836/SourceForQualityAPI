using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Model
{
    public class Contacts
    {
       [Key]
       public int Id { get; set; }
       [Required]
       public string Name { get; set; }
       [Required]
       public string Subject { get; set; }
       [Required]
       public string Email { get; set; }
       //[Required]
       //public Int64 Mobile { get; set; }
       [Required]
       public string Mobile { get; set; }
        [Required]
       public string Message { get; set; }
    }  
}
