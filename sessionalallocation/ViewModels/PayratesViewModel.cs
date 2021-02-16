using SessionalAllocation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SessionalAllocation.ViewModels
{
    public class PayratesViewModel
    {
        public Dictionary<int, Payrate> Payrates {get; set;}
    }
}
