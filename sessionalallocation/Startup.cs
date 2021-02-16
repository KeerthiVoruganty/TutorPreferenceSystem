using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SessionalAllocation.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SessionalAllocation.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SessionalAllocation
{
    public class AspNetUsersConfiguration
    {
        public AspNetUsersConfiguration(EntityTypeBuilder<ApplicationUser> entity)
        {
            entity.ToTable("AspNetUsers", "dbo");
            entity.HasKey(e => e.Id);
        }
    }
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // PART 1 to redirect user to login page on the site when they go to it.

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            String config = "Server=DESKTOP-JMR586K\\SCCM;Database=model;Trusted_Connection=True; ";
            Console.WriteLine(config);
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(config));
            services.AddIdentity<ApplicationUser, ApplicationRole>(
                options => options.Stores.MaxLengthForKeys = 128)
                .AddRoleManager<RoleManager<ApplicationRole>>()
                .AddDefaultTokenProviders()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton<IEmailSender, EmailSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseDefaultFiles();
            app.UseCookiePolicy();
            app.UseStaticFiles();
            app.UseAuthentication();
            // PART 2: Default Map route
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
            //Initialise database, and apply any migrations.
            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();

            //Create Super User and User Roles.
            CreateUserRoles(services).Wait();
            CreateSuperUser(services).Wait();
            //Initialise Payrates for begininning file.
            InitialisePayrates(services).Wait();

            //TODO: Demo User Data and Faculty. Remove after demonstration.
            CreateDemonstrationUsers(services).Wait();
            CreateAllFacultyData(services).Wait();
        }



        /*
         Service methods

            Used for creating users roles. Also used for demonstration test data.
        */

        private async Task CreateAllFacultyData(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Verify if database is created.
                context.Database.EnsureCreated();

                //Create Demonstration users.

                //Fetch all users
                var allUsers = context.Users.ToList<ApplicationUser>();

               /* if (!context.Faculty.Any())
                {
                    Faculty faculty = new Faculty { Name = "Faculty of Science, Engineering and Technology" };
                    context.Faculty.Add(faculty);
                    context.SaveChanges();
                }

                // If no schools, seed Schools.
                if (!context.School.Any())
                {
                    var facultys = context.Faculty.ToList<Faculty>();
                    var schools = new School[]
                    {
                        new School{Name = "School of Software and Electrical Engineering", Faculty = facultys.Find(x => x.Name.Equals("Faculty of Science, Engineering and Technology")).Id},
                        new School{Name = "School of Engineering", Faculty = facultys.Find(x => x.Name.Equals("Faculty of Science, Engineering and Technology")).Id},
                    };
                    foreach (School s in schools)
                    {
                        context.School.Add(s);
                    }
                    context.SaveChanges();
                }

                // If no departments, seed Departments.
                if (!context.Department.Any())
                {
                    // get all schools so we can use their Id
                    var schools = context.School.ToList<School>();
                    
                    var departments = new Department[]
                    {
                        new Department{Name = "Department of Computer Science and Software Engineering", School = schools.Find(x => x.Name.Equals("School of Software and Electrical Engineering")).Id, DepartmentOwnerNavigation = allUsers.Find(x => x.UserName.Equals("micah@swin.edu.au"))},
                        new Department{Name = "Department of Civil and Construction Engineering", School = schools.Find(x => x.Name.Equals("School of Engineering")).Id, DepartmentOwnerNavigation = allUsers.Find(x => x.UserName.Equals("micah@swin.edu.au"))},
                        new Department{Name = "Department of Telecommunications, Electrical, Robotics & Biomedical Engineering", School = schools.Find(x => x.Name.Equals("School of Software and Electrical Engineering")).Id, DepartmentOwnerNavigation = allUsers.Find(x => x.UserName.Equals("micah@swin.edu.au"))}
                    };
                    foreach (Department d in departments)
                    {
                        context.Department.Add(d);
                    }
                    context.SaveChanges();
                }*/

                //If no Units, seed Units.
                if (!context.Unit.Any())
                {
                    // get all departments so we can use their Id
                    var departments = context.Department.ToList<Department>();
                    var units = new Unit[]
                    {
                        new Unit{UnitCode="COS10003", UnitName="Computer Logic and Essentials", Department = departments.Find(x => x.Name.Equals("Department of Computer Science and Software Engineering")).Id, UnitOwnerNavigation = allUsers.Find(x => x.UserName.Equals("james@swin.edu.au"))},
                        new Unit{UnitCode = "CSM20001", UnitName="Design of Constructed Structures", Department = departments.Find(x => x.Name.Equals("Department of Civil and Construction Engineering")).Id, UnitOwnerNavigation = allUsers.Find(x => x.UserName.Equals("james@swin.edu.au"))},
                        new Unit{UnitCode="TNE40003", UnitName="Wireless Communications", Department = departments.Find(x => x.Name.Equals("Department of Telecommunications, Electrical, Robotics & Biomedical Engineering")).Id, UnitOwnerNavigation = allUsers.Find(x => x.UserName.Equals("james@swin.edu.au"))},
                        // Seeding for Demo to RHYS starts
                        new Unit{UnitCode="COS10004", UnitName="Computer Systems", Department = departments.Find(x => x.Name.Equals("Department of Computer Science and Software Engineering")).Id, UnitOwnerNavigation = allUsers.Find(x => x.UserName.Equals("peter@swin.edu.au"))},
                        new Unit{UnitCode="COS10005", UnitName="Web Development", Department = departments.Find(x => x.Name.Equals("Department of Computer Science and Software Engineering")).Id, UnitOwnerNavigation = allUsers.Find(x => x.UserName.Equals("jude@swin.edu.au"))},
                        new Unit{UnitCode="COS10009", UnitName="Introduction to Programming", Department = departments.Find(x => x.Name.Equals("Department of Computer Science and Software Engineering")).Id, UnitOwnerNavigation = allUsers.Find(x => x.UserName.Equals("jonah@swin.edu.au"))},
                        new Unit{UnitCode="COS10011", UnitName="Internet Technologies", Department = departments.Find(x => x.Name.Equals("Department of Computer Science and Software Engineering")).Id, UnitOwnerNavigation = allUsers.Find(x => x.UserName.Equals("amos@swin.edu.au"))},
                        new Unit{UnitCode="COS20001", UnitName="Usability", Department = departments.Find(x => x.Name.Equals("Department of Computer Science and Software Engineering")).Id, UnitOwnerNavigation = allUsers.Find(x => x.UserName.Equals("joel@swin.edu.au"))}
                        // Seeding for Demo to RHYS ends
                    };

                    foreach (Unit u in units)
                    {
                        context.Unit.Add(u);
                    }
                    context.SaveChanges();
                }

                if (!context.Class.Any())
                {
                    //get all units so we can user their Id
                    var units = context.Unit.ToList<Unit>();
                    var classes = new Class[]
                    {

                        //new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10003")).Id, ClassType="Lecture", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Monday", StartDate = DateTime.Parse("2019-01-01"), StartTimeScheduled=TimeSpan.Parse("11:00:00"), EndTimeScheduled=TimeSpan.Parse("13:00:00")},
                        //new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10003")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Monday", StartDate = DateTime.Parse("2019-01-01"), StartTimeScheduled=TimeSpan.Parse("11:00:00"), EndTimeScheduled=TimeSpan.Parse("13:00:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("CSM20001")).Id, ClassType="Lecture", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Monday", StartDate = DateTime.Parse("2019-01-01"), StartTimeScheduled=TimeSpan.Parse("11:00:00"), EndTimeScheduled=TimeSpan.Parse("13:00:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("CSM20001")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Monday", StartDate = DateTime.Parse("2019-01-01"), StartTimeScheduled=TimeSpan.Parse("11:00:00"), EndTimeScheduled=TimeSpan.Parse("13:00:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("TNE40003")).Id, ClassType="Lecture", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Monday", StartDate = DateTime.Parse("2019-01-01"), StartTimeScheduled=TimeSpan.Parse("11:00:00"), EndTimeScheduled=TimeSpan.Parse("13:00:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("TNE40003")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Monday", StartDate = DateTime.Parse("2019-01-01"), StartTimeScheduled=TimeSpan.Parse("11:00:00"), EndTimeScheduled=TimeSpan.Parse("13:00:00")},
                        // Seeding for Demo to RHYS starts
                        //
                        // COS10003
                        //
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10003")).Id, ClassType="Lecture", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("12:30:00"), EndTimeScheduled=TimeSpan.Parse("14:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10003")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Friday", StartDate = DateTime.Parse("2019-08-09"), StartTimeScheduled=TimeSpan.Parse("10:30:00"), EndTimeScheduled=TimeSpan.Parse("13:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10003")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Friday", StartDate = DateTime.Parse("2019-08-09"), StartTimeScheduled=TimeSpan.Parse("14:30:00"), EndTimeScheduled=TimeSpan.Parse("16:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10003")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Friday", StartDate = DateTime.Parse("2019-08-09"), StartTimeScheduled=TimeSpan.Parse("18:30:00"), EndTimeScheduled=TimeSpan.Parse("10:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10003")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Friday", StartDate = DateTime.Parse("2019-08-09"), StartTimeScheduled=TimeSpan.Parse("12:30:00"), EndTimeScheduled=TimeSpan.Parse("14:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10003")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("16:30:00"), EndTimeScheduled=TimeSpan.Parse("18:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10003")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("14:30:00"), EndTimeScheduled=TimeSpan.Parse("16:30:00")},
                        //
                        // COS10004
                        //
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10004")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("12:30:00"), EndTimeScheduled=TimeSpan.Parse("14:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10004")).Id, ClassType="Lecture", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Tuesday", StartDate = DateTime.Parse("2019-08-06"), StartTimeScheduled=TimeSpan.Parse("08:30:00"), EndTimeScheduled=TimeSpan.Parse("10:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10004")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("10:30:00"), EndTimeScheduled=TimeSpan.Parse("12:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10004")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("08:30:00"), EndTimeScheduled=TimeSpan.Parse("10:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10004")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("12:30:00"), EndTimeScheduled=TimeSpan.Parse("14:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10004")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("10:30:00"), EndTimeScheduled=TimeSpan.Parse("12:03:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10004")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Tuesday", StartDate = DateTime.Parse("2019-08-06"), StartTimeScheduled=TimeSpan.Parse("10:30:00"), EndTimeScheduled=TimeSpan.Parse("12:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10004")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("08:30:00"), EndTimeScheduled=TimeSpan.Parse("10:30:00")},
                        //
                        // COS10005
                        //
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10005")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Friday", StartDate = DateTime.Parse("2019-08-09"), StartTimeScheduled=TimeSpan.Parse("13:30:00"), EndTimeScheduled=TimeSpan.Parse("14:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10005")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Friday", StartDate = DateTime.Parse("2019-08-09"), StartTimeScheduled=TimeSpan.Parse("12:30:00"), EndTimeScheduled=TimeSpan.Parse("13:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10005")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("13:30:00"), EndTimeScheduled=TimeSpan.Parse("14:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10005")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("12:30:00"), EndTimeScheduled=TimeSpan.Parse("13:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10005")).Id, ClassType="Lecture", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("10:30:00"), EndTimeScheduled=TimeSpan.Parse("12:30:00")},
                        //
                        // COS10009
                        //
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("14:30:00"), EndTimeScheduled=TimeSpan.Parse("16:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("08:30:00"), EndTimeScheduled=TimeSpan.Parse("10:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("12:30:00"), EndTimeScheduled=TimeSpan.Parse("14:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("14:30:00"), EndTimeScheduled=TimeSpan.Parse("16:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Lecture", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Tuesday", StartDate = DateTime.Parse("2019-08-06"), StartTimeScheduled=TimeSpan.Parse("12:30:00"), EndTimeScheduled=TimeSpan.Parse("14:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Friday", StartDate = DateTime.Parse("2019-08-09"), StartTimeScheduled=TimeSpan.Parse("08:30:00"), EndTimeScheduled=TimeSpan.Parse("10:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Tuesday", StartDate = DateTime.Parse("2019-08-06"), StartTimeScheduled=TimeSpan.Parse("14:30:00"), EndTimeScheduled=TimeSpan.Parse("16:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("08:30:00"), EndTimeScheduled=TimeSpan.Parse("10:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("16:30:00"), EndTimeScheduled=TimeSpan.Parse("18:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("10:30:00"), EndTimeScheduled=TimeSpan.Parse("12:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Friday", StartDate = DateTime.Parse("2019-08-09"), StartTimeScheduled=TimeSpan.Parse("12:30:00"), EndTimeScheduled=TimeSpan.Parse("14:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Workshop", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Friday", StartDate = DateTime.Parse("2019-08-04"), StartTimeScheduled=TimeSpan.Parse("14:30:00"), EndTimeScheduled=TimeSpan.Parse("12:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("10:30:00"), EndTimeScheduled=TimeSpan.Parse("16:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("12:30:00"), EndTimeScheduled=TimeSpan.Parse("14:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("16:30:00"), EndTimeScheduled=TimeSpan.Parse("18:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10009")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Friday", StartDate = DateTime.Parse("2019-08-09"), StartTimeScheduled=TimeSpan.Parse("10:30:00"), EndTimeScheduled=TimeSpan.Parse("12:30:00")},
                        //
                        // COS10011
                        //
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10011")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("10:30:00"), EndTimeScheduled=TimeSpan.Parse("12:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10011")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("08:30:00"), EndTimeScheduled=TimeSpan.Parse("10:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10011")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("14:30:00"), EndTimeScheduled=TimeSpan.Parse("16:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10011")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("14:30:00"), EndTimeScheduled=TimeSpan.Parse("16:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10011")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("08:30:00"), EndTimeScheduled=TimeSpan.Parse("10:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10011")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("10:30:00"), EndTimeScheduled=TimeSpan.Parse("12:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10011")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Tuesday", StartDate = DateTime.Parse("2019-08-06"), StartTimeScheduled=TimeSpan.Parse("10:30:00"), EndTimeScheduled=TimeSpan.Parse("12:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10011")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("12:30:00"), EndTimeScheduled=TimeSpan.Parse("14:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10011")).Id, ClassType="Lab", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("12:30:00"), EndTimeScheduled=TimeSpan.Parse("14:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS10011")).Id, ClassType="Lecture", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Tuesday", StartDate = DateTime.Parse("2019-08-06"), StartTimeScheduled=TimeSpan.Parse("08:30:00"), EndTimeScheduled=TimeSpan.Parse("10:30:00")},
                        //
                        // COS20001
                        //
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS20001")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("08:30:00"), EndTimeScheduled=TimeSpan.Parse("10:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS20001")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Friday", StartDate = DateTime.Parse("2019-08-09"), StartTimeScheduled=TimeSpan.Parse("16:30:00"), EndTimeScheduled=TimeSpan.Parse("18:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS20001")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Friday", StartDate = DateTime.Parse("2019-08-09"), StartTimeScheduled=TimeSpan.Parse("08:30:00"), EndTimeScheduled=TimeSpan.Parse("10:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS20001")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("16:30:00"), EndTimeScheduled=TimeSpan.Parse("18:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS20001")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("12:30:00"), EndTimeScheduled=TimeSpan.Parse("14:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS20001")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("14:30:00"), EndTimeScheduled=TimeSpan.Parse("16:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS20001")).Id, ClassType="Lecture", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("08:30:00"), EndTimeScheduled=TimeSpan.Parse("10:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS20001")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Friday", StartDate = DateTime.Parse("2019-08-09"), StartTimeScheduled=TimeSpan.Parse("14:30:00"), EndTimeScheduled=TimeSpan.Parse("16:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS20001")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Wednesday", StartDate = DateTime.Parse("2019-08-07"), StartTimeScheduled=TimeSpan.Parse("14:30:00"), EndTimeScheduled=TimeSpan.Parse("16:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS20001")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Thursday", StartDate = DateTime.Parse("2019-08-08"), StartTimeScheduled=TimeSpan.Parse("10:30:00"), EndTimeScheduled=TimeSpan.Parse("12:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS20001")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Friday", StartDate = DateTime.Parse("2019-08-09"), StartTimeScheduled=TimeSpan.Parse("10:30:00"), EndTimeScheduled=TimeSpan.Parse("12:30:00")},
                        new Class{UnitId = units.Find(x => x.UnitCode.Equals("COS20001")).Id, ClassType="Tutorial", Allocated=false, Location="Hawthorn", Year="2019", StudyPeriod="Semester 2", DayOfWeek="Friday", StartDate = DateTime.Parse("2019-08-09"), StartTimeScheduled=TimeSpan.Parse("12:30:00"), EndTimeScheduled=TimeSpan.Parse("14:30:00")}
                        // Seeding for Demo to RHYS ends
                    };

                    foreach (Class c in classes)
                    {
                        context.Class.Add(c);
                    }
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Creates all the different User Roles for the STAS system.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private async Task CreateUserRoles(IServiceProvider serviceProvider)
        {
            // https://social.technet.microsoft.com/wiki/contents/articles/51333.asp-net-core-2-0-getting-started-with-identity-and-role-management.aspx#Step_5_Create_Role_and_Assign_User_for_Role
            // can also create a default user in here to be admin
            var _roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            string applicantRole = "Applicant";
            string applicantDescription = "User has applied for job.";

            string convenorRole = "Convenor";
            string convenorDescription = "Unit Convenor";

            string departmentChairRole = "DepartmentChair";
            string departmentChairDescription = "Department Chair";

            string adminRole = "Admin";
            string adminDescription = "Super user";

            if (!await _roleManager.RoleExistsAsync(applicantRole))
            {
                await _roleManager.CreateAsync(new ApplicationRole(applicantRole, applicantDescription, DateTime.Now));
            }

            if (!await _roleManager.RoleExistsAsync(convenorRole))
            {
                await _roleManager.CreateAsync(new ApplicationRole(convenorRole, convenorDescription, DateTime.Now));
            }

            if (!await _roleManager.RoleExistsAsync(departmentChairRole))
            {
                await _roleManager.CreateAsync(new ApplicationRole(departmentChairRole, departmentChairDescription, DateTime.Now));
            }

            if (!await _roleManager.RoleExistsAsync(adminRole))
            {
                await _roleManager.CreateAsync(new ApplicationRole(adminRole, adminDescription, DateTime.Now));
            }
        }

        /// <summary>
        /// Creates a super-user account with default username and password.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private async Task CreateSuperUser(IServiceProvider serviceProvider)
        {
            var _userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var superUser = new ApplicationUser
            {
                UserName = "admin@admin",
                FirstName = "Admin",
                LastName = "Account"
            };
            var password = "P@ssw0rd";
            var result = _userManager.CreateAsync(superUser, password).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(superUser, "Admin");
            }
        }

        // this code will need to be removed, only used for testing
        private async Task CreateDemonstrationUsers(IServiceProvider serviceProvider)
        {
            var _userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var testApplicant = new ApplicationUser
            {
                UserName = "applicant@test",
                FirstName = "Applicant",
                LastName = "Test"
            };
            var testConvenor = new ApplicationUser
            {
                UserName = "convenor@test",
                FirstName = "Convenor",
                LastName = "Test"
            };
            var testDepartmentChair = new ApplicationUser
            {
                UserName = "departmentchair@test",
                FirstName = "DepartmentChair",
                LastName = "Test"
            };
            var password = "P@ssw0rd";
            
            // Seeding for Demo to RHYS starts
            //
            // 6 Applicants
            //
            
            var seedMatthewApplicant = new ApplicationUser
            {
                UserName = "matthew@applicant",
                FirstName = "Matthew",
                LastName = "Applicant",
                Qualification = HighestQualificationType.PhD
            };
            var seedMarkApplicant = new ApplicationUser
            {
                UserName = "mark@applicant",
                FirstName = "Mark",
                LastName = "Applicant",
                Qualification = HighestQualificationType.PhD
            };
            var seedLukeApplicant = new ApplicationUser
            {
                UserName = "luke@applicant",
                FirstName = "Luke",
                LastName = "Applicant"
            };
            var seedJohnApplicant = new ApplicationUser
            {
                UserName = "john@applicant",
                FirstName = "John",
                LastName = "Applicant"
            };
            var seedTimothyApplicant = new ApplicationUser
            {
                UserName = "timothy@applicant",
                FirstName = "Timothy",
                LastName = "Applicant"
            };
            var seedTitusApplicant = new ApplicationUser
            {
                UserName = "titus@applicant",
                FirstName = "Titus",
                LastName = "Applicant"
            };
            // 6 Convenor
            var seedJamesConvenor = new ApplicationUser
            {
                UserName = "james@swin.edu.au",
                FirstName = "James",
                LastName = "Convenor"
            };
            var seedPeterConvenor = new ApplicationUser
            {
                UserName = "peter@swin.edu.au",
                FirstName = "Peter",
                LastName = "Convenor"
            };
            var seedJudeConvenor = new ApplicationUser
            {
                UserName = "jude@swin.edu.au",
                FirstName = "Jude",
                LastName = "Convenor"
            };
            var seedJonahConvenor = new ApplicationUser
            {
                UserName = "jonah@swin.edu.au",
                FirstName = "Jonah",
                LastName = "Convenor"
            };
            var seedAmosConvenor = new ApplicationUser
            {
                UserName = "amos@swin.edu.au",
                FirstName = "Amos",
                LastName = "Convenor"
            };
            var seedJoelConvenor = new ApplicationUser
            {
                UserName = "joel@swin.edu.au",
                FirstName = "Joel",
                LastName = "Convenor"
            };
            // 1 Department Chair
            var seedMicahDeptChair = new ApplicationUser
            {
                UserName = "micah@swin.edu.au",
                FirstName = "Micah",
                LastName = "DeparmentChair",
            };
            var seedPassword = "P@ssw0rd";
            // Seeding for Demo to RHYS ends            

            var result = _userManager.CreateAsync(testApplicant, password).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(testApplicant, "Applicant");
            }

            result = _userManager.CreateAsync(testConvenor, password).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(testConvenor, "Convenor");
            }

            result = _userManager.CreateAsync(testDepartmentChair, password).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(testDepartmentChair, "DepartmentChair");
            }

            //
            // Seeding for Demo to RHYS starts
            //
            result = _userManager.CreateAsync(seedMatthewApplicant, seedPassword).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(seedMatthewApplicant, "Applicant");
            }
            result = _userManager.CreateAsync(seedMarkApplicant, seedPassword).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(seedMarkApplicant, "Applicant");
            }
            result = _userManager.CreateAsync(seedLukeApplicant, seedPassword).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(seedLukeApplicant, "Applicant");
            }
            result = _userManager.CreateAsync(seedJohnApplicant, seedPassword).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(seedJohnApplicant, "Applicant");
            }
            result = _userManager.CreateAsync(seedTimothyApplicant, seedPassword).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(seedTimothyApplicant, "Applicant");
            }
            result = _userManager.CreateAsync(seedTitusApplicant, seedPassword).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(seedTitusApplicant, "Applicant");
            }
            result = _userManager.CreateAsync(seedJamesConvenor, seedPassword).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(seedJamesConvenor, "Convenor");
            }
            result = _userManager.CreateAsync(seedPeterConvenor, seedPassword).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(seedPeterConvenor, "Convenor");
            }
            result = _userManager.CreateAsync(seedJudeConvenor, seedPassword).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(seedJudeConvenor, "Convenor");
            }
            result = _userManager.CreateAsync(seedAmosConvenor, seedPassword).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(seedAmosConvenor, "Convenor");
            }
            result = _userManager.CreateAsync(seedJoelConvenor, seedPassword).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(seedJoelConvenor, "Convenor");
            }
            result = _userManager.CreateAsync(seedJonahConvenor, seedPassword).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(seedJonahConvenor, "Convenor");
            }
            result = _userManager.CreateAsync(seedMicahDeptChair, seedPassword).Result;
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(seedMicahDeptChair, "DepartmentChair");
            }
            //
            // Seeding for Demo to RHYS ends
            //
        }

        private async Task InitialisePayrates(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Verify if database is created.
                context.Database.EnsureCreated();

                // Payrates as at 1/3/19
                if (!context.Payrate.Any())
                {
                    context.Payrate.AddRange(
                        new Payrate
                        {
                            Name = "Lecturing A: Basic",
                            Code = "LA",
                            Rate = 193.02f
                        },

                        new Payrate
                        {
                            Name = "Lecturing B: Developed",
                            Code = "LB",
                            Rate = 257.36f
                        },

                        new Payrate
                        {
                            Name = "Lecturing C: Specialised",
                            Code = "LC",
                            Rate = 321.70f
                        },

                        new Payrate
                        {
                            Name = "Lecturing D: Repeat",
                            Code = "LD",
                            Rate = 128.68f
                        },

                        new Payrate
                        {
                            Name = "Tutoring E: Normal",
                            Code = "TE",
                            Rate = 138.69f
                        },

                        new Payrate
                        {
                            Name = "Tutoring F: Repeat",
                            Code = "TF",
                            Rate = 92.46f
                        },

                        new Payrate
                        {
                            Name = "Tutoring G: Normal(PhD/CoOrd)",
                            Code = "TG",
                            Rate = 165.16f
                        },

                        new Payrate
                        {
                            Name = "Tutoring H: Repeat(PhD/CoOrd)",
                            Code = "TH",
                            Rate = 110.11f
                        },

                        new Payrate
                        {
                            Name = "Other: Normal",
                            Code = "OS",
                            Rate = 46.23f
                        },

                        new Payrate
                        {
                            Name = "Other: PhD/Co-Ord",
                            Code = "OT",
                            Rate = 55.05f
                        }
                    );
                    context.SaveChanges();
                }
            }
        }
    }
}
