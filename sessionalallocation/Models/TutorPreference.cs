using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SessionalAllocation.Models
{
    public class TutorPreference
    {
        public TutorPreference(int rating = 5)
        {
            this.rating = rating;
        }
        public int Id { get; set; }
        public string ConvenorId { get; set; }
        public string TutorId { get; set; }
        public int rating { get; set; }
    }
}
