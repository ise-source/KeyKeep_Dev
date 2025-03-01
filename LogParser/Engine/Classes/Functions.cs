using LogParser.Engine.DataBase;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace LogParser.Engine.Classes
{
    public class Functions
    {
        private static Random rnd = new Random();
        private static readonly HttpClient client = new HttpClient();
        private static readonly string GitHubRepoUrl = $"https://api.github.com/repos/ise-source/KeyKeep_Release/releases/latest";
        public static readonly string directoryPath = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        private static dynamic Release { get; set; } = null;
        private static string LatestVersion { get; set; } = null;

        public static async Task LoadData()
        {
            Handler.AllData = await DataManager.LoaDataAsync();
        }

        public static async void SaveToBD()
        {
            await DataManager.SaveBD();
        }

        public static void NavigatePage(Page page)
        {
            Handler.MainFrame?.Navigate(page);
        }

        public static async Task OpenDialog(UserControl page)
        {
            if (Handler.MainDialogControl != null)
                await DialogHost.Show(page, Handler.MainDialogControl.Identifier).ConfigureAwait(false);
        }

        public static void CloseDialog()
        {
            if (Handler.MainDialogControl != null && Handler.MainDialogControl.IsOpen == true)
                Handler.MainDialogControl.IsOpen = false;
        }

        public static string EncryptPassword(int Lenght)
        {
            string Char = "•";

            string password = string.Empty;

            for (int i = 0; i < Lenght; i++)
                password += Char;

            return password;
        }

        public static string CreatePassword()
        {
            string chars = "1234567890@#$^qwertyuiop[]asdfghjkl;zxcvbnm";
            string password = string.Empty;

            for (int i = 0; i < 10; i++)
                password += chars[rnd.Next(chars.Length)];

            return password;
        }

        public static void AddToFavorite(DataModel model)
        {
            model.IsFavorite = true;
            Notify.Send("Добавлено в избранные!", TypeNotify.Success);
        }

        public static void RemoveToFavorite(DataModel model)
        {
            model.IsFavorite = false;
            Notify.Send("Удалено из избранных!", TypeNotify.Success);
        }

        public static async Task<bool> CheckUpdates()
        {
            try
            {
                if (Config.CheckAutoUpdates)
                {
                    Version currentVersion = new Version(Config.Current_Version);

                    client.DefaultRequestHeaders.Add("User-Agent", "request");

                    var response = await client.GetAsync(GitHubRepoUrl);

                    // Проверяем статус ответа
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Release = JsonConvert.DeserializeObject(responseBody);
                        LatestVersion = Release.tag_name;

                        if (new Version(LatestVersion) > currentVersion) return true;
                        else return false;
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        Notify.Send($"Проверка обновлений\nРепозиторий не найден!", TypeNotify.Error);
                        return false;
                    }
                    else
                    {
                        Notify.Send($"Проверка обновлений\nНе удалось получить данные: {response.StatusCode}", TypeNotify.Error);
                        return false;
                    }
                }
                else return false;
            }
            catch (Exception ex)
            {
                // Обработка других исключений
                Console.WriteLine($"Ошибка: {ex.Message}");
                return false;
            }
        }

        public static async Task LoadConfig()
        {
            try
            {
                string pathToDirectory = Path.Combine(directoryPath, "Config");
                string pathToFileINI = Path.Combine(pathToDirectory, "config.ini");

                if (!Directory.Exists(pathToDirectory))
                {
                    Directory.CreateDirectory(pathToDirectory);

                    File.WriteAllText(pathToFileINI, string.Empty);
                }

                if (!File.Exists(pathToFileINI))
                {
                    File.WriteAllText(pathToFileINI, string.Empty);
                }

                var cfgSettings = typeof(Config).GetProperties(BindingFlags.Public | BindingFlags.Static);

                foreach (var config in cfgSettings)
                {
                    WriteToConfig(pathToFileINI, config.Name, config.GetValue(null).ToString());
                }


                await ReadConfig(pathToFileINI);
            }
            catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
        }

        public static void WriteToConfig(string path, string key, string value, bool replase = false)
        {
            List<string> allLines = new List<string>(File.ReadAllLines(path));
            bool keyFound = false;


            for (int i = 0; i < allLines.Count(); i++)
            {
                string line = allLines[i];
                string[] parts = line.Split('=');
                string keyInCFG = parts[0];

                if (keyInCFG == key)
                {
                    if (replase) allLines[i] = $"{key}={value}";

                    keyFound = true;
                    break;
                }
            }

            if (!keyFound)
            {
                allLines.Add($"{key}={value}");
            }

            File.WriteAllLines(path, allLines);
        }

        public static async Task ReadConfig(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string line = await reader.ReadLineAsync();
                    string[] parts = line.Split('=');
                    string key = parts[0];
                    string value = parts[1];

                    if (key == "CheckAutoUpdates")
                        Config.CheckAutoUpdates = Convert.ToBoolean(value);
                }
            }
        }

        public static void CheckOutDatesVersionApp()
        {
            try
            {
                string[] exeFiles = Directory.GetFiles(directoryPath, "*.exe");

                foreach (var file in exeFiles)
                {
                    if (!file.Contains($"{assemblyName}.exe"))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
        }

        public static void CheckBackups()
        {
            try
            {
                if (Config.CreateAutoBackups)
                {
                    DateTime nowTime = DateTime.Now;

                    string pathToBK = Path.Combine(directoryPath, "BK");
                    string pathDate = Path.Combine(pathToBK, nowTime.ToString("dd.MM.yyyy"));

                    CheckOutDatesBK(pathToBK);

                    if (!Directory.Exists(pathToBK))
                    {
                        Directory.CreateDirectory(pathToBK);
                    }

                    if (!Directory.Exists(pathDate))
                    {
                        Directory.CreateDirectory(pathDate);
                    }

                    foreach (string file in Directory.GetFiles(directoryPath, "*.db"))
                    {
                        string filename = Path.GetFileName(file);

                        string destinationFile = Path.Combine(pathDate, filename);
                        File.Copy(file, destinationFile, true);

                        break;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
        }

        private static void CheckOutDatesBK(string pathToBK)
        {
            try
            {
                if (!Directory.Exists(pathToBK)) return;

                foreach (var dirs in Directory.GetDirectories(pathToBK))
                {
                    DateTime dateToCreatedDir = Directory.GetCreationTime(dirs);

                    if (dateToCreatedDir < DateTime.Now.AddDays(-7))
                    {
                        Directory.Delete(dirs, true);
                    }
                }

            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
        }

        public static async Task UpdateApplication(UpdatesModel model)
        {
            model.Status = "Устанавливаю связь";

            try
            {
                if (Release == null)
                {
                    Notify.Send("Нет информации о релизе", TypeNotify.Error);
                    return;
                }

                string downloadUrl = Release.assets[0].browser_download_url;

                try
                {
                    model.Status = "Загружаю обновление";
                    var response = await client.GetAsync(downloadUrl);
                    response.EnsureSuccessStatusCode();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes("update.zip", fileBytes);
                    }

                    model.Status = "Установливаю компоненты";

                    if (File.Exists("update.zip"))
                    {
                        ZipFile.ExtractToDirectory("update.zip", directoryPath + "\\Update");
                    }

                    model.Status = "Установка завершена! Перезагрузка...";

                    await Task.Delay(2000);

                    Application.Current.Shutdown();

                    foreach (string file in Directory.GetFiles(directoryPath + "\\Update"))
                    {
                        string filename = Path.GetFileName(file);

                        if (filename == "BD.db") continue;

                        File.Copy(file, Path.Combine(directoryPath, filename), true);
                    }

                    Process.Start(Path.Combine(directoryPath, $"KeyKeep_{LatestVersion}.exe"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
                finally
                {
                    if (File.Exists("update.zip"))
                        File.Delete("update.zip");

                    if (Directory.Exists("Update"))
                        Directory.Delete("Update", true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static async Task<string> GetIconFromWeb(Uri uri, string imageName)
        {
            try
            {
                string pathToDirectory = Path.Combine(directoryPath, "Images");
                string pathToImage = Path.Combine(pathToDirectory, imageName);

                if (!Directory.Exists(pathToDirectory))
                    Directory.CreateDirectory(pathToDirectory);


                if (!File.Exists(pathToImage))
                {
                    byte[] iconData = await client.GetByteArrayAsync(uri);
                    File.WriteAllBytes(pathToImage, iconData);
                }

                return pathToImage;
            }
            catch (HttpRequestException ex)
            {
                // Логирование ошибки HTTP запроса
                Console.WriteLine($"HTTP Request error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                // Логирование других ошибок
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
    }
}
