using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tizen.Security;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TizenDotNet1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        int dateOffset;
        private class Entry{
            public DateTime Date { get; set; }
            public string Dose { get; set; }
        }

        public MainPage()
        {
            InitializeComponent();
            dateOffset = 0;
            labelDate.Text = DateTime.Now.ToString("MMMM d");
            CheckResult result = PrivacyPrivilegeManager.CheckPermission("http://tizen.org/privilege/mediastorage");
            if (result.Equals("Ask"))
            {
                PrivacyPrivilegeManager.RequestPermission("http://tizen.org/privilege/mediastorage");
            }

            LoadDoseHelper();

        }
        private void OnButtonConfirm(object sender, EventArgs e)
        {
            Entry day = new Entry();
            day.Date = DateTime.Now.AddDays(dateOffset);
            day.Dose = labelDose.Text;
            string json = JsonSerializer.Serialize(day);
            string storage = Tizen.Applications.Application.Current.DirectoryInfo.Data;
            File.WriteAllText($"{storage}{DateTime.Now.AddDays(dateOffset).ToString("yyyy-M-dd")}.json", json);
        }
        private void OnButtonPlusDay(object sender, EventArgs e)
        {
            dateOffset += 1;
            labelDate.Text = DateTime.Now.AddDays(dateOffset).ToString("MMMM d");
            
            LoadDoseHelper();
        }
        private void OnButtonMinusDay(object sender, EventArgs e)
        {
            dateOffset -= 1;
            labelDate.Text = DateTime.Now.AddDays(dateOffset).ToString("MMMM d");
            LoadDoseHelper();
        }
        private void OnButtonSynchronize(object sender, EventArgs e)
        {
            var url = $"https://bloodwebapp.herokuapp.com/addvalues?date={DateTime.Now.AddDays(dateOffset).ToString("yyyy-M-dd")}&value={entry.Text}";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                var page = reader.ReadToEnd();
            }

        }
        private void LoadDoseHelper()
        {
            Entry day = new Entry();
            try
            {
                string storage = Tizen.Applications.Application.Current.DirectoryInfo.Data;
                string path = $"{storage}{DateTime.Now.AddDays(dateOffset).ToString("yyyy-M-dd")}.json";
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    day = JsonSerializer.Deserialize<Entry>(json);
                    labelDose.Text = day.Dose;
                    buttonSynchronize.IsEnabled = true;
                }
                else
                {
                    labelDose.Text = "(no dose)";
                    buttonSynchronize.IsEnabled = false;
                }
                entry.Text = String.Empty;
            }
            catch (System.Exception ex)
            {
                string error = ex.ToString();
            }
        }
        private void OnUnfocused(object sender, FocusEventArgs e)
        {
            entry.Text = String.Empty;
        }
        private void OnTextChanged(object sender, FocusEventArgs e)
        {
            if(entry.IsFocused)
                labelDose.Text = entry.Text;
        }

        //private void EntryOnFocus(object sender, FocusEventArgs e)
        //{
        //    double moveto = entry.TranslationY -= 50;
        //    entry.TranslateTo(entry.TranslationX, moveto, 200, Easing.SinInOut);
        //}
    }
}