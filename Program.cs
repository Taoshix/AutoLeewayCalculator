// Decompiled with JetBrains decompiler
// Type: LeewayCalculator.Program
// Assembly: LeewayCalculator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5FC93B0B-F9D0-470A-80E8-1FDEE2D43F01
// Assembly location: D:\Andet\Leeway_Calculator\LeewayCalculator.exe

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;
using System.IO;
using System.Diagnostics;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using System.Linq.Expressions;
using AutomaticLeewayCalculator;
using System.Runtime.InteropServices;

namespace LeewayCalculator
{
    public class Program
    {
        private static StructuredOsuMemoryReader _reader;
        private static bool _autoCalc = false;
        private static string _songsFolder = "";
        private static string _userName = "";
        private static SettingsManager _settingsManager = new SettingsManager(new List<Setting>());

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        private static void Main(string[] args)
        {
            LeewayCalculator leewayCalculator = new LeewayCalculator();
            _settingsManager.AddSetting(1, "!", "Leaderboard Lookups", "Toggle whether or not to show the leaderboard lookups", true);
            _settingsManager.AddSetting(2, "?", "Use Ingame Mods", "Toggle whether or not to use the mods you have ingame", true);
            _settingsManager.AddSetting(3, "*", "Always on Top", "Toggle whether or not the window should be always on top", false);
            _reader = StructuredOsuMemoryReader.Instance.GetInstanceForWindowTitleHint("");
            
            PrintIntroduction();

            _autoCalc = Console.ReadLine().ToLower() == "y";
            if (_autoCalc)
            {
                AutomaticMode(leewayCalculator);
            }
            else
            {
                ManualMode(leewayCalculator);
            }
        }

        private static void AutomaticMode(LeewayCalculator leewayCalculator)
        {
            OsuBaseAddresses osuBaseAddresses = new OsuBaseAddresses();
            CurrentBeatmap map = new CurrentBeatmap();
            string modInput;
            string savedMods;
            Console.WriteLine("\nAutomatic Mode");

            GetOsuProcess();
            PrintSettings(_settingsManager, true);

            Console.Write("Input mods, setting flags, or leave blank for 4 mod and then press ENTER: ");
            modInput = Console.ReadLine();
            modInput = ParseSettings(modInput);
            modInput = string.IsNullOrEmpty(modInput) ? "HDHRDTFL" : modInput;
            
            while (true)
            {
                do
                {
                    int currentMapID = 0;
                    string currentReadModsBinary = "";
                    while (!Console.KeyAvailable)
                    {
                        SetWindowAlwaysOnTop(_settingsManager.GetSetting(3).State);
                        _reader.TryReadProperty(map, "Id", out object ID);
                        _reader.TryReadProperty(osuBaseAddresses.GeneralData, nameof(GeneralData.Mods), out var emilly);
                        if (!_reader.CanRead || ID == null)
                        {
                            Console.WriteLine("osu! closed down");
                            AutomaticMode(leewayCalculator);
                        }
                        int mapId = (int)ID;
                        string readModsBinary = Convert.ToString((int)emilly, 2).PadLeft(32, '0');

                        if (currentMapID != mapId || (currentReadModsBinary != readModsBinary && _settingsManager.GetSetting(2).State))
                        {
                            currentMapID = mapId;
                            currentReadModsBinary = readModsBinary;
                            if (_reader.TryReadProperty(map, "FolderName", out object mapFolder) && _reader.TryReadProperty(map, "OsuFileName", out object mapFileName))
                            {
                                string absoluteFilename = Path.Combine(_songsFolder, ((string)mapFolder).TrimEnd(), ((string)mapFileName).TrimEnd());
                                string beatmap = File.ReadAllText(absoluteFilename);
                                string[] mods = _settingsManager.GetSetting(2).State ? leewayCalculator.GetMods(leewayCalculator.ReorderMods(ModsFromBinaryString(readModsBinary))) : leewayCalculator.GetMods(leewayCalculator.ReorderMods(modInput));

                                if (!leewayCalculator.IsValidModCombo(mods))
                                {
                                    mods = null;
                                }

                                Console.Clear();
                                Console.WriteLine("Automatic Mode - Press ESC to change mods/settings\n");
                                PrintSettings(_settingsManager, false);
                                leewayCalculator.PrintTable(mapId, beatmap, mods, _settingsManager.GetSetting(1).State, _userName);
                            }
                        }
                        Console.Title = $"AutomaticLeewayCalculator - Automatic Mode - {mapId} +{(_settingsManager.GetSetting(2).State ? leewayCalculator.ReorderMods(ModsFromBinaryString(readModsBinary)) : leewayCalculator.ReorderMods(modInput))}";
                        Thread.Sleep(50);
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
                PrintSettings(_settingsManager, true);
                Console.Write("Input new mods, setting flags, or leave blank for 4 mod and then press ENTER to continue: ");
                savedMods = modInput;

                modInput = Console.ReadLine();
                string afterSettings = ParseSettings(modInput);
                bool settingsChanged = modInput != afterSettings;
                modInput = afterSettings;
                modInput = string.IsNullOrEmpty(modInput) ? (settingsChanged ? savedMods : "HDHRDTFL") : modInput;
            }
        }

        private static void ManualMode(LeewayCalculator leewayCalculator)
        {
            Console.WriteLine("\nManual Mode");
            Console.WriteLine("Add a \"!\" at the start to toggle auto-clear.");
            Console.WriteLine("If osu! is running, you can type \"current\" instead of the ID to get the current map you are looking at in song select.");
            Console.Write("Enter Beatmap ID (+Mods): ");

            string input = Console.ReadLine().Trim();
            bool flag = true;
            while (true)
            {
                Console.WriteLine();
                if (input.StartsWith("!"))
                {
                    flag = !flag;
                    input = input.TrimStart('!');
                }
                if (flag)
                    Console.Clear();
                try
                {
                    string[] mods = null;
                    string beatmap;
                    int beatmapId;
                    if (Regex.IsMatch(input, "^\"(.*?)\".*?([A-Za-z]+)?$"))
                    {
                        Match match = Regex.Match(input, "^\"(.*?)\".*?([A-Za-z]+)?$");
                        beatmap = File.ReadAllText(match.Groups[1].Value);
                        beatmapId = leewayCalculator.GetBeatmapID(beatmap);
                        if (!string.IsNullOrEmpty(match.Groups[2].Value))
                            mods = leewayCalculator.GetMods(leewayCalculator.ReorderMods(match.Groups[2].Value.ToUpper()));
                    }
                    else if (input.Split(' ')[0] == "current")
                    {
                        string newMods = input.Split(' ').Length == 1 ? "HDHRDTFL" : input.Split(' ')[1].ToUpper().Replace("+", "");
                        mods = leewayCalculator.GetMods(leewayCalculator.ReorderMods(newMods));

                        GetOsuProcess();
                        CurrentBeatmap map = new CurrentBeatmap();
                        _reader.TryReadProperty(map, "Id", out object ID);
                        int mapId = (int)ID;
                        _reader.TryReadProperty(map, "FolderName", out object mapFolder);
                        _reader.TryReadProperty(map, "OsuFileName", out object mapFileName);
                        string absoluteFilename = Path.Combine(_songsFolder, ((string)mapFolder).TrimEnd(), ((string)mapFileName).TrimEnd());
                        beatmapId = mapId;
                        beatmap = File.ReadAllText(absoluteFilename);
                    }
                    else
                    {
                        Match match = Regex.Match(input, ".*(?:\\D|^)(\\d+).*?([A-Za-z]+)?$");
                        beatmapId = int.Parse(match.Groups[1].Value);
                        beatmap = leewayCalculator.GetBeatmap(beatmapId);
                        if (beatmap == "")
                        {
                            throw new Exception($"Unable to download beatmap with id {beatmapId}, does it exist? Got: empty beatmap");
                        }
                        mods = null;
                        if (!string.IsNullOrEmpty(match.Groups[2].Value))
                            mods = leewayCalculator.GetMods(leewayCalculator.ReorderMods(match.Groups[2].Value.ToUpper()));
                    }
                    if (!leewayCalculator.IsValidModCombo(mods))
                        mods = null;
                    Console.WriteLine("Manual Mode");
                    Console.Title = $"AutomaticLeewayCalculator - Manual Mode - {beatmapId} +{leewayCalculator.ReorderMods(leewayCalculator.GetModsString(mods))}";
                    leewayCalculator.PrintTable(beatmapId, beatmap, mods, true, _userName);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }
                Console.WriteLine("If osu! is running, you can type \"current\" instead of the ID");
                Console.Write("Enter Beatmap ID (+Mods): ");
                input = Console.ReadLine().Trim();
            }
        }

        private static string ParseSettings(string modInput)
        {
            modInput = modInput.Replace("+", "");
            foreach (var setting in _settingsManager.GetSettings())
            {
                if (modInput.Contains(setting.Prefix))
                {
                    _settingsManager.ToggleSetting(setting.Id);
                    modInput = modInput.Replace(setting.Prefix.ToString(), "");
                }
            }
            modInput = modInput.TrimStart(' ');
            return modInput;
        }

        private static string ModsFromBinaryString(string binaryString)
        {
            /*
            Hardrock   27
            DoubleTime 25
            Hidden     28
            Flashlight 21
            Easy       30
            HalfTime   23
            */
            string modString = "";
            for (int index = 0; index < binaryString.Length; ++index)
            {
                if (binaryString[index] == '1')
                {
                    switch (index)
                    {
                        case 21:
                            modString += "FL";
                            break;
                        case 23:
                            modString += "HT";
                            break;
                        case 25:
                            modString += "DT";
                            break;
                        case 28:
                            modString += "HD";
                            break;
                        case 27:
                            modString += "HR";
                            break;
                        case 30:
                            modString += "EZ";
                            break;
                    }
                }
            }
            return modString == "" ? "NM" : modString;
        }
        private static void GetOsuProcess()
        {
            Process[] processes = Process.GetProcessesByName("osu!");
            if (processes.Length == 0 || !_reader.CanRead || processes[0].HasExited)
            {
                Console.WriteLine("\nosu! is not running, please start osu!");
                Console.WriteLine("Waiting...");
                while (processes.Length == 0 || !_reader.CanRead || processes[0].HasExited)
                {
                    processes = Process.GetProcessesByName("osu!");
                    Thread.Sleep(50);
                }
            }
            OsuBaseAddresses osuBaseAddresses = new OsuBaseAddresses();
            _reader.TryRead(osuBaseAddresses.BanchoUser);
            _userName = osuBaseAddresses.BanchoUser.Username;
            Console.Clear();
            string osuExePath = processes[0].MainModule.FileName;
            _songsFolder = Path.Combine(Path.GetDirectoryName(osuExePath), "Songs");
        }

        private static void PrintSettings(SettingsManager settingsManager, bool showInstructions)
        {
            var settings = settingsManager.GetSettings();
            foreach (var setting in settings)
            {
                Console.Write($"{setting.Name}: ");
                Console.ForegroundColor = setting.State ? ConsoleColor.Green : ConsoleColor.Red;
                Console.Write(setting.State ? "ON" + "\n" : "OFF" + "\n");
                Console.ResetColor();

            }
            if (showInstructions)
            {
                foreach (var setting in settings)
                {
                    Console.WriteLine($"Type {setting.Prefix} to toggle {setting.Name}");
                }
            }
            Console.WriteLine();
        }

        private static void SetWindowAlwaysOnTop(bool isTop)
        {
            IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
            if (isTop)
                SetWindowPos(mainWindowHandle, new IntPtr(-1), 0, 0, 0, 0, 3);
            else
                SetWindowPos(mainWindowHandle, new IntPtr(-2), 0, 0, 0, 0, 3);
        }

        private static void PrintIntroduction()
        {
            Console.Title = "AutomaticLeewayCalculator";
            Console.Write("AutomaticLeewayCalculator by ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("aefrogdog");
            Console.ResetColor();
            Console.Write(" & ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Taoshi\n");
            Console.ResetColor();
            Console.WriteLine("\ny - Automatic Mode (Automatic leeway calculations as you look at maps in song select - osu! must be running)\nn - Manual Mode    (Same as the original leeway calculator)\n");
            Console.Write("Do you want to use automatic mode? (y/n): ");
        }
    }
}
