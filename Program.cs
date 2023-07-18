using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

class Program
{
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    static void Main()
    {
        string saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dexrn", "APIWallpaper");
        Directory.CreateDirectory(saveDirectory); // Create the directory if it doesn't exist

        string urlFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dexrn", "APIWallpaper", "url.txt");

        if (!File.Exists(urlFilePath))
        {
            ShowMessageBox("Error", "The '%localappdata%\\Dexrn\\APIWallpaper\\url.txt' file doesn't exist. Please make sure to create the file and specify the image URL.", MessageBoxIcon.Error);
            return;
        }

        string imageUrl = File.ReadAllText(urlFilePath).Trim(); // Read the URL from the file and trim any leading/trailing spaces

        string savePath = Path.Combine(saveDirectory, "downloaded-wallpaper.png");

        using (var client = new WebClient())
        {
            client.DownloadFile(imageUrl, savePath);
        }

        SetDesktopBackground(savePath);

    }

    static void SetDesktopBackground(string imagePath)
    {
        const int SPI_SETDESKWALLPAPER = 0x0014;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDCHANGE = 0x02;

        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

        key.SetValue("WallpaperStyle", "2"); // 2 - Stretch to fit desktop
        key.SetValue("TileWallpaper", "0"); // 0 - No tiling

        key.Close();

        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
    }

    static void ShowMessageBox(string caption, string message, MessageBoxIcon icon)
    {
        const uint MB_OK = 0x00000000;
        const uint MB_ICONERROR = 0x00000010;
        const uint MB_ICONINFORMATION = 0x00000040;

        uint type = MB_OK;
        switch (icon)
        {
            case MessageBoxIcon.Error:
                type |= MB_ICONERROR;
                break;
            case MessageBoxIcon.Information:
                type |= MB_ICONINFORMATION;
                break;
        }

        MessageBox(IntPtr.Zero, message, caption, type);
    }

    enum MessageBoxIcon
    {
        Error,
        Information
    }
}
