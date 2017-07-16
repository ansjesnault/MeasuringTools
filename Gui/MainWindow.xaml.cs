using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace MeasuringTools.Gui
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MeasuringToolViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            viewModel = new MeasuringToolViewModel();
            DataContext = viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double[] my_array = new double[10];

            for (int i = 0; i < my_array.Length; i++)
            {
                my_array[i] = Math.Sin(i);
                viewModel.Data.Collection.Add(new Point(i, my_array[i]));
            }
        }

        ObservableDataSource<Point> data = null;
        int j = 0;

        private void LineGraph_KeyDown(object sender, KeyEventArgs e)
        {
            if (data == null)
            {
                data = new ObservableDataSource<Point>();
                _chartPlotter.AddLineGraph(data, Color.FromRgb(255, 0, 0), 3.0d, "toto");
            }
            else
            {
                j+=10;
            }

            double[] my_array = new double[10];

            for (int i = 0; i < my_array.Length; i++)
            {
                my_array[i] = Math.Cos(i+j);
                data.Collection.Add(new Point(i+j, my_array[i]));
            }
        }
    }
}
