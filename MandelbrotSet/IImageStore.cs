namespace MandelbrotSet
{
    interface IImageStore
    {
        void SetImageSize(int w, int h);
        void ClearImage();
        void SetPixel(Pixel p);
        void SaveImage(string name);
    }

    public class Pixel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }
    }
}
