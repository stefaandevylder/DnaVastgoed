using DnaVastgoed.Data;
using DnaVastgoed.Data.Repositories;
using DnaVastgoed.Managers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace DnaVastgoed {

    public class Startup {

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers();
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DnaVastgoed", Version = "v1" });
            });

            ApplicationDbContext applicationDbContext = new ApplicationDbContext();
            applicationDbContext.Database.EnsureCreated();

            services.AddScoped(x => new PropertyRepository(applicationDbContext));
            services.AddScoped(x => new SubscriberRepository(applicationDbContext));

            services.AddScoped(x => new PostmarkManager(Configuration["Postmark"]));
            services.AddScoped(x => new PostalCodeManager());
            services.AddScoped(x => new CoordinatesManager());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            app.UseForwardedHeaders(new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DnaVastgoed v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }

    }
}
