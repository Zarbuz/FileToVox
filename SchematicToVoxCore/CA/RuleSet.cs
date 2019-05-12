using System;
using System.Collections.Generic;
using System.Text;

namespace FileToVox.CA
{
    public abstract class RuleSet
    {
        protected int _maxX = 0;
        protected int _maxY = 0;
        protected int _maxZ = 0;
        protected int[,,] _field;

        public RuleSet(int[,,] field, int maxX, int maxY, int maxZ)
        {
            _field = field;
            _maxX = maxX;
            _maxY = maxY;
            _maxZ = maxZ;
        }

        protected int GetNumberOfNeighbors(int x, int y, int z)
        {
            int neighbors = 0;

            if (x + 1 < _maxX && _field[x + 1, y, z] == 1)
                neighbors++;

            if (x - 1 >= 0 && _field[x - 1, y, z] == 1)
                neighbors++;

            if (y + 1 < _maxY && _field[x, y + 1, z] == 1)
                neighbors++;

            if (y - 1 >= 0 && _field[x, y - 1, z] == 1)
                neighbors++;

            if (z + 1 < _maxZ && _field[x, y, z + 1] == 1)
                neighbors++;

            if (z - 1 >= 0 && _field[x, y, z - 1] == 1)
                neighbors++;

            //diagonals
            if (x + 1 < _maxX && y + 1 < _maxY && z + 1 < _maxZ && _field[x + 1, y + 1, z + 1] == 1)
                neighbors++;

            if (x + 1 < _maxX && y + 1 < _maxY && z - 1 >= 0 && _field[x + 1, y + 1, z - 1] == 1)
                neighbors++;

            if (x + 1 < _maxX && y - 1 >= 0 && z + 1 < _maxZ && _field[x + 1, y - 1, z + 1] == 1)
                neighbors++;

            if (x + 1 < _maxX && y - 1 >= 0 && z - 1 >= 0 && _field[x + 1, y - 1, z - 1] == 1)
                neighbors++;

            if (x - 1 >= 0 && y + 1 < _maxY && z + 1 < _maxZ && _field[x - 1, y + 1, z + 1] == 1)
                neighbors++;

            if (x - 1 >= 0 && y - 1 >= 0 && z - 1 >= 0 && _field[x - 1, y - 1, z - 1] == 1)
                neighbors++;

            if (x - 1 >= 0 && y + 1 < _maxY && z - 1 >= 0 && _field[x - 1, y + 1, z - 1] == 1)
                neighbors++;

            if (x - 1 >= 0 && y - 1 < _maxY && z + 1 < _maxZ && _field[x - 1, y - 1, z + 1] == 1)
                neighbors++;

            return neighbors;
        }

        public void Tick()
        {
            int[,,] field2 = TickAlgorithm();
            Array.Copy(field2, _field, field2.Length);
        }

        public int GetWidth()
        {
            return _maxX;
        }

        public int GetLength()
        {
            return _maxY;
        }

        public int GetHeight()
        {
            return _maxZ;
        }

        public int[,,] GetField()
        {
            return _field;
        }

        protected abstract int[,,] TickAlgorithm();
    }
}
