using P2108Comparer.PropModels;
using P2108Comparer.ValidationRules;
using P2108Comparer.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class MultipleElevationAnglesInputControl : UserControl, INotifyPropertyChanged
    {
        private int _errorCnt = 0;

        /// <summary>
        /// Frequencies, in GHz
        /// </summary>
        public ObservableCollection<double> angles { get; set; } = new ObservableCollection<double>() { 10 };

        public double f__ghz { get; set; }

        /// <summary>
        /// Ground station height, in meter
        /// </summary>
        public double h__meter { get; set; }

        /// <summary>
        /// Clutter scenerio
        /// </summary>
        public TEMP2Model.Scenerio Scenerio { get; set; } = TEMP2Model.Scenerio.LowRise;

        /// <summary>
        /// Antenna type
        /// </summary>
        public TEMP2Model.Antenna Antenna { get; set; } = TEMP2Model.Antenna.Isotropic;

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

        public MultipleElevationAnglesInputControl()
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

        private void Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            var wndw = new AddFrequencyWindow();

            if (!wndw.ShowDialog().Value)
                return;

            if (angles.Count == 0)
            {
                ErrorCnt--;
                Validation.ClearInvalid(lb_angles.GetBindingExpression(ListBox.ItemsSourceProperty));
            }

            angles.Add(wndw.frequency);
        }

        private void Btn_Remove_Click(object sender, RoutedEventArgs e)
        {
            var itemsToRemove = new List<double>();

            foreach (double item in lb_angles.SelectedItems)
                itemsToRemove.Add(item);

            foreach (var item in itemsToRemove)
                angles.Remove(item);

            if (angles.Count == 0)
            {
                ErrorCnt++;

                var binding = lb_angles.GetBindingExpression(ListBox.ItemsSourceProperty);
                var error = new ValidationError(new DoubleValidation(), binding) { ErrorContent = "At least 1 elevation angle is required" };
                Validation.MarkInvalid(binding, error);
            }
        }

        /// <summary>
        /// Control if the 'Remove' button is enabled
        /// </summary>
        private void Lb_angles_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            btn_Remove.IsEnabled = lb_angles.SelectedItems.Count > 0;
    }
}
