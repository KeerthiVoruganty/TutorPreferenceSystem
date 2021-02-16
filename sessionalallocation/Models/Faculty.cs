using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SessionalAllocation.Models
{
    public partial class Faculty
    {
        public Faculty()
        {
            School = new HashSet<School>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        

        public virtual ICollection<School> School { get; set; }
    }
}
