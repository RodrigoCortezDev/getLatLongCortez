using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using Newtonsoft.Json;

namespace getLatLongCortez
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            btnConsulta.IsEnabled = false;
            pbCosulta.Visibility = Visibility.Visible;

            RootObject objRetorno = null;
            string strRet = "";
            string strEnd = txtEndereco.Text;

            txtLat.Text = "";
            txtlong.Text = "";

            await Task.Run(() =>
            {
                try
                {
                    strEnd = strEnd.Replace(",", "");
                    strEnd = strEnd.Replace(" ", "+");

                    HttpClient client = new HttpClient();
                    var respGet = client.GetAsync(@"https://maps.googleapis.com/maps/api/geocode/json?address=" + strEnd + "&key=AIzaSyAxaDOF9gTHtqObGL6q0agrQYlfKbxmLSM").Result;
                    strRet = respGet.Content.ReadAsStringAsync().Result;
                    objRetorno = JsonConvert.DeserializeObject<RootObject>(strRet);
                }
                catch (Exception ex)
                {
                    strRet = ex.Message;
                }
            });

            Mouse.OverrideCursor = Cursors.Arrow;
            btnConsulta.IsEnabled = true;
            pbCosulta.Visibility = Visibility.Collapsed;

            try
            {
                if (objRetorno != null && objRetorno.results.Any())
                {
                    txtLat.Text = objRetorno.results.FirstOrDefault().geometry.location.lat.ToString().Replace(",", ".");
                    txtlong.Text = objRetorno.results.FirstOrDefault().geometry.location.lng.ToString().Replace(",", ".");
                }
                txtResult.Text = strRet;
            }
            catch
            { }
        }

        private async void btnConsultaArquivo_OnClick(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            btnConsulta2.IsEnabled = false;
            pbCosulta.Visibility = Visibility.Visible;

            RootObject objRetorno = null;
            string strRet = "";
            string strErro = "";
            string strEnd = "";
            int intQtde = 0;
            bool blnUltrapassouLimite = false;
            bool blnExecutouAlgumaVez = false;

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Enderecos.csv"))
                txtResult.Text = txtResult.Text.Trim() + "O Arquivo 'Enderecos.csv' não foi localizado, favor deixa-lo junto ao EXE do programa!";
            else
            {
                await Task.Run(() =>
                {
                    try
                    {
                        string newLines = "";
                        string[] lines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "Enderecos.csv", Encoding.UTF8);
                        foreach (var line in lines)
                        {
                            if (newLines == "")
                                newLines += line + "\r\n";
                            else
                            {
                                string[] teste = line.Split(';');
                                if (teste.Length < 6 || teste[4].Trim() == "" || teste[5].Trim() == "")
                                {
                                    blnExecutouAlgumaVez = true;
                                    if (blnUltrapassouLimite == false)
                                    {
                                        intQtde++;
                                        strEnd = line.Replace(";", "+").Replace("++", "");
                                        HttpClient client = new HttpClient();
                                        var respGet = client.GetAsync(@"https://maps.googleapis.com/maps/api/geocode/json?address=" + strEnd + "&key=AIzaSyAxaDOF9gTHtqObGL6q0agrQYlfKbxmLSM").Result;
                                        strRet = respGet.Content.ReadAsStringAsync().Result;
                                        objRetorno = JsonConvert.DeserializeObject<RootObject>(strRet);

                                        if (objRetorno.status == "OVER_QUERY_LIMIT")
                                            blnUltrapassouLimite = true;
                                    }

                                    if (objRetorno != null && objRetorno.results.Any())
                                        newLines += line.Replace(";;", "") + ";" + objRetorno.results.FirstOrDefault().geometry.location.lat + ";" + objRetorno.results.FirstOrDefault().geometry.location.lng + "\r\n";
                                    else
                                        newLines += line + "\r\n";

                                    objRetorno = null;
                                }
                                else
                                    newLines += line + "\r\n";

                                if (blnUltrapassouLimite == false)
                                    Dispatcher.BeginInvoke(((Action) delegate { txtResult.Text = "Registros Verificados: " + intQtde; }));
                            }
                        }
                        StreamWriter file = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Enderecos.csv", false);
                        file.WriteLine(newLines);
                        file.Close();
                    }
                    catch (Exception ex)
                    {
                        strErro = "\r\n" + ex.Message;
                    }
                });
            }

            Mouse.OverrideCursor = Cursors.Arrow;
            btnConsulta2.IsEnabled = true;
            pbCosulta.Visibility = Visibility.Collapsed;

            if (strErro != "")
                txtResult.Text += "\r\nERRO: " + strRet;
            if (blnUltrapassouLimite)
                txtResult.Text += "\r\nUltrapassou o Limite, continuar outro dia.";
            if (blnExecutouAlgumaVez == false)
                txtResult.Text += "\r\nO arquivo: " + AppDomain.CurrentDomain.BaseDirectory + "Enderecos.csv, Está totamente processado!";
        }
    }


    public class AddressComponent
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public List<string> types { get; set; }
    }
    public class Northeast
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
    public class Southwest
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
    public class Bounds
    {
        public Northeast northeast { get; set; }
        public Southwest southwest { get; set; }
    }
    public class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
    public class Northeast2
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
    public class Southwest2
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
    public class Viewport
    {
        public Northeast2 northeast { get; set; }
        public Southwest2 southwest { get; set; }
    }
    public class Geometry
    {
        public Bounds bounds { get; set; }
        public Location location { get; set; }
        public string location_type { get; set; }
        public Viewport viewport { get; set; }
    }
    public class Result
    {
        public List<AddressComponent> address_components { get; set; }
        public string formatted_address { get; set; }
        public Geometry geometry { get; set; }
        public bool partial_match { get; set; }
        public string place_id { get; set; }
        public List<string> types { get; set; }
    }
    public class RootObject
    {
        public List<Result> results { get; set; }
        public string status { get; set; }
    }

}
