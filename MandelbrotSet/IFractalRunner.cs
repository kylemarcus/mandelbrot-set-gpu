namespace MandelbrotSet
{
    public interface IFractalRunner
    {
        void RenderVideo(FractalImageConfig ic, FractalVideoConfig fc);
        void RenderImage(FractalImageConfig c);
    }
}
