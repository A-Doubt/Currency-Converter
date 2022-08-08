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
using System.Data;
using System.Text.RegularExpressions;
using System.Net.Http;
using Newtonsoft.Json;

namespace CurrencyConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Root val = new Root();

        public MainWindow()
        {
            InitializeComponent();
            GetValue();
            ClearValues();
        }

        public static async Task<Root> GetData<T>(string url)
        {
            var myRoot = new Root();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(1);
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var ResponseString = await response.Content.ReadAsStringAsync();
                        var ResponseObject = JsonConvert.DeserializeObject<Root>(ResponseString);

                        return ResponseObject; // Return API response
                    }
                    return myRoot; // Otherwise return empty myRoot
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return myRoot;
            }
        }

        private async void GetValue()
        {
            val = await GetData<Root>("https://openexchangerates.org/api/latest.json?app_id=8ef1929fa5ee44b0a7dd131ef956074c");
            PopulateCurrencies();
        }

        private void ValidateAmountInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+$");
            e.Handled = regex.IsMatch(e.Text);
        }

        public void PopulateCurrencies()
        {
            DataTable dtCurrency = new DataTable();
            dtCurrency.Columns.Add("Name");
            dtCurrency.Columns.Add("Value");

            dtCurrency.Rows.Add("--Select a currency--", 0);
            dtCurrency.Rows.Add("EUR", val.rates.EUR);
            dtCurrency.Rows.Add("USD", val.rates.USD);
            dtCurrency.Rows.Add("CAD", val.rates.CAD);
            dtCurrency.Rows.Add("GBP", val.rates.GBP);
            dtCurrency.Rows.Add("PLN", val.rates.PLN);
            dtCurrency.Rows.Add("CHF", val.rates.CHF);

            cbFromCurrency.ItemsSource = dtCurrency.DefaultView;
            cbFromCurrency.DisplayMemberPath = "Name";
            cbFromCurrency.SelectedValuePath = "Value";

            cbToCurrency.ItemsSource = dtCurrency.DefaultView;
            cbToCurrency.DisplayMemberPath = "Name";
            cbToCurrency.SelectedValuePath = "Value";
        }

        private void ClearValues()
        {
            tbAmount.Text = "";
            cbFromCurrency.SelectedIndex = 0;
            cbToCurrency.SelectedIndex = 0;
            tbConvertedCurrency.Text = "";
        }
        private void ClearSelectionBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearValues();
        }
        private void ConvertCurrencyBtn_Click(object sender, RoutedEventArgs e)
        {
            double convertedValue;
            
            // Guard clauses
            if (tbAmount.Text == null || tbAmount.Text.Length == 0)
            {
                MessageBox.Show("Please enter the amount you want to convert.", "Invalid input.", MessageBoxButton.OK, MessageBoxImage.Information);
                tbAmount.Focus();
                return;
            }
            if (cbFromCurrency.SelectedIndex == 0 || cbToCurrency.SelectedValue == null)
            {
                MessageBox.Show("Please select from which currency you wish to convert..", "No selected currency.", MessageBoxButton.OK, MessageBoxImage.Information);
                cbFromCurrency.Focus();
                return;
            }
            if (cbToCurrency.SelectedIndex == 0 || cbToCurrency.SelectedValue == null)
            {
                MessageBox.Show("Please select to which currency you wish to convert.", "No selected currency.", MessageBoxButton.OK, MessageBoxImage.Information);
                cbToCurrency.Focus();
                return;
            }

            if (cbFromCurrency.Text == cbToCurrency.Text)
            {
                convertedValue = double.Parse(tbAmount.Text);
            } 
            else
            {
                convertedValue = 
                    (double.Parse(cbToCurrency.SelectedValue.ToString()) * double.Parse(tbAmount.Text)) / 
                    double.Parse(cbFromCurrency.SelectedValue.ToString());

            }

            tbConvertedCurrency.Text = cbToCurrency.Text + " " + convertedValue.ToString("N3");
        }
    }
}
