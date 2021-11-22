using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallHaven.WallHavenClient;

namespace WallHaven
{
    public class Settings
    {
        private static string AppSettingPath
        {
            get
            {
                string roming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                string appDataPath = Path.Combine(roming, string.IsNullOrWhiteSpace(appName) ? "WallHaven" : appName);
                string appSettingPath = Path.Combine(appDataPath, "settings.json");
                if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);
                return appSettingPath;
            }
        }

        public static string AppCacheImagePath => Path.Combine(AppSettingPath, "..", "wallhaven.jpg");

        public static void SaveSetting(Settings setting)
        {
            try
            {
                File.WriteAllText(AppSettingPath, JsonConvert.SerializeObject(setting));
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
        }

        public static Settings ReadSetting()
        {
            Settings settings = new Settings();
            if (File.Exists(AppSettingPath))
            {
                try
                {
                    string text = File.ReadAllText(AppSettingPath);
                    Settings? result = JsonConvert.DeserializeObject<Settings>(text);
                    if (result != null) settings = result;
                }
                catch (Exception)
                {
                }
            }
            return settings;
        }

        public string APIKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://wallhaven.cc/api/v1/";
        public bool General { get; set; } = true;
        public bool Anime { get; set; } = true;
        public bool People { get; set; } = true;
        public bool SFW { get; set; } = true;
        public bool Sketchy { get; set; } = true;
        public bool NSFW { get; set; } = true;
        public bool Wide { get; set; } = true;
        public bool UltraWide { get; set; } = true;
        public bool Portrait { get; set; } = true;
        public bool Square { get; set; } = true;
        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public Sorting Sort { get; set; } = Sorting.random;
    }
}
