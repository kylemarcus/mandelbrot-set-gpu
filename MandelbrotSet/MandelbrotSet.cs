using Microsoft.Extensions.Logging;
using System;

namespace MandelbrotSet
{
    class MandelbrotSet : IFractal
    {
        private int _frameHeight;
        private int _frameWidth;
        private int _maxIterations;
        private readonly IImageStore _imageStore;
        private readonly ILogger _logger;

        public MandelbrotSet(ILogger<MandelbrotSet> logger, IImageStore imageStore)
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
            double maxIm = minIm + (maxRe - minRe) * _frameHeight / _frameWidth;
            _logger.LogDebug($"Rendering frame minRe={minRe} maxRe={maxRe} minIm={minIm} maxIm={maxIm}");

            double Re_factor = (maxRe - minRe) / (_frameWidth - 1);
            double Im_factor = (maxIm - minIm) / (_frameHeight - 1);
            
            for (int y = 0; y < _frameHeight; y++)
            {
                double c_im = maxIm - y * Im_factor;

                for (int x = 0; x < _frameWidth; x++)
                {
                    double c_re = minRe + x * Re_factor;

                    // Set Z = c
                    double Z_re = c_re, Z_im = c_im;

                    bool isInside = true;
                    int ittr = 0;
                    for (uint n = 0; n < _maxIterations; n++)
                    {
                        // value of Z > 2 then not inside set
                        if (Z_re * Z_re + Z_im * Z_im > 4)
                        {
                            ittr = (int)n;
                            isInside = false;
                            break;
                        }

                        // Z = Z2 + c
                        double Z_im2 = Z_im * Z_im;
                        Z_im = 2 * Z_re * Z_im + c_im;
                        Z_re = Z_re * Z_re - Z_im2 + c_re;
                    }
                    if (isInside) {
                        PutBlackPixel(x, y);
                    } else {
                        PutColorPixel(x, y, ittr);
                    }
                }
            }
        }

        public void SaveFrame(string name)
        {
            _logger.LogDebug($"Saving image {name}");
            _imageStore.SaveImage(name);
        }

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
                    R = (byte) ((n * 255) / end), //255 red, 0 black
                    G = 0,
                    B = 0,
                    A = 255
                });
            }
        }

    }
}
