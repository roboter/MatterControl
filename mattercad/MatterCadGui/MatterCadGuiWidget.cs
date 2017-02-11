/*
Copyright (c) 2012, Lars Brubaker
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/

using MatterHackers.Agg;
using MatterHackers.Agg.Image;
using MatterHackers.Agg.OpenGlGui;
using MatterHackers.Agg.PlatformAbstract;
using MatterHackers.Agg.UI;
using MatterHackers.Agg.VertexSource;
using MatterHackers.Csg;
using MatterHackers.Csg.Operations;
using MatterHackers.Csg.Processors;
using MatterHackers.Csg.Solids;
using MatterHackers.Csg.Transform;
using MatterHackers.MatterCadGui.CsgEditors;
using MatterHackers.PolygonMesh;
using MatterHackers.PolygonMesh.Processors;
using MatterHackers.RenderOpenGl;
using MatterHackers.VectorMath;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace MatterHackers.MatterCad
{
    public class MatterCadGuiWidget : GuiWidget
    {
        private PolygonMesh.Mesh meshToRender = null;

        private TrackballTumbleWidget trackBallWidget;
        private Button outputScad;
        private Splitter verticleSpliter;

        private GuiWidget objectEditorView;
        private FlowLayoutWidget objectEditorList;
        private FlowLayoutWidget textSide;

        private Union rootUnion = new Union("root");

        public MatterCadGuiWidget()
        {
            rootUnion.Add(new Translate(new BoxPrimitive(10, 10, 20), 5, 10, 5));
            //rootUnion.Add(new BoxPrimitive(8, 20, 10));
            //rootUnion.Add(new Cylinder(10, 40));
            rootUnion.Add(new Translate(new Sphere(radius: 30), 15, 20, 40)); //not implemented
            //var testUnion = new Translate(new Box(10, 10, 20) - new Box(8, 20, 10), 5, 5, 5); //new Difference(
            rootUnion.Add(new LinearExtrude(new double[] { 1.1, 2.2, 3.3, 6.3 }, 3));
            //rootUnion.Add(testUnion);

            SuspendLayout();
            verticleSpliter = new Splitter();
            {
                // panel 1 stuff
                textSide = new FlowLayoutWidget(FlowDirection.TopToBottom);
                {
                    objectEditorView = new GuiWidget(300, 500);
                    objectEditorList = new FlowLayoutWidget();
                    objectEditorList.AddChild(CsgEditorBase.CreateEditorForCsg(rootUnion));
                    objectEditorView.AddChild(objectEditorList);
                    objectEditorView.BackgroundColor = RGBA_Bytes.LightGray;
                    //   matterScriptEditor.LocalBounds = new RectangleDouble(0, 0, 200, 300);
                    textSide.AddChild(objectEditorView);
                    textSide.BoundsChanged += new EventHandler(textSide_BoundsChanged);

                    FlowLayoutWidget topButtonBar = new FlowLayoutWidget();
                    {
                        Button loadMatterScript = new Button("Load Matter Script");
                        loadMatterScript.Click += loadMatterScript_Click;
                        topButtonBar.AddChild(loadMatterScript);

                        outputScad = new Button("Output SCAD");
                        outputScad.Click += outputScad_Click;
                        topButtonBar.AddChild(outputScad);
                    }
                    textSide.AddChild(topButtonBar);

                    FlowLayoutWidget bottomButtonBar = new FlowLayoutWidget();
                    {
                        Button loadStl = new Button("Load STL");
                        loadStl.Click += LoadStl_Click;
                        bottomButtonBar.AddChild(loadStl);
                    }
                    textSide.AddChild(bottomButtonBar);
                }

                // pannel 2 stuff
                FlowLayoutWidget renderSide = new FlowLayoutWidget(FlowDirection.TopToBottom);
                renderSide.AnchorAll();
                {
                    trackBallWidget = new TrackballTumbleWidget();
                    trackBallWidget.DrawGlContent += new EventHandler(glLightedView_DrawGlContent);
                    renderSide.AddChild(trackBallWidget);
                }
                verticleSpliter.Panel2.AddChild(renderSide);
                verticleSpliter.Panel1.AddChild(textSide);
            }
            ResumeLayout();

            AnchorAll();

            verticleSpliter.AnchorAll();

            textSide.AnchorAll();

            trackBallWidget.AnchorAll();

            AddChild(verticleSpliter);

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

                OrthographicZProjection.DrawTo(plateGraphics, meshToRender, lowerLeftInMM + offsetInMM, pixelsPerMm);
                plateGraphics.DrawString(Path.GetFileName(openParams.FileName), (offsetInMM.x + centerInMM.x) * pixelsPerMm, (offsetInMM.y - 10) * pixelsPerMm, 50, Agg.Font.Justification.Center);

                ImageBuffer logoImage = new ImageBuffer();
                ImageIO.LoadImageData("Logo.png", logoImage);
                plateGraphics.Render(logoImage, (plateInventory.Width - logoImage.Width) / 2, plateInventory.Height - logoImage.Height - 10 * pixelsPerMm);

                ImageIO.SaveImageData("plate Inventory.jpeg", plateInventory);
            });
        }

        public override void OnParentChanged(EventArgs e)
        {
            verticleSpliter.SplitterDistance = Parent.Width / 2;
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

                verticleSpliter.SplitterDistance = verticleSpliter.SplitterDistance - 1;
                verticleSpliter.SplitterDistance = verticleSpliter.SplitterDistance + 1;
            });
        }

        private void outputScad_Click(object sender, EventArgs mouseEvent)
        {
            if (rootUnion != null)
            {
                SaveFileDialogParams saveParams = new SaveFileDialogParams("Text files (*.scad)|*.scad");
                FileDialog.SaveFileDialog(saveParams, (ii) =>
                {
                    //Utilities.PutOnPlatformAndCenter(rootUnion)
                    OpenSCadOutput.Save(rootUnion, ii.FileName);
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