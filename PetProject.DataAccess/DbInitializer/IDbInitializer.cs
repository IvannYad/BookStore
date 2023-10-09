using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetProject.DataAccess.DbInitializer
{
    public interface IDbInitializer
    {
        /// <summary>
        ///     Method for creating admin user and roles
        /// </summary>
        void Initialize();
    }
}
