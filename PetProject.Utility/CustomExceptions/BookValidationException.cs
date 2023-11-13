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
        private readonly string _field;
        public BookValidationException(string field, string message)
        {
            _message = message;
            _field = field;
        }

        public new string Message
        {
            get { return _message; }
        }
        public string Field
        {
            get { return _field; }
        }
    }
}
