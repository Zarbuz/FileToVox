using System.Drawing;

namespace FileToVox.Quantizer
{
    public interface IQuantizer
    {
        Image QuantizeImage(Bitmap image, int alphaThreshold, int alphaFader, int macColorCount);
    }
}
