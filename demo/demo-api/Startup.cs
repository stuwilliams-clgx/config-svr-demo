using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Swashbuckle.AspNetCore.Swagger;

using ConfigServerLibrary;
using demo_api.Helpers;

namespace demo_api
{
    /// <summary>
    /// Startup
    /// </summary>
    public class Startup
    {
        private const string TITLE = "CF Conf Svr Demo";
        private const string VERSION = "v1";
        private const string DESCRIPTION = "A Demo of Config Server";
        private const string MYNAME = "Stuart Williams";
        private const string MYURL = "https://www.youtube.com/user/spookdejur1962";
        private const string MYEMAIL = "stuwilliams@corelogic.com";
        private const string LICENCE = "MIT";
        private const string LICENCEURL = "https://opensource.org/licenses/MIT";

        private const string CorsPolicyName = "AllowAll";

        private readonly string filepath = "/swagger/" + VERSION + "/swagger.json";

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="configuration">IConfiguration</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// IConfiguration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            var filePath = Path.Combine(System.AppContext.BaseDirectory, "demo-api.xml");

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddMvcCore().AddApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(VERSION, new Info { Title = TITLE, Version = VERSION, License = new License() { Name= LICENCE , Url = LICENCEURL }, Contact = new Contact() { Name = MYNAME, Email = MYEMAIL, Url = MYURL } });
                c.IncludeXmlComments(filePath);
                c.DescribeAllEnumsAsStrings();
                c.UseReferencedDefinitionsForEnums();
            });

            services.AddCors(options => {
                options.AddPolicy(CorsPolicyName,
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            ;
                    });
            });

            services.AddScoped<IConfigServerClient, ConfigServerClient>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">IApplicationBuilder</param>
        /// <param name="env">IHostingEnvironment</param>
        /// <param name="logger">ILogger</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<string> logger)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePages(async context =>
                {
                    context.HttpContext.Response.ContentType = "text/plain";
                    await context.HttpContext.Response.WriteAsync("Status code page, status code: " + context.HttpContext.Response.StatusCode);
                });
            }

            app.UseHttpsRedirection();
            app.UseHsts();

            app.ConfigureExceptionHandler(logger);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(filepath, TITLE);
                c.DocumentTitle = DESCRIPTION;
            });

            app.UseCors(CorsPolicyName);

            app.UseMvc();
        }
    }
}
