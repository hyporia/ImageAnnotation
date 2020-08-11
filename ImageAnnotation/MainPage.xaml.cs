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

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace ImageAnnotation
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IReadOnlyList<StorageFile> Images;
        private int CurrentImageIndex = -1;
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
				}
				finally
                {
                    LoadingIndicator.IsActive = false;
                }
			}
        }

        private async Task SetImage(int index)
		{
            if (Images != null && index > -1 && index < Images.Count)
			{
                this.image.Source = await GetImageFromFileAsync(Images[index]);
                CurrentImageIndex = index;
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

		private async void Button_ClickAsync(object sender, RoutedEventArgs e)
		{
            await GetImages();
            await SetImage(0);
        }

		private async void buttonNext_ClickAsync(object sender, RoutedEventArgs e)
		{
            await SetImage(++CurrentImageIndex);
		}

		private async void buttonPrev_ClickAsync(object sender, RoutedEventArgs e)
		{
            await SetImage(--CurrentImageIndex);
        }

		private async void Grid_KeyUp(object sender, KeyRoutedEventArgs e)
		{
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Right:
                    buttonNext_ClickAsync(null, null);
                    break;
                case Windows.System.VirtualKey.Left:
                    buttonPrev_ClickAsync(null, null);
                    break;
                default: break;
            }
        }
	}
}
