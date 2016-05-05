using System;

namespace MatterCad
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var cadWindow = new MatterCadMainWindow(true);
            cadWindow.UseOpenGL = true;
            cadWindow.Title = "MatterCAD";

            cadWindow.ShowAsSystemWindow();
        }
    }
}