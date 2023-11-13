using PetProject.Models;
using PetProject.Models.ViewModels;
using PetProject.Services.Interfaces;
using PetProject.Utility.CustomExceptions;

namespace PetProject.Services
{
    public class BookValidator : IValidator<Product>
    {
        public bool Validate(Product item)
        {
            if (item.Title is not null && !char.IsUpper(item.Title[0]))
            {
                throw new BookValidationException("Product.Title", "'Book Title' must start with capital letter");
            }

            return true;
        }
    }
}
