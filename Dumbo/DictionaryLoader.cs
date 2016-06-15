using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dumbo
{
    class DictionaryLoader
    {

        string RootPath;
        string StartSettingsPath;
        public List<Dict> Dictionaries;

        public DictionaryLoader()
        {
            RootPath = @"C:\Users\evan\modules\voicecode\dictionary\";
            StartSettingsPath = @"C:\Users\evan\modules\voicecode\settings.json";

            Dictionaries = new List<Dict>();
        }

        public void LoadDictionaryFiles()
        {
            DirSearch(RootPath);
        }

        public void LoadJson(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                Dict dict = JsonConvert.DeserializeObject<Dict>(json);
                Dictionaries.Add(dict);
            }
        }

        public Settings LoadSettings()
        {
            using (StreamReader r = new StreamReader(StartSettingsPath))
            {
                string json = r.ReadToEnd();
                var settings = JsonConvert.DeserializeObject<Settings>(json);
                foreach (var scope in settings.Scopes)
                {
                    Debug.Assert(scope.Value.Options.Contains(scope.Value.Current));
                }
                return settings;
            }
        }

        private void DirSearch(string sDir)
        {
            foreach (string f in Directory.GetFiles(sDir))
            {
                LoadJson(f);
            }
            foreach (string d in Directory.GetDirectories(sDir))
            {
                this.DirSearch(d);
            }
        }
    }

    class Dict
    {
        public Dictionary<string, string> Scopes;
        public Dictionary<string, string> Commands;
        public Dictionary<string, string> NumberedCommands;
        public string[] DictationCommands;
    }

    class Settings
    {
        public Dictionary<string, Scope> Scopes;
        public string EnableKeyword;
        public string DisableKeyword;
    }

    class Scope
    {
        public string Current;
        public string[] Options;
    }

}
