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

        CsgObject HeartShape(double heartWidth, double height, string name = "")
        {
            // CsgObject is our base class for all constructive solid geometry primitives
            CsgObject total;

            double boxSideSize = Math.Sqrt(heartWidth * heartWidth * 2);
            CsgObject heartBase = new Box(boxSideSize, boxSideSize, height);
            heartBase = new Rotate(heartBase, z: MathHelper.DegreesToRadians(45));

            total = heartBase;
            // now we make a cyliner for the side
            CsgObject leftBump = new Cylinder(boxSideSize/2, height);
            // position it where we want it
            leftBump = new Translate(leftBump, heartWidth / 2, heartWidth / 2);
            // and make another one on the other side by mirroring
            CsgObject rightHold = leftBump.NewMirrorAccrossX();
            // this is the way we actuall decide what boolean operation to put them together with
            total += leftBump; 
            total = new Union(total, rightHold, name);

            return total;  // and pass it back to the caller
        }

        CsgObject BoxBase(double heartWidth, double boxHeight)
        {
            double wallWidth = 4;
            CsgObject heartOutside = HeartShape(heartWidth, boxHeight - wallWidth);

            CsgObject heartLidLip = HeartShape(heartWidth - wallWidth, boxHeight);
            heartLidLip = new Align(heartLidLip, Face.Bottom, heartOutside, Face.Bottom);
            heartLidLip += heartOutside;

            CsgObject heartInside = HeartShape(heartWidth - wallWidth * 2, boxHeight - wallWidth);
            heartInside = new Align(heartInside, Face.Top, heartLidLip, Face.Top, offsetZ: .02);

            heartLidLip -= heartInside;

            return heartLidLip;
        }

        CsgObject GetLetter(char letter)
        {
            switch (letter)
            {
                case 'T':
                    {
                        CsgObject top = new Box(.8, .2, 1);
                        CsgObject center = new Box(.2, 1, 1);
                        center += new Align(top, Face.Back, center, Face.Back);
                        return center;
                    }

                case 'L':
                    {
                        CsgObject bottom = new Box(.8, .2, 1);
                        CsgObject left = new Box(.2, 1, 1);
                        left += new Align(bottom, Face.Left | Face.Front, left, Face.Left | Face.Front);
                        return left;
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        CsgObject BoxLid(double heartWidth, double boxHeight, char firstChar, char secondChar)
        {
            double wallWidth = 4;
            double lidFitExtra = .5;
            CsgObject heartOutside = HeartShape(heartWidth, boxHeight);
            CsgObject total = heartOutside;

            CsgObject heartInside = HeartShape(heartWidth - wallWidth + lidFitExtra, boxHeight - wallWidth);
            heartInside = new Align(heartInside, Face.Top, heartOutside, Face.Top, offsetZ: .02);

            total -= heartInside;

            CsgObject heartCutout = HeartShape(5, 4.1);
            total -= new Align(heartCutout, Face.Bottom, heartOutside, Face.Bottom, 0, 8, -.01);

            CsgObject firstLetter = GetLetter(firstChar);
            firstLetter = new Scale(firstLetter, 10, 10, 4.1);
            firstLetter = new Rotate(firstLetter, MathHelper.DegreesToRadians(180), 0, MathHelper.DegreesToRadians(180));
            firstLetter = new Translate(firstLetter, 12, 8);
            total -= new Align(firstLetter, Face.Bottom, heartOutside, Face.Bottom, 0, 8, -.01);

            CsgObject secondLetter = GetLetter(secondChar);
            secondLetter = new Scale(secondLetter, 10, 10, 4.1);
            secondLetter = new Rotate(secondLetter, MathHelper.DegreesToRadians(180), 0, MathHelper.DegreesToRadians(180));
            secondLetter = new Translate(secondLetter, -10, -8);
            total -= new Align(secondLetter, Face.Bottom, heartOutside, Face.Bottom, 0, 8, -.01);

            return total;
        }

        static void Main()
        {
            SimplePartTester partTester = new SimplePartTester();
            // an internal function to save as an .scad file
            OpenSCadOutput.Save(partTester.BoxBase(30, 15), "HeartBoxBase.scad");
            OpenSCadOutput.Save(partTester.BoxLid(30, 8, 'T', 'L'), "HeartBoxLid.scad");
            OpenSCadOutput.Save(partTester.HeartShape(30, 8), "HeartShape.scad"); 
        }
    }
}