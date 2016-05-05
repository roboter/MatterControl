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
        static double spoolDiameter = 20;
        static double spoolRestingDiameter = 20 - 6;
        static double spoolWidth = 95;
        static double height = 24;

        static CsgObject SpoolHolderTypeA()
        {
            Box edgeBox = new Box(10, 150, height);
            edgeBox.BevelEdge(Edge.LeftBack, 4);
            edgeBox.BevelEdge(Edge.RightBack, 4);
            CsgObject edge = edgeBox;
            Box holdLegBox = new Box(45, 8, height);
            holdLegBox.BevelEdge(Edge.RightBack, 8);
            CsgObject holdLeg = holdLegBox;
            holdLeg = new Align(holdLeg, Face.Left, edge, Face.Right, offsetX: -.1);
            CsgObject printerMount = edge + holdLeg;
            printerMount += Round.CreateFillet(edge, Face.Right, holdLeg, Face.Back, 15);
            CsgObject wallCutOut = new Box(5, 5, height);
            wallCutOut = new Align(wallCutOut, Face.Left | Face.Front, holdLeg, Face.Left | Face.Front, offsetY: -.1);
            printerMount -= wallCutOut;

            CsgObject filamentHole = new Cylinder(4, edge.XSize + 2, Alignment.x);
            filamentHole = new Align(filamentHole, Face.Back, edge, Face.Back, offsetY: -10);
            printerMount -= filamentHole;
            CsgObject filamentHoleLeftBevel = Round.CreateFillet(4, 6, 3, Alignment.x);
            filamentHoleLeftBevel = new SetCenter(filamentHoleLeftBevel, filamentHole.GetCenter());
            filamentHoleLeftBevel = new Align(filamentHoleLeftBevel, Face.Left, edge, Face.Left, offsetX: -.02);
            printerMount -= filamentHoleLeftBevel;
            CsgObject filamentHoleRightBevel = Round.CreateFillet(4, 6, 3, Alignment.negX);
            filamentHoleRightBevel = new SetCenter(filamentHoleRightBevel, filamentHole.GetCenter());
            filamentHoleRightBevel = new Align(filamentHoleRightBevel, Face.Right, edge, Face.Right, offsetX: .02);
            printerMount -= filamentHoleRightBevel;

            CsgObject spoolSupport = new Cylinder(spoolRestingDiameter / 2, spoolWidth, Alignment.x);
            CsgObject spoolRodTotal = spoolSupport;

            double fitThroughRadius = spoolDiameter / 2 - 1;
            CsgObject topCapBevel = new Cylinder(fitThroughRadius, spoolRestingDiameter / 2, 4, Alignment.x);
            topCapBevel = new Align(topCapBevel, Face.Right, spoolSupport, Face.Left, .01);
            spoolRodTotal += topCapBevel;

            CsgObject topCap = new Cylinder(fitThroughRadius, 4, Alignment.x);
            topCap = new Align(topCap, Face.Right, topCapBevel, Face.Left, .01);
            spoolRodTotal += topCap;

            CsgObject nutHoldRing = new Cylinder(spoolDiameter / 2, 8, Alignment.x);
            nutHoldRing = new Align(nutHoldRing, Face.Left, spoolSupport, Face.Right, offsetX: -.01);
            spoolRodTotal += nutHoldRing;
            spoolRodTotal = new Align(spoolRodTotal, Face.Bottom, printerMount, Face.Bottom, offsetZ: -4);
            CsgObject bottomCutOff = new Box(spoolRodTotal.XSize + 4, spoolRodTotal.YSize + 4, 10);
            bottomCutOff = new Align(bottomCutOff, Face.Top, printerMount, Face.Bottom);
            spoolRodTotal -= bottomCutOff;

            spoolRodTotal = new Align(spoolRodTotal, Face.Right | Face.Front, printerMount, Face.Left | Face.Front, offsetX: -.01, offsetY: 10);

            CsgObject total = printerMount + spoolRodTotal;

            return total;
        }

        static void Main()
        {
            OpenSCadOutput.Save(SpoolHolderTypeA(), "SpoolHolderTypeA20mm.scad");
        }
    }
}
