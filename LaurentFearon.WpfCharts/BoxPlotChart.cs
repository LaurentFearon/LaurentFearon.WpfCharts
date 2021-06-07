using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LaurentFearon.WpfCharts
{
    public class BoxPlotChart : ChartBase
    {
        private ToolTip toolTip = new System.Windows.Controls.ToolTip() { Content = " " };
        public BoxPlotChart()
        {
            this.ToolTip = this.toolTip;
        }

        protected override void OnRender(DrawingContext dc)
        {
            this.CalcBoundaries();

            if (this.DataSource != null && this.MaxValueMapping != null && this.MinValueMapping != null && this.FirstQuartileValueMapping != null && this.ThirdQuartileValueMapping != null && ((this.UseCategoricalAxis == false && this.XValueMapping != null) || (this.UseCategoricalAxis && this.CategoryMapping != null)))
            {
                this.DrawAxes(dc);
                this.DrawGraph(dc);
            }

            base.OnRender(dc);
        }

        private void DrawGraph(DrawingContext dc)
        {
            this.rects.Clear();

            int elementCount = 0;
            foreach(var obj in this.DataSource)
            {
                elementCount++;
            }

            double bxWidth = this.BoxWidth;
            double sumAllWidth = elementCount * (bxWidth+8);
            while (sumAllWidth > this.chartWidth && bxWidth > 1)
            {
                bxWidth--;
                sumAllWidth = elementCount * (bxWidth+8);
            }

            double maximumLineWidth = this.MaxLineWidth;
            double minimumLineWidth = this.MinLineWidth;
            if(elementCount * (maximumLineWidth + 8) > this.chartWidth)
            {
                maximumLineWidth = bxWidth;
            }
            if (elementCount * (minimumLineWidth + 8) > this.chartWidth)
            {
                minimumLineWidth = bxWidth;
            }

            double outlierRadius = this.OutliersRadius;
            if (elementCount * (outlierRadius + 8) > this.chartWidth)
            {
                outlierRadius = bxWidth;
            }

            if (this.UseCategoricalAxis)
            {
                for (int i = 0; i < this.Categories.Count(); i++)
                {
                    string category = this.Categories.ElementAt(i);
                    List<object> items = new List<object>();
                    foreach (var obj in this.DataSource)
                    {
                        if (this.CategoryMapping(obj) == category)
                        {
                            items.Add(obj);
                        }
                    }

                    double sumWidth = items.Count * bxWidth;
                    double startX = this.ToPixelValueX(i + 0.5) - sumWidth / 2.0;

                    int count = 0;
                    foreach (var obj in items)
                    {
                        Brush boxFillBrush = null;
                        if (this.SeriesBrushes.ContainsKey(count))
                        {
                            boxFillBrush = this.SeriesBrushes[count];
                        }
                        double x = startX + count * bxWidth + bxWidth / 2.0;
                        this.DrawBox(dc, this.PixelValueXToValueX(x), bxWidth, maximumLineWidth, minimumLineWidth, outlierRadius, obj, boxFillBrush);
                        count++;
                    }
                }
            }
            else
            {
                foreach (var obj in this.DataSource)
                {
                    double x = this.XValueMapping(obj);
                    this.DrawBox(dc, x, bxWidth, maximumLineWidth, minimumLineWidth, outlierRadius, obj);
                }
            }
        }

        private void DrawBox(DrawingContext dc, double x, double bxWidth, double maximumLineWidth, double minimumLineWidth, double outlierRadius, object obj, Brush boxFillBrush = null)
        {
            if (boxFillBrush == null)
            {
                boxFillBrush = this.BoxFill;
            }

            double maxValue = this.MaxValueMapping(obj);
            double minValue = this.MinValueMapping(obj);
            double q1 = this.FirstQuartileValueMapping(obj);
            double q3 = this.ThirdQuartileValueMapping(obj);
            double medianValue = double.MinValue;
            if (this.MedianValueMapping != null)
            {
                medianValue = this.MedianValueMapping(obj);
            }
            IEnumerable<double> outliers = null;
            if (this.OutliersMapping != null)
            {
                outliers = this.OutliersMapping(obj);
            }

            //Draw Max Line
            dc.DrawLine(this.StrokePen, new Point(this.ToPixelValueX(x) - maximumLineWidth / 2.0, this.ToPixelValueY(maxValue)), new Point(this.ToPixelValueX(x) + maximumLineWidth / 2.0, this.ToPixelValueY(maxValue)));
            //Drawe Min Line
            dc.DrawLine(this.StrokePen, new Point(this.ToPixelValueX(x) - minimumLineWidth / 2.0, this.ToPixelValueY(minValue)), new Point(this.ToPixelValueX(x) + minimumLineWidth / 2.0, this.ToPixelValueY(minValue)));

            //Draw connecting line from max to box
            dc.DrawLine(this.StrokePen, new Point(this.ToPixelValueX(x), this.ToPixelValueY(maxValue)), new Point(this.ToPixelValueX(x), this.ToPixelValueY(q3)));
            //Draw connecting line from min to box
            dc.DrawLine(this.StrokePen, new Point(this.ToPixelValueX(x), this.ToPixelValueY(minValue)), new Point(this.ToPixelValueX(x), this.ToPixelValueY(q1)));

            //Draw Box
            dc.DrawRoundedRectangle(boxFillBrush, this.BoxStrokePen, new Rect(this.ToPixelValueX(x) - bxWidth / 2.0, this.ToPixelValueY(q3), (this.ToPixelValueX(x) + bxWidth / 2.0) - (this.ToPixelValueX(x) - bxWidth / 2.0), this.ToPixelValueY(q1) - this.ToPixelValueY(q3)), this.CornerRadius, this.CornerRadius);

            if (medianValue > double.MinValue)
            {
                //Draw Median Line
                dc.DrawLine(this.MedianStrokePen, new Point(this.ToPixelValueX(x) - bxWidth / 2.0, this.ToPixelValueY(medianValue)), new Point(this.ToPixelValueX(x) + bxWidth / 2.0, this.ToPixelValueY(medianValue)));
            }

            if (outliers != null && outliers.Count() > 0)
            {
                foreach (var y in outliers)
                {
                    dc.DrawEllipse(this.OutliersFill, this.OutliersStrokePen, new Point(this.ToPixelValueX(x), this.ToPixelValueY(y)), outlierRadius, outlierRadius);
                }
            }

            this.rects.Add(new KeyValuePair<Rect, object>(new Rect(this.ToPixelValueX(x) - bxWidth / 2.0, this.ToPixelValueY(maxValue), (this.ToPixelValueX(x) + bxWidth / 2.0) - (this.ToPixelValueX(x) - bxWidth / 2.0), this.ToPixelValueY(minValue) - this.ToPixelValueY(maxValue)), obj));
        }

        List<KeyValuePair<Rect, object>> rects = new List<KeyValuePair<Rect, object>>();

        protected override void OnMouseMove(MouseEventArgs e)
        {
            //base.OnMouseMove(e);

            Point pt = Mouse.GetPosition(this);
            var selectedRect = rects.Where(x => x.Key.Contains(pt)).FirstOrDefault();
            if (selectedRect.Key != default(Rect))
            {
                //ToolTipService.SetShowDuration(this, 3000);
                //this.ToolTip = "AAA";
                //base.OnToolTipOpening(e);
                object obj = selectedRect.Value;
                //double x = this.XValueMapping(obj);
                double maxValue = this.MaxValueMapping(obj);
                double minValue = this.MinValueMapping(obj);
                double q1 = this.FirstQuartileValueMapping(obj);
                double q3 = this.ThirdQuartileValueMapping(obj);

                double medianValue = double.MinValue;
                if (this.MedianValueMapping != null)
                {
                    medianValue = this.MedianValueMapping(obj);
                }
                string category = this.CategoryMapping != null ? this.CategoryMapping(obj) : "";
                string description = this.DescriptionMapping != null ? this.DescriptionMapping(obj) : "";


                string toolTipContent = string.Format("Max:\t{0}\r\nQ3:\t{1}\r\nMed:\t{4}\r\nQ1:\t{2}\r\nMin:\t{3}", maxValue.ToString("0.######"), q3.ToString("0.######"), q1.ToString("0.######"), minValue.ToString("0.######"), medianValue > double.MinValue + 1 ? medianValue.ToString("0.######") : "");
                if (!string.IsNullOrEmpty(category))
                {
                    if (category.Contains("\n"))
                    {
                        toolTipContent += string.Format("\r\nCat:\t{0}", category.Insert(category.IndexOf("\n") + 1, "\t"));
                    }
                    else
                    {
                        toolTipContent += string.Format("\r\nCat:\t{0}", category);
                    }
                }
                if (!string.IsNullOrEmpty(description))
                {
                    toolTipContent += string.Format("\r\nDescr:\t{0}", description);
                }
                this.toolTip.Content = toolTipContent;
                this.toolTip.IsOpen = true;
                //if (this.toolTip.PlacementRectangle != Rect.Empty)
                //{
                //    this.toolTip.PlacementRectangle = new Rect(this.toolTip.PlacementRectangle.X, pt.Y, this.toolTip.PlacementRectangle.Width, this.toolTip.PlacementRectangle.Height);
                //}
            }
            else
            {
                //ToolTipService.SetShowDuration(this, 0);
                //e.Handled = true;
                this.toolTip.IsOpen = false;
            }
        }

        private Pen strokePen = null;
        private Pen StrokePen
        {
            get
            {
                if (this.strokePen == null || this.strokePen.Brush != this.StrokeBrush || this.strokePen.Thickness != this.StrokeThickness)
                {
                    this.strokePen = new Pen(this.StrokeBrush, this.StrokeThickness);
                }

                return this.strokePen;
            }
        }

        private Pen boxStrokePen = null;
        private Pen BoxStrokePen
        {
            get
            {
                if (this.boxStrokePen == null || this.boxStrokePen.Brush != this.BoxStroke || this.boxStrokePen.Thickness != this.BoxStrokeThickness)
                {
                    this.boxStrokePen = new Pen(this.BoxStroke, this.BoxStrokeThickness);
                }

                return this.boxStrokePen;
            }
        }

        private Pen outliersStrokePen = null;
        private Pen OutliersStrokePen
        {
            get
            {
                if (this.outliersStrokePen == null || this.outliersStrokePen.Brush != this.OutliersStroke || this.outliersStrokePen.Thickness != this.OutliersStrokeThickness)
                {
                    this.outliersStrokePen = new Pen(this.OutliersStroke, this.OutliersStrokeThickness);
                }

                return this.outliersStrokePen;
            }
        }

        private Pen medianStrokePen = null;
        private Pen MedianStrokePen
        {
            get
            {
                if (this.medianStrokePen == null || this.medianStrokePen.Brush != this.MedianStroke || this.medianStrokePen.Thickness != this.MedianStrokeThickness)
                {
                    this.medianStrokePen = new Pen(this.MedianStroke, this.MedianStrokeThickness);
                }

                return this.medianStrokePen;
            }
        }

        protected override DataInfo GetDataInfo()
        {
            this.values.Clear();
            if (this.DataSource != null && this.MaxValueMapping != null && this.MinValueMapping != null && ((this.XValueMapping != null && !this.UseCategoricalAxis) || (this.CategoryMapping != null && this.UseCategoricalAxis)))
            {
                DataInfo di = new DataInfo();

                List<string> categories = new List<string>();
                if (this.UseCategoricalAxis && this.CategoryMapping != null)
                {
                    foreach (var obj in this.DataSource)
                    {
                        categories.Add(this.CategoryMapping(obj));
                    }
                }
                di.Categories = categories;

                foreach (var dataItem in this.DataSource)
                {
                    this.DetermineMinMaxForDataItem(dataItem, di, categories);
                }

                return di;
            }

            return DataInfo.Empty;
        }

        private void DetermineMinMaxForDataItem(object dataItem, DataInfo di, List<string> categories)
        {
            double x = 0;
            if (this.UseCategoricalAxis && this.CategoryMapping != null)
            {
                string category = this.CategoryMapping(dataItem);
                x = categories.IndexOf(category);
            }
            else
            {
                x = this.XValueMapping(dataItem);
            }

            double y = this.MaxValueMapping(dataItem);
            this.values.Add(new Point(x, y));

			if (x > di.MaxX)
			{
				di.MaxX = x;
			}
			if (x < di.MinX)
			{
				di.MinX = x;
			}
			if (y > di.MaxY)
			{
				di.MaxY = y;
			}
			if (y < di.MinY)
			{
				di.MinY = y;
			}

			y = this.MinValueMapping(dataItem);
            if (y > di.MaxY)
            {
                di.MaxY = y;
            }
            if (y < di.MinY)
            {
                di.MinY = y;
            }

            if (this.OutliersMapping != null)
            {
                var outliers = this.OutliersMapping(dataItem);

                if (outliers != null)
                {
                    foreach (var outlier in outliers)
                    {
                        if (outlier > di.MaxY)
                        {
                            di.MaxY = outlier;
                        }
                        if (outlier < di.MinY)
                        {
                            di.MinY = outlier;
                        }
                    }
                }
            }
        }

        public Func<object, double> MaxValueMapping { get; set; }
        public Func<object, double> MinValueMapping { get; set; }
        public Func<object, double> FirstQuartileValueMapping { get; set; }
        public Func<object, double> ThirdQuartileValueMapping { get; set; }
        public Func<object, double> MedianValueMapping { get; set; }
        public Func<object, IEnumerable<double>> OutliersMapping { get; set; }
        public double BoxWidth { get; set; } = 16.0;
        public double MinLineWidth { get; set; } = 16.0;
        public double MaxLineWidth { get; set; } = 16.0;
        public Brush BoxFill { get; set; } = Brushes.Gainsboro;
        public Brush BoxStroke { get; set; } = Brushes.Black;
        public double BoxStrokeThickness { get; set; } = 1.0;
        public Brush StrokeBrush { get; set; } = Brushes.Black;
        public double StrokeThickness { get; set; } = 1.0;
        public Brush MedianStroke { get; set; } = Brushes.Black;
        public double MedianStrokeThickness { get; set; } = 1.0;
        public Brush OutliersFill { get; set; } = Brushes.Gainsboro;
        public Brush OutliersStroke { get; set; } = Brushes.Black;
        public double OutliersStrokeThickness { get; set; } = 1.0;
        public double OutliersRadius { get; set; } = 4.0;
        public double CornerRadius { get; set; } = 6;
    }
}
