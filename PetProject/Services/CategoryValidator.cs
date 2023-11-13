using PetProject.Models;
using PetProject.Models.ViewModels;
using PetProject.Services.Interfaces;
using PetProject.Utility.CustomExceptions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Text.RegularExpressions;

namespace PetProject.Services
{
    public class CategoryValidator : IValidator<Category>
    {
        public bool Validate(Category item)
        {
            if (item.Name is not null && !Regex.IsMatch(item.Name, @"^[a-zA-Z]+$"))
            {
                throw new CategoryValidationException("Name", "'Category Name' must only consists of latin letters");
            }

            if (item.Name is not null && !char.IsUpper(item.Name[0]))
            {
                throw new CategoryValidationException("Name", "'Category Name' must start with capital letter");
            }

            return true;
        }
    }
}
