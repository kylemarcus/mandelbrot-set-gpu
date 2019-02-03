using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace MandelbrotSet
{
    class ImageSharpImageStore : IImageStore
    {
        private Image<Rgba32> image;

        public void SetImageSize(int w, int h)
        {
            image = new Image<Rgba32>(w, h);
        }

        public void ClearImage()
        {
            image.Mutate(ctx => ctx.BackgroundColor(Rgba32.White));
        }

        public void SaveImage(string name)
        {
            image.Save(name);
        }

        public void SetPixel(Pixel p)
        {
            image[p.X, p.Y] = new Rgba32(p.R, p.G, p.B, p.A);
        }
    }
}
