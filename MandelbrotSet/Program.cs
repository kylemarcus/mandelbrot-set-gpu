using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace MandelbrotSet
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<Program>>();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            logger.LogInformation("MandelbrotSet http://warp.povusers.org/Mandelbrot/");

            IFractal mandelbrotSet = serviceProvider.GetService<MandelbrotSet>();
            mandelbrotSet.InitFrame(1000, 1000, 30);
            mandelbrotSet.RenderFrame(-1.3, 0.02012, 1.7E-4);
            //mandelbrotSet.RenderFrame(-2.0, 1.0, -1.2);
            mandelbrotSet.SaveFrame($"mandelbrotSet.png");

            stopWatch.Stop();
            logger.LogInformation($"Frame generation took {stopWatch.ElapsedMilliseconds / 1000} sec ({stopWatch.ElapsedMilliseconds} ms)");
            Console.ReadLine();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IFractal, MandelbrotSet>();
            services.AddScoped<IImageStore, ImageSharpImageStore>();
            services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .AddTransient<MandelbrotSet>();
        }
    }
}
