using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;

namespace MandelbrotSet
{
    public class MandelbrotSetRunner : IFractalRunner
    {
        private readonly ILogger<MandelbrotSetRunner> _logger;
        private readonly IFractal _fractal;
        private static readonly string IMAGE_OUT_DIR = "out";

        public MandelbrotSetRunner(ILogger<MandelbrotSetRunner> logger, IFractal fractal)
        {
            _logger = logger;
            _fractal = fractal;

            _logger.LogInformation("MandelbrotSet http://warp.povusers.org/Mandelbrot/");
        }

        public void RenderImage(FractalImageConfig fractalImageConfig)
        {
            RenderSingleFrame(_fractal, fractalImageConfig);
        }

        public void RenderVideo(FractalImageConfig fractalImageConfig, FractalVideoConfig fractalVideoConfig)
        {
            Directory.CreateDirectory(IMAGE_OUT_DIR);

            LoopFrames(_fractal, fractalImageConfig, fractalVideoConfig);

            FfmpegRenderVideo();

            //Directory.Delete(IMAGE_OUT_DIR, true);
        }

        private void LoopFrames(IFractal fractal, FractalImageConfig fractalImageConfig, FractalVideoConfig fractalVideoConfig)
        {
            var myFractalImageConfig = fractalImageConfig.Clone() as FractalImageConfig;

            var nameSplit = fractalImageConfig.Name.Split('.');
            var prefix = nameSplit[0];
            var ext = nameSplit[1];

            for (int frame = 0; frame < fractalVideoConfig.Frames; frame++)
            {
                myFractalImageConfig.Name = $"{prefix}_{frame}.{ext}";

                RenderSingleFrame(fractal, myFractalImageConfig);

                //myFractalImageConfig.MinRe += fractalVideoConfig.ResChange;
                //myFractalImageConfig.MaxRe -= fractalVideoConfig.ResChange;
                //myFractalImageConfig.MinIm += fractalVideoConfig.ResChange;
                myFractalImageConfig.MaxItr += 1;
            }
        }

        private void RenderSingleFrame(IFractal fractal, FractalImageConfig fractalImageConfig)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            fractal.InitFrame(fractalImageConfig.Width, fractalImageConfig.Height, fractalImageConfig.MaxItr);
            fractal.RenderFrame(fractalImageConfig.MinRe, fractalImageConfig.MaxRe, fractalImageConfig.MinIm);
            fractal.SaveFrame(fractalImageConfig.Name);

            stopWatch.Stop();
            _logger.LogInformation($"Frame generation took {stopWatch.ElapsedMilliseconds / 1000} sec ({stopWatch.ElapsedMilliseconds} ms)");
        }

        private void FfmpegRenderVideo()
        {
            _logger.LogDebug("Rendering video by ffmpeg");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = @"/C C:\ffmpeg\bin\ffmpeg.exe -y -r 30 -i out\ms_%d.png out.mp4",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = Process.Start(processStartInfo);
            var output = process.StandardOutput.ReadToEnd();  // look into this https://forums.techguy.org/threads/solved-c-asynchronous-process-standardoutput-read.741449/
            process.WaitForExit();

            _logger.LogDebug($"output from process: {output}");
        }
    }
}
