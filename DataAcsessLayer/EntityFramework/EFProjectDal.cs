using DataAccessLayer.Concrete.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAcsessLayer.EntityFramework
{
    public class EFProjectDal : GenericRepositoryDal<EntityLayer.Concrete.Project>, Abstract.IProjectDal
    {
        public EFProjectDal(Concrete.Context.AppDbContext context) : base(context)
        {
        }

    }
}
