using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Concrete
{
    public class ProjectImage
    {

            public int Id { get; set; }

            [Required]
            public string ImageUrl { get; set; }

            public int ProjectId { get; set; }

            public bool IsCover { get; set; } = false;

            public int Order { get; set; } = 0;

            // Navigation Property
            public Project Project { get; set; }
        }
    }

