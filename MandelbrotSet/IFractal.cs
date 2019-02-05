namespace MandelbrotSet
{
    public interface IFractal
    {
        void InitFrame(int width, int height, int maxItr);
        void RenderFrame(double minRe, double maxRe, double minIm);
        void SaveFrame(string name);
    }
}
