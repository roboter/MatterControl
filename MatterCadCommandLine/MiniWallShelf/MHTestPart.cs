using System; // we need some usings to get use started
using System.Diagnostics;
using System.IO;
using MatterHackers.Csg; // our constructive solid geometry base classes
using MatterHackers.Csg.Solids;
using MatterHackers.Csg.Transform;
using MatterHackers.Csg.Processors;
using MatterHackers.Csg.Operations;
using MatterHackers.VectorMath; // helper math functions
using System.Management;

namespace SimplePartScripting
{
    class SimplePartTester
    {
        readonly double centerMountHolesRadius = 6;
        readonly double wallWidth = 4;

        public SimplePartTester()
        {
        }

        CsgObject MiniWallShelf(double shelfWidth, double shelfDepth, string name = "")
        {
            // CsgObject is our base class for all constructive solid geometry primitives
            CsgObject total;

            Box shelfPlatformBox = new Box(shelfWidth, wallWidth, shelfDepth);
            shelfPlatformBox.BevelEdge(Edge.LeftTop, 10);
            shelfPlatformBox.BevelEdge(Edge.RightTop, 10);
            CsgObject shelfPlatform = shelfPlatformBox;
            total = shelfPlatform;

            CsgObject support = new Box(wallWidth, 50, shelfDepth);
            support = new Align(support, Face.Back, shelfPlatform, Face.Front, offsetY: -.1);
            total += support;

            total += Round.CreateFillet(shelfPlatform, Face.Front, support, Face.Left, 10);
            total += Round.CreateFillet(shelfPlatform, Face.Front, support, Face.Right, 10);

            return total;  // and pass it back to the caller
        }

        CsgObject FilamentWallHook()
        {
            double distanceFromWall = 100;
            double lowHeight = 20;
            double screwDiameter = 4.5;

            Box wallPlateBox = new Box(100, wallWidth, lowHeight);
            wallPlateBox.BevelEdge(Edge.LeftTop, wallPlateBox.ZSize);
            wallPlateBox.BevelEdge(Edge.RightTop, wallPlateBox.ZSize);
            CsgObject wallPlate = wallPlateBox;
            wallPlate = new Align(wallPlate, Face.Bottom, 0, 0, 0);
            CsgObject total = wallPlate;

            Box mainArmBox = new Box(wallWidth, distanceFromWall, lowHeight);
            mainArmBox.BevelEdge(Edge.FrontTop, 30);
            CsgObject mainArm = mainArmBox;
            mainArm = new Align(mainArm, Face.Back | Face.Bottom, wallPlate, Face.Back | Face.Bottom);
            total += mainArm;

            Box wallFlatBox = new Box(20, wallWidth, 60);
            wallFlatBox.BevelEdge(Edge.LeftTop, wallFlatBox.XSize / 2 - wallWidth);
            wallFlatBox.BevelEdge(Edge.RightTop, wallFlatBox.XSize / 2 - wallWidth);
            CsgObject wallFlat = wallFlatBox;
            wallFlat = new Align(wallFlat, Face.Bottom, wallPlate, Face.Bottom);
            total += wallFlat;

            CsgObject leftWallFillet = Round.CreateFillet(wallPlate, Face.Top, wallFlat, Face.Left, 20);
            total += leftWallFillet;
            total += leftWallFillet.NewMirrorAccrossX();

            Box wallBraceBox = new Box(wallWidth, 20, wallFlatBox.ZSize);
            wallBraceBox.BevelEdge(Edge.FrontTop, wallBraceBox.YSize/2);
            CsgObject wallBrace = wallBraceBox;
            wallBrace = new Align(wallBrace, Face.Back | Face.Top, wallFlat, Face.Back | Face.Top, offsetY: -.02);
            total += wallBrace;

            total += Round.CreateFillet(wallBrace, Face.Front, mainArm, Face.Top, 10);

            // create the floor for the bracket
            Box backFloorBox = new Box(wallPlate.XSize, 30, wallWidth);
            backFloorBox.BevelEdge(Edge.LeftFront, 20);
            backFloorBox.BevelEdge(Edge.RightFront, 20);
            CsgObject backFloor = backFloorBox;
            backFloor = new Align(backFloor, Face.Back | Face.Bottom, wallPlate, Face.Back | Face.Bottom);
            total += backFloor;

            Box middleFloorBox = new Box(20, mainArmBox.YSize, wallWidth);
            middleFloorBox.BevelEdge(Edge.LeftFront, middleFloorBox.XSize/2);
            middleFloorBox.BevelEdge(Edge.RightFront, middleFloorBox.XSize / 2);
            CsgObject middleFloor = middleFloorBox;
            middleFloor = new Align(middleFloor, Face.Back | Face.Bottom, wallPlate, Face.Back | Face.Bottom);
            total += middleFloor;

            total += Round.CreateFillet(backFloor, Face.Front, middleFloor, Face.Left, 20);
            total += Round.CreateFillet(backFloor, Face.Front, middleFloor, Face.Right, 20);

            // make the fillets for the inside braces
            double edgeSupportRadius = 3;
            Round leftJoinsRound = new Round(wallPlate.XSize / 2 - wallWidth, mainArm.YSize - 10, wallBrace.ZSize - wallWidth);
            leftJoinsRound.RoundEdge(Edge.RightBottom, edgeSupportRadius);
            leftJoinsRound.RoundEdge(Edge.RightBack, edgeSupportRadius);
            leftJoinsRound.RoundEdge(Edge.BackBottom, edgeSupportRadius);
            CsgObject leftJoins = leftJoinsRound;
            leftJoins = new Align(leftJoins, Face.Back, wallPlate, Face.Front);
            leftJoins = new Align(leftJoins, Face.Right, mainArmBox, Face.Left);
            leftJoins = new Align(leftJoins, Face.Bottom, backFloor, Face.Top);
            total += leftJoins;
            total += leftJoins.NewMirrorAccrossX();

            // screw holes
            CsgObject leftScrewHole = new Cylinder(screwDiameter/2, wallWidth + .1, Alignment.y);
            leftScrewHole = new Translate(leftScrewHole, wallPlate.XSize / 2 - 20, 0, wallPlate.ZSize / 2);
            total -= leftScrewHole;
            total -= leftScrewHole.NewMirrorAccrossX();

            // holes for display insert
            CsgObject centerMountHole = CenterMountHole();
            centerMountHole = new Align(centerMountHole, Face.Bottom | Face.Front, mainArm, Face.Bottom | Face.Front, offsetY:5, offsetZ: -.02);

            CsgObject centerSupport = new Cylinder(centerMountHolesRadius + wallWidth/2, centerMountHole.ZSize + wallWidth);
            CsgObject leftSupport = new Cylinder(centerSupport.XSize / 4 + 1, centerSupport.ZSize);
            leftSupport = new Translate(leftSupport, centerMountHolesRadius);
            centerSupport += leftSupport;
            centerSupport += leftSupport.NewMirrorAccrossX();
            centerSupport = new SetCenter(centerSupport, centerMountHole.GetCenter());
            centerSupport = new Align(centerSupport, Face.Bottom, centerMountHole, Face.Bottom, offsetZ: .02);

            CsgObject mountPlate = new Cylinder(50, wallWidth);
            mountPlate = new SetCenter(mountPlate, centerMountHole.GetCenter());
            mountPlate = new Align(mountPlate, Face.Bottom, wallPlate, Face.Bottom);
            total += mountPlate;
            
            //centerSupport -= centerHoles;
            total += centerSupport;
            total -= centerMountHole;

            // this is so we can see the size of the fillament
            CsgObject spool = new Cylinder(165 / 2, 85);
            spool -= new Cylinder(19 / 2, spool.ZSize + 1);
            spool = new Translate(spool, 0, -spool.YSize / 2 - 10, -spool.ZSize / 2, name: "% test");
            total += spool;

            return total;
        }

        CsgObject CenterMountHole(double reduceMM = 0)
        {
            CsgObject centerMountHole = new Cylinder(centerMountHolesRadius - reduceMM / 2, 6);
            CsgObject leftMountHoles = new Cylinder(centerMountHole.XSize / 4 - reduceMM / 2, centerMountHole.ZSize);
            leftMountHoles = new Translate(leftMountHoles, centerMountHolesRadius);
            centerMountHole += leftMountHoles;
            centerMountHole += leftMountHoles.NewMirrorAccrossX();

            return centerMountHole;
        }

        CsgObject FilamentMountPeg(double spoolDiameter)
        {
            CsgObject centerMountHole = CenterMountHole(1.5);
            CsgObject spoolHold = new Cylinder(spoolDiameter/2, 4);
            spoolHold = new Align(spoolHold, Face.Top, centerMountHole, Face.Bottom, offsetZ: .02);

            CsgObject total = centerMountHole + spoolHold;

            CsgObject cutOff = new Box(spoolHold.Size);
            total -= new Align(cutOff, Face.Bottom, total, Face.Top, offsetZ: -1.5);

            return total;
        }

        static void Main()
        {
#if false
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver");
            foreach (ManagementObject obj in searcher.Get())
            {
                string s;

                //object deviceNameProperty = obj.GetPropertyValue("DeviceName");
                object deviceNameProperty = obj.GetPropertyValue("DriverProviderName");
                if (deviceNameProperty != null)
                {
                    s = string.IsNullOrEmpty(deviceNameProperty.ToString()) ? string.Empty : deviceNameProperty.ToString();
                    Debug.Print(s);
                }
            }
#endif

            SimplePartTester partTester = new SimplePartTester();
            // an internal function to save as an .scad file
            OpenSCadOutput.Save(partTester.MiniWallShelf(170, 140), "MiniWallShelf.scad");

            OpenSCadOutput.Save(partTester.FilamentWallHook(), "FilamentShelf.scad");
            OpenSCadOutput.Save(partTester.FilamentMountPeg(19), "FilamentMountPeg20mm.scad");
            OpenSCadOutput.Save(partTester.FilamentMountPeg(29), "FilamentMountPeg30mm.scad");
        }
    }
}