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
        static double rodDiameter = 9;
        static double spoolDiameter = 20;
        static double spoolRestingDiameter = 20 - 6;
        static double spoolWidth = 95;
        static double nutWidth = 6.24;
        static double nutDiameter = 16;

        static CsgObject OutletWallPlate(bool printShort)
        {
            CsgObject total;

            CsgObject plateBottom = new Cylinder(spoolRestingDiameter / 2, spoolWidth, Alignment.x);
            total = plateBottom;

            return total;
        }

        static void Main()
        {
            OpenSCadOutput.Save(OutletWallPlate(true), "Outlet Wall Plate.scad", "import (\"refwallplate.stl\");\n");
        }
    }
}
