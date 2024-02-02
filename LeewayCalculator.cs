// Decompiled with JetBrains decompiler
// Type: LeewayCalculator.Program
// Assembly: LeewayCalculator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5FC93B0B-F9D0-470A-80E8-1FDEE2D43F01
// Assembly location: D:\Andet\Leeway_Calculator\LeewayCalculator.exe

using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace LeewayCalculator
{
    public static partial class Program
    {
        public class LeewayCalculator
        {
            private WebClient _webClient = new WebClient();

            public float CalcRotations(int length, float adjustTime)
            {
                float num1 = 0.0f;
                float num2 = (float)(8E-05 + (double)Math.Max(0.0f, 5000f - length) / 1000.0 / 2000.0) / adjustTime;
                float val1 = 0.0f;
                int num3 = (int)(length - Math.Floor(50.0 / 3.0 * (double)adjustTime));
                for (int index = 0; index < num3; ++index)
                {
                    val1 += num2;
                    num1 += (float)(Math.Min((double)val1, 0.05) / Math.PI);
                }
                return num1;
            }

            public double CalcLeeway(int length, float adjustTime, double od, int difficultyModifier)
            {
                int num = CalcRotReq(length, od, difficultyModifier);
                float d = CalcRotations(length, adjustTime);
                return num % 2 != 0 && Math.Floor((double)d) % 2.0 != 0.0 ? (double)d - Math.Floor((double)d) + 1.0 : (double)d - Math.Floor((double)d);
            }

            public int CalcRotReq(int length, double od, int difficultyModifier)
            {
                switch (difficultyModifier)
                {
                    case 2:
                        od /= 2.0;
                        break;
                    case 16:
                        od = Math.Min(10.0, od * 1.4);
                        break;
                }
                double num = od <= 5.0 ? 3.0 + 0.4 * od : 2.5 + 0.5 * od;
                return (int)(length / 1000.0 * num);
            }

            public string CalcAmount(int rotations, int rotReq)
            {
                double num = Math.Max(0, rotations - (rotReq + 3));
                if (rotReq % 2 != 0)
                    return Math.Floor(num / 2.0).ToString() + "k (F)";
                return num % 2.0 == 0.0 ? (num / 2.0).ToString() + "k (T)" : Math.Floor(num / 2.0).ToString() + "k+100 (T)";
            }

            public int CalcSpinBonus(int length, double od, float adjustTime, int difficultyModifier)
            {
                int num1 = (int)CalcRotations(length, adjustTime);
                int num2 = CalcRotReq(length, od, difficultyModifier);
                return (num2 % 2 != 0 ? (num2 + 3) / 2 * 100 : (int)Math.Floor(num1 / 2.0) * 100) + (int)Math.Floor((num1 - (num2 + 3)) / 2.0) * 1100;
            }

            public string GetBeatmap(int id) => _webClient.DownloadString("https://old.ppy.sh/osu/" + id);

            public List<string> GetBeatmapHitObjects(string beatmap)
            {
                string[] strArray = beatmap.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                List<string> beatmapHitObjects = new List<string>();
                for (int index1 = 0; index1 < strArray.Length; ++index1)
                {
                    if (strArray[index1].Contains("HitObjects"))
                    {
                        for (int index2 = index1 + 1; index2 < strArray.Length && strArray[index2].Length > 1; ++index2)
                            beatmapHitObjects.Add(strArray[index2]);
                        break;
                    }
                }
                return beatmapHitObjects;
            }

            public float GetHP(string beatmap) => float.Parse(Regex.Match(beatmap, "HPDrainRate:(.*?)\n").Groups[1].Value, CultureInfo.InvariantCulture);

            public float GetCS(string beatmap) => float.Parse(Regex.Match(beatmap, "CircleSize:(.*?)\n").Groups[1].Value, CultureInfo.InvariantCulture);

            public float GetOD(string beatmap) => float.Parse(Regex.Match(beatmap, "OverallDifficulty:(.*?)\n").Groups[1].Value, CultureInfo.InvariantCulture);

            public double GetSliderMult(string beatmap) => double.Parse(Regex.Match(beatmap, "SliderMultiplier:(.*?)\n").Groups[1].Value, CultureInfo.InvariantCulture);

            public double GetSliderTRate(string beatmap) => double.Parse(Regex.Match(beatmap, "SliderTickRate:(.*?)\n").Groups[1].Value, CultureInfo.InvariantCulture);

            public string GetTitle(string beatmap) => Regex.Match(beatmap, "Title:(.*?)\n").Groups[1].Value.Trim();

            public string GetArtist(string beatmap) => Regex.Match(beatmap, "Artist:(.*?)\n").Groups[1].Value.Trim();

            public string GetDifficultyName(string beatmap) => Regex.Match(beatmap, "Version:(.*?)\n").Groups[1].Value.Trim();

            public float GetAdjustTime(string[] mods)
            {
                foreach (string mod in mods)
                {
                    if (mod.Equals("DT") || mod.Equals("NC"))
                        return 1.5f;
                    if (mod.Equals("HT"))
                        return 0.75f;
                }
                return 1f;
            }

            public int GetDifficultyModifier(string[] mods)
            {
                foreach (string mod in mods)
                {
                    switch (mod)
                    {
                        case "HR":
                            return 16;
                        case "EZ":
                            return 2;
                        default:
                            continue;
                    }
                }
                return 0;
            }

            public int GetBeatmapVersion(string beatmap) => int.Parse(Regex.Match(beatmap, "osu file format v([0-9]+)").Groups[1].Value);

            public List<int[]> GetSpinners(string beatmap)
            {
                List<string> beatmapHitObjects = GetBeatmapHitObjects(beatmap);
                List<double[]> timingPoints = GetTimingPoints(beatmap);
                int beatmapVersion = GetBeatmapVersion(beatmap);
                double sliderMult = GetSliderMult(beatmap);
                double sliderTrate = GetSliderTRate(beatmap);
                List<int[]> spinners = new List<int[]>();
                int num = 0;
                foreach (string str in beatmapHitObjects)
                {
                    char[] chArray = new char[1] { ',' };
                    string[] strArray = str.Split(chArray);
                    switch (GetObjectType(int.Parse(strArray[3])))
                    {
                        case 0:
                            ++num;
                            break;
                        case 1:
                            double length = double.Parse(strArray[7], CultureInfo.InvariantCulture);
                            int slides = int.Parse(strArray[6]);
                            double[] beatLengthAt = GetBeatLengthAt(int.Parse(strArray[2]), timingPoints);
                            int tickCount = CalculateTickCount(length, slides, sliderMult, sliderTrate, beatLengthAt[0], beatLengthAt[1], beatmapVersion);
                            num += tickCount + slides + 1;
                            break;
                        case 3:
                            spinners.Add(new int[2]
                            {
                num,
                int.Parse(strArray[5]) - int.Parse(strArray[2])
                            });
                            ++num;
                            break;
                    }
                }
                return spinners;
            }

            public string GetModsString(string[] mods)
            {
                string modsString = "";
                foreach (string mod in mods)
                    modsString += mod;
                return modsString;
            }
            public string BestModCombination(string beatmap)
            {
                var allMods = new List<string[]>
                {
                    new string[4] { "HD", "DT", "HR", "FL" },
                    new string[3] { "HD", "DT", "FL" },
                    new string[3] { "HD", "FL", "EZ" },
                    new string[3] { "HD", "HT", "FL" },
                    new string[2] { "HD", "FL" },
                };
                string bestMods = "";
                int highestScore = 0;
                foreach (var mods in allMods)
                {
                    int score = CalculateMaxScore(beatmap, mods);
                    if (score > highestScore)
                    {
                        highestScore = score;
                        bestMods = GetModsString(mods);
                    }
                }
                return $"{string.Format("{0:n0}", highestScore)} ({ReorderMods(bestMods)})";
            }

            public int CalculateMaxScore(string beatmap, string[] mods)
            {
                double hp = (double)GetHP(beatmap);
                double cs = (double)GetCS(beatmap);
                double od = (double)GetOD(beatmap);
                int beatmapVersion = GetBeatmapVersion(beatmap);
                float adjustTime = GetAdjustTime(mods);
                int difficultyModifier = GetDifficultyModifier(mods);
                double sliderMult = GetSliderMult(beatmap);
                double sliderTrate = GetSliderTRate(beatmap);
                List<string> beatmapHitObjects = GetBeatmapHitObjects(beatmap);
                int startTime = int.Parse(beatmapHitObjects[0].Split(',')[2]);
                int endTime = int.Parse(beatmapHitObjects[beatmapHitObjects.Count - 1].Split(',')[2]);
                List<double[]> timingPoints = GetTimingPoints(beatmap);
                int num1 = 0;
                int num2 = 0;
                int num3 = 0;
                int num4 = CalculateDrainTime(beatmap, startTime, endTime) / 1000;
                double num5 = (int)Math.Round((hp + od + cs + (double)Clamp((float)(beatmapHitObjects.Count / (double)num4 * 8.0), 0.0f, 16f)) / 38.0 * 5.0) * CalculateModMultiplier(mods);
                int num6 = 0;
                foreach (string str in beatmapHitObjects)
                {
                    char[] chArray = new char[1] { ',' };
                    string[] strArray = str.Split(chArray);
                    int objectType = GetObjectType(int.Parse(strArray[3]));
                    if (objectType == 0)
                    {
                        ++num6;
                        num1 += 300 + (int)(Math.Max(0, num2 - 1) * (12.0 * num5));
                        ++num2;
                    }
                    if (objectType == 1)
                    {
                        double length = double.Parse(strArray[7], CultureInfo.InvariantCulture);
                        int slides = int.Parse(strArray[6]);
                        double[] beatLengthAt = GetBeatLengthAt(int.Parse(strArray[2]), timingPoints);
                        int tickCount = CalculateTickCount(length, slides, sliderMult, sliderTrate, beatLengthAt[0], beatLengthAt[1], beatmapVersion);
                        num3 += tickCount * 10 + (slides + 1) * 30;
                        num2 += tickCount + slides + 1;
                        num1 += 300 + (int)(Math.Max(0, num2 - 1) * (12.0 * num5));
                    }
                    else if (objectType == 3)
                    {
                        num1 += 300 + (int)(Math.Max(0, num2 - 1) * (12.0 * num5));
                        int length = int.Parse(strArray[5]) - int.Parse(strArray[2]);
                        num3 += CalcSpinBonus(length, od, adjustTime, difficultyModifier);
                        ++num2;
                    }
                }
                return num1 + num3;
            }

            public int CalculateTickCount(
              double length,
              int slides,
              double sliderMult,
              double sliderTRate,
              double beatLength,
              double sliderVMult,
              int beatmapVersion)
            {
                double num1 = Clamp(Math.Abs(sliderVMult), 10.0, 1000.0) * length * beatLength / (sliderMult * 10000.0);
                double num2 = beatLength / sliderTRate;
                if (beatmapVersion < 8)
                    num2 *= Clamp(Math.Abs(sliderVMult), 10.0, 1000.0) / 100.0;
                double num3 = num1 - num2;
                int num4 = 0;
                for (; num3 >= 10.0; num3 -= num2)
                    ++num4;
                return num4 + num4 * (slides - 1);
            }

            public int CalculateDrainTime(string beatmap, int startTime, int endTime)
            {
                string[] strArray1 = beatmap.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                List<int> intList = new List<int>();
                for (int index1 = 0; index1 < strArray1.Length; ++index1)
                {
                    if (strArray1[index1].Contains("Break Periods"))
                    {
                        for (int index2 = index1 + 1; index2 < strArray1.Length; ++index2)
                        {
                            string[] strArray2 = strArray1[index2].Split(',');
                            if (strArray2.Length == 3)
                                intList.Add(int.Parse(strArray2[2]) - int.Parse(strArray2[1]));
                            else
                                break;
                        }
                        break;
                    }
                }
                int drainTime = endTime - startTime;
                foreach (int num in intList)
                    drainTime -= num;
                return drainTime;
            }

            public List<double[]> GetTimingPoints(string beatmap)
            {
                string[] strArray1 = beatmap.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                List<double[]> timingPoints = new List<double[]>();
                for (int index1 = 0; index1 < strArray1.Length; ++index1)
                {
                    if (strArray1[index1].Contains("TimingPoints"))
                    {
                        for (int index2 = index1 + 1; index2 < strArray1.Length; ++index2)
                        {
                            string[] strArray2 = strArray1[index2].Split(',');
                            if (strArray2.Length > 1)
                            {
                                double num1 = double.Parse(strArray2[0], CultureInfo.InvariantCulture);
                                double num2 = double.Parse(strArray2[1], CultureInfo.InvariantCulture);
                                timingPoints.Add(new double[2] { num1, num2 });
                            }
                            else
                                break;
                        }
                        break;
                    }
                }
                foreach (double[] numArray in timingPoints)
                {
                    if (numArray[1] > 0.0)
                    {
                        timingPoints.Insert(0, new double[2]
                        {
              0.0,
              numArray[1]
                        });
                        break;
                    }
                }
                return timingPoints;
            }

            public int GetBeatmapID(string beatmap) => Regex.IsMatch(beatmap, "BeatmapID:([0-9]+)") ? int.Parse(Regex.Match(beatmap, "BeatmapID:([0-9]+)").Groups[1].Value) : -1;

            public double[] GetBeatLengthAt(int time, List<double[]> timingPoints)
            {
                double num1 = 0.0;
                double num2 = -100.0;
                for (int index = 0; index < timingPoints.Count; ++index)
                {
                    if (time >= timingPoints[index][0])
                    {
                        if (timingPoints[index][1] > 0.0)
                        {
                            num1 = timingPoints[index][1];
                            num2 = -100.0;
                        }
                        else
                            num2 = timingPoints[index][1];
                    }
                }
                return new double[2] { num1, num2 };
            }

            public double Clamp(double value, double min, double max)
            {
                if (value < min)
                    return min;
                return value > max ? max : value;
            }

            public float Clamp(float value, float min, float max)
            {
                if ((double)value < (double)min)
                    return min;
                return (double)value > (double)max ? max : value;
            }

            public int GetObjectType(int id)
            {
                string str1 = "00000000" + Convert.ToString(id, 2);
                string str2 = str1.Substring(str1.Length - 8, 8);
                if (str2[4].Equals('1'))
                    return 3;
                return str2[6].Equals('1') ? 1 : 0;
            }

            public string[] GetMods(string mods)
            {
                if (mods == null || mods.Length < 2 || mods.Length % 2 != 0)
                    return new string[4] { "HD", "NC", "HR", "FL" };
                string[] mods1 = new string[mods.Length / 2];
                for (int index = 0; index < mods1.Length; ++index)
                    mods1[index] = mods.Substring(index * 2, 2);
                return mods1;
            }

            public bool IsValidModCombo(string[] mods)
            {
                if (mods == null || mods.Length < 1)
                    return false;

                // Check for specific valid combinations
                if (mods.SequenceEqual(new[] { "HD", "DT", "HR", "FL" }) || mods.SequenceEqual(new[] { "HD", "NC", "HR", "FL" }))
                    return true;

                // Check individual mods and their combinations
                for (int i = 0; i < mods.Length; ++i)
                {
                    if (!IsValidMod(mods[i]) || (mods[i] == "NM" && mods.Length > 2))
                        return false;

                    for (int j = i + 1; j < mods.Length; ++j)
                    {
                        if (!IsValidCombination(mods[i], mods[j]))
                            return false;
                    }
                }

                return true;
            }

            private bool IsValidCombination(string mod1, string mod2)
            {
                switch (mod1)
                {
                    case "DT":
                        return !((mod2 == "NC") || (mod2 == "HT"));
                    case "NC":
                        return !((mod2 == "DT") || (mod2 == "HT"));
                    case "HT":
                        return !((mod2 == "DT") || (mod2 == "NC"));
                    case "HR":
                        return !(mod2 == "EZ");
                    case "EZ":
                        return !(mod2 == "HR");
                    default:
                        return true;
                }
            }

            private bool IsValidMod(string mod)
            {
                return mod == "HD" || mod == "DT" || mod == "HR" || mod == "FL" || mod == "NC" || mod == "HT" || mod == "EZ";
            }

            public string RemoveUselessMods(string mods)
            {
                if (mods.Equals("None") || mods.Equals("SD") || mods.Equals("PF") || mods.Equals("TD") || mods.Equals("SO"))
                {
                    return "NM";
                }

                if (mods.Length == 2)
                {
                    return mods;
                }

                string usefulMods = "";
                string[] modArray = mods.Split(',');

                foreach (string mod in modArray)
                {
                    if (mod.Equals("HD") || mod.Equals("DT") || mod.Equals("NC") || mod.Equals("HR") || mod.Equals("FL") || mod.Equals("EZ") || mod.Equals("HT"))
                    {
                        usefulMods += mod;
                    }
                }

                return string.IsNullOrEmpty(usefulMods) ? "NM" : usefulMods;
            }

            public string ReorderMods(string mods)
            {
                if (mods.Length <= 2)
                    return mods;

                string reorderedMods = "";

                // Check for HD
                for (int i = 0; i < mods.Length - 1; i += 2)
                {
                    if (mods.Substring(i, 2).Equals("HD"))
                    {
                        reorderedMods += mods.Substring(i, 2);
                        break;
                    }
                }

                // Check for DT, NC, HT
                for (int i = 0; i < mods.Length - 1; i += 2)
                {
                    if (mods.Substring(i, 2).Equals("DT") || mods.Substring(i, 2).Equals("NC") || mods.Substring(i, 2).Equals("HT"))
                    {
                        reorderedMods += mods.Substring(i, 2);
                        break;
                    }
                }

                // Check for HR, EZ
                for (int i = 0; i < mods.Length - 1; i += 2)
                {
                    if (mods.Substring(i, 2).Equals("HR") || mods.Substring(i, 2).Equals("EZ"))
                    {
                        reorderedMods += mods.Substring(i, 2);
                        break;
                    }
                }

                // Check for FL
                for (int i = 0; i < mods.Length - 1; i += 2)
                {
                    if (mods.Substring(i, 2).Equals("FL"))
                    {
                        reorderedMods += mods.Substring(i, 2);
                        break;
                    }
                }

                return reorderedMods;
            }

            public bool HasLeaderboard(string beatmapPage) => !string.IsNullOrEmpty(beatmapPage) && Regex.IsMatch(beatmapPage, "<td><b>(.*?)</b></td>");

            public double CalculateModMultiplier(string[] mods)
            {
                double modMultiplier = 1.0;

                foreach (string mod in mods)
                {
                    switch (mod)
                    {
                        case "NF":
                        case "EZ":
                            modMultiplier *= 0.5;
                            break;

                        case "HT":
                            modMultiplier *= 0.3;
                            break;

                        case "HD":
                            modMultiplier *= 1.06;
                            break;

                        case "HR":
                            modMultiplier *= 1.06;
                            break;

                        case "DT":
                        case "NC":
                        case "FL":
                            modMultiplier *= 1.12;
                            break;

                        case "SO":
                            modMultiplier *= 0.9;
                            break;

                        default:
                            modMultiplier *= 0.0;
                            break;
                    }
                }

                return modMultiplier;
            }

            public string[,] GetTop50(string beatmapPage)
            {
                MatchCollection matchCollection = Regex.Matches(beatmapPage, "<tr class='row[1-2]p'>.*?c=(\\d+)&.*?<td>(?:<b>)?((?:\\d{1,3},)?(?:(?:\\d{1,3},)+)?(?:\\d{1,3}))(?:</b>)?</td>.*?<td>(?:<b>)?(.*?)%(?:</b>)?</td>.*?<td>(None|(?:(?:[A-Z]{2},)+)?[A-Z]{2})</td>.*?</tr>", RegexOptions.Singleline);
                string[,] top50 = new string[matchCollection.Count, 4];
                int index = 0;
                foreach (Match match in matchCollection)
                {
                    top50[index, 0] = match.Groups[2].Value.Replace(",", null);
                    top50[index, 1] = match.Groups[3].Value;
                    top50[index, 2] = ReorderMods(RemoveUselessMods(match.Groups[4].Value));
                    top50[index, 3] = match.Groups[1].Value;
                    ++index;
                }
                return top50;
            }

            public int GetBestScoreIndex(string[,] scores, string[] mods)
            {
                for (int bestScoreIndex = 0; bestScoreIndex < scores.GetLength(0); ++bestScoreIndex)
                {
                    if (IsSameMods(mods, GetMods(scores[bestScoreIndex, 2])))
                        return bestScoreIndex;
                }
                return -1;
            }

            public bool IsSameMods(string[] mods1, string[] mods2)
            {
                if (mods1.Length != mods2.Length)
                    return false;
                for (int index = 0; index < mods1.Length; ++index)
                {
                    if (mods1[index].Equals("DT") || mods1[index].Equals("NC"))
                    {
                        if (!mods2.Contains("DT") && !mods2.Contains("NC"))
                            return false;
                    }
                    else if (!mods2.Contains(mods1[index]))
                        return false;
                }
                return true;
            }

            public string GetBeatmapPage(int mapID) => _webClient.DownloadString("https://old.ppy.sh/b/" + mapID);

            public string GetNumberOneOnBeatmapPage(string beatmapPage)
            {
                string foundUsername = "";
                string searchString = "is in the lead";
                string[] lines = beatmapPage.Split('\n');
                foreach (string line in lines)
                {
                    if (line.Contains(searchString))
                    {

                        char stopChar = '<';
                        int startIndex = line.IndexOf("'>") + 2;
                        while (line[startIndex] != stopChar)
                        {
                            foundUsername += line[startIndex];
                            startIndex++;
                        }
                        break;
                    }
                }
                return foundUsername;
            }

            public string[] CalculateBeatmapMaxScore(string beatmap)
            {
                string[] modCombinations = new string[]
                {
                    "HDNCHRFL", "HDNCFL", "HDHRFL", "HDFL", "EZHDNCFL",
                    "EZHDFL", "EZHDHTFL", "HDHTFL", "HDHTHRFL"
                };

                int maxScore = 0;
                string maxScoreMods = "";

                foreach (string mods in modCombinations)
                {
                    int currentScore = CalculateMaxScore(beatmap, GetMods(mods));

                    if (currentScore > maxScore)
                    {
                        maxScore = currentScore;
                        maxScoreMods = mods;
                    }
                }

                return new string[] { maxScore.ToString(), maxScoreMods };
            }
            private void PrintLeaderboardInfo(int num3, int maxScore, bool youAreNumberOne, string str, string numberOne, double num4)
            {
                if (num3 == maxScore)
                {
                    PrintCappedInfo(youAreNumberOne, str, numberOne);
                }
                else if (num3 < maxScore)
                {
                    PrintNotCappedInfo(youAreNumberOne, str, numberOne, maxScore - num3);
                }
                else
                {
                    PrintOverCapInfo(youAreNumberOne, num3, maxScore, str, numberOne, num3 - maxScore, num4);
                }
            }

            private void PrintCappedInfo(bool youAreNumberOne, string str, string numberOne)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(" Capped");
                Console.ForegroundColor = ConsoleColor.Gray;

                if (youAreNumberOne)
                {
                    Console.Write($": ( {str} ) #1: {numberOne} ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("(That's you!)\n");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    Console.WriteLine($": ({str}) #1: {numberOne}");
                }
            }

            private void PrintNotCappedInfo(bool youAreNumberOne, string str, string numberOne, int scoreDifference)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" Not Capped");
                Console.ForegroundColor = ConsoleColor.Gray;

                if (youAreNumberOne)
                {
                    Console.Write($": +{string.Format("{0:n0}", scoreDifference)} ({str}) #1: {numberOne} ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("(That's you!)\n");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    Console.WriteLine($": +{string.Format("{0:n0}", scoreDifference)} ({str}) #1: {numberOne}");
                }
            }

            private void PrintOverCapInfo(bool youAreNumberOne, int num3, int maxScore, string str, string numberOne, int scoreDifference, double num4)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(" Over Cap");
                Console.ForegroundColor = ConsoleColor.Gray;

                Console.Write($": +{string.Format("{0:n0}", scoreDifference)} ({str}) #1: {numberOne} ");

                if (youAreNumberOne)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("(That's you!)\n");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                if (num3 % 100 == maxScore % 100)
                {
                    PrintReworkInfo(num4);
                }

                Console.WriteLine();
            }

            private void PrintReworkInfo(double num4)
            {
                if (num4 < 2801770131.0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(" Pre-Spin Rework");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(" Post-Spin Rework");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }

            public void PrintTable(int beatmapID, string beatmap, string[] mods, bool leaderboardLookups)
            {
                string beatmapPage = null;
                if (beatmapID > 0 && leaderboardLookups)
                    beatmapPage = GetBeatmapPage(beatmapID);
                bool flag = false;
                int maxScore;
                if (mods == null)
                {
                    string[] beatmapMaxScore = CalculateBeatmapMaxScore(beatmap);
                    maxScore = int.Parse(beatmapMaxScore[0]);
                    mods = GetMods(beatmapMaxScore[1]);
                    if (!IsSameMods(mods, GetMods("HDNCHRFL")))
                        flag = true;
                }
                else
                    maxScore = CalculateMaxScore(beatmap, mods);
                GetModsString(mods);
                List<int[]> spinners = GetSpinners(beatmap);
                float adjustTime = GetAdjustTime(mods);
                float od = GetOD(beatmap);
                int difficultyModifier = GetDifficultyModifier(mods);
                ConsoleTable consoleTable = new ConsoleTable(new string[6]
                {
                    "#",
                    "Length",
                    "Combo",
                    "Amount (+100)",
                    "Rotations",
                    "Leeway"
                });
                for (int index = 0; index < spinners.Count; ++index)
                {
                    int length = spinners[index][1];
                    int num1 = spinners[index][0];
                    float rotations = CalcRotations(length, adjustTime);
                    int rotReq = CalcRotReq(length, (double)od, difficultyModifier);
                    string str = CalcAmount((int)rotations, rotReq);
                    double num2 = CalcLeeway(length, adjustTime, (double)od, difficultyModifier);
                    consoleTable.AddRow(index + 1, length, num1, str, string.Format("{0:0.00000}", rotations), string.Format("{0:0.00000}", num2));
                }
                Console.WriteLine($" {GetArtist(beatmap)} - {GetTitle(beatmap)} ({GetDifficultyName(beatmap)})");
                Console.Write($" Max Score: {string.Format("{0:n0}", maxScore)} (");
                if (flag)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(GetModsString(mods));
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                    Console.Write(GetModsString(mods));
                Console.WriteLine(")");
                Console.WriteLine($" Best Mods: {BestModCombination(beatmap)}");
                if (HasLeaderboard(beatmapPage) && leaderboardLookups)
                {
                    string numberOne = GetNumberOneOnBeatmapPage(beatmapPage);
                    bool youAreNumberOne = numberOne == _userName;
                    int firstPlaceScore = -1;
                    string modsFromTop50 = null;
                    double beatmapUnixTimestamp = 2801770131.0;

                    if (flag)
                    {
                        string[,] top50 = GetTop50(beatmapPage);
                        firstPlaceScore = int.Parse(top50[0, 0]);
                        modsFromTop50 = top50[0, 2];
                        beatmapUnixTimestamp = double.Parse(top50[0, 3], CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        string[,] top50 = GetTop50(beatmapPage);
                        int bestScoreIndex = GetBestScoreIndex(top50, mods);

                        if (bestScoreIndex >= 0)
                        {
                            firstPlaceScore = int.Parse(top50[bestScoreIndex, 0]);
                            modsFromTop50 = top50[bestScoreIndex, 2];
                            beatmapUnixTimestamp = double.Parse(top50[bestScoreIndex, 3], CultureInfo.InvariantCulture);
                        }
                    }

                    if (firstPlaceScore >= 0)
                    {
                        PrintLeaderboardInfo(firstPlaceScore, maxScore, youAreNumberOne, modsFromTop50, numberOne, beatmapUnixTimestamp);
                    }
                }

                consoleTable.Options.EnableCount = false;
                consoleTable.Write();

            }
        }
    }
}
