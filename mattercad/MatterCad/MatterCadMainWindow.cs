using MatterHackers.Agg;
using MatterHackers.Agg.OpenGlGui;
using MatterHackers.Agg.UI;
using MatterHackers.Csg.Operations;
using MatterHackers.Csg.Solids;

using MatterHackers.Csg.Solids;

using MatterHackers.Csg.Transform;
using MatterHackers.RenderOpenGl;
using System;

namespace MatterCad
{
    internal class MatterCadMainWindow : SystemWindow
    {
        private Union rootUnion = new Union("root");
        private TrackballTumbleWidget trackBallWidget;

        public MatterCadMainWindow(bool renderRayTrace)
            : base(800, 600)
        {
            rootUnion.Add(new Translate(new Box(10, 10, 20), 5, 10, 5));
            rootUnion.Add(new Box(8, 20, 10));

            SuspendLayout();

            FlowLayoutWidget renderSide = new FlowLayoutWidget(FlowDirection.TopToBottom);
            renderSide.AnchorAll();
            {
                trackBallWidget = new TrackballTumbleWidget();
                trackBallWidget.DrawGlContent += glLightedView_DrawGlContent;
                renderSide.AddChild(trackBallWidget);
            }
            AddChild(renderSide);

            trackBallWidget.AnchorAll();

            ResumeLayout();

            AnchorAll();
            BackgroundColor = RGBA_Bytes.Gray;
        }

        private void glLightedView_DrawGlContent(object sender, EventArgs e)
        {
            if (rootUnion != null)
            {
                RenderCsgToGl.Render(rootUnion);
            }
        }
    }
}