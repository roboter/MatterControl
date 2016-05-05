using System;
using System.Diagnostics;
using MatterHackers.Csg;
using MatterHackers.Csg.Solids;
using MatterHackers.Csg.Transform;
using MatterHackers.Csg.Processors;
using MatterHackers.VectorMath;

namespace SimplePartScripting
{
    public abstract class SpoolType
    {
        public double spoolDiameter;
        public double bearingDiameter;
        public double bearingHeight;
        public double retainingWallHeight;
        public double wallWidth;

        public SpoolType()
        {
            bearingDiameter = 22.5;
            bearingHeight = 7;
            retainingWallHeight = bearingHeight;
            wallWidth = 4;
        }
    }

    public class Spool32mm : SpoolType
    {
        public Spool32mm()
        {
            spoolDiameter = 32;
        }
    }

    public class Spool20mm : SpoolType
    {
        public Spool20mm()
        {
            spoolDiameter = 20;
        }
    }

    public class Spool52mm : SpoolType
    {
        public Spool52mm()
        {
            spoolDiameter = 52;
        }
    }

    public class PackingBagSpool : SpoolType
    {
        public PackingBagSpool()
        {
            retainingWallHeight = bearingHeight + 10;
            wallWidth = 6;
            spoolDiameter = 75;
        }
    }

    static class CreateBearingHolder
    {
        //static double spoolDiameter = 40;

        static CsgObject SpoolBearingHolder(SpoolType spoolType)
        {
            // CSG object is a Constructive Solid Geometry Object (a basic part in our system for doing boolean operations).
            CsgObject spoolBearingHolder;  // the csg object we will use as the master part.

            double holdLipExtra = 0;
            if (spoolType.spoolDiameter < spoolType.bearingDiameter)
            {
                CsgObject bearingBottom = new Cylinder(spoolType.spoolDiameter / 2 + 1, spoolType.bearingHeight);
                spoolBearingHolder = new Cylinder(spoolType.spoolDiameter / 2 + 1, spoolType.spoolDiameter / 2 - 1, spoolType.retainingWallHeight + spoolType.wallWidth);
                bearingBottom = new Align(bearingBottom, Face.Top, spoolBearingHolder, Face.Bottom, offsetZ: .02);
                spoolBearingHolder += bearingBottom;
                holdLipExtra = spoolType.bearingHeight;
            }
            else
            {
                spoolBearingHolder = new Cylinder(spoolType.spoolDiameter / 2 + 1, spoolType.spoolDiameter / 2 - 1, spoolType.retainingWallHeight + spoolType.wallWidth);
            }

            CsgObject insideHole = new Cylinder(spoolType.spoolDiameter / 2 - spoolType.wallWidth, spoolType.retainingWallHeight);
            insideHole = new Align(insideHole, Face.Top, spoolBearingHolder, Face.Top, offsetZ: .02);
            spoolBearingHolder -= insideHole;

            CsgObject spoolHoldLip = new Cylinder(spoolType.spoolDiameter / 2 + spoolType.wallWidth, spoolType.wallWidth + holdLipExtra);
            spoolHoldLip = new Align(spoolHoldLip, Face.Bottom, spoolBearingHolder, Face.Bottom);
            spoolBearingHolder += spoolHoldLip;

            CsgObject bearingHolder = new Cylinder((spoolType.bearingDiameter + spoolType.wallWidth) / 2, spoolType.bearingHeight + spoolType.wallWidth);
            bearingHolder = new Align(bearingHolder, Face.Bottom, spoolBearingHolder, Face.Bottom);
            spoolBearingHolder += bearingHolder;

            CsgObject bearingHole = new Cylinder(spoolType.bearingDiameter / 2, spoolType.bearingHeight);
            bearingHole = new Align(bearingHole, Face.Bottom, spoolBearingHolder, Face.Bottom, offsetZ: -.02);
            spoolBearingHolder -= bearingHole;

            CsgObject revolution = new RotateExtrude(new double[] { 0, -.02, spoolType.wallWidth, 0, 0, spoolType.wallWidth + .02 }, spoolType.bearingDiameter / 2 - spoolType.wallWidth);
            revolution = new Align(revolution, Face.Top, bearingHolder, Face.Top);

            spoolBearingHolder -= revolution;

            CsgObject rodHole = new Cylinder(spoolType.bearingDiameter / 2 - spoolType.wallWidth, spoolBearingHolder.ZSize + 2);
            rodHole = new Align(rodHole, Face.Top, spoolBearingHolder, Face.Top, offsetZ: .02);
            spoolBearingHolder -= rodHole;

            return spoolBearingHolder;
        }

        static CsgObject BearingWallHanger()
        {
            double bearingInnerDiameter = 8;
            CsgObject total;

            Box wallPlateBox = new Box(40, 56, 4);
            wallPlateBox.BevelEdge(Edge.LeftFront, wallPlateBox.XSize / 2);
            wallPlateBox.BevelEdge(Edge.RightFront, wallPlateBox.XSize / 2);
            wallPlateBox.BevelEdge(Edge.LeftBack, wallPlateBox.XSize / 2);
            wallPlateBox.BevelEdge(Edge.RightBack, wallPlateBox.XSize / 2);

            CsgObject wallPlate = wallPlateBox;
            total = wallPlate;
            total -= new Translate(new Cylinder(2, wallPlate.ZSize + .1), 0, 20);
            total -= new Translate(new Cylinder(2, wallPlate.ZSize + .1), 0, -20);
            Cylinder centralShaft = new Cylinder((bearingInnerDiameter-1)/2, 30 + wallPlate.ZSize);
            total += new Align(centralShaft, Face.Bottom, wallPlate, Face.Bottom);

            total += new Align(new Cylinder(bearingInnerDiameter*2, (bearingInnerDiameter - 1) / 2, 5 + wallPlate.ZSize), Face.Bottom, wallPlate, Face.Bottom);
            //total += Round.CreateFillet(centralShaft, wallPlate, Face.Top);

            total += new Align(new Cylinder((bearingInnerDiameter - 1) / 2, (bearingInnerDiameter - 1) / 3, 3), Face.Bottom, total, Face.Top, offsetZ: -.01);

            return total;
        }

        static void Main()
        {
            OpenSCadOutput.Save(SpoolBearingHolder(new Spool20mm()), "SpoolBearingHolder 20mm.scad");
            OpenSCadOutput.Save(SpoolBearingHolder(new Spool32mm()), "SpoolBearingHolder 32mm.scad");
            OpenSCadOutput.Save(SpoolBearingHolder(new PackingBagSpool()), "PackingBagsBearingHolder.scad");
            OpenSCadOutput.Save(SpoolBearingHolder(new Spool52mm()), "SpoolBearingHolder 52mm.scad");

            OpenSCadOutput.Save(BearingWallHanger(), "BearingWallHanger.scad");
        }
    }
}
