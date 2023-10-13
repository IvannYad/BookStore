﻿using PetProject.DataAccess.Repository.IRepository;
using PetProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using PetProject.DataAccess.Data;

namespace PetProject.DataAccess.Repository
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        private ApplicationDbContext _context;
        public ProductImageRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;       
        }

        public void Update(ProductImage productImage)
        {
            _context.ProductImages.Update(productImage);
        }
    }
}
