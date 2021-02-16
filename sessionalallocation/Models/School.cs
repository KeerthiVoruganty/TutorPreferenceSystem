using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SessionalAllocation.Models
{
    public partial class School
    {
        public School()
        {
            Department = new HashSet<Department>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Faculty { get; set; }

        public virtual Faculty FacultyNavigation { get; set; }
        public virtual ICollection<Department> Department { get; set; }
    }
}
