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
        public SimplePartTester()
        {
        }

        CsgObject FilamentDisplayHolder()
        {
            CsgObject total;

            double wallWidth = 3;
            double height = 68;
            double faceWidth = 27;
            CsgObject frontFace = new Box(faceWidth, wallWidth, height);
            total = frontFace;

            double cabinateWidth = 2;
            CsgObject topFace = new Box(wallWidth, wallWidth * 2 + cabinateWidth, height);
            topFace = new Align(topFace, Face.Left | Face.Front, frontFace, Face.Left | Face.Front);
            total += topFace;

            double clipAmount = 6;
            CsgObject backClip = new Box(wallWidth + clipAmount, wallWidth, height);
            backClip = new Align(backClip, Face.Left | Face.Back, topFace, Face.Left | Face.Back);
            total += backClip;

            double frontHold = 17;
            CsgObject frontClip = new Box(wallWidth, wallWidth + wallWidth, height);
            frontClip = new Align(frontClip, Face.Left | Face.Front, frontFace, Face.Left | Face.Front, frontHold);
            total += frontClip;

            return total;
        }

        static void Main()
        {
            SimplePartTester partTester = new SimplePartTester();
            // an internal function to save as an .scad file
            OpenSCadOutput.Save(partTester.FilamentDisplayHolder(), "FilamentDisplayHolder.scad");
        }
    }
}