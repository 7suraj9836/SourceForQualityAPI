using SourceforqualityAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SourceforqualityAPI.Interfaces
{
    public interface IContactService
    {
        List<Contacts> GetContacts(int pageNumber, int pageSize);
        //Task<List<Contacts>> GetContacts();
        string SaveContact(Contacts contact);

    }
}
