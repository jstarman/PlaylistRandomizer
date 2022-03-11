using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PlaylistRandomizer;
using PlaylistRandomizer.Spotify;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console()
        .ReadFrom.Configuration(ctx.Configuration));

    builder.Services
      .Configure<SpotifyAuthorizeConfig>(builder.Configuration.GetSection(typeof(SpotifyAuthorizeConfig).Name))
      .AddHttpClient()
      .AddTransient<SpotifyTokenConfig>()
      .AddTransient<IWebApi, WebApi>()
      .AddSingleton<PlaylistManager>()
      .AddControllers();

    var app = builder.Build();
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
