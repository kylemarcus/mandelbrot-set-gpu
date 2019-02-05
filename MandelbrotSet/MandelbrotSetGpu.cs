using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using Microsoft.Extensions.Logging;

namespace MandelbrotSet
{
    class MandelbrotSetGpu : IFractal
    {
        private int _frameHeight;
        private int _frameWidth;
        private int _maxIterations;
        private readonly IImageStore _imageStore;
        private readonly ILogger _logger;

        public MandelbrotSetGpu(ILogger<MandelbrotSetGpu> logger, IImageStore imageStore)
        {
            _logger = logger;
            _imageStore = imageStore;
        }

        public void InitFrame(int width, int height, int maxItr)
        {
            _logger.LogDebug($"Initializing frame size {width}x{height} with {maxItr} max itr");

            _frameWidth = width;
            _frameHeight = height;
            _maxIterations = maxItr;

            _imageStore.SetImageSize(_frameWidth, _frameHeight);
            _imageStore.ClearImage();
        }

        public void RenderFrame(double minRe, double maxRe, double minIm)
        {
            int[] data = new int[_frameWidth * _frameHeight];
            CompileKernel(true);
            CalcGPU(data, _frameWidth, _frameHeight, _maxIterations); // ILGPU-GPU-Mode

            for (int row = 0; row < _frameWidth; row++)
            {
                for (int col = 0; col < _frameHeight; col++)
                {
                    if (data[row * _frameWidth + col] == _maxIterations)
                    {
                        PutBlackPixel(row, col);
                    }
                    else
                    {
                        PutColorPixel(row, col, data[row * _frameWidth + col]);
                    }
                }
            }
        }

        public void SaveFrame(string name)
        {
            _logger.LogDebug($"Saving image {name}");
            _imageStore.SaveImage(name);
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -









        private void PutBlackPixel(int x, int y)
        {
            _imageStore.SetPixel(new Pixel
            {
                X = x,
                Y = y,
                R = 0,
                G = 0,
                B = 0,
                A = 255
            });
        }

        // black -> red -> white
        private void PutColorPixel(int x, int y, int n)
        {
            if (n > _maxIterations / 2 - 1)
            {
                // MaxIterations/2 to MaxIterations-1
                // color goes from red to white

                var range = (_maxIterations - 1) - (_maxIterations / 2);
                var start = n - range;

                _imageStore.SetPixel(new Pixel
                {
                    X = x,
                    Y = y,
                    R = 255,
                    G = (byte)((start * 255) / range),
                    B = (byte)((start * 255) / range),
                    A = 255
                });
            }
            else
            {
                // 0 to MaxIterations/2-1
                // color goes from black to red

                var end = _maxIterations / 2 - 1;

                _imageStore.SetPixel(new Pixel
                {
                    X = x,
                    Y = y,
                    R = (byte)((n * 255) / end), //255 red, 0 black
                    G = 0,
                    B = 0,
                    A = 255
                });
            }
        }





        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        /// <summary>
        /// ILGPU kernel for Mandelbrot set.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="max_iterations"></param>
        /// <param name="output"></param>
        static void MandelbrotKernel(
            Index index,
            int width, int height, int max_iterations,
            ArrayView<int> output)
        {
            float h_a = -2.0f;
            float h_b = 1.0f;
            float v_a = -1.0f;
            float v_b = 1.0f;

            if (index >= output.Length)
                return;

            int img_x = index % width;
            int img_y = index / width;

            float x0 = h_a + img_x * (h_b - h_a) / width;
            float y0 = v_a + img_y * (v_b - v_a) / height;
            float x = 0.0f;
            float y = 0.0f;
            int iteration = 0;
            while ((x * x + y * y < 2 * 2) && (iteration < max_iterations))
            {
                float xtemp = x * x - y * y + x0;
                y = 2 * x * y + y0;
                x = xtemp;
                iteration = iteration + 1;
            }
            output[index] = iteration;
        }

        private static Context context;
        private static Accelerator accelerator;
        private static System.Action<Index, int, int, int, ArrayView<int>> mandelbrot_kernel;

        /// <summary>
        /// Compile the mandelbrot kernel in ILGPU-CPU or ILGPU-CUDA mode.
        /// </summary>
        /// <param name="withCUDA"></param>
        public static void CompileKernel(bool withCUDA)
        {
            context = new Context();
            if (withCUDA)
                accelerator = new CudaAccelerator(context);
            else
                accelerator = new CPUAccelerator(context);

            mandelbrot_kernel = accelerator.LoadAutoGroupedStreamKernel<
                Index, int, int, int, ArrayView<int>>(MandelbrotKernel);
        }

        /// <summary>
        /// Dispose lightning-context and cuda-context.
        /// </summary>
        public static void Dispose()
        {
            accelerator.Dispose();
            context.Dispose();
        }

        /// <summary>
        /// Calculate the mandelbrot set on the GPU.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="max_iterations"></param>
        public static void CalcGPU(int[] buffer, int width, int height, int max_iterations)
        {
            int num_values = buffer.Length;
            var dev_out = accelerator.Allocate<int>(num_values);

            // Launch kernel
            mandelbrot_kernel(num_values, width, height, max_iterations, dev_out.View);
            accelerator.Synchronize();
            dev_out.CopyTo(buffer, 0, 0, num_values);

            dev_out.Dispose();
            return;
        }
    }
}
