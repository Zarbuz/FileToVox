using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using nQuant;

namespace FileToVox.Quantizer
{
    public class Quantizer : QuantizerBase, IQuantizer
    {
        protected override QuantizedPalette GetQuantizedPalette(
          int colorCount,
          ColorData data,
          IEnumerable<Box> cubes,
          int alphaThreshold)
        {
            int pixelsCount1 = data.PixelsCount;
            LookupData lookupData = this.BuildLookups(cubes, data);
            IList<int> quantizedPixels = data.QuantizedPixels;
            for (int index = 0; index < pixelsCount1; ++index)
            {
                byte[] bytes = BitConverter.GetBytes(quantizedPixels[index]);
                quantizedPixels[index] = lookupData.Tags[(int)bytes[3], (int)bytes[2], (int)bytes[1], (int)bytes[0]];
            }
            int[] numArray1 = new int[colorCount + 1];
            int[] numArray2 = new int[colorCount + 1];
            int[] numArray3 = new int[colorCount + 1];
            int[] numArray4 = new int[colorCount + 1];
            int[] numArray5 = new int[colorCount + 1];
            QuantizedPalette quantizedPalette = new QuantizedPalette(pixelsCount1);
            IList<Pixel> pixels = data.Pixels;
            int pixelsCount2 = data.PixelsCount;
            IList<Lookup> lookups = lookupData.Lookups;
            int count = lookups.Count;
            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            for (int index1 = 0; index1 < pixelsCount2; ++index1)
            {
                Pixel pixel = pixels[index1];
                quantizedPalette.PixelIndex[index1] = -1;
                if ((int)pixel.Alpha > alphaThreshold)
                {
                    int argb = pixel.Argb;
                    int index2;
                    if (!dictionary.TryGetValue(argb, out index2))
                    {
                        index2 = quantizedPixels[index1];
                        int num1 = int.MaxValue;
                        for (int index3 = 0; index3 < count; ++index3)
                        {
                            Lookup lookup = lookups[index3];
                            int num2 = (int)pixel.Alpha - lookup.Alpha;
                            int num3 = (int)pixel.Red - lookup.Red;
                            int num4 = (int)pixel.Green - lookup.Green;
                            int num5 = (int)pixel.Blue - lookup.Blue;
                            int num6 = num2 * num2 + num3 * num3 + num4 * num4 + num5 * num5;
                            if (num6 < num1)
                            {
                                num1 = num6;
                                index2 = index3;
                            }
                        }
                        dictionary[argb] = index2;
                    }
                    numArray1[index2] += (int)pixel.Alpha;
                    numArray2[index2] += (int)pixel.Red;
                    numArray3[index2] += (int)pixel.Green;
                    numArray4[index2] += (int)pixel.Blue;
                    ++numArray5[index2];
                    quantizedPalette.PixelIndex[index1] = index2;
                }
            }
            for (int index = 0; index < colorCount; ++index)
            {
                if (numArray5[index] > 0)
                {
                    numArray1[index] /= numArray5[index];
                    numArray2[index] /= numArray5[index];
                    numArray3[index] /= numArray5[index];
                    numArray4[index] /= numArray5[index];
                }
                Color color = Color.FromArgb(numArray1[index], numArray2[index], numArray3[index], numArray4[index]);
                quantizedPalette.Colors.Add(color);
            }
            quantizedPalette.Colors.Add(Color.FromArgb(0, 0, 0, 0));
            return quantizedPalette;
        }
    }
}
