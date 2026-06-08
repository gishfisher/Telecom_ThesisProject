using System;
using System.Collections.Generic;
using System.Linq;
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
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.Services;

namespace Telecom_ThesisProject.MVVM.View
{
    /// <summary>
    /// Логика взаимодействия для Home.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        private TariffService _tariffService;

        public HomeView()
        {
            InitializeComponent();
            _tariffService = new TariffService();
            Loaded += (s, e) => LoadTariffChart();
        }

        private void LoadTariffChart()
        {
            var tariffs = _tariffService.GetTariffStats();

            var series = TariffChart.Series["Tariffs"];
            series.Points.Clear();

            foreach (var t in tariffs)
            {
                var point = series.Points.Add(t.Value);
                point.LegendText = t.Key;
                point.Label = $"{t.Key}\n{t.Value}";
            }
        }
    }
}
