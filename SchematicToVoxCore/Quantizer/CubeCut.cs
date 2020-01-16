using System;
using System.Collections.Generic;
using System.Text;

namespace FileToVox.Quantizer
{
    internal struct CubeCut
    {
        public readonly byte? Position;
        public readonly float Value;

        public CubeCut(byte? cutPoint, float result)
        {
            this.Position = cutPoint;
            this.Value = result;
        }
    }
}
