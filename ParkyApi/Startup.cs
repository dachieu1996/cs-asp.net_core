using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParkyApi.Data;
using ParkyApi.Models.Repository;
using ParkyApi.Models.Repository.IRepository;
using ParkyApi.ParkyMapper;

namespace ParkyApi
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
            services.AddDbContext<ApplicationDbContext>
                (options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<INationalParkRepository, NationalParkRepository>();
            services.AddScoped<ITrailRepository, TrailRepository>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("ParkyOpenApiSpecNP",
                    new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        Title = "Parky Api (National Park)",
                        Version = "1",
                        Description = "National Parks API"
                    });

                options.SwaggerDoc("ParkyOpenApiSpecTrails",
                    new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        Title = "Parky Api (Trails)",
                        Version = "1",
                        Description = "Trails API"
                    });
            });

            services.AddAutoMapper(typeof(ParkyMappings));
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/ParkyOpenApiSpecNP/swagger.json", "National Parks");
                options.SwaggerEndpoint("/swagger/ParkyOpenApiSpecTrails/swagger.json", "Trails");
                options.RoutePrefix = "";
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
