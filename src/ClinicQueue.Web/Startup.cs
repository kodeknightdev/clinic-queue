using ClinicQueue.Web.Data;
using ClinicQueue.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace ClinicQueue.Web
{
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
            services.AddDbContext<ClinicDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ClinicDb")));

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Staff/Login";
                    options.LogoutPath = "/Staff/Logout";
                    options.Events.OnRedirectToLogin = context =>
                    {
                        // fetch() polling wants a status code, not the sign-in page HTML
                        if (context.Request.Headers["Accept"].ToString().Contains("application/json"))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }

                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    };
                });

            services.Configure<ClinicOptions>(Configuration.GetSection(ClinicOptions.SectionName));

            services.AddSingleton<IClock, LocalClock>();
            services.AddScoped<IQueueService, QueueService>();

            services.AddHealthChecks()
                .AddDbContextCheck<ClinicDbContext>();

            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Staff");
                options.Conventions.AllowAnonymousToPage("/Staff/Login");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
