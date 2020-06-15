using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlaylistRandomizer.Spotify;
using Serilog;

namespace PlaylistRandomizer
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
            services
                .AddHttpClient()
                .AddSingleton(Configuration.GetSection(typeof(SpotifyAuthorizeConfig).Name).Get<SpotifyAuthorizeConfig>())
                .AddTransient<SpotifyTokenConfig>()
                .AddTransient<IWebApi, WebApi>()
                .AddSingleton<PlaylistManager>()
                .AddSingleton(s =>
                {
                    Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();

                    return Log.Logger;
                })
                .AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
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
