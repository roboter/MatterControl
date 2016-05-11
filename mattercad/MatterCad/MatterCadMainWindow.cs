using MatterHackers.Agg;
using MatterHackers.Agg.Image;
using MatterHackers.Agg.OpenGlGui;
using MatterHackers.Agg.PlatformAbstract;
using MatterHackers.Agg.UI;
using MatterHackers.Agg.VertexSource;
using MatterHackers.Csg.Operations;

using MatterHackers.Csg.Solids;
using MatterHackers.Csg.Solids;

using MatterHackers.Csg.Transform;
using MatterHackers.PolygonMesh.Processors;
using MatterHackers.PolygonMesh.Rendering;
using MatterHackers.RenderOpenGl;
using MatterHackers.VectorMath;
using System;
using System.Globalization;
using System.IO;

namespace MatterCad
{
    internal class MatterCadMainWindow : SystemWindow
    {
        private MatterHackers.PolygonMesh.Mesh meshToRender = null;

        private TrackballTumbleWidget trackBallWidget;
        private Button outputScad;


        private GuiWidget objectEditorView;
        private FlowLayoutWidget objectEditorList;

        private Union rootUnion = new Union("root");

        public MatterCadMainWindow(bool renderRayTrace) : base(800, 600)
        {
            rootUnion.Add(new Translate(new BoxPrimitive(10, 10, 20), 5, 10, 5));
            rootUnion.Add(new BoxPrimitive(8, 20, 10));

            SuspendLayout();

            // panel 1 stuff


            // pannel 2 stuff
            FlowLayoutWidget renderSide = new FlowLayoutWidget(FlowDirection.TopToBottom);
            renderSide.AnchorAll();

            trackBallWidget = new TrackballTumbleWidget();
            trackBallWidget.DrawGlContent += new EventHandler(glLightedView_DrawGlContent);
            renderSide.AddChild(trackBallWidget);




            ResumeLayout();

            AnchorAll();

            renderSide.AnchorAll();



            trackBallWidget.AnchorAll();

            AddChild(renderSide);

            BackgroundColor = RGBA_Bytes.White;
        }

        private void LoadStl_Click(object sender, EventArgs e)
        {
            OpenFileDialogParams openParams = new OpenFileDialogParams("STL Files|*.stl");

            FileDialog.OpenFileDialog(openParams, (par) =>
            {
                loadedFileName = openParams.FileName;

                meshToRender = StlProcessing.Load(par.FileName);

                ImageBuffer plateInventory = new ImageBuffer((int)(300 * 8.5), 300 * 11, 32, new BlenderBGRA());
                Graphics2D plateGraphics = plateInventory.NewGraphics2D();
                plateGraphics.Clear(RGBA_Bytes.White);

                double inchesPerMm = 0.0393701;
                double pixelsPerInch = 300;
                double pixelsPerMm = inchesPerMm * pixelsPerInch;
                AxisAlignedBoundingBox aabb = meshToRender.GetAxisAlignedBoundingBox();
                Vector2 lowerLeftInMM = new Vector2(-aabb.minXYZ.x, -aabb.minXYZ.y);
                Vector3 centerInMM = (aabb.maxXYZ - aabb.minXYZ) / 2;
                Vector2 offsetInMM = new Vector2(20, 30);

                {
                    RectangleDouble bounds = new RectangleDouble(offsetInMM.x * pixelsPerMm,
                        offsetInMM.y * pixelsPerMm,
                        (offsetInMM.x + aabb.maxXYZ.x - aabb.minXYZ.x) * pixelsPerMm,
                        (offsetInMM.y + aabb.maxXYZ.y - aabb.minXYZ.y) * pixelsPerMm);
                    bounds.Inflate(3 * pixelsPerMm);
                    RoundedRect rect = new RoundedRect(bounds, 3 * pixelsPerMm);
                    plateGraphics.Render(rect, RGBA_Bytes.LightGray);
                    Stroke rectOutline = new Stroke(rect, .5 * pixelsPerMm);
                    plateGraphics.Render(rectOutline, RGBA_Bytes.DarkGray);
                }

                //  OrthographicZProjection.DrawTo(plateGraphics, meshToRender, lowerLeftInMM + offsetInMM, pixelsPerMm);
                plateGraphics.DrawString(Path.GetFileName(openParams.FileName), (offsetInMM.x + centerInMM.x) * pixelsPerMm, (offsetInMM.y - 10) * pixelsPerMm, 50, MatterHackers.Agg.Font.Justification.Center);

                ImageBuffer logoImage = new ImageBuffer();
                ImageIO.LoadImageData("Logo.png", logoImage);
                plateGraphics.Render(logoImage, (plateInventory.Width - logoImage.Width) / 2, plateInventory.Height - logoImage.Height - 10 * pixelsPerMm);

                ImageIO.SaveImageData("plate Inventory.jpeg", plateInventory);
            });
        }

        public override void OnParentChanged(EventArgs e)
        {
            
            base.OnParentChanged(e);
        }

        private void textSide_BoundsChanged(object sender, EventArgs e)
        {
            objectEditorView.LocalBounds = new RectangleDouble(0, 0, ((GuiWidget)sender).Width - 10, objectEditorView.Height);
            Invalidate();
        }

        private void glLightedView_DrawGlContent(object sender, EventArgs e)
        {
            if (rootUnion != null)
            {
                RenderCsgToGl.Render(rootUnion);
            }
            if (meshToRender != null)
            {
                RenderMeshToGl.Render(meshToRender, RGBA_Bytes.Gray);
            }
        }

        private string loadedFileName;

        private void loadMatterScript_Click(object sender, EventArgs mouseEvent)
        {
            // this should save and load json
            //  throw new NotImplementedException();

            FileDialog.OpenFileDialog(new OpenFileDialogParams("MatterScript Files, c-sharp code|*.part;*.cs"), (openParams) =>
            {
                loadedFileName = openParams.FileName;
                string extension = Path.GetExtension(openParams.FileName).ToUpper(CultureInfo.InvariantCulture);
                if (extension == ".CS")
                {
                }
                else if (extension == ".VB")
                {
                }

                string text = File.ReadAllText(loadedFileName);

                StreamReader streamReader = new StreamReader(loadedFileName);
                objectEditorView.Text = streamReader.ReadToEnd();
                streamReader.Close();
                
            });
        }

        private void outputScad_Click(object sender, EventArgs mouseEvent)
        {
            if (rootUnion != null)
            {
                SaveFileDialogParams saveParams = new SaveFileDialogParams("Text files (*.scad)|*.scad");
                FileDialog.SaveFileDialog(saveParams, (ii) =>
                {
                    //   OpenSCadOutput.Save(Utilities.PutOnPlatformAndCenter(rootUnion), ii);
                });
            }
        }

        public override void OnDraw(Graphics2D graphics2D)
        {
            graphics2D.Clear(RGBA_Bytes.White);

            base.OnDraw(graphics2D);
        }
    }
}