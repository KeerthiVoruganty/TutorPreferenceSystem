using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SessionalAllocation.Models
{
    public class Payrate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public float Rate { get; set; }
    }
}
