using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SessionalAllocation.ViewModels
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        // key is role name, value true/false
        public Dictionary<string, bool> UserRoles { get; set; }
    }
}
