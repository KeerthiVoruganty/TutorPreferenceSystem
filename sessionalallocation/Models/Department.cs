using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SessionalAllocation.Models
{
    public partial class Department
    {
        public Department()
        {
            Unit = new HashSet<Unit>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int School { get; set; }
        [Display(Name = "Department Chair")]
        public string DepartmentOwner { get; set; }
        [Display(Name = "School")]
        public virtual School SchoolNavigation { get; set; }
        public virtual ICollection<Unit> Unit { get; set; }
        [Display(Name = "Department Chair")]
        public virtual ApplicationUser DepartmentOwnerNavigation { get; set; }
    }
}
