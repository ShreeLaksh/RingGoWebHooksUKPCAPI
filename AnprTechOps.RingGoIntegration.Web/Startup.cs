using ANPRTechOps.RingGoIntegration.Web.Models;
using ANPRTechOps.RingGoIntegration.Web.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//*** Adding namespace for OpenApi***
//using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore

namespace ANPRTechOps.RingGoIntegration.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            //***Swagger dependency added to generate swagger ***
            services.SwaggerDoc(options =>
            {
            options.SwaggerDoc("UKPCAPI", new OpenApiInfo()
                {
                Title = "RingGoAuthentication API",
                version = "v1"
                });
            });
            //

            var settings = Configuration.GetSection("RingGo").Get<RingGoSettings>();
            services.AddAuthentication(o =>
            {
                o.DefaultScheme = RingGoAuthenticationHandler.AuthenticationSchemeName;
            }).AddScheme<RingGoAuthenticationSchemeOptions, RingGoAuthenticationHandler>(RingGoAuthenticationHandler.AuthenticationSchemeName, options =>
            {
                options.PrivateKey = settings.PrivateKey;
                options.PublicKey = settings.PublicKey;
                options.ValidateBearerHmac = settings.ValidateBearerHmac;
            });

            services.AddScoped<IRingGoRepository, RingGoRepository>();
            services.Configure<RingGoSettings>(Configuration.GetSection("RingGo"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            //Swagger dependencies useswagger and useswaggerui added
            //swagger exposes swagger document object as JSON endpoint
            //swaggerUI generates API documentation and allows to test API calls
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
