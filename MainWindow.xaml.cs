using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using AudioPlayer.Classes;
using AudioPlayer.Handlers;
using System.Timers;
using System.Threading;
using System.Windows.Media;
using AudioPlayer.Forms;

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

            //Set default colorscheme
            ApplicationColorScheme = new ApplicationColorScheme()
            {
                ApplicationBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCB9B2")),
                ApplicationButtonText = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCB9B2")),
                ApplicationDatagridText = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000")),
                ApplicationBanner = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8C2F39")),
                ApplicationLogo = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#461220")),
            };
            SetApplicationColorScheme(ApplicationColorScheme, null);


            //Set Assembly version to app
            lblVersion.Content = "Ver: " + GetAssemblyVersion();

            //Set Default values
            SetAppDefaultValues();

            // creating the datagrid tables
            DataGridObjects_Folder = new List<DataGridObject>();
            DataGridObjects_FileData = new List<DataGridObject>();
            TrackFrameDatas = new Dictionary<string, string>();

            //Populate default datagrid columns
            PopulateDataGridWithColumns(dataGridFolderList, DataGridFolderColumnNames);
            PopulateDataGridWithColumns(dataGridFileData, DataGridTrackDataColumnNames);

            //Create timer function
            _timer = new System.Timers.Timer(250); //Updates every quarter second.
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        }

        private string? SelectedFolderPath { set; get; }
        private string CurrentLoadedFile { set; get; }
        private bool Playing { set; get; }
        private bool NewSong { set; get; }
        private readonly System.Timers.Timer _timer;
        private SynchronizationContext _uiContext = SynchronizationContext.Current;

        private Players.NPlayer.NPlayer NPlayer { set; get; }

        //Datagrid Objects
        private List<string> DataGridFolderColumnNames = new List<string>() { "TrackNo", "Track" };
        private List<string> DataGridTrackDataColumnNames = new List<string>() { "Info", "Value" };
        private Dictionary<string, string> TrackFrameDatas;
        private List<DataGridObject> DataGridObjects_Folder;
        private List<DataGridObject> DataGridObjects_FileData;
        private ApplicationColorScheme ApplicationColorScheme;


        //Trach handler
        private Mp3TrackHandler Mp3TrackHandler { set; get; }

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

            //Stop player if its running and set flags
            StopPlayer();
            Playing = false;
            NewSong = false;

            //Populate Folder File View
            PopulateFolderFileDataGridWithValues();

            //Populate Track data
            LoadTrackData();
            PopulateTrackDataGridWithValues();

        }

        private void btnStepBackSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectPrevious();
        }

        private void btnRewindSelect_Click(object sender, RoutedEventArgs e)
        {
            RewindTrack();
        }

        private void btnStopSelect_Click(object sender, RoutedEventArgs e)
        {
            btnPlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            Playing = false;
            NewSong = false;
            StopPlayer();
        }

        private void btnPlayPauseSelect_Click(object sender, RoutedEventArgs e)
        {
            if(Playing && !NewSong)
            {
                NPlayer.TogglePlayPause(NPlayer.GetVolume());
            }
            else
            {
                try
                {
                    if(NewSong)
                    {
                        StopPlayer();
                    }

                    StartPlayer();

                    //Set states
                    btnPlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                    Playing = true;
                    NewSong = false;
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
                        StopPlayer();
                        Playing = true;
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
                }

                if (Playing)
                {
                    StopPlayer();
                    Playing = true;
                    NewSong = true;
                    btnPlayPauseSelect_Click(this, null);
                }

            }
            catch { }
        }

        private void btnRewindStepSelect_Click(object sender, RoutedEventArgs e)
        {
            var currentTime = NPlayer.GetPositionInSeconds();
            if (currentTime < 30)
            {
                NPlayer.SetPosition(0);
            }
            else
            {
                NPlayer.SetPosition(currentTime - 30);
            }
        }

        private void btnFastForwardStepSelect_Click(object sender, RoutedEventArgs e)
        {
            var currentTime = NPlayer.GetPositionInSeconds();
            var maxTime = NPlayer.GetLenghtInSeconds();
            if (maxTime < currentTime + 30)
            {
                NPlayer.SetPosition(maxTime - 1);
            }
            else
            {
                NPlayer.SetPosition(currentTime + 30);
            }
        }

        private void btnFastForwardSelect_Click(object sender, RoutedEventArgs e)
        {
            FastForwardTrack();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var _settings = new Settings_Form(ApplicationColorScheme);
                _settings.UpdatedColorscheme += SetApplicationColorScheme;
                _settings.Show();
            }
            catch (Exception ex)
            {

            }
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            var _aboutForm = new Forms.About_Form();
            _aboutForm.ShowDialog();
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
                    Binding = new System.Windows.Data.Binding("RowValues[" + headerItem.Trim().ToUpper() + "]"),
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

        private void PopulateTrackDataGridWithValues()
        {
            //Empty datagrid first.
            dataGridFileData.ItemsSource = null;
            DataGridObjects_FileData.Clear();

  
            if (TrackFrameDatas.Count != 0)
            {
                try
                {
                    int i = 1;
                    foreach (var frameData in TrackFrameDatas)
                    {

                        //Constructor
                        var dic = new Dictionary<string, string>();

                        //Populate data
                        try
                        {
                            dic.Add(DataGridTrackDataColumnNames[0].ToUpper(), frameData.Key);
                            dic.Add(DataGridTrackDataColumnNames[1].ToUpper(), frameData.Value);
                        }
                        catch { }

                        //Create object and add to List.
                        var o = new DataGridObject()
                        {
                            RowValues = dic
                        };
                        DataGridObjects_FileData.Add(o);
                    }

                    //Bind the source
                    dataGridFileData.ItemsSource = DataGridObjects_FileData;
                }
                catch { }

                dataGridFileData.SelectedIndex = 0;
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

            LoadTrackData();
            PopulateTrackDataGridWithValues();
        }

        //=======================================================
        //                     Events
        //=======================================================

        private void SetControlsPause()
        {
            btnPlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.PlayPause;
        }

        private void SetControlsPlayPause()
        {
            btnPlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
        }

        private void SetControlsStopped()
        {
            if (!Playing)
            { 
                btnPlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                Playing = false;
            }
            else
            {
                SelectNext();
            }
        }

        private void SetApplicationColorScheme(object sender, EventArgs e)
        {
            var scheme = (ApplicationColorScheme)sender;

            //Buttons
            btnAbout.Foreground = scheme.ApplicationButtonText;
            btnBack.Foreground = scheme.ApplicationButtonText;
            btnForward.Foreground = scheme.ApplicationButtonText;
            btnExit.Foreground = scheme.ApplicationButtonText;
            btnFastForward.Foreground = scheme.ApplicationButtonText;
            btnOpen.Foreground = scheme.ApplicationButtonText;
            btnPlayIcon.Foreground = scheme.ApplicationButtonText;
            btnPlayPause.Foreground = scheme.ApplicationButtonText;
            btnFastForwardStep.Foreground = scheme.ApplicationButtonText;
            btnFastForward.Foreground = scheme.ApplicationButtonText;
            btnRewind.Foreground = scheme.ApplicationButtonText;
            btnRewindStep.Foreground = scheme.ApplicationButtonText;
            btnSettings.Foreground = scheme.ApplicationButtonText;
            btnStop.Foreground = scheme.ApplicationButtonText;

            //DataGrids and grids
            dataGridFileData.Background = scheme.ApplicationBackground;
            dataGridFolderList.Background = scheme.ApplicationBackground;
            dataGridFileData.Foreground = scheme.ApplicationDatagridText;
            dataGridFolderList.Foreground = scheme.ApplicationDatagridText;
            gridFooter.Background = scheme.ApplicationBackground;
            gridDatagrid.Background = scheme.ApplicationBackground;

            //Labels
            lbl_ScoreText.Foreground = scheme.ApplicationButtonText;

            //Banner
            grdMenyBar.Background = scheme.ApplicationBanner;

            //Logo
            appIcon.Foreground = scheme.ApplicationLogo;
            



            ApplicationColorScheme = scheme;
        }

        //=======================================================
        //                     Player Actions
        //=======================================================

        private void StartPlayer()
        {
            var selectedValue = dataGridFolderList.SelectedValue as DataGridObject;

            if (selectedValue != null)
            {

                //Select file?
                CurrentLoadedFile = $"{SelectedFolderPath}\\{selectedValue.RowValues.FirstOrDefault(x => x.Key == "TRACK").Value}";

                //Start player
                NPlayer = new Players.NPlayer.NPlayer(CurrentLoadedFile, 1.0f);
                NPlayer.Play(PlaybackState.Stopped, NPlayer.GetVolume());


                //Next song should be selected automatically.
                NPlayer.PlayerPaused += SetControlsPause;
                NPlayer.PlayerResumed += SetControlsPlayPause;
                NPlayer.PlayerStopped += SetControlsStopped;

                //Enable timer
                _timer.Enabled = true;
            }
        }

        private void StopPlayer()
        {
            if (NPlayer != null)
            {
                NPlayer.Stop();
                btnPlayIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;

                //Next song should be selected automatically.
                NPlayer.PlayerPaused -= SetControlsPause;
                NPlayer.PlayerResumed -= SetControlsPlayPause;
                NPlayer.PlayerStopped -= SetControlsStopped;

                //Disabled timer and reset UI Label
                _timer.Enabled = false;
                lbl_ScoreText.Content = "00:00:00";
            }

            this.Title = "Audioplayer";
           
        }

        private void LoadTrackData()
        {
            try
            {
                //Get the selected value from datagrid
                var selectedValue = dataGridFolderList.SelectedValue as DataGridObject;

                if (selectedValue != null)
                {
                    //Read file data
                    var filePath = $"{SelectedFolderPath}\\{selectedValue.RowValues.FirstOrDefault(x => x.Key == "TRACK").Value}";
                    var trackData = File.ReadAllBytes(filePath);

                    //Constructor
                    Mp3TrackHandler = new Mp3TrackHandler(trackData);

                    var tagId = Mp3TrackHandler.HeaderData.TagIdentifier();
                    var tagVersion = Mp3TrackHandler.HeaderData.TagVersion();
                    var tagSize = Mp3TrackHandler.HeaderData.TagSize();

                    var tags = Mp3TrackHandler.FrameData.ExtracedFrameData(tagSize);

                    if(tags.Count != 0)
                    {
                        var artist = (tags.FirstOrDefault(x => x.FrameIdentifier == "TPE1").FrameData ?? "").Replace("\0", "");
                        var album = (tags.FirstOrDefault(x => x.FrameIdentifier == "TALB").FrameData ?? "").Replace("\0", "");
                        var title = (tags.FirstOrDefault(x => x.FrameIdentifier == "TIT2").FrameData ?? "").Replace("\0", "");

                    TrackFrameDatas = new Dictionary<string, string>
                        {
                            { "TrackID", tagId },
                            { "TagVer.", tagVersion },
                            { "TrackNo", (tags.FirstOrDefault(x => x.FrameIdentifier == "TRCK").FrameData ?? "").Replace("\0", "") },
                            { "Artist", artist },
                            { "Year", (tags.FirstOrDefault(x => x.FrameIdentifier == "TYER").FrameData ?? "").Replace("\0", "") },
                            { "Album", album },
                            { "Title", title }
                        };

                        //Update Window title
                        this.Title = $"Audioplayer | {artist} | {album} | {title}";
                    }


                }
            }
            catch { }
        }

        private void RewindTrack()
        {
            try
            {
                var currentPosition = NPlayer.GetPositionInSeconds();

                if (currentPosition > 1)
                {
                    NPlayer.SetPosition(currentPosition - 1);
                }
                else
                {
                    if (dataGridFolderList.SelectedIndex > 0)
                    {
                        SelectPrevious();
                    }
                    if (currentPosition > 1)
                    {
                        var trackLength = NPlayer.GetLenghtInSeconds();
                        NPlayer.SetPosition(trackLength - 1);
                    }
                }
            }
            catch { }
        }

        private void FastForwardTrack()
        {
            try
            {
                var currentPosition = NPlayer.GetPositionInSeconds();
                var trackLength = NPlayer.GetLenghtInSeconds();

                if (currentPosition < trackLength - 1)
                {
                    NPlayer.SetPosition(currentPosition + 1);
                }
                else
                {
                    if (dataGridFolderList.SelectedIndex < dataGridFolderList.Items.Count)
                    {
                        SelectNext();
                    }
                }
            }
            catch { }
        }

        //=======================================================
        //                     Timer Actions
        //=======================================================

        async Task<string> GetPosition(Players.NPlayer.NPlayer nPlayer)
        {
            return TimeSpan.FromSeconds(Math.Round(NPlayer.GetPositionInSeconds())).ToString();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                _uiContext.Post(new SendOrPostCallback(new Action<object>(o => 
                { 
                    lbl_ScoreText.Content = Task.Run(() => GetPosition(NPlayer)).Result;
                })), null);
            }
            catch { }
        }

    }
}
