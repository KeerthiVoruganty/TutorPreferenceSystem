using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SessionalAllocation.Areas.Identity.Pages.Account.Manage
{
    public static class ManageNavPages
    {
        public static string Index => "Index";

        public static string ChangePassword => "ChangePassword";

        public static string ExternalLogins => "ExternalLogins";

        public static string PersonalData => "PersonalData";
        public static string Qualifications => "Qualifications";
        public static string TeachingExperience => "TeachingExperience";

        public static string TimeAvailability => "TimeAvailability";

        //public static string TwoFactorAuthentication => "TwoFactorAuthentication";

        public static string ResumeUpload => "ResumeUpload";

        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);
        public static string QualificationsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Qualifications);
        public static string TeachingExperienceNavClass(ViewContext viewContext) => PageNavClass(viewContext, TeachingExperience);

        public static string TimeAvailabilityNavClass(ViewContext viewContext) => PageNavClass(viewContext, TimeAvailability);

        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

        public static string ExternalLoginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ExternalLogins);

        public static string PersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, PersonalData);

        //public static string TwoFactorAuthenticationNavClass(ViewContext viewContext) => PageNavClass(viewContext, TwoFactorAuthentication);

        public static string ResumeUploadNavClass(ViewContext viewContext) => PageNavClass(viewContext, ResumeUpload);

        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}