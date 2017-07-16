using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MeasuringTools.Gui
{
    public class MeasuringToolViewModel
    {
        public ObservableDataSource<Point> Data { get; set; }

        public MeasuringToolViewModel()
        {
            Data = new ObservableDataSource<Point>();
        }
    }
}
