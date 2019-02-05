using System;

namespace MandelbrotSet
{
    public class FractalImageConfig : ICloneable
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
}
