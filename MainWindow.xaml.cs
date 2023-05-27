using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using P2108Comparer.Converters;
using P2108Comparer.PropModels;
using P2108Comparer.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        /// <summary>
        /// Is analysis able to be exported to CSV
        /// </summary>
        private bool _isExportable = false;

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
        }

        private void RenderSingleCurve()
        {
            var inputControl = grid_InputControls.Children[0] as SingleCurveInputsControl;
            double f__ghz = inputControl.f__ghz;
            double theta__deg = inputControl.theta__deg;
            double h__meter = inputControl.h__meter;
            var scenerio = inputControl.Scenerio;

            PlotModel.Series.Clear();

            // generate time curves
            var p2108Series = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                StrokeThickness = 2,
                Title = "Rec P.2108-1",
                Color = Constants.Colors[0]
            };
            var TEMP2Series = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                StrokeThickness = 2,
                Title = "Proposed Revision",
                Color = Constants.Colors[1]
            };
            for (double p = 0.01; p <= 99.99; p += 0.01)
            {
                P2108.AeronauticalStatisticalModel(f__ghz, theta__deg, p, out double L_ces__db);
                p2108Series.Points.Add(new DataPoint(L_ces__db, p));

                TEMP2Model.AeronauticalStatisticalModel(f__ghz, theta__deg, p, h__meter, scenerio, false, out double L_clt__db);
                TEMP2Series.Points.Add(new DataPoint(L_clt__db, p));
            }

            // add series
            PlotModel.Series.Add(p2108Series);
            PlotModel.Series.Add(TEMP2Series);

            // update title
            PlotModel.Title = $"Slant-Path Clutter Model Comparison at {f__ghz} GHz";
            PlotModel.Subtitle = $"theta = {theta__deg} deg; h = {h__meter} meter; {scenerio} (h_s = {(int)scenerio} meter)";

            PlotModel.Legends.Add(new Legend()
            {
                LegendPosition = LegendPosition.BottomRight,
                LegendPlacement = LegendPlacement.Inside,
                LegendOrientation = LegendOrientation.Vertical,
                LegendBorder = OxyColors.Black,
                LegendBackground = OxyColors.White
            });

            plot.InvalidatePlot();
        }

        private void Mi_Exit_Click(object sender, RoutedEventArgs e) => this.Close();

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
                    //Export = CsvExport_SingleCurveInit;

                    userControl = new SingleCurveInputsControl();
                    break;

                /*case PlotMode.MultipleLowTerminals:
                    Render = RenderMultipleLowHeights;
                    Export = CsvExport_MultipleLowTerminals;

                    userControl = new MultipleLowHeightsInputsControl();

                    IsModeOfPropEnabled = false;
                    IsModeOfPropChecked = false;
                    break;

                case PlotMode.MultipleHighTerminals:
                    Render = RenderMultipleHighHeights;
                    Export = CsvExport_MultipleHighTerminals;

                    userControl = new MultipleHighHeightsInputsControl();

                    IsModeOfPropEnabled = false;
                    IsModeOfPropChecked = false;
                    break;

                case PlotMode.MultipleTimes:
                    Render = RenderMultipleTimes;
                    Export = CsvExport_MultipleTimePercentages;

                    userControl = new MultipleTimeInputsControl();

                    IsModeOfPropEnabled = false;
                    IsModeOfPropChecked = false;
                    break;*/
            }

            grid_InputControls.Children.Add(userControl);

            // define binding for input validation errors
            Binding inputErrorBinding = new Binding("ErrorCnt");
            inputErrorBinding.Source = userControl;
            inputErrorBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            inputErrorBinding.Converter = new IntegerToBooleanConverter();

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
    }
}
