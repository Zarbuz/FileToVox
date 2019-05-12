using System;
using System.Collections.Generic;
using System.Text;

namespace FileToVox.CA
{
    public class RuleGeneric : RuleSet
    {
        private int _a;
        private int _b;

        public RuleGeneric(int[,,] field, int maxX, int maxY, int maxZ, int a, int b)
            : base(field, maxX, maxY, maxZ)
        {
            _a = a;
            _b = b;
        }

        /// <summary>
        /// Converts a number to an array of digits. For example 123 becomes 1,2,3
        /// </summary>
        /// <param name="digits"></param>
        /// <returns></returns>
        protected List<int> ToDigitArray(int digits)
        {
            List<int> result = new List<int>();
            string digitString = digits.ToString();

            foreach (char digit in digitString)
            {
                result.Add(Convert.ToInt32(digit.ToString()));
            }

            return result;
        }


        protected override int[,,] TickAlgorithm()
        {
            int[,,] field2 = new int[_maxX, _maxY, _maxZ];
            List<int> birthDigits = ToDigitArray(_b);
            List<int> liveDigits = ToDigitArray(_a);

            // A/B
            // The first number(s) is what is required for a cell to continue.
            // The second number(s) is the requirement for birth.

            for (int y = 0; y < _maxY; y++)
            {
                for (int z = 0; z < _maxZ; z++)
                {
                    for (int x = 0; x < _maxX; x++)
                    {
                        bool processed = false;
                        int neighbors = GetNumberOfNeighbors(x, y, z);
                        foreach (int digit in birthDigits)
                        {
                            if (neighbors == digit)
                            {
                                //cell is born
                                field2[x, y, z] = 1;
                                processed = true;
                                break;
                            }
                        }

                        if (processed)
                        {
                            continue;
                        }

                        foreach (int digit in liveDigits)
                        {
                            if (neighbors == digit)
                            {
                                //cell continue
                                field2[x, y, z] = _field[x, y, z];
                                processed = true;
                                break;
                            }
                        }

                        if (processed)
                        {
                            continue;
                        }

                        //cell dies
                        field2[x, y, z] = 0;
                    }
                }
            }

            return field2;
        }
    }
}
