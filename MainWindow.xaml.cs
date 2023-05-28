using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using P2108Comparer.Converters;
using P2108Comparer.PropModels;
using P2108Comparer.UserControls;
using P2108Comparer.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace P2108Comparer
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// X-axis for the plot
        /// </summary>
        private readonly LinearAxis _xAxis;

        /// <summary>
        /// Y-axis for the plot
        /// </summary>
        private readonly LinearAxis _yAxis;

        private BackgroundWorker _versionCheckerWorker;

        /// <summary>
        /// Data backing the plot UI control
        /// </summary>
        public PlotModel PlotModel { get; set; }

        private delegate void RenderPlot();
        private RenderPlot Render;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public MainWindow()
        {
            InitializeComponent();

            PlotModel = new PlotModel() { Title = "P.2108 Comparison" };

            // configure x-axis
            _xAxis = new LinearAxis();
            _xAxis.Title = "Clutter Loss (dB)";
            _xAxis.FontSize = 16;
            _xAxis.MajorGridlineStyle = OxyPlot.LineStyle.Dot;
            _xAxis.Position = AxisPosition.Bottom;

            // configure y-axis
            _yAxis = new LinearAxis();
            _yAxis.Title = "Percentage (%)";
            _yAxis.FontSize = 16;
            _yAxis.MajorGridlineStyle = OxyPlot.LineStyle.Dot;
            _yAxis.Position = AxisPosition.Left;
            _yAxis.Minimum = Constants.YAXIS_MIN_DEFAULT;
            _yAxis.Maximum = Constants.YAXIS_MAX_DEFAULT;

            // add axis' to plot
            PlotModel.Axes.Add(_xAxis);
            PlotModel.Axes.Add(_yAxis);

            PlotModel.Background = OxyColors.White;
            PlotModel.TitleFontSize = 20;
            PlotModel.SubtitleFontSize = 16;

            // enable data binding
            DataContext = this;

            Render = RenderSingleCurve;

            _versionCheckerWorker = new BackgroundWorker();
            _versionCheckerWorker.DoWork += _versionCheckerWorker_DoWork;
            _versionCheckerWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Check if an updated version of the application is available for download
        /// </summary>
        private void _versionCheckerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var appVersion = Assembly.GetExecutingAssembly().GetName().Version;

            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident / 6.0)");
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                HttpResponseMessage response = client.GetAsync(@"https://api.github.com/repos/wkozma/p2108-comparer/releases/latest").GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                // parse version number
                var index_start = responseBody.IndexOf("tag_name");
                var index_end = responseBody.IndexOf(",", index_start);
                var version = responseBody.Substring(index_start, index_end - index_start).Replace(@"""", "").Split(':')[1];

                int major = Convert.ToInt32(version.Split('.')[0].Replace("v", ""));
                int minor = Convert.ToInt32(version.Split('.')[1]);

                if (major > appVersion.Major ||
                    major == appVersion.Major && minor > appVersion.Minor)
                    MessageBox.Show("Updated Version Available.\nDownload from GitHub page.\nSee About Window for link.", "Update Available", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                // do nothing is something fails
            }
        }

        /// <summary>
        /// Render a single curve comparison
        /// </summary>
        private void RenderSingleCurve()
        {
            // grab values from input control
            var inputControl = grid_InputControls.Children[0] as SingleCurveInputsControl;
            double f__ghz = inputControl.f__ghz;
            double theta__deg = inputControl.theta__deg;
            double h__meter = inputControl.h__meter;
            var scenerio = inputControl.Scenerio;
            var antenna = inputControl.Antenna;

            // clear any current data on the plot
            PlotModel.Series.Clear();

            // set up line series
            var p2108Series = new LineSeries()
            {
                LineStyle = LineStyle.Dot,
                StrokeThickness = 2,
                Title = "Rec P.2108-1",
                Color = Constants.Colors[0]
            };
            var TEMP2Series = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                StrokeThickness = 2,
                Title = "Proposed Revision",
                Color = Constants.Colors[0]
            };

            // generate curve data from 0.01% to 99.99%
            for (double p = 0.01; p <= 99.99; p += 0.01)
            {
                P2108.AeronauticalStatisticalModel(f__ghz, theta__deg, p, out double L_ces__db);
                p2108Series.Points.Add(new DataPoint(L_ces__db, p));

                TEMP2Model.AeronauticalStatisticalModel(f__ghz, theta__deg, p, h__meter, scenerio, antenna, out double L_clt__db);
                TEMP2Series.Points.Add(new DataPoint(L_clt__db, p));
            }

            // add series to plot
            PlotModel.Series.Add(p2108Series);
            PlotModel.Series.Add(TEMP2Series);

            // update plot title
            PlotModel.Title = $"Slant-Path Clutter Model Comparison at {f__ghz} GHz";
            PlotModel.Subtitle = $"theta = {theta__deg} deg; h = {h__meter} meter; {scenerio} (h_s = {(int)scenerio} meter)";

            // add legend to plot
            PlotModel.Legends.Add(new Legend()
            {
                LegendPosition = OxyPlot.Legends.LegendPosition.BottomRight,
                LegendPlacement = LegendPlacement.Inside,
                LegendOrientation = LegendOrientation.Vertical,
                LegendBorder = OxyColors.Black,
                LegendBackground = OxyColors.White
            });

            // force plot redraw
            plot.InvalidatePlot();
            ResetPlotAxis();
        }

        private void RenderMultipleFrequencies()
        {
            // grab values from input control
            var inputControl = grid_InputControls.Children[0] as MultipleFrequenciesInputControl;
            var fs__ghz = inputControl.frequencies;
            double theta__deg = inputControl.theta__deg;
            double h__meter = inputControl.h__meter;
            var scenerio = inputControl.Scenerio;
            var antenna = inputControl.Antenna;

            // clear any current data on the plot
            PlotModel.Series.Clear();

            for (int i = 0; i < fs__ghz.Count; i++)
            {
                double f__ghz = fs__ghz[i];
                var color = Constants.Colors[i %  Constants.Colors.Length];

                // set up line series
                var p2108Series = new LineSeries()
                {
                    LineStyle = LineStyle.Dot,
                    StrokeThickness = 2,
                    Title = $"Rec P.2108-1, {f__ghz} GHz",
                    Color = color
                };
                var TEMP2Series = new LineSeries()
                {
                    LineStyle = LineStyle.Solid,
                    StrokeThickness = 2,
                    Title = $"Proposed Revision, {f__ghz} GHz",
                    Color = color
                };

                // generate curve data from 0.01% to 99.99%
                for (double p = 0.01; p <= 99.99; p += 0.01)
                {
                    P2108.AeronauticalStatisticalModel(f__ghz, theta__deg, p, out double L_ces__db);
                    p2108Series.Points.Add(new DataPoint(L_ces__db, p));

                    TEMP2Model.AeronauticalStatisticalModel(f__ghz, theta__deg, p, h__meter, scenerio, antenna, out double L_clt__db);
                    TEMP2Series.Points.Add(new DataPoint(L_clt__db, p));
                }

                // add series to plot
                PlotModel.Series.Add(p2108Series);
                PlotModel.Series.Add(TEMP2Series);

                // update plot title
                PlotModel.Title = $"Slant-Path Clutter Model Comparison at Multiple Frequencies";
                PlotModel.Subtitle = $"theta = {theta__deg} deg; h = {h__meter} meter; {scenerio} (h_s = {(int)scenerio} meter)";
            }

            // add legend to plot
            PlotModel.Legends.Add(new Legend()
            {
                LegendPosition = OxyPlot.Legends.LegendPosition.BottomRight,
                LegendPlacement = LegendPlacement.Inside,
                LegendOrientation = LegendOrientation.Vertical,
                LegendBorder = OxyColors.Black,
                LegendBackground = OxyColors.White
            });

            // force plot redraw
            plot.InvalidatePlot();
            ResetPlotAxis();
        }

        private void RenderMultipleElevationAngles()
        {
            // grab values from input control
            var inputControl = grid_InputControls.Children[0] as MultipleElevationAnglesInputControl;
            double f__ghz = inputControl.f__ghz;
            var angles = inputControl.angles;
            double h__meter = inputControl.h__meter;
            var scenerio = inputControl.Scenerio;
            var antenna = inputControl.Antenna;

            // clear any current data on the plot
            PlotModel.Series.Clear();

            for (int i = 0; i < angles.Count; i++)
            {
                double theta__deg = angles[i];
                var color = Constants.Colors[i % Constants.Colors.Length];

                // set up line series
                var p2108Series = new LineSeries()
                {
                    LineStyle = LineStyle.Dot,
                    StrokeThickness = 2,
                    Title = $"Rec P.2108-1, {theta__deg} deg",
                    Color = color
                };
                var TEMP2Series = new LineSeries()
                {
                    LineStyle = LineStyle.Solid,
                    StrokeThickness = 2,
                    Title = $"Proposed Revision, {theta__deg} deg",
                    Color = color
                };

                // generate curve data from 0.01% to 99.99%
                for (double p = 0.01; p <= 99.99; p += 0.01)
                {
                    P2108.AeronauticalStatisticalModel(f__ghz, theta__deg, p, out double L_ces__db);
                    p2108Series.Points.Add(new DataPoint(L_ces__db, p));

                    TEMP2Model.AeronauticalStatisticalModel(f__ghz, theta__deg, p, h__meter, scenerio, antenna, out double L_clt__db);
                    TEMP2Series.Points.Add(new DataPoint(L_clt__db, p));
                }

                // add series to plot
                PlotModel.Series.Add(p2108Series);
                PlotModel.Series.Add(TEMP2Series);

                // update plot title
                PlotModel.Title = $"Slant-Path Clutter Model Comparison at Multiple Elevation Angles";
                PlotModel.Subtitle = $"freq = {f__ghz} GHz; h = {h__meter} meter; {scenerio} (h_s = {(int)scenerio} meter)";
            }

            // add legend to plot
            PlotModel.Legends.Add(new Legend()
            {
                LegendPosition = OxyPlot.Legends.LegendPosition.BottomRight,
                LegendPlacement = LegendPlacement.Inside,
                LegendOrientation = LegendOrientation.Vertical,
                LegendBorder = OxyColors.Black,
                LegendBackground = OxyColors.White
            });

            // force plot redraw
            plot.InvalidatePlot();
            ResetPlotAxis();
        }

        /// <summary>
        /// Exit the application
        /// </summary>
        private void Mi_Exit_Click(object sender, RoutedEventArgs e) => this.Close();

        /// <summary>
        /// Export the plot as a PNG image to a user-defined file
        /// </summary>
        private void Mi_SaveAsImage_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog
            {
                DefaultExt = ".png",
                Filter = "Portable Network Graphics (.png)|*.png"
            };

            if (fileDialog.ShowDialog().Value)
            {
                var pngExporter = new OxyPlot.Wpf.PngExporter
                {
                    Width = 600,
                    Height = 400
                };
                OxyPlot.Wpf.ExporterExtensions.ExportToFile(pngExporter, PlotModel, fileDialog.FileName);
            }
        }

        /// <summary>
        /// Update the plot
        /// </summary>
        private void Btn_Render_Click(object sender, RoutedEventArgs e) => Render();

        void Command_PlotMode(object sender, ExecutedRoutedEventArgs e)
        {
            // grab the command parameter
            var plotMode = (PlotMode)e.Parameter;

            // set the appropriate menu check
            foreach (MenuItem mi in mi_Mode.Items)
                mi.IsChecked = (PlotMode)mi.CommandParameter == plotMode;

            // reset the UI
            grid_InputControls.Children.Clear();
            PlotModel.Series.Clear();
            plot.InvalidatePlot();

            UserControl userControl = null;

            // set the application with the correct UI elements and configuration
            switch (plotMode)
            {
                case PlotMode.Single:
                    Render = RenderSingleCurve;

                    userControl = new SingleCurveInputsControl();
                    break;

                case PlotMode.MultipleFrequencies:
                    Render = RenderMultipleFrequencies;

                    userControl = new MultipleFrequenciesInputControl();
                    break;

                case PlotMode.MultipleElevationAngles:
                    Render = RenderMultipleElevationAngles;

                    userControl = new MultipleElevationAnglesInputControl();
                    break;
            }

            grid_InputControls.Children.Add(userControl);

            // define binding for input validation errors
            Binding inputErrorBinding = new Binding("ErrorCnt");
            inputErrorBinding.Source = userControl;
            inputErrorBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            inputErrorBinding.Converter = new IntegerToBooleanConverter();

            // bind the Render button to the input error count
            BindingOperations.SetBinding(btn_Render, IsEnabledProperty, inputErrorBinding);

            // force update the view
            PlotModel.Series.Clear();
            plot.InvalidatePlot();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // initialized application for Single Curve Mode
            var command = PlotModeCommand.Command;
            command.Execute(PlotMode.Single);
        }

        private void Mi_About_Click(object sender, RoutedEventArgs e)
            => new AboutWindow().ShowDialog();

        private void Mi_SetAxisLimits_Click(object sender, RoutedEventArgs e)
        {
            var limitsWndw = new SetAxisWindow()
            {
                XAxisMaximum = _xAxis.ActualMaximum,
                XAxisMinimum = _xAxis.ActualMinimum,
                YAxisMaximum = _yAxis.ActualMaximum,
                YAxisMinimum = _yAxis.ActualMinimum,
            };

            if (!limitsWndw.ShowDialog().Value)
                return;

            _xAxis.Maximum = limitsWndw.XAxisMaximum;
            _xAxis.Minimum = limitsWndw.XAxisMinimum;

            _yAxis.Maximum = limitsWndw.YAxisMaximum;
            _yAxis.Minimum = limitsWndw.YAxisMinimum;

            plot.InvalidatePlot();
        }

        private void Mi_ResetAxisLimits_Click(object sender, RoutedEventArgs e) => ResetPlotAxis();

        private void ResetPlotAxis()
        {
            _xAxis.Minimum = Double.NaN;
            _xAxis.Minimum = Double.NaN;
            _yAxis.Maximum = Double.NaN;
            _yAxis.Minimum = Double.NaN;

            plot.ResetAllAxes();
        }

        private void CommandBinding_LegendPosition(object sender, ExecutedRoutedEventArgs e)
        {
            // clear checks
            foreach (MenuItem mi in mi_View_LegendPosition.Items)
                mi.IsChecked = false;

            var position = (LegendPosition)e.Parameter;

            switch (position)
            {
                case LegendPosition.Northwest:
                    PlotModel.Legends[0].LegendPosition = OxyPlot.Legends.LegendPosition.TopLeft;
                    mi_LegendPosition_Northwest.IsChecked = true;
                    break;
                case LegendPosition.Northeast:
                    PlotModel.Legends[0].LegendPosition = OxyPlot.Legends.LegendPosition.TopRight;
                    mi_LegendPosition_Northeast.IsChecked = true;
                    break;
                case LegendPosition.Southwest:
                    PlotModel.Legends[0].LegendPosition = OxyPlot.Legends.LegendPosition.BottomLeft;
                    mi_LegendPosition_Southwest.IsChecked= true;
                    break;
                case LegendPosition.Southeast:
                    PlotModel.Legends[0].LegendPosition = OxyPlot.Legends.LegendPosition.BottomRight;
                    mi_LegendPosition_Southeast.IsChecked = true;
                    break;
            }

            plot.InvalidatePlot();
        }
    }
}
