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
        static double wallWidth = 4;
        static double aluminumBarWidth = 44.5;
        static double clipStickOut = 20;
        static double clipWidth = 10;
        static double clipHeight = 2;

        static CsgObject ThingToHold()
        {
            CsgObject bar = new Box(160, 50, aluminumBarWidth);
            CsgObject tab = new Box(clipWidth, clipStickOut, clipHeight);
            tab = new Align(tab, Face.Top | Face.Back, bar, Face.Bottom | Face.Front);
            CsgObject total = bar + tab;

            return total;
        }

        static CsgObject WindowShadeBracket()
        {
            CsgObject total = new Box(wallWidth + aluminumBarWidth + clipHeight + wallWidth, 40, 20);

            CsgObject aluminumBarHole = new Box(aluminumBarWidth, 40, 21);
            aluminumBarHole = new Align(aluminumBarHole, Face.Front | Face.Left, total, Face.Front | Face.Left, offsetX: wallWidth, offsetY: -wallWidth);
            total -= aluminumBarHole;
            
            CsgObject holderTabHole = new Box(clipHeight, wallWidth * 2, clipWidth);
            holderTabHole = new Align(holderTabHole, Face.Back | Face.Bottom, total, Face.Back | Face.Bottom);
            holderTabHole = new Align(holderTabHole, Face.Left, aluminumBarHole, Face.Right, -.02, .02, -.02);
            total -= holderTabHole;



            return total;
        }

        static void Main()
        {
            OpenSCadOutput.Save(WindowShadeBracket(), "WindowShadeBracket.scad");
            OpenSCadOutput.Save(ThingToHold(), "WindowShadeBracket.scad");
        }
    }
}
