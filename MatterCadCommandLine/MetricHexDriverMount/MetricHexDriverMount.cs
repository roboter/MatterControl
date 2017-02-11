using System; // we need some usings to get use started
using System.Diagnostics;
using System.IO;
using MatterHackers.Csg; // our constructive solid geometry base classes
using MatterHackers.Csg.Solids;
using MatterHackers.Csg.Transform;
using MatterHackers.Csg.Processors;
using MatterHackers.Csg.Operations;
using MatterHackers.VectorMath; // helper math functions

namespace SimplePartScripting
{
    class SimplePartTester
    {
        double wallWidth = 4;
        double distToShaftCenter = 15;

        public SimplePartTester()
        {
        }

        CsgObject DriverHolder(double diameter)
        {
            diameter *= 1.5;
            Box topHolder = new Box(diameter + wallWidth * 2, wallWidth, distToShaftCenter + diameter * 2 + wallWidth);
            topHolder.BevelEdge(Edge.LeftTop, topHolder.XSize / 2);
            topHolder.BevelEdge(Edge.RightTop, topHolder.XSize / 2);

            CsgObject total = topHolder;

            CsgObject hole = new Cylinder(diameter/2, topHolder.YSize + .1, Alignment.y);
            hole = new Align(hole, Face.Bottom, topHolder, Face.Bottom, offsetZ: distToShaftCenter + diameter);
            total -= hole;

            return total;
        }

        CsgObject CreatePart()
        {
            // CsgObject is our base class for all constructive solid geometry primitives
            CsgObject total;

            Box backPlateBox = new Box(185, 40, wallWidth, "backPlate");
            backPlateBox.BevelEdge(Edge.LeftFront, backPlateBox.YSize / 2);
            backPlateBox.BevelEdge(Edge.LeftBack, backPlateBox.YSize / 2);
            backPlateBox.BevelEdge(Edge.RightFront, backPlateBox.YSize / 2);
            backPlateBox.BevelEdge(Edge.RightBack, backPlateBox.YSize / 2);
            CsgObject backPlate = backPlateBox;

            total = backPlate;

            double position = 22;
            total += MakeAndPositionOne(backPlateBox, backPlate, position, 5);
            position += 30;
            total += MakeAndPositionOne(backPlateBox, backPlate, position, 4);
            position += 30;
            total += MakeAndPositionOne(backPlateBox, backPlate, position, 3);
            position += 28;
            total += MakeAndPositionOne(backPlateBox, backPlate, position, 2.5);
            position += 15;
            total += MakeAndPositionOne(backPlateBox, backPlate, position, 2);
            position += 15;
            total += MakeAndPositionOne(backPlateBox, backPlate, position, 1.5);
            position += 15;
            total += MakeAndPositionOne(backPlateBox, backPlate, position, 1.27);

            total -= new Translate(new Cylinder(2, wallWidth + .1), -backPlate.XSize / 2 + 15);
            total -= new Translate(new Cylinder(2, wallWidth + .1), backPlate.XSize / 2 - 15);

            return total;  // and pass it back to the caller
        }

        private CsgObject MakeAndPositionOne(CsgObject total, CsgObject backPlate, double offset, double size)
        {
            CsgObject driverMount = DriverHolder(size);
            driverMount = new Align(driverMount, Face.Bottom, total, Face.Top, offsetZ: -.02);
            driverMount += Round.CreateFillet(driverMount, Face.Front, backPlate, Face.Top, wallWidth);
            driverMount += Round.CreateFillet(driverMount, Face.Back, backPlate, Face.Top, wallWidth);

            driverMount += driverMount.NewMirrorAccrossY(14);

            driverMount = new Align(driverMount, Face.Left | Face.Front, total, Face.Left | Face.Front, offsetX: offset);
            return driverMount;
        }

        static void Main()
        {
            SimplePartTester partTester = new SimplePartTester();
            // an internal function to save as an .scad file
            OpenSCadOutput.Save(partTester.CreatePart(), "MetricHexDriverMount.scad"); 
        }
    }
}