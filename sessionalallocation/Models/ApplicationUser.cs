using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SessionalAllocation.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser() : base() { }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PreferredName { get; set; }
        public string SwinburneID { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        // this will need to be changed to int
        public string PostalCode { get; set; }

        public string LinkedInProfileUrl { get; set; }
        public string Country { get; set; }
        //public string Department { get; set; }
        public string YoutubeUrl { get; set; }
        //Level of qualifications

        // Fall under you highest qualification
        [Display(Name = "Highest Qualification")]
        public HighestQualificationType Qualification { get; set; }
        [Display(Name = "Qualification Name")]
        public string QualificationName { get; set; }
        public string QulificationCompletionYear { get; set; }
        //
        // Fall under you current studying qualification
        public string AreYouStudying { get; set; }
        public string CurrentStudyingQualification { get; set; }
        public CurrentStudyingQualType CurrentQualificationType { get; set; }
        public string CitizenshipStudyStatus { get; set; }
        public bool StudyingAtSwinburne { get; set; }
        // Teaching Experience Section 
        public string NumberYearsWorkExperience { get; set; }
        public string PreviousTeachingExperience { get; set; }
        public string Publications { get; set; }
        public string TutorTraining { get; set; }
        public string OtherTraining { get; set; }
        public string CanvasTraining { get; set; }
        public string FSETSessionalInduction { get; set; }
        public string SwinburneSessionalInduction { get; set; }
        public string AustralianWorkRights { get; set; }
        public string WorkRights { get; set; }
        public string VisaType { get; set; }
        public string VisaNumber { get; set; }
        public bool UserFullyRegistered { get; set; }
        public string ResumeFileName { get; set; }
        public byte[] ResumeContent { get; set; }
        public virtual ICollection<Applications> Applications { get; set; }
        public virtual ICollection<Class> Class { get; set; }
        public virtual ICollection<Unit> Unit { get; set; }
        [Display(Name = "Roles")]
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
        public virtual ICollection<Department> Department { get; set; }

        [Display(Name = "Name")]
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        // get all role names and return as a string
        public string UserRolesString
        {
            get
            {
                List<string> userRoles = new List<string>();
                foreach (var role in UserRoles)
                {
                    userRoles.Add(role.Role.Name);
                }
                return string.Join(", ", userRoles);
            }
        }
    }
    /// <summary>
    /// Enum List of Qualification Types for Applicants.
    /// </summary>
    public enum HighestQualificationType
    {
        //Always ensure No Qualification is the FIRST option.
        [Display(Name = "No Qualification")]
        NoQualification,
        [Display(Name = "High School")]
        HighSchool,
        [Display(Name = "Diploma")]
        Diploma,
        [Display(Name = "Bachelor")]
        Bachelor,
        [Display(Name = "Masters")]
        Masters,
        [Display(Name = "PhD")]
        PhD
    }
    /// <summary>
    /// Enum List of Qual Types if they are currently studying.
    /// </summary>
    public enum CurrentStudyingQualType
    {
        [Display(Name = "Diploma")]
        Diploma,
        [Display(Name = "Bachelor")]
        Bachelor,
        [Display(Name = "Masters")]
        Masters,
        [Display(Name = "PhD")]
        PhD
    }
}
