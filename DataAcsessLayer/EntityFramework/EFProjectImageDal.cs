using DataAccessLayer.Concrete.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAcsessLayer.EntityFramework
{
    public class EFProjectImageDal : GenericRepositoryDal<EntityLayer.Concrete.ProjectImage>, Abstract.IProjectImageDal
    {
        public EFProjectImageDal(Concrete.Context.AppDbContext context) : base(context)
        {
        }
    }
}
