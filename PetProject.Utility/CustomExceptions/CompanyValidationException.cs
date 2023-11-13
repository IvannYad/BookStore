using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetProject.Utility.CustomExceptions
{
    public class CompanyValidationException : Exception
    {
        private readonly string _message;
        public CompanyValidationException(string message)
        {
            _message = message;
        }

        public new string Message
        {
            get { return _message; }
        }
    }
}
