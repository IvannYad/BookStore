using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetProject.DataAccess.Data;
using PetProject.Models;
using PetProject.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetProject.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public DbInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        public void Initialize()
        {

            // Push migrations if they are not applied.
            try
            {
                if (_context.Database.GetPendingMigrations().Count() > 0)
                {
                    _context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            // Create roles if they are not created.
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();

                // If roles are not created, create admin user as well.
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "yAdmin@gmail.com",
                    Email = "yAdmin@gmail.com",
                    Name = "Ivan Yad",
                    PhoneNumber = "0687779933",
                    StreetAddress = "Test st. 33",
                    State = "Lviv",
                    PostalCode = "77077",
                    City = "Lviv",
                }, "#Admin07").GetAwaiter().GetResult();

                ApplicationUser user = _context.ApplicationUsers.FirstOrDefault(user => user.Email == "yAdmin@gmail.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }
        }
    }
}
