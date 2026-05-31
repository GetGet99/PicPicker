using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PicPicker;

[QuickMarkup("""
    string ImagePath = "";
    <setup>
        var theme = ThemeBrushes.Global;
    </setup>
    <root>
        <Border Margin=4 BorderBrush=`theme.CardStroke` BorderThickness=1 Width=200 MinHeight=100>
            <Grid
                @Tapped+=`CopyImageToClipboard(ImagePath)`
                ContextFlyout=<MenuFlyout>
                    <MenuFlyoutItem Icon=<SymbolIcon(Delete) /> Text="Delete" @Click+=`DeleteRequest?.Invoke()` />
                </MenuFlyout>>
                <Image Source=`ImagePath is "" ? null
                    : new BitmapImage(new Uri(ImagePath, UriKind.Absolute)) { DecodePixelWidth = 200 }`
                    Stretch=UniformToFill
                />
                <Border Bottom Background=`theme.LayerFill` Padding=4>
                    <TextBlock Text=`ImagePath is "" ? "" : System.IO.Path.GetFileName(ImagePath)` Right
                        Foreground=`theme.PrimaryText`
                        TextTrimming=WordEllipsis
                    />
                </Border>
            </Grid>
        </Border>
    </root>
    """)]
partial class ImageDisplay : IQuickMarkupComponent
{
    public event Action? Completed;
    public event Action? DeleteRequest;
    async void CopyImageToClipboard(string path)
    {
        try
        {
            var storageFile = await StorageFile.GetFileFromPathAsync(path);
            var streamRef = RandomAccessStreamReference.CreateFromFile(storageFile);

            var dataPackage = new DataPackage();
            dataPackage.SetBitmap(streamRef);
            dataPackage.SetStorageItems(new[] { storageFile });

            var ext = System.IO.Path.GetExtension(path).ToLower();
            if (ext is ".png")
                dataPackage.SetData("PNG", streamRef);
            else if (ext is ".gif")
                dataPackage.SetData("GIF", streamRef);

            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
            Completed?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}