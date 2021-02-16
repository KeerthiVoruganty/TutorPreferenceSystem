using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SessionalAllocation.Models
{
    public partial class Unit
    {
        public Unit()
        {
            Class = new HashSet<Class>();
        }

        public int Id { get; set; }
        [Display(Name = "Unit Code")]
        public string UnitCode { get; set; }
        [Display(Name = "Unit Name")]
        public string UnitName { get; set; }
        [Display(Name = "Department")]
        public int Department { get; set; }
        [Display(Name = "Convenor")]
        public string UnitOwner { get; set; }

        public string UnitLink { get; set; }

        [Display(Name = "Department")]
        public virtual Department DepartmentNavigation { get; set; }
        [Display(Name = "Convenor")]
        public virtual ApplicationUser UnitOwnerNavigation { get; set; }
        public virtual ICollection<Class> Class { get; set; }
    }
}
