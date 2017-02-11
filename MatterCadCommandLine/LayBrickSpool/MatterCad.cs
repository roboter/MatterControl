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

    public class SmallSpool : SpoolType
    {
        public SmallSpool()
        {
            spoolDiameter = 32;
        }
    }

    static class CreateBearingHolder
    {
        //static double spoolDiameter = 40;

        static CsgObject SpoolBearingHolder(SpoolType spoolType)
        {
            // CSG object is a Constructive Solid Geometry Object (a basic part in our system for doing boolean operations).
            CsgObject spoolBearingHolder;  // the csg object we will use as the master part.

            spoolBearingHolder = new Cylinder(spoolType.spoolDiameter / 2 + 1, spoolType.spoolDiameter / 2 - 1, spoolType.retainingWallHeight + spoolType.wallWidth);

            CsgObject insideHole = new Cylinder(spoolType.spoolDiameter / 2 - spoolType.wallWidth, spoolType.retainingWallHeight);
            insideHole = new Align(insideHole, Face.Top, spoolBearingHolder, Face.Top, offsetZ: .02);
            spoolBearingHolder -= insideHole;

            CsgObject spoolHoldLip = new Cylinder(spoolType.spoolDiameter / 2 + spoolType.wallWidth, spoolType.wallWidth);
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

        static double wallWidth = 4;
        static CsgObject LayBrickSpoolHalf()
        {
            CsgObject bearingHolder = SpoolBearingHolder(new SmallSpool());
            CsgObject total = bearingHolder;

            CsgObject ring = new Cylinder(130 / 2, wallWidth);
            ring -= new Cylinder(120 / 2, wallWidth + .1);
            ring = new Align(ring, Face.Bottom, bearingHolder, Face.Bottom);
            total += ring;

            CsgObject spoke = new Box(70, wallWidth, wallWidth);
            spoke = new Align(spoke, Face.Bottom | Face.Left, bearingHolder, Face.Bottom | Face.Right, offsetX: - 2);
            total += spoke;
            total += new Rotate(spoke, 0, 0, MathHelper.Tau / 3);
            total += new Rotate(spoke, 0, 0, MathHelper.Tau / 3 * 2);

            CsgObject hub = new Align(new Box(wallWidth, wallWidth * 2, 40), Face.Bottom, spoke, Face.Bottom);
            hub = new Translate(hub, 62);
            total += hub;
            total += new Rotate(hub, 0, 0, MathHelper.Tau / 3);
            total += new Rotate(hub, 0, 0, MathHelper.Tau / 3 * 2);

            CsgObject hubHolder = new Box(new Vector3(hub.XSize + wallWidth, hub.YSize + wallWidth, wallWidth * 2));
            hubHolder = new SetCenter(hubHolder, hub.GetCenter());
            hubHolder = new Align(hubHolder, Face.Bottom, hub, Face.Bottom);
            
            CsgObject hubHole = new Box(new Vector3(hub.XSize + 1, hub.YSize + 1, wallWidth * 2));
            hubHole = new SetCenter(hubHole, hub.GetCenter());
            hubHole = new Align(hubHole, Face.Bottom, hub, Face.Bottom, offsetZ: + wallWidth);
            hubHolder -= hubHole;
            hubHolder = new Rotate(hubHolder, 0, 0, MathHelper.Tau / 6);

            total += hubHolder;
            total += new Rotate(hubHolder, 0, 0, MathHelper.Tau / 3);
            total += new Rotate(hubHolder, 0, 0, MathHelper.Tau / 3 * 2);

            return total;
        }

        static void Main()
        {
            OpenSCadOutput.Save(LayBrickSpoolHalf(), "LayBrickSpoolHalf.scad");
        }
    }
}
