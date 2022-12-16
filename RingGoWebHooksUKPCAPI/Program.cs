using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
 
using System.Configuration;
using ANPRTechOps.RingGoIntegration.Web.Models;
using ANPRTechOps.RingGoIntegration.Web.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ANPRTechOps.RingGoIntegration.Web;
using System.Security.Cryptography.X509Certificates;

public class Program
{
    public static void Main(string[] args)
    {


        var builder = WebApplication.CreateBuilder(args);
        {
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1",
                new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Api",
                    Description = "Demo API for showing Swagger",
                    Version = "v1"
                });
            });
            var settings = builder.Configuration.GetSection("RingGo").Get<RingGoSettings>();
            builder.Services.AddAuthentication(o =>
            {
                o.DefaultScheme = RingGoAuthenticationHandler.AuthenticationSchemeName;
            }).AddScheme<RingGoAuthenticationSchemeOptions, RingGoAuthenticationHandler>(RingGoAuthenticationHandler.AuthenticationSchemeName, options =>
            {
                options.PrivateKey = settings.PrivateKey;
                options.PublicKey = settings.PublicKey;
                options.ValidateBearerHmac = settings.ValidateBearerHmac;
            });

            builder.Services.AddScoped<IRingGoRepository, RingGoRepository>();
            builder.Services.Configure<RingGoSettings>(builder.Configuration.GetSection("RingGo"));

            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("Api/swagger/v1/swagger.json", "v1");
                    options.RoutePrefix = string.Empty;
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            //app.MapControllerRoute(
            //name: "default",
            //pattern: "{controller=home}/{action=Index}/{id?}");

            app.Run();


        }

    }
}






 