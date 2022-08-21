using DiscUtils.Iso9660;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinUI.Interop.CoreWindow;

namespace IsoCreator.UI
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void BrowseSourceButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            FolderPicker folderPicker = new();
            (folderPicker as object as IInitializeWithWindow).Initialize(CoreWindowInterop.CoreWindowHwnd);
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
                SourcePathTextBox.Text = folder.Path;
        }

        private async void BrowseDestinationButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            FileSavePicker savePicker = new();
            (savePicker as object as IInitializeWithWindow).Initialize(CoreWindowInterop.CoreWindowHwnd);
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Iso file", new List<string>() { ".iso" });
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
                DestinationPathTextBox.Text = file.Path;
        }

        private async void GenerateButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SettingsContainer.IsEnabled = false;
            ProgressRing.Visibility = Visibility.Visible;

            try
            {
                string source = SourcePathTextBox.Text;
                string destination = DestinationPathTextBox.Text;
                await Task.Run(() =>
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Highest;

                    CDBuilder builder = new();

                    FileInfo fileInfo = new(source);
                    builder.VolumeIdentifier = Path.GetFileNameWithoutExtension(destination);

                    DirectoryInfo di = new(source);
                    PopulateFromFolder(builder, di, di.FullName);

                    builder.Build(destination);
                });

                FolderLauncherOptions folderLauncherOptions = new();
                folderLauncherOptions.ItemsToSelect.Add(await StorageFile.GetFileFromPathAsync(destination));
                await Launcher.LaunchFolderPathAsync(Path.GetDirectoryName(destination), folderLauncherOptions);
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new();
                dialog.Title = $"Error: {ex.GetType().Name}";
                dialog.Content = ex.Message;
                dialog.PrimaryButtonText = "Close";
                _ = dialog.ShowAsync();
            }

            ProgressRing.Visibility = Visibility.Collapsed;
            SettingsContainer.IsEnabled = true;
        }

        private static void PopulateFromFolder(CDBuilder builder, DirectoryInfo di, string basePath)
        {
            foreach (FileInfo file in di.GetFiles())
                builder.AddFile(file.FullName.Substring(basePath.Length), file.FullName);

            foreach (DirectoryInfo dir in di.GetDirectories())
                PopulateFromFolder(builder, dir, basePath);
        }
    }
}
