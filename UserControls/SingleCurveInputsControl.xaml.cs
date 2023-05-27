using P2108Comparer.PropModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace P2108Comparer.UserControls
{
    public partial class SingleCurveInputsControl : UserControl, INotifyPropertyChanged
    {
        private int _errorCnt = 0;

        /// <summary>
        /// Frequency, in GHz
        /// </summary>
        public double f__ghz { get; set; }

        /// <summary>
        /// Elevation angle, in deg
        /// </summary>
        public double theta__deg { get; set; }

        /// <summary>
        /// Ground station height, in meter
        /// </summary>
        public double h__meter { get; set; }

        /// <summary>
        /// Clutter scenerio
        /// </summary>
        public TEMP2Model.Scenerio Scenerio { get; set; } = TEMP2Model.Scenerio.LowRise;

        /// <summary>
        /// Number of validation errors
        /// </summary>
        public int ErrorCnt
        {
            get { return _errorCnt; }
            set
            {
                _errorCnt = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SingleCurveInputsControl()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void TextBox_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                ErrorCnt++;
            else
                ErrorCnt--;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
