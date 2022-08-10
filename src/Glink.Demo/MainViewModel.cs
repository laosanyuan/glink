using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Glink.Demo
{
    [ObservableObject]
    public partial class MainViewModel
    {
        #region [Fields]
        private readonly SKColor _greenColor = SKColor.Parse("#1BE08E");
        private readonly SKColor _redColor = SKColor.Parse("#F23857");
        private readonly SKColor _blueColor = SKColor.Parse("#118AF7");
        private readonly SKColor _foreground = SKColor.Parse("#61688A");
        private readonly SKColor _lineColor = SKColor.Parse("#303859");

        // ma5数据
        private readonly ObservableCollection<DateTimePoint> _ma5Values = new ObservableCollection<DateTimePoint>();
        // 秒k数据
        private readonly ObservableCollection<FinancialPoint> _secondValues = new ObservableCollection<FinancialPoint>();
        // 分k数据
        private readonly ObservableCollection<FinancialPoint> _minuteValues = new ObservableCollection<FinancialPoint>();
        #endregion

        #region [Properties]

        #region [坐标轴设置]
        /// <summary>
        /// 秒k线x轴设置
        /// </summary>
        public Axis[] SecondXAxes { get; private set; } = null!;
        /// <summary>
        /// 分钟k线x轴设置
        /// </summary>
        public Axis[] MinuteXAxes { get; private set; } = null!;
        /// <summary>
        /// 通用y轴设置
        /// </summary>
        public Axis[] YAxes { get; private set; } = null!;
        #endregion

        #region [绘图序列设置]
        public ISeries[] SecondSeries { get; private set; } = null!;

        public ISeries[] MinuteSeries { get; private set; } = null!;

        public ISeries[] Ma5Series { get; private set; } = null!;
        #endregion

        public object Ma5Sync { get; } = new object();

        public object SecondSync { get; } = new object();

        public object MinuteSync { get; } = new object();

        /// <summary>
        /// Debug下调试
        /// </summary>
        public Visibility SupportTest { get; set; } = Visibility.Hidden;
        #endregion

        public MainViewModel()
        {
#if DEBUG
            this.SupportTest = Visibility.Visible;
#endif
            this.Ma5Series = new ISeries[]
{
                new LineSeries<DateTimePoint>
                {
                    Name = "MA5",
                    Values = _ma5Values,
                    GeometrySize = 0,
                    AnimationsSpeed = new TimeSpan(0),
                    LineSmoothness = 1,
                    Stroke = new SolidColorPaint(_blueColor) { StrokeThickness = 1 },
                },
};
            this.SecondSeries = new ISeries[]
            {
                new CandlesticksSeries<FinancialPoint>
                {
                    Values = _secondValues,
                    MaxBarWidth = 3,
                    AnimationsSpeed = new TimeSpan(0),
                    DownStroke  = new SolidColorPaint(_greenColor) { StrokeThickness = 1 },
                    UpStroke  = new SolidColorPaint(_redColor) { StrokeThickness = 1 },
                    DownFill = new SolidColorPaint(_greenColor) { StrokeThickness = 1 },
                    UpFill = new SolidColorPaint(_redColor) { StrokeThickness = 1 },
                }
            };

            this.MinuteSeries = new ISeries[]
            {
                new CandlesticksSeries<FinancialPoint>
                {
                    Values = _minuteValues,
                    MaxBarWidth = 3,
                    AnimationsSpeed = new TimeSpan(0),
                    DownStroke  = new SolidColorPaint(_greenColor) { StrokeThickness = 1 },
                    UpStroke  = new SolidColorPaint(_redColor) { StrokeThickness = 1 },
                    DownFill = new SolidColorPaint(_greenColor) { StrokeThickness = 1 },
                    UpFill = new SolidColorPaint(_redColor) { StrokeThickness = 1 },
                }
            };

            this.YAxes = new Axis[]
            {
                new Axis
                {
                    SeparatorsPaint = new SolidColorPaint(_lineColor) { StrokeThickness = 1 },
                    LabelsPaint = new SolidColorPaint(_foreground) { StrokeThickness = 1 },
                    LabelsRotation = 15,
                }
            };

            this.SecondXAxes = new Axis[] {
                new Axis
                {
                    SeparatorsPaint = new SolidColorPaint(_lineColor){ StrokeThickness = 1},
                    LabelsPaint = new SolidColorPaint(_foreground){ StrokeThickness = 1},
                    LabelsRotation = 15,
                    Labeler = value => new DateTime((long)value).ToString("HH:mm:ss"),
                    UnitWidth = TimeSpan.FromSeconds(1).Ticks,
                }
            };

            this.MinuteXAxes = new Axis[]
            {
                new Axis
                {
                    SeparatorsPaint = new SolidColorPaint(_lineColor){ StrokeThickness = 1},
                    LabelsPaint = new SolidColorPaint(_foreground){ StrokeThickness = 1},
                    LabelsRotation = 15,
                    Labeler = value => new DateTime((long)value).ToString("HH:mm"),
                    UnitWidth = TimeSpan.FromMinutes(0.3).Ticks,
                }
            };
        }

        #region [Methods]
        public void UpdateData((string, byte[]) data)
        {
            if (data.Item2 == null)
            {
                return;
            }
            // id => 
            // 1、5: MA5
            // 3: 秒k
            // 4: 分k
            switch (data.Item1)
            {
                case "1":
                    UpdateMa5(data.Item2);
                    break;
                case "5":
                    if (!doubleMA5Flag)
                    {
                        doubleMA5Flag = true;
                        this._ma5Values.Clear();
                    }
                    UpdateMa5(data.Item2);
                    break;
                case "3":
                    UpdateSecondKLine(data.Item2);
                    break;
                case "4":
                    UpdateMinuteKLine(data.Item2);
                    break;
            };
        }

        private bool doubleMA5Flag = false;

        // 更新MA5数据
        private void UpdateMa5(byte[] data)
        {
            try
            {
                var timeInt = BitConverter.ToInt32(data, 5);
                var ma5 = BitConverter.ToDouble(data, 10);
                lock (Ma5Sync)
                {
                    DateTime tmpTime = ConvertTime(timeInt);

                    if (_ma5Values.Count > 0 && _ma5Values.Last().DateTime >= tmpTime)
                    {
                        var indexItem = _ma5Values.FirstOrDefault(t => t.DateTime > tmpTime);
                        if (indexItem != null)
                        {
                            var index = _ma5Values.IndexOf(indexItem);
                            _ma5Values.Insert(index, new DateTimePoint(tmpTime, ma5));
                        }
                    }
                    else
                    {
                        this._ma5Values.Add(new DateTimePoint(tmpTime, ma5));
                    }
                    if (_ma5Values.Count > 500)
                    {
                        _ma5Values.RemoveAt(0);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        // 更新秒K线数据
        private void UpdateSecondKLine(byte[] data)
        {
            try
            {
                var time = ConvertTime(BitConverter.ToInt32(data, 0));
                var high = BitConverter.ToDouble(data, 5);
                var low = BitConverter.ToDouble(data, 14);
                var open = BitConverter.ToDouble(data, 23);
                var close = BitConverter.ToDouble(data, 32);

                lock (SecondSync)
                {
                    if (this._secondValues.Count <= 0 || time > this._secondValues.LastOrDefault()?.Date)
                    {
                        this._secondValues?.Add(new FinancialPoint(time, high, open, close, low));
                    }
                    if (this._secondValues!.Count > 150)
                    {
                        this._secondValues.RemoveAt(0);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        // 更新分k线数据
        private void UpdateMinuteKLine(byte[] data)
        {
            try
            {
                var time = ConvertTime(BitConverter.ToInt32(data, 0));
                var high = BitConverter.ToDouble(data, 5);
                var low = BitConverter.ToDouble(data, 14);
                var close = BitConverter.ToDouble(data, 32);

                lock (MinuteSync)
                {
                    if (this._minuteValues.Count <= 0)
                    {
                        var open = BitConverter.ToDouble(data, 23);
                        FinancialPoint financialPoint = new FinancialPoint(time, high, open, close, low);
                        this._minuteValues?.Add(financialPoint);
                    }
                    else
                    {
                        if (time == this._minuteValues?.LastOrDefault()?.Date)
                        {
                            var last = this._minuteValues[this._minuteValues.Count - 1];
                            if (high > last.High)
                            {
                                last.High = high;
                            }
                            if (low < last.Low)
                            {
                                last.Low = low;
                            }
                            last.Close = close;
                            return;
                        }
                        else if (time > this._minuteValues?.LastOrDefault()?.Date)
                        {
                            var open = BitConverter.ToDouble(data, 23);
                            FinancialPoint financialPoint = new FinancialPoint(time, high, open, close, low);
                            this._minuteValues?.Add(financialPoint);
                            if (this._minuteValues!.Count > 150)
                            {
                                this._minuteValues.RemoveAt(0);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        // int时间转DateTime，秒
        private DateTime ConvertTime(int timeInt)
        {
            // 09_30_00_000
            var result = new DateTime(
                2022,
                8,
                6,
                timeInt / 1_00_00_000,
                timeInt % 1_00_00_000 / 1_00_000,
                timeInt % 1_00_000 / 1000);
            return result;
        }
        #endregion
    }
}
