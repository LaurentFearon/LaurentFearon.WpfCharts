using System.Windows;
using System.Windows.Media;

namespace LaurentFearon.WpfCharts
{
    public class SimpleLineChart : ChartBase
    {
        private Pen blackPen = new Pen(Brushes.Black, 1);

        protected override void OnRender(DrawingContext dc)
        {
            this.CalcBoundaries();

            if (this.DataSource != null && this.XValueMapping != null && this.YValueMapping != null)
            {
                this.DrawAxes(dc);
                this.DrawGraph(dc);
            }

            base.OnRender(dc);
        }



        private void DrawAxes(DrawingContext dc)
        {
            dc.DrawRectangle(Brushes.Transparent, this.blackPen, new Rect(this.left, this.top, this.chartWidth, this.chartHeight));
        }

        private void DrawGraph(DrawingContext dc)
        {
            double? lastX = null, lastY = null;

            dc.DrawLine(new Pen(Brushes.Red, 1), new Point(this.left, this.bottom - 100.0 * this.scaleY), new Point(this.left + 100.0 * this.scaleX, this.bottom - 100.0 * this.scaleY));
            dc.DrawLine(new Pen(Brushes.Yellow, 1), new Point(this.left, this.bottom - 50.0 * this.scaleY), new Point(this.left + 100.0 * this.scaleX, this.bottom - 50.0 * this.scaleY));
            dc.DrawLine(new Pen(Brushes.Green, 1), new Point(this.left, this.bottom - 0.0 * this.scaleY), new Point(this.left + 100.0 * this.scaleX, this.bottom - 0.0 * this.scaleY));

            foreach (var obj in this.DataSource)
            {
                double x = this.XValueMapping(obj);
                double y = this.YValueMapping(obj);

                if (lastX != null && lastY != null)
                {
                    //dc.DrawLine(this.blackPen, new Point(this.left + lastX.Value * this.scaleX, this.bottom - lastY.Value * this.scaleY), new Point(this.left + x * this.scaleX, this.bottom - y * this.scaleY));
                    dc.DrawLine(this.blackPen, new Point(this.ToPixelValueX(lastX.Value), this.ToPixelValueY(lastY.Value)), new Point(this.ToPixelValueX(x), this.ToPixelValueY(y)));
                }

                lastX = x;
                lastY = y;
            }
        }
    }
}
