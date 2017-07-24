using System;
using System.IO;
using System.Windows;



namespace Witcher3MapViewer
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private bool hideInaccessible, showEvents, showRaces, showTreasure, manualMode;
        private string SaveFolder;
        public bool RequestNewGame = false;

        public SettingsDialog()
        {
            InitializeComponent();
            PopulateWindowWithSettings();
        }

        private void PopulateWindowWithSettings()
        {
            SaveFolder = Properties.Settings.Default.SaveFolder;
            if (Properties.Settings.Default.ManualMode)
                SetManualMode.IsChecked = true;
            else
                SetAutomaticMode.IsChecked = true;
            if (Directory.Exists(SaveFolder))
            {
                savefolderpathbox.Text = SaveFolder;
                savefolderfoundstatuslabel.Content = "Folder OK";
            }

            showEvents = Properties.Settings.Default.showEvents;
            showRaces = Properties.Settings.Default.showRaces;
            showTreasure = Properties.Settings.Default.showTreasure;
            manualMode = Properties.Settings.Default.ManualMode;

            accessibleCheckbox.IsChecked = hideInaccessible;
            eventCheckbox.IsChecked = showEvents;
            racesCheckbox.IsChecked = showRaces;
            treasureCheckbox.IsChecked = showTreasure;
        }

        private void TryToFindSavePath()
        {
            string foo = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string ersatzpath = Path.Combine(foo, "The Witcher 3", "gamesaves");
            if (Directory.Exists(ersatzpath))
            {
                savefolderpathbox.Text = ersatzpath;
                savefolderfoundstatuslabel.Content = "Found successfully";
                SaveFolder = ersatzpath;
            }
            else
            {
                savefolderfoundstatuslabel.Content = "Unable to find path";
            }
        }

        private void SetManualMode_Checked(object sender, RoutedEventArgs e)
        {
            if (SetAutomaticMode.IsChecked == true)
            {
                SavePathGrid.IsEnabled = true;
                manualMode = false;
                if (!Directory.Exists(SaveFolder))
                {
                    TryToFindSavePath();
                }
            }
            else
            {
                SavePathGrid.IsEnabled = false;
                manualMode = true;
            }
        }

        private void chooseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                savefolderpathbox.Text = dialog.SelectedPath;
                savefolderfoundstatuslabel.Content = "Path ok";
                SaveFolder = dialog.SelectedPath;
            }
        }

        private void SaveCloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(SaveFolder) && !manualMode)
            {
                MessageBox.Show("Folder does not exist. Please enter a valid path or choose manual mode.");
                return;
            }
            Properties.Settings.Default.SaveFolder = SaveFolder;
            Properties.Settings.Default.ManualMode = manualMode;
            Properties.Settings.Default.hideInaccessible = hideInaccessible;
            Properties.Settings.Default.showEvents = showEvents;
            Properties.Settings.Default.showRaces = showRaces;
            Properties.Settings.Default.showTreasure = showTreasure;
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ResetAllButton_Click(object sender, RoutedEventArgs e)
        {
            string sMessageBoxText = "This will delete all progress. Continue?";
            string sCaption = "Warning";

            MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
            MessageBoxImage icnMessageBox = MessageBoxImage.Warning;
            MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            if(rsltMessageBox == MessageBoxResult.Yes)
            {
                RequestNewGame = true;
            }
        }

        private void accessibleCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            hideInaccessible = accessibleCheckbox.IsChecked ?? false;
        }

        private void eventCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            showEvents = eventCheckbox.IsChecked ?? false;
        }

        private void racesCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            showRaces = racesCheckbox.IsChecked ?? false;
        }

        private void treasureCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            showTreasure = treasureCheckbox.IsChecked ?? false;
        }
    }
}
