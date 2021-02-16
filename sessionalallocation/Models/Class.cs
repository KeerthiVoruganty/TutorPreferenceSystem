using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SessionalAllocation.Models
{
    public partial class Class
    {
        public Class()
        {
            Applications = new HashSet<Applications>();
        }

        public int Id { get; set; }
        public int UnitId { get; set; }
        [Display(Name = "Class Type")]
        public string ClassType { get; set; }
        public string TutorAllocated { get; set; }
        public bool Allocated { get; set; }
        public bool Approved { get; set; }
        public string Location { get; set; }
        public string Year { get; set; }
        [Display(Name = "Study Period")]
        public string StudyPeriod { get; set; }
        [Display(Name = "Day")]
        public string DayOfWeek { get; set; }
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Display(Name = "Start Time")]
        public TimeSpan StartTimeScheduled { get; set; }
        [Display(Name = "End Time")]
        public TimeSpan EndTimeScheduled { get; set; }
        public string roomDetails { get; set; }
        [Display(Name = "Tutor")]
        public virtual ApplicationUser TutorAllocatedNavigation { get; set; }
        [Display(Name = "Unit")]
        public virtual Unit Unit { get; set; }
        public virtual ICollection<Applications> Applications { get; set; }
        [Display(Name = "Start Date")]
        public string DateOnlyString
        {
            get
            {
                return StartDate.ToShortDateString();
            }
        }
    }
}
