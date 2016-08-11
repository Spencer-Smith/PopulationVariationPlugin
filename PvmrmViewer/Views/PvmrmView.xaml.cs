using System;
using System.Windows;
using Ookii.Dialogs;
using PvmrmViewer.ViewModels;
using System.Windows.Documents;
using System.Diagnostics;

namespace PvmrmViewer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PvmrmView : Window
    {
        public PvmrmViewViewModel PvmrmViewViewModel { get; set; }

        public PvmrmView(string infileName)
        {
            InitializeComponent();
            PvmrmViewViewModel = new PvmrmViewViewModel();
            DataContext = PvmrmViewViewModel;
            PvmrmViewViewModel.InputFileName = infileName;
            Open();
        }

        public void Open()
        {
            try
            {
                PvmrmViewViewModel.Open();
                if (PvmrmViewViewModel.Entries.Count == 0)
                    MessageBox.Show("No Variation Found", "Population Variation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show(this, "Unable to open file!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine(e);
            }
        }

        public void SaveClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaSaveFileDialog { DefaultExt = ".txt", Filter = @"Text Files (*.txt)|*.txt" };

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    PvmrmViewViewModel.Save(dialog.FileName);
                }
                catch (Exception)
                {
                    MessageBox.Show(this, "Unable to save file! Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void AboutClick(object sender, RoutedEventArgs e)
        {
            var about = new AboutView();
            about.Show();
        }

        public void FaqClick(object sender, RoutedEventArgs e)
        {
            var faq = new FaqView();
            faq.Show();
        }

        public void ContactClick(object sender, RoutedEventArgs e)
        {
            var contact = new ContactView();
            contact.Show();
        }

        public void ExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnHyperlinkClick(object sender, RoutedEventArgs e)
        {
            var destination = ((Hyperlink)e.OriginalSource).NavigateUri;
            Trace.WriteLine("Browsing to " + destination);

            using (Process browser = new Process())
            {
                browser.StartInfo = new ProcessStartInfo
                {
                    FileName = destination.ToString(),
                    UseShellExecute = true,
                    ErrorDialog = true
                };
                browser.Start();
            }
        }
    }
}
