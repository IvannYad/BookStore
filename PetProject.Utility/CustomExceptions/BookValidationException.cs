using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetProject.Utility.CustomExceptions
{
    public class BookValidationException : Exception
    {
        private readonly string _message;
        public BookValidationException(string message)
        {
            _message = message;
        }

        public new string Message
        {
            get { return _message; }
        }
    }
}
