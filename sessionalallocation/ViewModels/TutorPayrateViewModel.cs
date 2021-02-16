using SessionalAllocation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SessionalAllocation.ViewModels
{
    public class TutorPayrateViewModel
    {
        public string ClassStartDate { get; set; }
        public TimeSpan ClassStartTime { get; set; }
        public TimeSpan ClassDuration { get; set; }
        public string ClassType { get; set; }
        public string ClassDayOfWeek { get; set; }
        // key is week# bool for if teaching week or not
        public Dictionary<int, bool> Weeks { get; set; }
        public string PayrateCode { get; set; }
        public string StaffStatus { get; set; }
        public bool NewStaff { get; set; }
        public string TutorFullName { get; set; }

        // used to view profile of tutor
        public string TutorId { get; set; }

        // information needed for when tutor is new
        public string TutorFirstName { get; set; }
        public string TutorLastName { get; set; }
        public string TutorEmail { get; set; }
        public string TutorAddress { get; set; }
        public string TutorSuburb { get; set; }
        public string TutorPostCode { get; set; }
        public string TutorMobileNumber { get; set; }
    }
}
