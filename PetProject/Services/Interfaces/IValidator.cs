namespace PetProject.Services.Interfaces
{
    public interface IValidator<T>
    {
        bool Validate(T item);
    }
}
