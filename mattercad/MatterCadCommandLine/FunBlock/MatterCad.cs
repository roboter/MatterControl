using System;
using System.Diagnostics;
using MatterHackers.Csg;
using MatterHackers.Csg.Solids;
using MatterHackers.Csg.Transform;
using MatterHackers.Csg.Processors;
using MatterHackers.VectorMath;

namespace SimplePartScripting
{
    static class CreateBearingHolder
    {
        static double height = 10;

        static CsgObject HoldPart(bool addative = true)
        {
            double extra = 0;
            if (!addative)
            {
                extra = .5;
            }

            double radius = 3;
            CsgObject total = new Cylinder(radius + extra/2, height + extra);
            total += new Translate(new Box(radius * 2, 2 + extra, height + extra), 1 - extra/2);

            return total;
        }

        static CsgObject FunBrick()
        {
            CsgObject body = new Box(20, 20, height);
            CsgObject total = body;

            total += new Align(HoldPart(), Face.Right, body, Face.Left, offsetX: .02);
            total -= new Align(HoldPart(false), Face.Right, body, Face.Right, offsetX: .02);
            total += new Rotate(new Align(HoldPart(), Face.Right, body, Face.Left, offsetX: .02), 0, 0, MathHelper.Tau/4);
            total -= new Rotate(new Align(HoldPart(false), Face.Right, body, Face.Right, offsetX: .02), 0, 0, MathHelper.Tau/4);

            total += new Translate(new Align(new Box(4, 4, 2), Face.Bottom, total, Face.Top, offsetZ: -1), -1, -1);
            total -= new Translate(new Align(new Box(5, 5, 2), Face.Bottom, total, Face.Bottom, offsetZ: -.1), -1, -1);

            return total;
        }

        static void Main()
        {
            CsgObject total = FunBrick();
            //total += new Translate(FunBrick(), 20.25);
            OpenSCadOutput.Save(total, "FunBrick.scad");
        }
    }
}
