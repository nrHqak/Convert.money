using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Convert.Money
{
    public class Form1 : Form
    {
        private readonly CurrencyService _currencyService = new CurrencyService();

        private ComboBox _fromCurrencyComboBox;
        private ComboBox _toCurrencyComboBox;
        private TextBox _amountTextBox;
        private Button _convertButton;
        private Button _refreshRatesButton;
        private Label _resultLabel;

        private TextBox _calculatorTextBox;
        private Button _calcButton;
        private Button _useResultButton;
        private Label _calculatorResultLabel;
        private decimal _calculatorResult;

        private ComboBox _chartCurrencyComboBox;
        private ComboBox _periodComboBox;
        private Chart _currencyChart;

        public Form1()
        {
            InitializeComponent();
            LoadCurrencies();
            UpdateChart();
        }

        private void InitializeComponent()
        {
            Text = "Convert.Money";
            Size = new Size(900, 650);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(236, 241, 248);
            Font = new Font("Segoe UI", 10);

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Appearance = TabAppearance.Normal
            };

            var converterTab = new TabPage("Конвертер + Калькулятор") { BackColor = BackColor };
            var chartTab = new TabPage("График курсов") { BackColor = BackColor };

            tabs.TabPages.Add(converterTab);
            tabs.TabPages.Add(chartTab);
            Controls.Add(tabs);

            BuildConverterTab(converterTab);
            BuildChartTab(chartTab);
        }

        private void BuildConverterTab(Control parent)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            parent.Controls.Add(panel);

            var title = new Label
            {
                Text = "Быстрая конвертация валют",
                Font = new Font("Segoe UI Semibold", 16),
                ForeColor = Color.FromArgb(31, 41, 55),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            _fromCurrencyComboBox = CreateComboBox(new Point(10, 70));
            _toCurrencyComboBox = CreateComboBox(new Point(220, 70));

            _amountTextBox = new TextBox
            {
                Location = new Point(430, 70),
                Width = 170,
                BorderStyle = BorderStyle.FixedSingle
            };

            _convertButton = CreateButton("Конвертировать", new Point(620, 68));
            _convertButton.Click += ConvertButton_Click;

            _refreshRatesButton = CreateButton("Обновить курсы", new Point(620, 112));
            _refreshRatesButton.Click += RefreshRatesButton_Click;

            _resultLabel = new Label
            {
                Text = "Результат: -",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                AutoSize = true,
                Location = new Point(10, 120)
            };

            var calculatorTitle = new Label
            {
                Text = "Калькулятор (пример: 12.5*3-2)",
                Font = new Font("Segoe UI Semibold", 12),
                AutoSize = true,
                Location = new Point(10, 190),
                ForeColor = Color.FromArgb(31, 41, 55)
            };

            _calculatorTextBox = new TextBox
            {
                Location = new Point(10, 230),
                Width = 520,
                BorderStyle = BorderStyle.FixedSingle
            };

            _calcButton = CreateButton("Посчитать", new Point(550, 228));
            _calcButton.Width = 160;
            _calcButton.Click += CalculateButton_Click;

            _useResultButton = CreateButton("Использовать в сумме", new Point(720, 228));
            _useResultButton.Width = 160;
            _useResultButton.Click += UseResultButton_Click;

            _calculatorResultLabel = new Label
            {
                Text = "Результат калькулятора: -",
                AutoSize = true,
                Location = new Point(10, 272),
                ForeColor = Color.FromArgb(55, 65, 81)
            };

            panel.Controls.AddRange(new Control[]
            {
                title, _fromCurrencyComboBox, _toCurrencyComboBox, _amountTextBox,
                _convertButton, _refreshRatesButton, _resultLabel, calculatorTitle,
                _calculatorTextBox, _calcButton, _useResultButton, _calculatorResultLabel
            });
        }

        private void BuildChartTab(Control parent)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            parent.Controls.Add(panel);

            _chartCurrencyComboBox = CreateComboBox(new Point(10, 20));
            _chartCurrencyComboBox.SelectedIndexChanged += ChartFilterChanged;

            _periodComboBox = CreateComboBox(new Point(220, 20));
            _periodComboBox.Items.AddRange(new object[] { "День", "Неделя", "Месяц" });
            _periodComboBox.SelectedIndex = 0;
            _periodComboBox.SelectedIndexChanged += ChartFilterChanged;

            _currencyChart = new Chart
            {
                Location = new Point(10, 70),
                Size = new Size(840, 470),
                BackColor = Color.White,
                BorderlineColor = Color.Gainsboro,
                BorderlineDashStyle = ChartDashStyle.Solid,
                BorderlineWidth = 1
            };

            var area = new ChartArea("RateArea");
            area.AxisX.Title = "Дата";
            area.AxisY.Title = "Курс к KZT";
            area.AxisX.MajorGrid.LineColor = Color.Gainsboro;
            area.AxisY.MajorGrid.LineColor = Color.Gainsboro;
            area.AxisX.Interval = 1;
            area.AxisY.IsStartedFromZero = false;
            _currencyChart.ChartAreas.Add(area);

            var series = new Series("Курс")
            {
                ChartType = SeriesChartType.Spline,
                BorderWidth = 3,
                Color = Color.FromArgb(37, 99, 235),
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 7,
                XValueType = ChartValueType.String
            };
            _currencyChart.Series.Add(series);

            panel.Controls.AddRange(new Control[] { _chartCurrencyComboBox, _periodComboBox, _currencyChart });
        }

        private ComboBox CreateComboBox(Point location)
        {
            return new ComboBox
            {
                Location = location,
                Width = 190,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
        }

        private Button CreateButton(string text, Point location)
        {
            return new RoundedButton
            {
                Text = text,
                Location = location,
                Width = 170,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Radius = 14
            };
        }

        private void LoadCurrencies()
        {
            var codes = _currencyService.GetCurrencies().Select(c => c.Code).ToArray();
            _fromCurrencyComboBox.Items.AddRange(codes);
            _toCurrencyComboBox.Items.AddRange(codes);
            _chartCurrencyComboBox.Items.AddRange(codes);

            _fromCurrencyComboBox.SelectedItem = "USD";
            _toCurrencyComboBox.SelectedItem = "KZT";
            _chartCurrencyComboBox.SelectedItem = "USD";
        }

        private void ConvertButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_amountTextBox.Text))
            {
                MessageBox.Show("Введите сумму для конвертации.");
                return;
            }

            if (!decimal.TryParse(_amountTextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount) &&
                !decimal.TryParse(_amountTextBox.Text, out amount))
            {
                MessageBox.Show("Сумма должна быть числом.");
                return;
            }

            try
            {
                decimal result = _currencyService.Convert(amount, _fromCurrencyComboBox.Text, _toCurrencyComboBox.Text);
                _resultLabel.Text = $"Результат: {result:F2} {_toCurrencyComboBox.Text}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshRatesButton_Click(object sender, EventArgs e)
        {
            _currencyService.RefreshRates();
            UpdateChart();
            MessageBox.Show("Курсы обновлены (статическая заглушка).");
        }

        private void CalculateButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_calculatorTextBox.Text))
            {
                MessageBox.Show("Введите выражение для расчёта.");
                return;
            }

            try
            {
                var dataTable = new DataTable();
                object value = dataTable.Compute(_calculatorTextBox.Text.Replace(',', '.'), string.Empty);
                _calculatorResult = System.Convert.ToDecimal(value);
                _calculatorResultLabel.Text = $"Результат калькулятора: {_calculatorResult:F2}";
            }
            catch
            {
                MessageBox.Show("Неверное математическое выражение.");
            }
        }

        private void UseResultButton_Click(object sender, EventArgs e)
        {
            _amountTextBox.Text = _calculatorResult.ToString("F2");
        }

        private void ChartFilterChanged(object sender, EventArgs e)
        {
            if (_chartCurrencyComboBox.SelectedItem != null && _periodComboBox.SelectedItem != null)
            {
                UpdateChart();
            }
        }

        private void UpdateChart()
        {
            string code = _chartCurrencyComboBox.SelectedItem?.ToString() ?? "USD";
            string period = _periodComboBox.SelectedItem?.ToString() ?? "День";
            decimal baseRate = _currencyService.GetRateToKzt(code);

            List<KeyValuePair<string, decimal>> points = BuildChartPoints(period, baseRate);

            var series = _currencyChart.Series["Курс"];
            series.Points.Clear();
            foreach (var point in points)
            {
                series.Points.AddXY(point.Key, point.Value);
            }

            decimal min = points.Min(p => p.Value);
            decimal max = points.Max(p => p.Value);
            decimal margin = Math.Max(1m, (max - min) * 0.35m);

            var area = _currencyChart.ChartAreas["RateArea"];
            area.AxisY.Minimum = (double)(min - margin);
            area.AxisY.Maximum = (double)(max + margin);
        }

        private static List<KeyValuePair<string, decimal>> BuildChartPoints(string period, decimal baseRate)
        {
            if (period == "День")
            {
                return new List<KeyValuePair<string, decimal>>
                {
                    new KeyValuePair<string, decimal>("08:00", baseRate - 1.8m),
                    new KeyValuePair<string, decimal>("12:00", baseRate + 0.7m),
                    new KeyValuePair<string, decimal>("16:00", baseRate - 0.4m),
                    new KeyValuePair<string, decimal>("20:00", baseRate + 1.1m)
                };
            }

            if (period == "Неделя")
            {
                string[] labels = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
                decimal[] offsets = { -2.3m, -1.2m, 0.8m, -0.5m, 1.6m, 0.4m, -0.7m };

                return labels
                    .Select((label, i) => new KeyValuePair<string, decimal>(label, baseRate + offsets[i]))
                    .ToList();
            }

            var monthPoints = new List<KeyValuePair<string, decimal>>();
            for (int day = 1; day <= 30; day += 3)
            {
                decimal wave = (day % 2 == 0 ? 1.3m : -1.1m) + (day % 5) * 0.3m;
                monthPoints.Add(new KeyValuePair<string, decimal>(day.ToString(), baseRate + wave));
            }

            return monthPoints;
        }

        private class RoundedButton : Button
        {
            public int Radius { get; set; } = 12;

            protected override void OnPaint(PaintEventArgs pevent)
            {
                base.OnPaint(pevent);
                using (var path = GetRoundRectangle(ClientRectangle, Radius))
                {
                    Region = new Region(path);
                    FlatAppearance.BorderSize = 0;
                }
            }

            private static GraphicsPath GetRoundRectangle(Rectangle rect, int radius)
            {
                int diameter = radius * 2;
                var path = new GraphicsPath();

                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();

                return path;
            }
        }
    }
}
