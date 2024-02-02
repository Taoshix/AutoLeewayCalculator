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
using static LeewayCalculator.Program;
using System.Runtime.InteropServices;

namespace LeewayCalculator
{
    public static partial class Program
    {
        private static StructuredOsuMemoryReader reader;
        private static bool autoCalc = false;
        private static string songsFolder = "";
        private static string username = "";
        private static SettingsManager SettingsManager = new SettingsManager();
        private static List<Setting> Settings = new List<Setting>();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        private static void Main(string[] args)
        {
            Program.LeewayCalculator leewayCalculator = new Program.LeewayCalculator();
            SettingsManager.AddSetting(1, "!", "Leaderboard Lookups", "Toggle whether or not to show the leaderboard lookups", true);
            SettingsManager.AddSetting(2, "?", "Use Ingame Mods", "Toggle whether or not to use the mods you have ingame", true);
            SettingsManager.AddSetting(3, "*", "Always on Top", "Toggle whether or not the window should be always on top", false);
            Settings = SettingsManager.GetSettings();
            reader = StructuredOsuMemoryReader.Instance.GetInstanceForWindowTitleHint("");
            
            PrintIntroduction();

            autoCalc = Console.ReadLine().ToLower() == "y";
            if (autoCalc)
            {
            start:
                bool changeMods = false;

                Console.WriteLine("\nAutomatic Mode");

                OsuBaseAddresses osuBaseAddresses = new OsuBaseAddresses();
                GetOsuProcess();
                PrintSettings(Settings, true);

                Console.Write("Input mods, setting flags, or leave blank for 4 mod and then press ENTER: ");
                string modInput = Console.ReadLine();
                modInput = ParseSettings(modInput);
                if (modInput == "")
                {
                    modInput = "HDHRDTFL";
                }
                string savedMods = modInput;
                CurrentBeatmap map = new CurrentBeatmap();

                while (true)
                {
                    do
                    {
                        int currentMapID = 0;
                        string currentReadModsBinary = "";
                        while (!Console.KeyAvailable)
                        {
                            reader.TryReadProperty(map, "Id", out object ID);
                            reader.TryReadProperty(osuBaseAddresses.GeneralData, nameof(GeneralData.Mods), out var emilly);
                            if (!reader.CanRead || ID == null)
                            {
                                Console.WriteLine("osu! closed down");
                                goto start;
                            }
                            int mapId = (int)ID;
                            string readModsBinary = Convert.ToString((int)emilly, 2).PadLeft(32, '0');

                            if (currentMapID != mapId || (currentReadModsBinary != readModsBinary && SettingsManager.GetSetting(2).State))
                            {
                                currentMapID = mapId;
                                currentReadModsBinary = readModsBinary;
                                reader.TryReadProperty(map, "FolderName", out object mapFolder);
                                reader.TryReadProperty(map, "OsuFileName", out object mapFileName);

                                if (mapFolder != null && mapFileName != null)
                                {
                                    string absoluteFilename = Path.Combine(songsFolder, ((string)mapFolder).TrimEnd(), ((string)mapFileName).TrimEnd());
                                    string beatmap = File.ReadAllText(absoluteFilename);
                                    string[] mods;
                                    mods = SettingsManager.GetSetting(2).State ? leewayCalculator.GetMods(leewayCalculator.ReorderMods(ModsFromBinaryString(readModsBinary))) : leewayCalculator.GetMods(leewayCalculator.ReorderMods(modInput));
                                    if (!leewayCalculator.IsValidModCombo(mods))
                                        mods = (string[])null;
                                    Console.Clear();
                                    Console.WriteLine("Automatic Mode - Press ESC to change mods/settings\n");
                                    PrintSettings(Settings, false);
                                    leewayCalculator.PrintTable(mapId, beatmap, mods, SettingsManager.GetSetting(1).State);
                                }
                            }
                            Console.Title = $"AutomaticLeewayCalculator - Automatic Mode - {mapId} +{(SettingsManager.GetSetting(2).State ? leewayCalculator.ReorderMods(ModsFromBinaryString(readModsBinary)) : leewayCalculator.ReorderMods(modInput))}";
                            Thread.Sleep(50);
                        }
                    } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
                    PrintSettings(Settings, true);
                    Console.Write("Input new mods, setting flags, or leave blank for 4 mod and then press ENTER to continue: ");
                    modInput = Console.ReadLine();
                    modInput = ParseSettings(modInput);
                    if (modInput == "" && !changeMods)
                    {
                        modInput = "HDHRDTFL";
                    }
                    else if (modInput == "" && changeMods)
                    {
                        modInput = savedMods;
                    }
                    changeMods = false;
                    savedMods = modInput;
                }
            }
            else
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
                        string[] mods = (string[])null;
                        string beatmap;
                        int beatmapId;
                        if (Regex.IsMatch(input, "^\"(.*?)\".*?([A-Za-z]+)?$"))
                        {
                            Match match = Regex.Match(input, "^\"(.*?)\".*?([A-Za-z]+)?$");
                            beatmap = System.IO.File.ReadAllText(match.Groups[1].Value);
                            beatmapId = leewayCalculator.GetBeatmapID(beatmap);
                            if (!string.IsNullOrEmpty(match.Groups[2].Value))
                                mods = leewayCalculator.GetMods(leewayCalculator.ReorderMods(match.Groups[2].Value.ToUpper()));
                        }
                        else if (input.Split(' ')[0] == "current")
                        {
                            string newMods = "";
                            if (input.Split(' ').Length == 1)
                            {
                                newMods = "HDHRDTFL";
                            }
                            else
                            {
                                newMods = input.Split(' ')[1].ToUpper().Replace("+", "");
                            }
                            mods = leewayCalculator.GetMods(leewayCalculator.ReorderMods(newMods));
                            GetOsuProcess();
                            CurrentBeatmap map = new CurrentBeatmap();
                            reader.TryReadProperty(map, "Id", out object ID);
                            int mapId = (int)ID;
                            reader.TryReadProperty(map, "FolderName", out object mapFolder);
                            reader.TryReadProperty(map, "OsuFileName", out object mapFileName);
                            string absoluteFilename = Path.Combine(songsFolder, ((string)mapFolder).TrimEnd(), ((string)mapFileName).TrimEnd());
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
                                throw new Exception($"Unable to download beatmap with id {beatmapId}, does it exist?");
                            }
                            mods = (string[])null;
                            if (!string.IsNullOrEmpty(match.Groups[2].Value))
                                mods = leewayCalculator.GetMods(leewayCalculator.ReorderMods(match.Groups[2].Value.ToUpper()));
                        }
                        if (!leewayCalculator.IsValidModCombo(mods))
                            mods = (string[])null;
                        Console.WriteLine("Manual Mode");
                        Console.Title = $"AutomaticLeewayCalculator - Manual Mode - {beatmapId} +{leewayCalculator.ReorderMods(leewayCalculator.GetModsString(mods))}";
                        leewayCalculator.PrintTable(beatmapId, beatmap, mods, true);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: {ex}");
                        Console.ResetColor();
                    }
                    Console.WriteLine("If osu! is running, you can type \"current\" instead of the ID");
                    Console.Write("Enter Beatmap ID (+Mods): ");
                    input = Console.ReadLine().Trim();
                }
            }
        }

        private static string ParseSettings(string modInput)
        {
            modInput = modInput.Replace("+", "");
            foreach (var setting in Settings)
            {
                if (modInput.Contains(setting.Prefix))
                {
                    SettingsManager.ToggleSetting(setting.Id);
                    modInput = modInput.Replace(setting.Prefix.ToString(), "");
                }
            }
            modInput = modInput.TrimStart(' ');
            return modInput;
        }

        private static string ModsFromBinaryString(string binaryString)
        {
            /*
            Hardrock 27
            DoubleTime 25
            Hidden 28
            Flashlight 21
            Easy 30
            HalfTime 23
            */
            string str = "";
            for (int index = 0; index < binaryString.Length; ++index)
            {
                if (binaryString[index] == '1')
                {
                    switch (index)
                    {
                        case 21:
                            str += "FL";
                            break;
                        case 23:
                            str += "HT";
                            break;
                        case 25:
                            str += "DT";
                            break;
                        case 28:
                            str += "HD";
                            break;
                        case 27:
                            str += "HR";
                            break;
                        case 30:
                            str += "EZ";
                            break;
                    }
                }
            }
            return str == "" ? "NM" : str;
        }
        private static void GetOsuProcess()
        {
            Process[] processes = Process.GetProcessesByName("osu!");
            if (processes.Length == 0 || !reader.CanRead || processes[0].HasExited)
            {
                Console.WriteLine("\nosu! is not running, please start osu!");
                Console.WriteLine("Waiting...");
                while (processes.Length == 0 || !reader.CanRead || processes[0].HasExited)
                {
                    processes = Process.GetProcessesByName("osu!");
                    Thread.Sleep(50);
                }
            }
            BanchoUser user = new BanchoUser();
            user.IsLoggedIn = true;
            OsuBaseAddresses osuBaseAddresses = new OsuBaseAddresses();
            reader.TryRead(osuBaseAddresses.BanchoUser);
            username = osuBaseAddresses.BanchoUser.Username;
            Console.Clear();
            string osuExePath = processes[0].MainModule.FileName;
            songsFolder = Path.Combine(Path.GetDirectoryName(osuExePath), "Songs");
        }

        private static void PrintSettings(List<Setting> settings, bool showInstructions)
        {
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
