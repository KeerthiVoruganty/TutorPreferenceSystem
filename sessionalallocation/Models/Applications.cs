using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SessionalAllocation.Models
{
    public partial class Applications
    {
        
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string Applicant { get; set; }
        public int AppliedClass { get; set; }
        [Display(Name = "Provisionally Allocated")]
        public bool ProvisionallyAllocated { get; set; }
        public bool Approved { get; set; }
        public int Preference { get; set; }

        [Display(Name = "Applicant")]
        public virtual ApplicationUser ApplicantNavigation { get; set; }
        [Display(Name = "Class Type")]
        public virtual Class AppliedClassNavigation { get; set; }

        public string UnitName
        {
            get
            {
                return AppliedClassNavigation.Unit.UnitName;
            }
        }

    }
}
