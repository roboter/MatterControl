using System;
using System.Diagnostics;
using MatterHackers.Csg;
using MatterHackers.Csg.Solids;
using MatterHackers.Csg.Processors;
using MatterHackers.Csg.Transform;
using MatterHackers.VectorMath;

namespace SimplePartScripting
{
    static class SimplePartTester
    {
        static CsgObject SquishyBlock(int count, double size)
        {
            CsgObject totalZRods = new Box(1, 1, 1);
            CsgObject totalXRods = new Box(1, 1, 1);
            CsgObject totalYRods = new Box(1, 1, 1);
            double barWidth = 2;

            double offset = (size - barWidth) / (count-1);
            for (int y = 0; y < count; y++)
            {
                for (int x = 0; x < count; x++)
                {
                    CsgObject rodX = new Translate(new Box(size, barWidth, barWidth), 0, offset * x, offset * y);
                    totalXRods += rodX;
                    CsgObject rodY = new Translate(new Box(barWidth, size, barWidth), offset * x, 0, offset * y);
                    totalYRods += rodY;
                    CsgObject rodZ = new Translate(new Box(barWidth, barWidth, size), offset * x, offset * y, 0);
                    totalZRods += rodZ;
                }
            }

            totalXRods = new SetCenter(totalXRods);
            totalYRods = new SetCenter(totalYRods);
            totalZRods = new SetCenter(totalZRods);

            return totalXRods + totalYRods + totalZRods;
        }

        static void Main()
        {
            OpenSCadOutput.Save(SquishyBlock(10, 50), "SquishyBlock10.scad");
            OpenSCadOutput.Save(SquishyBlock(4, 20), "SquishyBlock4.scad");
        }
    }
}
