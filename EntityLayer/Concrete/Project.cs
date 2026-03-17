using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Concrete
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; }

        public int Year { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Property
        public ICollection<ProjectImage> ProjectImages { get; set; } = new List<ProjectImage>();
    }
}
