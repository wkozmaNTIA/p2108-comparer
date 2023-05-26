using OxyPlot;
using OxyPlot.Axes;
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
            _xAxis.Minimum = Constants.XAXIS_MIN_DEFAULT;
            _xAxis.Maximum = Constants.XAXIS_MAX_DEFAULT;
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

        private void Btn_Render_Click(object sender, RoutedEventArgs e)
        {

        }

        private void XAxis_Changed(object sender, AxisChangedEventArgs e) => IsExportable = false;
    }
}
