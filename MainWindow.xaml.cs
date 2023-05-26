using Microsoft.Win32;
using NAudio.Gui;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Binding = System.Windows.Data.Binding;

namespace AudioPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Set Assembly version to app
            lblVersion.Content = "Ver: " + GetAssemblyVersion();

            //Set Default values
            SetAppDefaultValues();

            // creating the datagrid tables
            DataGridObjects_Folder = new List<DataGridObject>();
            DataGridObjects_FileData = new List<DataGridObject>();

            //Populate default datagrid columns
            PopulateDataGridWithColumns(dataGridFolderList, DataGridFolderColumnNames);
            PopulateDataGridWithColumns(dataGridFileData, DataGridFileColumnNames);

            //Next song should be selected automatically.
            NPlayer = new Players.NPlayer.NPlayer();
            NPlayer.PlaybackPaused += SetControlsPause;
            NPlayer.PlaybackResumed += SetControlsPlayPause;
            NPlayer.PlaybackStopped += SetControlsStopped;
        }

        private string? SelectedFolderPath { set; get; }
        private string CurrentLoadedFile { set; get; }
        private bool Playing { set; get; }
        private bool NewSong { set; get; }

        private Players.NPlayer.NPlayer NPlayer { set; get; }

        //Datagrid Objects
        private List<string> DataGridFolderColumnNames = new List<string>() { "TrackNo", "Track" };
        private List<string> DataGridFileColumnNames = new List<string>() { "Info", "Value" };
        private List<DataGridObject> DataGridObjects_Folder;
        private List<DataGridObject> DataGridObjects_FileData;

        //=======================================================
        //                  Button Actions
        //=======================================================

        private void btnFolderSelect_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new FolderBrowserDialog()
            { 
                RootFolder = Environment.SpecialFolder.MyMusic
            };

            //Let the user select folder
            fileDialog.ShowDialog();

            //Write folder to parameters and labels
            SelectedFolderPath = fileDialog.SelectedPath;
            lblCurrentFolder.Content = $"Path: {fileDialog.SelectedPath}";

            //Populate Folder File View
            PopulateFolderFileDataGridWithValues();

            btnPlayPauseSelect_Click(sender, e);

        }

        private void btnStepBackSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectPrevious();
        }

        private void btnStopSelect_Click(object sender, RoutedEventArgs e)
        {
            btnPlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            Playing = false;
            NPlayer.Stop();
        }

        private void btnPlayPauseSelect_Click(object sender, RoutedEventArgs e)
        {
            if(Playing)
            {
                NPlayer.TogglePlayPause(NPlayer.GetVolume());
            }
            else
            {
                try
                {
                    if(NewSong)
                    {
                        NPlayer.Stop();
                    }

                    var selectedValue = dataGridFolderList.SelectedValue as DataGridObject;

                    if (selectedValue != null)
                    {
                        //Select file?
                        CurrentLoadedFile = $"{SelectedFolderPath}\\{selectedValue.RowValues.FirstOrDefault(x => x.Key == "TRACK").Value}";

                        //Start player
                        NPlayer = new Players.NPlayer.NPlayer(CurrentLoadedFile, 1.0f);
                        NPlayer.Play(PlaybackState.Stopped, NPlayer.GetVolume());

                        //Set states
                        btnPlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                        Playing = true;
                        NewSong = false;
                    }
                }
                catch { }
            }
        }

        private void btnStepForwardSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectNext();
        }

        private void SelectNext()
        {
            try
            {
                if ((dataGridFolderList.SelectedIndex + 1) < dataGridFolderList.Items.Count)
                {
                    dataGridFolderList.SelectedIndex = dataGridFolderList.SelectedIndex + 1;

                    if (Playing)
                    {
                        NPlayer.Stop();
                        Playing = false;
                        NewSong = true;
                        btnPlayPauseSelect_Click(this, null);
                    }
                }

            }
            catch { }
        }

        private void SelectPrevious()
        {
            try
            {
                if (dataGridFolderList.SelectedIndex > 0)
                {
                    dataGridFolderList.SelectedIndex = dataGridFolderList.SelectedIndex - 1;

                    if (Playing)
                    {
                        NPlayer.Stop();
                        Playing = false;
                        NewSong = true;
                        btnPlayPauseSelect_Click(this, null);
                    }
                }

            }
            catch { }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //=======================================================
        //                  Populate Actions
        //=======================================================

        private void SetAppDefaultValues()
        {
            SelectedFolderPath = "";
            Playing = false;
        }

        private string GetAssemblyVersion()
        {
            var returnString = "0.0.0.0";
            try
            {
                returnString = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            catch { }

            return returnString;
        }

        private void PopulateDataGridWithColumns(DataGrid dataGrid, List<string> headerData)
        {
            // Creating the columns
            foreach (var headerItem in headerData/*.OrderBy(r => r).ToList()*/)
            {

                var column = new DataGridTextColumn
                {
                    Header = headerItem.Trim().ToUpper(),
                    Binding = new Binding("RowValues[" + headerItem.Trim().ToUpper() + "]"),
                    IsReadOnly = false,
                    Visibility = Visibility.Visible
                };

                dataGrid.Columns.Add(column);
            }
        }

        private void PopulateFolderFileDataGridWithValues()
        {
            //Empty datagrid first.
            dataGridFolderList.ItemsSource = null;
            DataGridObjects_Folder.Clear();

            if (string.IsNullOrEmpty(SelectedFolderPath))
            {
                return;
            }

            var filesInFolder = GetAllSupportedFilesInFolder(SelectedFolderPath);

            if(filesInFolder.Count != 0)
            {
                try
                {
                    int i = 1;
                    foreach (var file in filesInFolder)
                    {
                        
                        //Constructor
                        var dic = new Dictionary<string, string>();

                        //Get the headerData and create an Array of it
                        var fileData = file.Name;

                        //Populate data
                        try
                        {
                            dic.Add(DataGridFolderColumnNames[0].ToUpper(), i.ToString());
                            dic.Add(DataGridFolderColumnNames[1].ToUpper(), fileData);
                        }
                        catch 
                        {
                            dic.Add(DataGridFolderColumnNames[0].ToUpper(), i.ToString());
                            dic.Add(DataGridFolderColumnNames[1].ToUpper(), "");
                        }
                        
                        //Create object and add to List.
                        var o = new DataGridObject()
                        {
                            RowValues = dic
                        };
                        DataGridObjects_Folder.Add(o);

                        i++;
                    }

                    //Bind the source
                    dataGridFolderList.ItemsSource = DataGridObjects_Folder;
                }
                catch { }

                dataGridFolderList.SelectedIndex = 0;
            }
        }

        private List<FileInfo> GetAllSupportedFilesInFolder(string folder)
        {
            try
            {
                var di = new DirectoryInfo(folder);
                return di.GetFiles("*.mp3").ToList();
            }
            catch { return new List<FileInfo>(); }
        }

        private void dataGridFolderList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (Playing)
            {
                NewSong = true;
                NPlayer.Pause();
                btnPlayPauseSelect_Click(this, null);
            }
        }

        //=======================================================
        //                     Events
        //=======================================================

        private void SetControlsPause()
        {
            btnPlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
        }

        private void SetControlsPlayPause()
        {
            btnPlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
        }

        private void SetControlsStopped()
        {
            btnPlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            Playing = false;
        }
    }

    public class DataGridObject
    {
        public DataGridObject() : base()
        {
            RowValues = new Dictionary<string, string>();
        }

        public Dictionary<string, string> RowValues { get; set; }
    }

}
