using DataAccessLayer.Concrete.Repository;
using DataAcsessLayer.Concrete.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAcsessLayer.EntityFramework
{
    public class EFUserDal : GenericRepositoryDal<EntityLayer.Concrete.User>, Abstract.IUserDal
    {
        public EFUserDal(AppDbContext context) : base(context)
        {
        }
    }
}
