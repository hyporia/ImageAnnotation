using ImageAnnotation.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Newtonsoft.Json;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace ImageAnnotation
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IReadOnlyList<StorageFile> Images;
        private List<MyImage> MyImages = new List<MyImage>();
        private MyImage CurrentMyImage;
        private int CurrentImageIndex = 0;
        private string CurrentFolderPath;
        private string CurrentFolderName;
        private StorageFolder currentImageFolder;
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        public MainPage()
        {
            this.InitializeComponent();


        }

        private async Task GetImages()
		{
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            var folder = await picker.PickSingleFolderAsync();
            if (folder != default(StorageFolder))
            {
                
				try
                {
                    LoadingIndicator.IsActive = true;
                    Images = await folder.GetFilesAsync();
                    currentImageFolder = folder;
                    CurrentFolderPath = folder.Path;
                    CurrentFolderName = folder.Name;
                }
				finally
                {
                    LoadingIndicator.IsActive = false;
                }
			}
        }

        private async Task SetImage()
		{
            if (Images != null && CurrentImageIndex > -1 && CurrentImageIndex < Images.Count)
			{
                this.image.Source = await GetImageFromFileAsync(Images[CurrentImageIndex]);
            }
        }

        private async Task<BitmapImage> GetImageFromFileAsync(StorageFile file)
		{
            using (var randomAccessStream = await file.OpenAsync(FileAccessMode.Read))
            {
                var result = new BitmapImage();
                await result.SetSourceAsync(randomAccessStream);
                return result;
            }
        }

		private async void Page_LoadedAsync(object sender, RoutedEventArgs e)
		{

		}

        private async Task SetCurrentCheckboxesStates()
		{
            if (CurrentMyImage == null) return;
            checkBox_IsDamaged.IsChecked = Convert.ToBoolean(CurrentMyImage.IsDamaged);
            checkBox_HasLabel.IsChecked = Convert.ToBoolean(CurrentMyImage.HasLabel);
            checkBox_IsDirty.IsChecked = Convert.ToBoolean(CurrentMyImage.IsDirty);
        }

        private async Task SetCurrentMyImage()
		{
            if (Images == null || CurrentImageIndex < 0 || CurrentImageIndex >= Images.Count) return;
            if (CurrentImageIndex >= MyImages.Count)
			{
                CurrentMyImage = new MyImage { Name = Images[CurrentImageIndex].Name };
                MyImages.Add(CurrentMyImage);
			}
			else
			{
                CurrentMyImage = MyImages[CurrentImageIndex];  
			}
            await SetCurrentCheckboxesStates();
        }

		private async void ButtonPickFolder_ClickAsync(object sender, RoutedEventArgs e)
		{
            await GetImages();
            await RestoreLastState();
            await SetImage();
            await SetCurrentMyImage();
        }

		private async void buttonNext_ClickAsync(object sender, RoutedEventArgs e)
		{
            if (CurrentImageIndex + 1 < Images.Count)
            {
                CurrentImageIndex++;
                await SetImage();
                await SetCurrentMyImage();
            }
		}

		private async void buttonPrev_ClickAsync(object sender, RoutedEventArgs e)
		{
            if (CurrentImageIndex - 1 >= 0)
            {
                CurrentImageIndex--;
                await SetImage();
                await SetCurrentMyImage();
            }
        }

		private async void Grid_KeyUpAsync(object sender, KeyRoutedEventArgs e)
		{
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Right:
                    buttonNext_ClickAsync(null, null);
                    break;
                case Windows.System.VirtualKey.Left:
                    buttonPrev_ClickAsync(null, null);
                    break;
                case Windows.System.VirtualKey.Number1:
                    ReverseCheckBox(checkBox_IsDirty);
                    CheckBox_IsDirty_Click(null, null);
                    break;
                case Windows.System.VirtualKey.Number2:
                    ReverseCheckBox(checkBox_HasLabel);
                    CheckBox_HasLabel_Click(null, null);
                    break;
                case Windows.System.VirtualKey.Number3:
                    ReverseCheckBox(checkBox_IsDamaged);
                    CheckBox_IsDamaged_Click(null, null);
                    break;
                case Windows.System.VirtualKey.S:
                    await SaveCurrentState();
                    break;
                default: break;
            }
        }

		private void CheckBox_IsDirty_Click(object sender, RoutedEventArgs e)
		{
            if (CurrentMyImage == null) return;
            CurrentMyImage.IsDirty = Convert.ToInt16(checkBox_IsDirty.IsChecked);
		}

		private void CheckBox_HasLabel_Click(object sender, RoutedEventArgs e)
		{
            if (CurrentMyImage == null) return;
            CurrentMyImage.HasLabel = Convert.ToInt16(checkBox_HasLabel.IsChecked);
        }

		private void CheckBox_IsDamaged_Click(object sender, RoutedEventArgs e)
		{
            if (CurrentMyImage == null) return;
            CurrentMyImage.IsDamaged = Convert.ToInt16(checkBox_IsDamaged.IsChecked);
        }

        private void ReverseCheckBox(CheckBox checkBox)
		{
            checkBox.IsChecked = !checkBox.IsChecked;
		}

        private async Task SaveCurrentState()
		{
            
            if (MyImages != null && MyImages.Any())
			{
				try
                {
                    textBox_State.Text = "Saving state...";
                    textBox_State.Visibility = Visibility.Visible;

                    var fileName = $"{CurrentFolderName}_{DateTime.Now:ddMMyy_HHmmss}.json";
                    var jsonString = JsonConvert.SerializeObject(MyImages);

                    var myImagesJsonFileLocal = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(myImagesJsonFileLocal, jsonString);

                    var annotationFolder = await currentImageFolder.CreateFolderAsync("annotation", CreationCollisionOption.OpenIfExists);
                    var myImagesJsonFile = await annotationFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(myImagesJsonFile, jsonString);


                    localSettings.Values["LastFileName"] = fileName;
                    localSettings.Values["LastImageIndex"] = CurrentImageIndex;
                    localSettings.Values["LastFolderPath"] = CurrentFolderPath;

                    textBox_State.Visibility = Visibility.Collapsed;
                }
				catch(Exception ex)
                {
                    var myImagesJsonFile = await localFolder.CreateFileAsync($"error_{DateTime.Now:ddMMyy_HHmmss}.txt",
                                CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(myImagesJsonFile, ex.Message);
                    textBox_State.Text = "Error";
                }
            }

        }

        private async Task RestoreLastState()
        {
            try
            {
                textBox_State.Text = "Restoring state...";
                textBox_State.Visibility = Visibility.Visible;

                var hasAllVariables = localSettings.Values.ContainsKey("LastFolderPath") && localSettings.Values.ContainsKey("LastImageIndex")
                    && localSettings.Values.ContainsKey("LastFileName");

                if (hasAllVariables)
				{
                    var lastFolderPath  = (string)localSettings.Values["LastFolderPath"];
                    if (CurrentFolderPath == lastFolderPath)
                    {
                        var fileName = (string)localSettings.Values["LastFileName"];
                        CurrentImageIndex = (int)localSettings.Values["LastImageIndex"];

                        var myImagesJsonFile = await localFolder.GetFileAsync(fileName);
                        var jsonString = await FileIO.ReadTextAsync(myImagesJsonFile);

                        MyImages = JsonConvert.DeserializeObject<List<MyImage>>(jsonString);
                    }

                }
                textBox_State.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                var myImagesJsonFile = await localFolder.CreateFileAsync($"error_{DateTime.Now:ddMMYY_HHmmss}.txt",
                            CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(myImagesJsonFile, ex.Message);
                textBox_State.Text = "Error";
            }
        }
    }
}
