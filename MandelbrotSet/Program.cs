using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace MandelbrotSet
{
    class Program
    {
        private static ILogger myLogger;
        private static readonly string IMAGE_OUT_DIR = "out";

        static void Main(string[] args)
        {

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            myLogger = serviceProvider.GetService<ILogger<Program>>();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            myLogger.LogInformation("MandelbrotSet http://warp.povusers.org/Mandelbrot/");

            IFractal mandelbrotSet = serviceProvider.GetService<MandelbrotSet>();

            Directory.CreateDirectory(IMAGE_OUT_DIR);

            var fc = new FractalConfig
            {
                Width = 3840,
                Height = 2160,
                MaxItr = 30,
                Name = "out\\ms.png",
                MinRe = -2.0,
                MaxRe = 1.0,
                MinIm = -1.2
            };

            var lc = new LoopConfig
            {
                Frames = 2,
                ResChange = 0.001
            };

            LoopFrames(mandelbrotSet, fc, lc);

            /*RenderSingleFrame(mandelbrotSet, new FractalConfig
            {
                Width = 1000, Height = 1000, MaxItr = 30, Name = "ms.png",
                MinRe = -1.3, MaxRe = 0.02012, MinIm = 1.7E-4
                //minRe = -2.0, maxRe = 1.0, minIm = -1.2
            });*/

            RenderVideo();

            Directory.Delete(IMAGE_OUT_DIR, true);

            stopWatch.Stop();
            myLogger.LogInformation($"Program took {stopWatch.ElapsedMilliseconds / 1000} sec ({stopWatch.ElapsedMilliseconds} ms)");
            Console.ReadLine();

            NLog.LogManager.Shutdown();
        }

        private static void RenderVideo()
        {
            myLogger.LogDebug("Rendering video");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = @"/C C:\ffmpeg\bin\ffmpeg.exe -y -r 30 -i out\ms_%d.png out.mp4",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = Process.Start(processStartInfo);
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            myLogger.LogDebug($"output from process: {output}");

            //string strCmdText = "/C C:\ffmpeg\bin\ffmpeg.exe -r 30 -i out\\ms_%d.png out.avi";
            //Process.Start("CMD.exe", strCmdText).WaitForExit();
        }

        private static void RenderSingleFrame(IFractal fractal, FractalConfig config)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            fractal.InitFrame(config.Width, config.Height, config.MaxItr);
            fractal.RenderFrame(config.MinRe, config.MaxRe, config.MinIm);
            fractal.SaveFrame(config.Name);

            stopWatch.Stop();
            myLogger.LogInformation($"Frame generation took {stopWatch.ElapsedMilliseconds / 1000} sec ({stopWatch.ElapsedMilliseconds} ms)");
        }

        private static void LoopFrames(IFractal fractal, FractalConfig fractalConfig, LoopConfig loopConfig)
        {
            var myFractalConfig = fractalConfig.Clone() as FractalConfig;

            var nameSplit = fractalConfig.Name.Split('.');
            var prefix = nameSplit[0];
            var ext = nameSplit[1];

            for (int frame = 0; frame < loopConfig.Frames; frame++)
            {    
                myFractalConfig.Name = $"{prefix}_{frame}.{ext}";

                RenderSingleFrame(fractal, myFractalConfig);

                myFractalConfig.MinRe += loopConfig.ResChange;
                myFractalConfig.MaxRe -= loopConfig.ResChange;
                myFractalConfig.MinIm += loopConfig.ResChange;
            }
        }

        private class FractalConfig : ICloneable
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int MaxItr { get; set; }
            public double MinRe { get; set; }
            public double MaxRe { get; set; }
            public double MinIm { get; set; }
            public string Name { get; set; }

            public object Clone()
            {
                return MemberwiseClone();
            }
        }

        private class LoopConfig
        {
            public double ResChange { get; set; }
            public int Frames { get; set; }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IFractal, MandelbrotSet>();
            services.AddScoped<IImageStore, ImageSharpImageStore>();
            services.AddLogging(builder => builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                }))
                .AddTransient<MandelbrotSet>();
        }
    }
}
