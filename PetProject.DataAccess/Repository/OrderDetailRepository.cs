using PetProject.DataAccess.Data;
using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetProject.DataAccess.Repository
{
    public class OrderDetailRepository: Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicationDbContext _context;
        public OrderDetailRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public void Update(OrderDetail orderDetail)
        {
            _context.OrderDetails.Update(orderDetail);
        }
    }
}
