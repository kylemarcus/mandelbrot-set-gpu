using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Diagnostics;
using NLog;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace MandelbrotSet
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopWatch = null;
            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                var servicesProvider = BuildDi();
                using (servicesProvider as IDisposable)
                {
                    stopWatch = new Stopwatch();
                    stopWatch.Start();

                    var runner = servicesProvider.GetRequiredService<MandelbrotSetRunner>();

                    runner.RenderVideo(new FractalImageConfig
                    {
                        Width = 1920,
                        Height = 1080,
                        MaxItr = 60,
                        Name = "out\\ms.png",
                        MinRe = -0.758703023456444008810,
                        MaxRe = -0.757731022039652920710,
                        MinIm = 0.076033921475166087556
                    }, new FractalVideoConfig
                    {
                        Frames = 600,
                        ResChange = 0.001
                    });

                    Console.WriteLine("Press ANY key to exit");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                stopWatch?.Stop();
                logger.Info($"Program took {stopWatch?.ElapsedMilliseconds / 1000} sec ({stopWatch?.ElapsedMilliseconds} ms)");

                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        private static IServiceProvider BuildDi()
        {
            var services = new ServiceCollection();

            services.AddScoped<IFractal, MandelbrotSet>();
            services.AddScoped<IImageStore, ImageSharpImageStore>();

            // Runner is the custom class
            services.AddTransient<MandelbrotSetRunner>();

            // configure Logging with NLog
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
            });

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
