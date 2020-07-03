using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace SpriterDemo
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        [STAThread]
        public static void Main()
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            IServiceCollection services = new ServiceCollection();
            services.AddOptions();
            services.Configure<SpriterDemoOptions>((Configuration.GetSection(SpriterDemoOptions.SpriterDemo)));
            services.AddSingleton<SpriteDemoGame>();

            var provider = services.BuildServiceProvider();

            using (var game = provider.GetService<SpriteDemoGame>())
                game.Run();
        }
    }
}
