using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    using global::BulkyBook.DataAccess.Data;
    using global::BulkyBook.DataAccess.Repository.IRepository;
    using global::BulkyBook.Models;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;

    namespace BulkyBook.DataAccess.Repository
    {
        public class CompanyRepository : Repository<Company>, ICompanyRepository
        {
            private readonly ApplicationDbContext _db;
            public CompanyRepository(ApplicationDbContext db) : base(db)
            {
                _db = db;
            }

            public void Update(Company obj)
            {
                _db.Companies.Update(obj);
            }
        }
    }

}
