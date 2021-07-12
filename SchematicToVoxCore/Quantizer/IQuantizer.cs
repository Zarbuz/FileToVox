using System.Drawing;

namespace FileToVox.Quantizer
{
    public interface IQuantizer
    {
        Bitmap QuantizeImage(Bitmap image, int alphaThreshold, int alphaFader, int macColorCount);
    }
}
