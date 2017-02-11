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
        static CsgObject HollowBlock()
        {
            double wallWidth = 4;
            double radius = 8;
            Box OutsideBox = new Box(63, 56, 40);
            OutsideBox.BevelEdge(Edge.LeftBack, radius);
            OutsideBox.BevelEdge(Edge.LeftFront, radius);
            OutsideBox.BevelEdge(Edge.RightBack, radius);
            OutsideBox.BevelEdge(Edge.RightFront, radius);

            //Box InsideBox = new Box(OutsideBox.XSize - wallWidth, OutsideBox.YSize - wallWidth, OutsideBox.ZSize - wallWidth);
            //InsideBox.BevelEdge(Edge.LeftBack, radius);
            //InsideBox.BevelEdge(Edge.LeftFront, radius);
            //InsideBox.BevelEdge(Edge.RightBack, radius);
            //InsideBox.BevelEdge(Edge.RightFront, radius);
            //CsgObject InsideBoxAligned = new Align(InsideBox, Face.Bottom, OutsideBox, Face.Bottom, offsetZ: -.02);

            CsgObject total = OutsideBox ;
            return total;
        }

        static void Main()
        {
            OpenSCadOutput.Save(HollowBlock(), "ABSTowerTest.scad");
        }
    }
}
