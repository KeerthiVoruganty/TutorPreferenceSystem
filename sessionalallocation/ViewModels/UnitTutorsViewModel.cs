using SessionalAllocation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SessionalAllocation.ViewModels
{
    public class UnitTutorsViewModel
    {
        public string UnitCode { get; set; }
        public Dictionary<int, TutorPayrateViewModel> Tutors { get; set; }
    }
}
