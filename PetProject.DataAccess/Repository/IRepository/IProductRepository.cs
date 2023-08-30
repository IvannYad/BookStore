using PetProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetProject.DataAccess.Repository.IRepository
{
    public interface IProductRepository: IRepository.IRepository<Product>
    {
        void Update(Product product);
    }
}
