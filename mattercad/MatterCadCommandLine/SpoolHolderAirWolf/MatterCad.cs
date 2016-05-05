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

        static CsgObject SpoolrHolderAirWolf(bool printShort)
        {
            CsgObject total;

            CsgObject spoolSupport = new Cylinder(spoolRestingDiameter / 2, spoolWidth, Alignment.x);
            total = spoolSupport;

            double fitThroughRadius = spoolDiameter / 2 - 1;
            CsgObject topCapBevel = new Cylinder(fitThroughRadius, spoolRestingDiameter / 2, 4, Alignment.x);
            topCapBevel = new Align(topCapBevel, Face.Right, spoolSupport, Face.Left, .01);
            total += topCapBevel;

            CsgObject topCap = new Cylinder(fitThroughRadius, 4, Alignment.x);
            topCap = new Align(topCap, Face.Right, topCapBevel, Face.Left, .01);
            total += topCap;

            CsgObject nutHoldRing = new Cylinder(spoolDiameter / 2, 8, Alignment.x);
            nutHoldRing = new Align(nutHoldRing, Face.Left, spoolSupport, Face.Right, offsetX: -.01);
            total += nutHoldRing;

            CsgObject fillet = Round.CreateFillet(spoolDiameter / 2, spoolDiameter, nutHoldRing.XSize, Alignment.negX);
            fillet = new Align(fillet, Face.Left, spoolSupport, Face.Right, offsetX: -.01);
            total += fillet;

            CsgObject insideOfFilletSpace = new Cylinder(spoolRestingDiameter / 2, nutHoldRing.XSize, Alignment.x);
            insideOfFilletSpace = new Align(insideOfFilletSpace, Face.Right, nutHoldRing, Face.Right);
            total += insideOfFilletSpace;

            CsgObject holdRing = new Cylinder(spoolDiameter, 4, Alignment.x);
            holdRing = new Align(holdRing, Face.Left, nutHoldRing, Face.Right, -.01);
            total += holdRing;

            CsgObject centerHole = new Cylinder(rodDiameter / 2, spoolWidth * 2, Alignment.x);
            total -= centerHole;

            CsgObject nutHole = new NGonExtrusion(nutDiameter/2, 6, nutWidth, Alignment.x);
            nutHole = new Align(nutHole, Face.Right, holdRing, Face.Right, offsetX: .01);
            total -= nutHole;

            if (printShort)
            {
                CsgObject cutOffBottom = new Box(total.XSize + 1, total.YSize + 1, total.ZSize);
                cutOffBottom = new SetCenter(cutOffBottom, total.GetCenter());
                cutOffBottom = new Translate(cutOffBottom, 0, 0, -23.5);
                total -= cutOffBottom;
            }
            else
            {
                CsgObject nutHoleTop = new Cylinder(spoolRestingDiameter / 2, .4, Alignment.x, "Nut Hole Top");
                nutHoleTop = new Align(nutHoleTop, Face.Left, nutHole, Face.Left);
                total += nutHoleTop;
            }

            return total;
        }

        static CsgObject TopMount()
        {
            Box baseBarBox = new Box(40, 140, 14);
            baseBarBox.BevelEdge(Edge.LeftBack, 10);
            baseBarBox.BevelEdge(Edge.RightBack, 10);
            CsgObject baseBar = baseBarBox;
            baseBar = new Translate(baseBar, 54, 70, -21);
            CsgObject total = baseBar;

            CsgObject spoolHolder = new Rotate(SpoolrHolderAirWolf(false), y: MathHelper.DegreesToRadians(90));
            spoolHolder = new SetCenter(spoolHolder, baseBar.GetCenter());
            spoolHolder = new Align(spoolHolder, Face.Bottom | Face.Back, baseBar, Face.Back | Face.Top, offsetZ: -5);
            total += spoolHolder;

            CsgObject bottom = new Box(baseBar.XSize, 10, 42);
            bottom = new Align(bottom, Face.Bottom | Face.Left | Face.Front, baseBar, Face.Bottom | Face.Left | Face.Front);
            total += bottom;

            CsgObject hold = new Box(baseBar.XSize, 10, 42);
            hold = new Align(hold, Face.Bottom | Face.Left | Face.Front, baseBar, Face.Bottom | Face.Left | Face.Front, offsetY: bottom.YSize + 11.5);
            total += hold;

            total += Round.CreateFillet(baseBar, Face.Top, hold, Face.Back, 10);
            
            return total;
        }

        static void Main()
        {
            OpenSCadOutput.Save(SpoolrHolderAirWolf(true), "SpoolHolderAirWolf20mm.scad");
            OpenSCadOutput.Save(new Rotate(SpoolrHolderAirWolf(false), y: MathHelper.DegreesToRadians(90)), "Up SpoolHolderAirWolf20mm.scad");

            OpenSCadOutput.Save(TopMount(), "AirWolf Top Mount 20mm.scad", "% import(\"spool holder base.STL\");\n");
        }
    }
}
