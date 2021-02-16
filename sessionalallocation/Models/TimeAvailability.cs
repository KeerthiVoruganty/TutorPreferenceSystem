using System;
using System.ComponentModel.DataAnnotations;

namespace SessionalAllocation.Models
{
    public class TimeAvailability
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Day of the Week")]
        public DaysOfWeek WeekDay { get; set; }
        // To indicate if timerange is all day or not.
        public bool IsAllDay { get; set; }
        [DataType(DataType.Time)]
        public DateTime FromTime { get; set; }
        [DataType(DataType.Time)]
        public DateTime EndTime { get; set; }
        public ApplicationUser ApplicantNavigation { get; set; }
    }

    public enum DaysOfWeek
    {
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday
    }
}
