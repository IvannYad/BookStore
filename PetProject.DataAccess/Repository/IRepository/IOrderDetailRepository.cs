using PetProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PetProject.DataAccess.Repository.IRepository
{
    public interface IOrderDetailRepository : IRepository.IRepository<OrderDetail>
    {
        void Update(OrderDetail orderDetail);
    }
}
