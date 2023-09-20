using PetProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetProject.DataAccess.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository.IRepository<ApplicationUser>
    {
        void Update(ApplicationUser applicationUser);
    }
}
