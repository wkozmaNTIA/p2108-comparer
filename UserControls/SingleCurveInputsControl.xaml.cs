using P2108Comparer.PropModels;
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

namespace P2108Comparer.UserControls
{
    public partial class SingleCurveInputsControl : UserControl
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
