using OxyPlot;
using OxyPlot.Axes;
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
        /// Is figure able to be saved as image
        /// </summary>
        private bool _isSaveable = false;

        /// <summary>
        /// Data backing the plot UI control
        /// </summary>
        public PlotModel PlotModel { get; set; }

        /// <summary>
        /// Is the plot in an exportable state
        /// </summary>
        public bool IsExportable
        {
            get { return _isExportable; }
            set
            {
                _isExportable = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Is the plot in a saveable state
        /// </summary>
        public bool IsSaveable
        {
            get { return _isSaveable; }
            set
            {
                _isSaveable = value;
                OnPropertyChanged();
            }
        }

        private delegate void RenderPlot();
        private RenderPlot Render;

        private delegate void ExportData();
        private ExportData Export;

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
            //_xAxis.Minimum = Constants.XAXIS_MIN_DEFAULT;
            //_xAxis.Maximum = Constants.XAXIS_MAX_DEFAULT;
            _xAxis.MajorGridlineStyle = OxyPlot.LineStyle.Dot;
            _xAxis.Position = AxisPosition.Bottom;
            _xAxis.AxisChanged += XAxis_Changed;

            // configure y-axis
            _yAxis = new LinearAxis();
            _yAxis.Title = "Percentage (%)";
            _yAxis.MajorGridlineStyle = OxyPlot.LineStyle.Dot;
            _yAxis.Position = AxisPosition.Left;
            _yAxis.Minimum = Constants.YAXIS_MIN_DEFAULT;
            _yAxis.Maximum = Constants.YAXIS_MAX_DEFAULT;

            // add axis' to plot
            PlotModel.Axes.Add(_xAxis);
            PlotModel.Axes.Add(_yAxis);

            // enable data binding
            DataContext = this;

            Render = RenderSingleCurve;
        }

        private void RenderSingleCurve()
        {
            IsExportable = false;

            var inputControl = grid_InputControls.Children[0] as SingleCurveInputsControl;

            PlotModel.Series.Clear();

            // generate time curves
            var p2108Series = new LineSeries();
            var TEMP2Series = new LineSeries();
            for (double t = 0.01; t <= 99.99; t += 0.01)
            {
                P2108.AeronauticalStatisticalModel(inputControl.f__ghz, inputControl.theta__deg, t, out double L_ces__db);
                p2108Series.Points.Add(new DataPoint(L_ces__db, t));

                TEMP2Model.AeronauticalStatisticalModel(inputControl.f__ghz, inputControl.theta__deg, t, 3, TEMP2Model.Scenerio.MidRise, false, out double L_clt__db);
                TEMP2Series.Points.Add(new DataPoint(L_clt__db, t));
            }

            PlotModel.Series.Add(p2108Series);
            PlotModel.Series.Add(TEMP2Series);

            plot.InvalidatePlot();
        }

        private void Mi_Exit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Mi_SaveAsImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Mi_Export_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_Render_Click(object sender, RoutedEventArgs e) => Render();

        private void XAxis_Changed(object sender, AxisChangedEventArgs e) => IsExportable = false;

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
