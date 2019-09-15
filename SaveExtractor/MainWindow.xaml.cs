using System.IO;
using Witcher3MapViewer;
using System.Windows;
using Microsoft.Win32;

namespace SaveExtractor
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

        public void dumpfile(string filename)
        {

            using (FileStream compressedStream = File.OpenRead(filename))
            using (Stream decompressedStream = ChunkedLz4File.Decompress(compressedStream))
            using (FileStream outputstream = File.OpenWrite(filename.Substring(0, filename.Length-4) + "decomp.sav"))
            {
                decompressedStream.CopyTo(outputstream);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                DefaultExt = ".sav",
                Filter = "Save Files (*.sav)|*.sav"
            };
            bool? result = dlg.ShowDialog();
            string path;
            if (result == true)
            {
                path = dlg.FileName;
                dumpfile(path);
            }
        }
    }
}
