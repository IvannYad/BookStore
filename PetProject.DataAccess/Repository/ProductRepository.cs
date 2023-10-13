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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            var productFromDb = _context.Products.FirstOrDefault(x => x.Id == product.Id);
            if (productFromDb != null)
            {
                productFromDb.Title = product.Title;
                productFromDb.ISBN = product.ISBN;
                productFromDb.Price = product.Price;
                productFromDb.Price50 = product.Price50;
                productFromDb.Price100 = product.Price100;
                productFromDb.ListPrice = product.ListPrice;
                productFromDb.Description = product.Description;
                productFromDb.CategoryId = product.CategoryId;
                productFromDb.Author = product.Author;
                productFromDb.ProductImages = product.ProductImages;

                _context.Products.Update(productFromDb);
            }
        }
    }
}
