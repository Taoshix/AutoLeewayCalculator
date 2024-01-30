using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticLeewayCalculator
{
    public class SettingsManager
    {
        public List<Setting> Settings = new List<Setting>();
        public SettingsManager() { }
        public SettingsManager(List<Setting> settings)
        {
            Settings = settings;
        }
        public bool ToggleSetting(int id)
        {
            return Settings.Find(x => x.Id == id).ToggleState();
        }
        public void ToggleSetting(string name)
        {
            Settings.Find(x => x.Name == name).ToggleState();
        }
        public void ToggleSetting(Setting setting)
        {
            Settings.Find(x => x.Id == setting.Id).ToggleState();
        }
        public void ToggleSetting(int id, bool state)
        {
            Settings.Find(x => x.Id == id).State = state;
        }
        public void ToggleSetting(string name, bool state)
        {
            Settings.Find(x => x.Name == name).State = state;
        }
        public void ToggleSetting(Setting setting, bool state)
        {
            Settings.Find(x => x.Id == setting.Id).State = state;
        }
        public void ModifySetting(int id, string name, string description, bool state)
        {
            Settings.Find(x => x.Id == id).Name = name;
            Settings.Find(x => x.Id == id).Description = description;
            Settings.Find(x => x.Id == id).State = state;
        }
        public void AddSetting(int id, string prefix, string name, string description, bool state)
        {
            Settings.Add(new Setting(id, prefix, name, description, state));
        }
        public void RemoveSetting(int id)
        {
            Settings.Remove(Settings.Find(x => x.Id == id));
        }
        public List<Setting> GetSettings()
        {
            return Settings;
        }
        public Setting GetSetting(int id)
        {
            return Settings.Find(x => x.Id == id);
        }
    }
}
