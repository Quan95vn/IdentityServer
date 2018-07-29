using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialSecurity.Auth.Configuration;
using SocialSecurity.Data;
using SocialSecurity.Domain.Configuration;
using SocialSecurity.Domain.Entities;
using System.IO;
using System.Reflection;

namespace SocialSecurity.Auth
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        private readonly int _sslPort = 443;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _env = env;

            if (env.IsDevelopment())
            {
                var launchConfiguration = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile(Path.Combine("Properties", "launchSettings.json"))
                    .Build();
                // During development we won't be using port 443
                _sslPort = launchConfiguration.GetValue<int>("iisSettings::iisExpress:sslPort");
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var domainSettings = new DomainSettings();
            Configuration.GetSection(nameof(DomainSettings)).Bind(domainSettings);
            services.Configure<DomainSettings>(options => Configuration.GetSection(nameof(DomainSettings)).Bind(options));

            var appSettings = new AppSettings();
            Configuration.GetSection(nameof(AppSettings)).Bind(appSettings);

            var connectionString = appSettings.ConnectionStrings.AuthContext;
            var migrationsAssembly = typeof(DataModule).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<IdentityContext>(o => o.UseSqlServer(connectionString,
              optionsBuilder =>
                  optionsBuilder.MigrationsAssembly(migrationsAssembly)));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
              .AddEntityFrameworkStores<IdentityContext>()
              .AddDefaultTokenProviders();

            services.AddMvc(options =>
            {
                //options.Filters.Add(new RequireHttpsAttribute());
                options.SslPort = _sslPort;
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddIdentityServer(options =>
            {
                options.UserInteraction.LoginUrl = "/Account/login";
                options.UserInteraction.LogoutUrl = "/Account/logout";
            })
            // Replace with your certificate's thumbPrint, path, and password
            .AddInMemoryApiResources(Domain.Authentication.Resources.GetApis(domainSettings.Api))
            .AddInMemoryIdentityResources(Domain.Authentication.Resources.GetIdentityResources())
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder =>
                    builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));

                // this enables automatic token cleanup. this is optional.
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 30; // interval in seconds
            })
            .AddAspNetIdentity<ApplicationUser>()
            ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseMvc();
        }
    }
}