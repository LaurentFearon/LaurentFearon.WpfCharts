using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LaurentFearon.WpfCharts
{
    public class ChartBase : Control
    {
        protected double left, top, chartWidth, chartHeight, right, bottom, rangeX, rangeY, scaleX, scaleY, graphWidth, graphHeight;
        private IEnumerable dataSource;
        private Pen transparentPen = new Pen(Brushes.Transparent, 0);
        private SolidColorBrush semiTransparentBrush = new SolidColorBrush(Color.FromArgb(0xB9, 0xFF, 0xFF, 0xFF));
        private Pen crossHairPen = null;
        private Pen gridPen = null;
        private Pen majorTickPen = null;
        private Pen minorTickPen = null;
        private Pen borderPen = null;
        private Pen yAxisborderPen = null;
        private Pen xAxisborderPen = null;
        protected DataInfo currentDataInfo = DataInfo.Empty;

        public ChartBase()
        {
            this.ClipToBounds = true;
            this.BorderBrush = Brushes.Black;
            this.BorderThickness = new Thickness(1);
            this.MouseMove += ChartBase_MouseMove;
        }

        private void ChartBase_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.ShowCrossHairX || this.ShowCrossHairY)
            {
                Point pt = e.GetPosition(this);
                this.CrossHairX = pt.X - this.Padding.Left - this.YAxisWidth; //+ axes width
                this.CrossHairY = pt.Y - this.Padding.Top; //+ axes height

                this.CrossHairXValue = this.CrossHairX / this.scaleX;

                Point ptClosest = this.FindClosestPoint(this.CrossHairXValue);
                this.CrossHairXValue = ptClosest.X;
                this.CrossHairYValue = ptClosest.Y;
                //this.CrossHairYValue = this.CrossHairY / this.scaleY;
                this.InvalidateVisual();
            }
        }

        private Point FindClosestPoint(double x)
        {
            Point pt1 = this.values.Find(z => z.X >= x);
            Point pt2 = this.values.FindLast(z => z.X <= x);

            if (Math.Abs(pt1.X - x) > Math.Abs(pt2.X - x))
            {
                return pt2;
            }
            else
            {
                return pt1;
            }
        }

        protected double CrossHairX { get; set; }
        protected double CrossHairY { get; set; }

        protected double CrossHairXValue { get; set; }
        protected double CrossHairYValue { get; set; }
        protected double YAxisWidthDefault { get; set; } = 40;
        protected double YAxisWidth { get; set; } = 30;
        protected double XAxisHeight { get; set; } = 40;
        public double AxisTextSize { get; set; } = 12;
        public double MinAxisTextSize { get; set; } = 10;
        public double CrossHairRadius { get; set; } = 5.0;
        public bool ShowCrossHairX { get; set; } = true;
        public bool ShowCrossHairY { get; set; } = true;
        public Brush CrossHairBrush { get; set; } = Brushes.DarkGreen;
        public Brush CrossHairTextBrush { get; set; } = Brushes.DarkGreen;
        public Brush CrossHairFillBrush { get; set; } = Brushes.Transparent;
        public double CrossHairThickness { get; set; } = 0.8;
        public double CrossHairTextSize { get; set; } = 13;
        public string CrossHairTextXFormat { get; set; } = "X: {0:0.####}";
        public string CrossHairTextYFormat { get; set; } = "Y: {0:0.####}";
        public bool ShowVerticalGrid { get; set; } = true;
        public bool ShowHorizontalGrid { get; set; } = true;
        public Brush GridBrush { get; set; } = Brushes.DarkGray;
        public double GridThickness { get; set; } = 0.5;
        public Brush MajorTickBrush { get; set; } = Brushes.Black;
        public double MajorTickThickness { get; set; } = 0.5;
        public double MajorTickLength { get; set; } = 10;
        public Brush MinorTickBrush { get; set; } = Brushes.Black;
        public double MinorTickThickness { get; set; } = 0.5;
        public double MinorTickLength { get; set; } = 4;
        public Brush YAxisBorderBrush { get; set; } = Brushes.Black;
        public Brush YAxisFillBrush { get; set; } = Brushes.GhostWhite;
        public double YAxisBorderThickness { get; set; } = 1.0;
        public Brush XAxisBorderBrush { get; set; } = Brushes.Black;
        public Brush XAxisFillBrush { get; set; } = Brushes.GhostWhite;
        public double XAxisBorderThickness { get; set; } = 1.0;
        public double XAxisMinimum { get; set; } = double.MinValue;
        public double XAxisMaximum { get; set; } = double.MaxValue;
        public double YAxisMinimum { get; set; } = double.MinValue;
        public double YAxisMaximum { get; set; } = double.MaxValue;
        public Color GraphLineColor { get; set; } = Colors.Black;
        public Dictionary<int, Brush> SeriesBrushes { get; set; } = new Dictionary<int, Brush>();
        public Func<object, string> CategoryMapping { get; set; }
        public IEnumerable<string> Categories { get; set; } = new List<string>();
        public bool UseCategoricalAxis { get; set; }
        public bool HasDescriptions { get; set; }
        public Func<object, string> DescriptionMapping { get; set; }

        private Pen GridPen
        {
            get
            {
                if (this.gridPen == null || this.gridPen.Brush != this.GridBrush || this.gridPen.Thickness != this.GridThickness)
                {
                    this.gridPen = new Pen(this.GridBrush, this.GridThickness);
                }

                return this.gridPen;
            }
        }

        private Pen MajorTickPen
        {
            get
            {
                if (this.majorTickPen == null || this.majorTickPen.Brush != this.MajorTickBrush || this.majorTickPen.Thickness != this.MajorTickThickness)
                {
                    this.majorTickPen = new Pen(this.MajorTickBrush, this.MajorTickThickness);
                }

                return this.majorTickPen;
            }
        }

        private Pen MinorTickPen
        {
            get
            {
                if (this.minorTickPen == null || this.minorTickPen.Brush != this.MinorTickBrush || this.minorTickPen.Thickness != this.MinorTickThickness)
                {
                    this.minorTickPen = new Pen(this.MinorTickBrush, this.MinorTickThickness);
                }

                return this.minorTickPen;
            }
        }

        private Pen BorderPen
        {
            get
            {
                if (this.borderPen == null || this.borderPen.Brush != this.BorderBrush || this.borderPen.Thickness != this.BorderThickness.Left)
                {
                    this.borderPen = new Pen(this.BorderBrush, this.BorderThickness.Left);
                }

                return this.borderPen;
            }
        }

        private Pen YAxisBorderPen
        {
            get
            {
                if (this.yAxisborderPen == null || this.yAxisborderPen.Brush != this.YAxisBorderBrush || this.yAxisborderPen.Thickness != this.YAxisBorderThickness)
                {
                    this.yAxisborderPen = new Pen(this.YAxisBorderBrush, this.YAxisBorderThickness);
                }

                return this.yAxisborderPen;
            }
        }

        private Pen XAxisBorderPen
        {
            get
            {
                if (this.xAxisborderPen == null || this.xAxisborderPen.Brush != this.XAxisBorderBrush || this.xAxisborderPen.Thickness != this.XAxisBorderThickness)
                {
                    this.xAxisborderPen = new Pen(this.XAxisBorderBrush, this.XAxisBorderThickness);
                }

                return this.xAxisborderPen;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this.IsMouseOver)
            {
                this.DrawCrossHair(drawingContext);
            }

            base.OnRender(drawingContext);
        }

        private void DrawCrossHair(DrawingContext drawingContext)
        {
            if (this.crossHairPen == null)
            {
                this.crossHairPen = new Pen(this.CrossHairBrush, this.CrossHairThickness);
            }

            if (this.ShowCrossHairX)
            {
                //X Crosshairs
                drawingContext.DrawLine(crossHairPen, new Point(this.ToPixelValueX(this.CrossHairXValue), 0), new Point(this.ToPixelValueX(this.CrossHairXValue), this.ToPixelValueY(this.CrossHairYValue) - this.CrossHairRadius));
                drawingContext.DrawLine(crossHairPen, new Point(this.ToPixelValueX(this.CrossHairXValue), this.ToPixelValueY(this.CrossHairYValue) + this.CrossHairRadius), new Point(this.ToPixelValueX(this.CrossHairXValue), this.ActualHeight));
            }

            if (this.ShowCrossHairY)
            {
                //Y Crosshairs
                drawingContext.DrawLine(crossHairPen, new Point(0, this.ToPixelValueY(this.CrossHairYValue)), new Point(this.ToPixelValueX(this.CrossHairXValue) - this.CrossHairRadius, this.ToPixelValueY(this.CrossHairYValue)));
                drawingContext.DrawLine(crossHairPen, new Point(this.ToPixelValueX(this.CrossHairXValue) + this.CrossHairRadius, this.ToPixelValueY(this.CrossHairYValue)), new Point(this.ActualWidth, this.ToPixelValueY(this.CrossHairYValue)));
            }

            if (this.ShowCrossHairX || this.ShowCrossHairY)
            {
                drawingContext.DrawRectangle(this.semiTransparentBrush, this.transparentPen, new Rect(this.ToPixelValueX(this.CrossHairXValue) + 4, this.ToPixelValueY(this.CrossHairYValue) - this.CrossHairTextSize * 3.0, 66, 36));
                drawingContext.DrawEllipse(this.CrossHairFillBrush, this.crossHairPen, new Point(this.ToPixelValueX(this.CrossHairXValue), this.ToPixelValueY(this.CrossHairYValue)), this.CrossHairRadius, this.CrossHairRadius);
                drawingContext.DrawText(new FormattedText(string.Format(this.CrossHairTextYFormat, this.CrossHairYValue), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), this.CrossHairTextSize, this.CrossHairTextBrush), new Point(this.ToPixelValueX(this.CrossHairXValue) + 8, this.ToPixelValueY(this.CrossHairYValue) - this.CrossHairTextSize - this.CrossHairTextSize / 2.0));
                drawingContext.DrawText(new FormattedText(string.Format(this.CrossHairTextXFormat, this.CrossHairXValue), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), this.CrossHairTextSize, this.CrossHairTextBrush), new Point(this.ToPixelValueX(this.CrossHairXValue) + 8, this.ToPixelValueY(this.CrossHairYValue) - this.CrossHairTextSize * 2.0 - this.CrossHairTextSize * 0.75));
            }
        }

        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(nameof(DataSource), typeof(IEnumerable), typeof(ChartBase));

        public IEnumerable DataSource
        {
            get { return this.dataSource; }
            set
            {
                if (this.dataSource != value)
                {
                    this.RemoveOldCollectionChangedEvent();
                    this.dataSource = value;
                    this.OnDataSourceChanged();
                    this.AddCollectionChangedEvent();
                }
            }
        }

        private void AddCollectionChangedEvent()
        {
	        INotifyCollectionChanged ncc = this.dataSource as INotifyCollectionChanged;
	        if (ncc != null)
	        {
		        ncc.CollectionChanged += this.DataSource_CollectionChanged;
	        }
        }

        private void RemoveOldCollectionChangedEvent()
        {
	        if (this.dataSource != null)
	        {
		        INotifyCollectionChanged nccBefore = this.dataSource as INotifyCollectionChanged;
		        if (nccBefore != null)
		        {
			        nccBefore.CollectionChanged -= this.DataSource_CollectionChanged;
		        }
	        }
        }

        private void DataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnDataSourceChanged();
        }

        public Func<object, double> XValueMapping { get; set; }
        public Func<object, double> YValueMapping { get; set; }

        protected virtual void OnDataSourceChanged()
        {
            this.InvalidateVisual();
        }

        public enum RoundDirectionEnum
        {
            UP,
            DOWN
        }
        private static double RoundToNextInterval(double value, double interval, RoundDirectionEnum direction)
        {
            double roundedValue = -1;
            double mod = value % interval;
            if(direction == RoundDirectionEnum.UP)
            {
                roundedValue = value - mod + interval;
            }
            else
            {
                roundedValue = value - mod;
            }

            return roundedValue;
        }

        protected virtual void CalcBoundaries()
        {
            this.YAxisWidth = this.YAxisWidthDefault;

            this.left = this.Padding.Left;
            this.top = this.Padding.Top;
            this.chartWidth = this.ActualWidth - this.Padding.Left - this.Padding.Right;
            this.chartHeight = this.ActualHeight - this.Padding.Top - this.Padding.Bottom;
            this.graphWidth = this.chartWidth - this.YAxisWidth;
            this.graphHeight = this.chartHeight - this.XAxisHeight;
            this.right = this.left + this.chartWidth;
            this.bottom = this.top + this.chartHeight;

            if (this.DataSource != null)
            {
                this.currentDataInfo = this.GetDataInfo();

                System.Diagnostics.Debug.Assert(this.currentDataInfo.MinX >= 0);

                this.currentDataInfo.FactorX = GetDivisionFactor(this.currentDataInfo.MaxX);
                this.currentDataInfo.FactorY = GetDivisionFactor(this.currentDataInfo.MaxY);

                this.currentDataInfo.RoundedMaxX = this.XAxisMaximum < double.MaxValue - 1 ? this.XAxisMaximum : (this.currentDataInfo.MaxX + (this.currentDataInfo.MaxX - this.currentDataInfo.MinX) * 0.05);
                this.currentDataInfo.RoundedMinX = this.XAxisMinimum > double.MinValue + 1 ? this.XAxisMinimum : (this.currentDataInfo.MinX % this.currentDataInfo.FactorX == 0 ? this.currentDataInfo.MinX : (this.currentDataInfo.MinX - Math.Abs(this.currentDataInfo.MinX % this.currentDataInfo.FactorX)));

                double tempRangeY = (this.YAxisMaximum < double.MaxValue - 1 ? this.YAxisMaximum : this.currentDataInfo.MaxY) - (this.YAxisMinimum > double.MinValue + 1 ? this.YAxisMinimum : this.currentDataInfo.MinY);
                if(tempRangeY == 0)
                {
                    tempRangeY = 1;
                }
                double intervalY = this.CalculateInterval(tempRangeY);
                if (this.YAxisMaximum < double.MaxValue - 1)
                {
                    this.currentDataInfo.RoundedMaxY = this.YAxisMaximum;
                }
                else
                {
	                this.currentDataInfo.RoundedMaxY = RoundToNextInterval(this.currentDataInfo.MaxY, intervalY, RoundDirectionEnum.UP) + intervalY;
                }
                if (this.YAxisMinimum > double.MinValue + 1)
                {
                    this.currentDataInfo.RoundedMinY = this.YAxisMinimum;
                }
                else
                {
	                this.currentDataInfo.RoundedMinY = RoundToNextInterval(this.currentDataInfo.MinY, intervalY, RoundDirectionEnum.DOWN) - intervalY;
                }


                var ft = new FormattedText((this.currentDataInfo.RoundedMaxY).ToString("0.##"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), this.AxisTextSize, Brushes.Black);
                var ft2 = new FormattedText((this.currentDataInfo.RoundedMinY).ToString("0.##"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), this.AxisTextSize, Brushes.Black);
                double value = this.currentDataInfo.RoundedMaxY;
                if (ft2.Width > ft.Width)
                {
                    ft = ft2;
                    value = this.currentDataInfo.RoundedMinY;
                }

                double newFontSize = this.AxisTextSize;
                while (ft.Width > this.YAxisWidth - this.MajorTickLength - 2 && newFontSize > this.MinAxisTextSize)
                {
                    newFontSize--;
                    if (newFontSize < 1)
                    {
                        break;
                    }
                    ft = new FormattedText((value).ToString("0.##"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), newFontSize, Brushes.Black);
                }

                if(ft.Width > this.YAxisWidth - this.MajorTickLength - 2)
                {
                    this.YAxisWidth = this.YAxisWidth + (ft.Width - (this.YAxisWidth - this.MajorTickLength - 2)) + 2;
                    this.graphWidth = this.chartWidth - this.YAxisWidth;
                }


                this.rangeX = this.currentDataInfo.RoundedMaxX - this.currentDataInfo.RoundedMinX;
                if(this.UseCategoricalAxis)
                {
                    rangeX = this.currentDataInfo.MaxX + 1;
                }
                this.rangeY = this.currentDataInfo.RoundedMaxY - this.currentDataInfo.RoundedMinY;
                this.scaleX = this.graphWidth / this.rangeX;
                this.scaleY = this.graphHeight / this.rangeY;

                //System.Diagnostics.Debug.WriteLine("_________________________________________________");
                //System.Diagnostics.Debug.WriteLine("RoundedMinX: {0}", this.currentDataInfo.RoundedMinX);
                //System.Diagnostics.Debug.WriteLine("RoundedMaxX: {0}", this.currentDataInfo.RoundedMaxX);
                //System.Diagnostics.Debug.WriteLine("RoundedMinY: {0}", this.currentDataInfo.RoundedMinY);
                //System.Diagnostics.Debug.WriteLine("RoundedMaxY: {0}", this.currentDataInfo.RoundedMaxY);
                //System.Diagnostics.Debug.WriteLine("rangeX: {0}", this.rangeX);
                //System.Diagnostics.Debug.WriteLine("rangeY: {0}", this.rangeY);
                //System.Diagnostics.Debug.WriteLine("scaleX: {0}", this.scaleX);
                //System.Diagnostics.Debug.WriteLine("scaleY: {0}", this.scaleY);
            }
        }

        private static double GetDivisionFactor(double value)
        {
            decimal num = (decimal)value;
            decimal mod = 0.0000000001M;
            while ((num % mod == 0 && num / mod > 10)
                || (num % mod != 0 && num / mod > 10))
            {
                mod *= 10;
            }

            return (double)mod;
        }

        protected List<Point> values = new List<Point>();
        protected virtual DataInfo GetDataInfo()
        {
            this.values.Clear();
            if (this.DataSource != null && this.XValueMapping != null && this.YValueMapping != null)
            {
                DataInfo di = new DataInfo();
                List<string> categories = new List<string>();

                foreach (var obj in this.DataSource)
                {
                    double x = this.XValueMapping(obj);
                    double y = this.YValueMapping(obj);
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

                    if(this.UseCategoricalAxis && this.CategoryMapping != null)
                    {
                        categories.Add(this.CategoryMapping(obj));
                    }
                }

                di.Categories = categories;

                return di;
            }

            return DataInfo.Empty;
        }

        protected double ToPixelValueX(double xVal)
        {
            return this.left + this.YAxisWidth + (xVal - this.currentDataInfo.RoundedMinX) * this.scaleX;
        }

        protected double PixelValueXToValueX(double pixelX)
        {
            return (pixelX - this.left - this.YAxisWidth) / scaleX + this.currentDataInfo.RoundedMinX;
        }

        protected double ToPixelValueY(double yVal)
        {
            return this.bottom - this.XAxisHeight - (yVal - this.currentDataInfo.RoundedMinY) * this.scaleY;
        }

        protected void DrawAxes(DrawingContext dc)
        {
            if(this.UseCategoricalAxis)
            {
                this.Categories = this.currentDataInfo.Categories;
            }

            //Border of entire Chart Control
            dc.DrawRectangle(Brushes.Transparent, this.BorderPen, new Rect(this.left, this.top, this.chartWidth, this.chartHeight));

            //draw right boundary of Y-Axis
            dc.DrawRectangle(this.YAxisFillBrush, this.YAxisBorderPen, new Rect(this.left, this.top, this.YAxisWidth, this.chartHeight));
            //draw top boundary of X-Axis
            dc.DrawRectangle(this.XAxisFillBrush, this.XAxisBorderPen, new Rect(this.left + this.YAxisWidth, this.top + this.graphHeight, this.graphWidth, this.XAxisHeight));

            this.DrawYAxis(dc);

            this.DrawXAxis(dc);

            //todo: make smarter axis labels that jump to smart intervals i.e: from 5 to 20 to 40 to 50 to 100... or from 0.1 to 0.2 to 0.4........
        }

        private void DrawXAxis(DrawingContext dc)
        {
            if (this.UseCategoricalAxis)
            {
                for(int i=0; i<this.Categories.Count(); i++)
                {
                    string category = this.Categories.ElementAt(i);
                    var formattedText = new FormattedText(category, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), this.AxisTextSize, Brushes.Black);

                    double newFontSize = this.AxisTextSize;
                    while (formattedText.Width > (this.ToPixelValueX(1) - this.ToPixelValueX(0)))
                    {
                        newFontSize--;
                        if(newFontSize < 1)
                        {
                            break;
                        }
                        formattedText = new FormattedText(category, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), newFontSize, Brushes.Black);
                    }

                    //X-Axis Text
                    dc.DrawText(formattedText, new Point(this.ToPixelValueX(i + 0.5) - formattedText.Width / 2.0, this.top + this.graphHeight + (this.XAxisHeight-formattedText.Height)/2.0));

                    if (this.ShowVerticalGrid)
                    {
                        //Vertical Grid Lines
                        dc.DrawLine(this.GridPen, new Point(this.ToPixelValueX((i + 1)), this.top), new Point(this.ToPixelValueX((i + 1)), this.top + this.graphHeight));
                    }
                }
            }
            else
            {
                double currX = this.currentDataInfo.RoundedMinX;
                while (currX < this.currentDataInfo.RoundedMaxX)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (i != 0)
                        {
                            //Small X-Axis Ticks
                            dc.DrawLine(this.MinorTickPen, new Point(this.ToPixelValueX(currX + i * this.currentDataInfo.FactorX / 10.0), this.top + this.graphHeight), new Point(this.ToPixelValueX(currX + i * this.currentDataInfo.FactorX / 10.0), this.top + this.graphHeight + this.MinorTickLength));
                        }
                    }

                    //Large X-Axis Ticks
                    dc.DrawLine(this.MajorTickPen, new Point(this.ToPixelValueX(currX), this.top + this.graphHeight), new Point(this.ToPixelValueX(currX), this.top + this.graphHeight + this.MajorTickLength));


                    if (currX == this.currentDataInfo.RoundedMinX)
                    {
                        //X-Axis Text for 0
                        var ft = new FormattedText((currX).ToString("0.######"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), this.AxisTextSize, Brushes.Black);
                        dc.DrawText(ft, new Point(this.ToPixelValueX(currX) + 2, this.top + this.graphHeight + 10));
                    }
                    else
                    {
                        //X-Axis Text
                        var ft = new FormattedText((currX).ToString("0.######"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), this.AxisTextSize, Brushes.Black);
                        dc.DrawText(ft, new Point(this.ToPixelValueX(currX) - ft.Width / 2.0, this.top + this.graphHeight + 10));
                    }

                    if (this.ShowVerticalGrid)
                    {
                        //Vertical Grid Lines
                        dc.DrawLine(this.GridPen, new Point(this.ToPixelValueX(currX), this.top), new Point(this.ToPixelValueX(currX), this.top + this.graphHeight));
                    }

                    currX += this.currentDataInfo.FactorX;
                }
            }
        }

        private void DrawYAxis(DrawingContext dc)
        {
            double yInterval = this.CalculateInterval(this.currentDataInfo.RoundedMaxY - this.currentDataInfo.RoundedMinY);

            double currY = this.currentDataInfo.RoundedMinY;
            bool hasZero = false;
            while (currY < this.currentDataInfo.RoundedMaxY)
            {
                if (this.ShowHorizontalGrid)
                {
                    //Horizontal Grid Lines
                    dc.DrawLine(this.GridPen, new Point(this.left + this.YAxisWidth, this.ToPixelValueY(currY)), new Point(this.left + this.chartWidth, this.ToPixelValueY(currY)));
                }

                for (int i = 0; i < 10; i++)
                {
                    if (i != 0)
                    {
                        //Small Y-Axis Ticks
                        dc.DrawLine(this.MinorTickPen, new Point(this.left + this.YAxisWidth - this.MinorTickLength, this.ToPixelValueY(currY + i * yInterval / 10.0)), new Point(this.left + this.YAxisWidth, this.ToPixelValueY(currY + i * yInterval / 10.0)));
                    }
                }

                //Large Y-Axis Ticks
                dc.DrawLine(this.MajorTickPen, new Point(this.left + this.YAxisWidth - this.MajorTickLength, this.ToPixelValueY(currY)), new Point(this.left + this.YAxisWidth, this.ToPixelValueY(currY)));

                //Y-Axis Text
                var ft = new FormattedText((currY).ToString("0.######"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), this.AxisTextSize, Brushes.Black);

                double newFontSize = this.AxisTextSize;
                while (ft.Width > this.YAxisWidth - this.MajorTickLength - 2 && newFontSize > this.MinAxisTextSize)
                {
                    newFontSize--;
                    if (newFontSize < 1)
                    {
                        break;
                    }
                    ft = new FormattedText((currY).ToString("0.######"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), newFontSize, Brushes.Black);
                }

                dc.DrawText(ft, new Point(this.left + 2, this.ToPixelValueY(currY) - /*this.AxisTextSize / 2.0*/ ft.Height / 2.0));

                if(currY == 0)
                {
                    hasZero = true;
                }

                currY += yInterval;
            }

            if(!hasZero && this.currentDataInfo.RoundedMinY < 0 && this.currentDataInfo.RoundedMaxY > 0)
            {
                dc.DrawLine(this.GridPen, new Point(this.left + this.YAxisWidth - this.MajorTickLength, this.ToPixelValueY(0)), new Point(this.left + this.YAxisWidth, this.ToPixelValueY(0)));

                if (this.ShowHorizontalGrid)
                {
                    var pen = new Pen(this.GridPen.Brush, this.GridPen.Thickness);
                    //pen.MiterLimit 

                    DashStyle ds = new DashStyle();
                    ds.Dashes.Add(32);
                    ds.Dashes.Add(32);
                    pen.DashStyle = ds;

                    //Horizontal Grid Lines
                    dc.DrawLine(pen, new Point(this.left + this.YAxisWidth, this.ToPixelValueY(0)), new Point(this.left + this.chartWidth, this.ToPixelValueY(0)));
                }

                var ft = new FormattedText((0).ToString("0.######"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), this.AxisTextSize, this.GridPen.Brush);
                double newFontSize = this.AxisTextSize;
                while (ft.Width > this.YAxisWidth - this.MajorTickLength - 2 && newFontSize > this.MinAxisTextSize)
                {
                    newFontSize--;
                    if (newFontSize < 1)
                    {
                        break;
                    }
                    ft = new FormattedText((0).ToString("0.######"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), newFontSize, this.GridPen.Brush);
                }
                dc.DrawText(ft, new Point(this.left + 2, this.ToPixelValueY(0) - /*this.AxisTextSize / 2.0*/ ft.Height / 2.0));
            }
        }

        protected double CalculateInterval(double range)
        {
            double x = Math.Pow(10.0, Math.Floor(Math.Log10(range)));
            if (range / x >= 5)
                return x;
            else if (range / (x / 2.0) >= 5)
                return x / 2.0;
            else
                return x / 5.0;
        }

        protected class DataInfo
        {
	        public double MinX { get; set; } = double.MaxValue;
	        public double MaxX { get; set; } = double.MinValue;
	        public double MinY { get; set; } = double.MaxValue;
	        public double MaxY { get; set; } = double.MinValue;

	        public double RoundedMinX { get; set; } = double.MaxValue;
	        public double RoundedMaxX { get; set; } = double.MinValue;
	        public double RoundedMinY { get; set; } = double.MaxValue;
	        public double RoundedMaxY { get; set; } = double.MinValue;

	        public double FactorX { get; set; }
	        public double FactorY { get; set; }

	        public static DataInfo Empty = new DataInfo();
	        public IEnumerable<string> Categories { get; set; }
        }
    }
}
