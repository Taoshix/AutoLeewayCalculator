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
            private const float DT = 1.5f;
            private const float HT = 0.75f;
            private const int HR = 16;
            private const int EZ = 2;
            private const int CIRCLE = 0;
            private const int SLIDER = 1;
            private const int SPINNER = 3;
            private WebClient wc = new WebClient();

            public float CalcRotations(int length, float adjustTime)
            {
                float num1 = 0.0f;
                float num2 = (float)(8E-05 + (double)System.Math.Max(0.0f, 5000f - (float)length) / 1000.0 / 2000.0) / adjustTime;
                float val1 = 0.0f;
                int num3 = (int)((double)length - System.Math.Floor(50.0 / 3.0 * (double)adjustTime));
                for (int index = 0; index < num3; ++index)
                {
                    val1 += num2;
                    num1 += (float)(System.Math.Min((double)val1, 0.05) / System.Math.PI);
                }
                return num1;
            }

            public double CalcLeeway(int length, float adjustTime, double od, int difficultyModifier)
            {
                int num = this.CalcRotReq(length, od, difficultyModifier);
                float d = this.CalcRotations(length, adjustTime);
                return num % 2 != 0 && System.Math.Floor((double)d) % 2.0 != 0.0 ? (double)d - System.Math.Floor((double)d) + 1.0 : (double)d - System.Math.Floor((double)d);
            }

            public int CalcRotReq(int length, double od, int difficultyModifier)
            {
                switch (difficultyModifier)
                {
                    case 2:
                        od /= 2.0;
                        break;
                    case 16:
                        od = System.Math.Min(10.0, od * 1.4);
                        break;
                }
                double num = od <= 5.0 ? 3.0 + 0.4 * od : 2.5 + 0.5 * od;
                return (int)((double)length / 1000.0 * num);
            }

            public string CalcAmount(int rotations, int rotReq)
            {
                double num = (double)System.Math.Max(0, rotations - (rotReq + 3));
                if (rotReq % 2 != 0)
                    return System.Math.Floor(num / 2.0).ToString() + "k (F)";
                return num % 2.0 == 0.0 ? (num / 2.0).ToString() + "k (T)" : System.Math.Floor(num / 2.0).ToString() + "k+100 (T)";
            }

            public int CalcSpinBonus(int length, double od, float adjustTime, int difficultyModifier)
            {
                int num1 = (int)this.CalcRotations(length, adjustTime);
                int num2 = this.CalcRotReq(length, od, difficultyModifier);
                return (num2 % 2 != 0 ? (num2 + 3) / 2 * 100 : (int)System.Math.Floor((double)num1 / 2.0) * 100) + (int)System.Math.Floor((double)(num1 - (num2 + 3)) / 2.0) * 1100;
            }

            public string GetBeatmap(int id) => this.wc.DownloadString("https://old.ppy.sh/osu/" + (object)id);

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

            public float GetHP(string beatmap) => float.Parse(Regex.Match(beatmap, "HPDrainRate:(.*?)\n").Groups[1].Value, (IFormatProvider)CultureInfo.InvariantCulture);

            public float GetCS(string beatmap) => float.Parse(Regex.Match(beatmap, "CircleSize:(.*?)\n").Groups[1].Value, (IFormatProvider)CultureInfo.InvariantCulture);

            public float GetOD(string beatmap) => float.Parse(Regex.Match(beatmap, "OverallDifficulty:(.*?)\n").Groups[1].Value, (IFormatProvider)CultureInfo.InvariantCulture);

            public double GetSliderMult(string beatmap) => double.Parse(Regex.Match(beatmap, "SliderMultiplier:(.*?)\n").Groups[1].Value, (IFormatProvider)CultureInfo.InvariantCulture);

            public double GetSliderTRate(string beatmap) => double.Parse(Regex.Match(beatmap, "SliderTickRate:(.*?)\n").Groups[1].Value, (IFormatProvider)CultureInfo.InvariantCulture);

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
                List<string> beatmapHitObjects = this.GetBeatmapHitObjects(beatmap);
                List<double[]> timingPoints = this.GetTimingPoints(beatmap);
                int beatmapVersion = this.GetBeatmapVersion(beatmap);
                double sliderMult = this.GetSliderMult(beatmap);
                double sliderTrate = this.GetSliderTRate(beatmap);
                List<int[]> spinners = new List<int[]>();
                int num = 0;
                foreach (string str in beatmapHitObjects)
                {
                    char[] chArray = new char[1] { ',' };
                    string[] strArray = str.Split(chArray);
                    switch (this.GetObjectType(int.Parse(strArray[3])))
                    {
                        case 0:
                            ++num;
                            break;
                        case 1:
                            double length = double.Parse(strArray[7], (IFormatProvider)CultureInfo.InvariantCulture);
                            int slides = int.Parse(strArray[6]);
                            double[] beatLengthAt = this.GetBeatLengthAt(int.Parse(strArray[2]), timingPoints);
                            int tickCount = this.CalculateTickCount(length, slides, sliderMult, sliderTrate, beatLengthAt[0], beatLengthAt[1], beatmapVersion);
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
                return $"{string.Format("{0:n0}", (object)highestScore)} ({ReorderMods(bestMods)})";
            }

            public int CalculateMaxScore(string beatmap, string[] mods)
            {
                double hp = (double)this.GetHP(beatmap);
                double cs = (double)this.GetCS(beatmap);
                double od = (double)this.GetOD(beatmap);
                int beatmapVersion = this.GetBeatmapVersion(beatmap);
                float adjustTime = this.GetAdjustTime(mods);
                int difficultyModifier = this.GetDifficultyModifier(mods);
                double sliderMult = this.GetSliderMult(beatmap);
                double sliderTrate = this.GetSliderTRate(beatmap);
                List<string> beatmapHitObjects = this.GetBeatmapHitObjects(beatmap);
                int startTime = int.Parse(beatmapHitObjects[0].Split(',')[2]);
                int endTime = int.Parse(beatmapHitObjects[beatmapHitObjects.Count - 1].Split(',')[2]);
                List<double[]> timingPoints = this.GetTimingPoints(beatmap);
                int num1 = 0;
                int num2 = 0;
                int num3 = 0;
                int num4 = this.CalculateDrainTime(beatmap, startTime, endTime) / 1000;
                double num5 = (double)(int)System.Math.Round((hp + od + cs + (double)this.Clamp((float)((double)beatmapHitObjects.Count / (double)num4 * 8.0), 0.0f, 16f)) / 38.0 * 5.0) * this.CalculateModMultiplier(mods);
                int num6 = 0;
                foreach (string str in beatmapHitObjects)
                {
                    char[] chArray = new char[1] { ',' };
                    string[] strArray = str.Split(chArray);
                    int objectType = this.GetObjectType(int.Parse(strArray[3]));
                    if (objectType == 0)
                    {
                        ++num6;
                        num1 += 300 + (int)((double)System.Math.Max(0, num2 - 1) * (12.0 * num5));
                        ++num2;
                    }
                    if (objectType == 1)
                    {
                        double length = double.Parse(strArray[7], (IFormatProvider)CultureInfo.InvariantCulture);
                        int slides = int.Parse(strArray[6]);
                        double[] beatLengthAt = this.GetBeatLengthAt(int.Parse(strArray[2]), timingPoints);
                        int tickCount = this.CalculateTickCount(length, slides, sliderMult, sliderTrate, beatLengthAt[0], beatLengthAt[1], beatmapVersion);
                        num3 += tickCount * 10 + (slides + 1) * 30;
                        num2 += tickCount + slides + 1;
                        num1 += 300 + (int)((double)System.Math.Max(0, num2 - 1) * (12.0 * num5));
                    }
                    else if (objectType == 3)
                    {
                        num1 += 300 + (int)((double)System.Math.Max(0, num2 - 1) * (12.0 * num5));
                        int length = int.Parse(strArray[5]) - int.Parse(strArray[2]);
                        num3 += this.CalcSpinBonus(length, od, adjustTime, difficultyModifier);
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
                double num1 = this.Clamp(System.Math.Abs(sliderVMult), 10.0, 1000.0) * length * beatLength / (sliderMult * 10000.0);
                double num2 = beatLength / sliderTRate;
                if (beatmapVersion < 8)
                    num2 *= this.Clamp(System.Math.Abs(sliderVMult), 10.0, 1000.0) / 100.0;
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
                                double num1 = double.Parse(strArray2[0], (IFormatProvider)CultureInfo.InvariantCulture);
                                double num2 = double.Parse(strArray2[1], (IFormatProvider)CultureInfo.InvariantCulture);
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
                    if ((double)time >= timingPoints[index][0])
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
                if (mods == null)
                    return false;
                int num;
                if (!mods.Equals((object)new string[4]
                {
          "HD",
          "DT",
          "HR",
          "FL"
                }))
                    num = mods.Equals((object)new string[4]
                    {
            "HD",
            "NC",
            "HR",
            "FL"
                    }) ? 1 : 0;
                else
                    num = 1;
                if (num != 0)
                    return true;
                for (int index1 = 0; index1 < mods.Length; ++index1)
                {
                    if (!this.IsValidMod(mods[index1]) || mods[index1].Equals("NM") && mods.Length > 2)
                        return false;
                    if (index1 + 1 < mods.Length)
                    {
                        if (mods[index1].Equals("DT"))
                        {
                            for (int index2 = index1 + 1; index2 < mods.Length; ++index2)
                            {
                                if (mods[index2].Equals("NC") || mods[index2].Equals("HT"))
                                    return false;
                            }
                        }
                        else if (mods[index1].Equals("NC"))
                        {
                            for (int index3 = index1 + 1; index3 < mods.Length; ++index3)
                            {
                                if (mods[index3].Equals("DT") || mods[index3].Equals("HT"))
                                    return false;
                            }
                        }
                        else if (mods[index1].Equals("HT"))
                        {
                            for (int index4 = index1 + 1; index4 < mods.Length; ++index4)
                            {
                                if (mods[index4].Equals("DT") || mods[index4].Equals("NC"))
                                    return false;
                            }
                        }
                        else if (mods[index1].Equals("HR"))
                        {
                            for (int index5 = index1 + 1; index5 < mods.Length; ++index5)
                            {
                                if (mods[index5].Equals("EZ"))
                                    return false;
                            }
                        }
                        else if (mods[index1].Equals("EZ"))
                        {
                            for (int index6 = index1 + 1; index6 < mods.Length; ++index6)
                            {
                                if (mods[index6].Equals("HR"))
                                    return false;
                            }
                        }
                    }
                }
                return true;
            }

            public bool IsValidMod(string mod) => mod.Equals("EZ") || mod.Equals("HT") || mod.Equals("HD") || mod.Equals("HR") || mod.Equals("DT") || mod.Equals("NC") || mod.Equals("FL") || mod.Equals("NM");

            public string RemoveUselessMods(string mods)
            {
                if (mods.Equals("None") || mods.Equals("SD") || mods.Equals("PF") || mods.Equals("TD") || mods.Equals("SO"))
                    return "NM";
                if (mods.Length == 2)
                    return mods;
                string str1 = "";
                string str2 = mods;
                char[] chArray = new char[1] { ',' };
                foreach (string str3 in str2.Split(chArray))
                {
                    if (str3.Equals("HD") || str3.Equals("DT") || str3.Equals("NC") || str3.Equals("HR") || str3.Equals("FL") || str3.Equals("EZ") || str3.Equals("HT"))
                        str1 += str3;
                }
                return string.IsNullOrEmpty(str1) ? "NM" : str1;
            }

            public string ReorderMods(string mods)
            {
                if (mods.Length <= 2)
                    return mods;
                string str = "";
                for (int startIndex = 0; startIndex < mods.Length - 1; startIndex += 2)
                {
                    if (mods.Substring(startIndex, 2).Equals("HD"))
                    {
                        str += mods.Substring(startIndex, 2);
                        break;
                    }
                }
                for (int startIndex = 0; startIndex < mods.Length - 1; startIndex += 2)
                {
                    if (mods.Substring(startIndex, 2).Equals("DT") || mods.Substring(startIndex, 2).Equals("NC") || mods.Substring(startIndex, 2).Equals("HT"))
                    {
                        str += mods.Substring(startIndex, 2);
                        break;
                    }
                }
                for (int startIndex = 0; startIndex < mods.Length - 1; startIndex += 2)
                {
                    if (mods.Substring(startIndex, 2).Equals("HR") || mods.Substring(startIndex, 2).Equals("EZ"))
                    {
                        str += mods.Substring(startIndex, 2);
                        break;
                    }
                }
                for (int startIndex = 0; startIndex < mods.Length - 1; startIndex += 2)
                {
                    if (mods.Substring(startIndex, 2).Equals("FL"))
                    {
                        str += mods.Substring(startIndex, 2);
                        break;
                    }
                }
                return str;
            }

            public bool HasLeaderboard(string beatmapPage) => !string.IsNullOrEmpty(beatmapPage) && Regex.IsMatch(beatmapPage, "<td><b>(.*?)</b></td>");

            public double CalculateModMultiplier(string[] mods)
            {
                double modMultiplier = 1.0;
                foreach (string mod in mods)
                {
                    if (mod.Equals("NF") || mod.Equals("EZ"))
                    {
                        modMultiplier *= 0.5;
                    }
                    else
                    {
                        int num;
                        switch (mod)
                        {
                            case "HT":
                                modMultiplier *= 0.3;
                                continue;
                            case "HD":
                                num = 1;
                                break;
                            default:
                                num = mod.Equals("HR") ? 1 : 0;
                                break;
                        }
                        if (num != 0)
                            modMultiplier *= 1.06;
                        else if (mod.Equals("DT") || mod.Equals("NC") || mod.Equals("FL"))
                            modMultiplier *= 1.12;
                        else if (mod.Equals("SO"))
                            modMultiplier *= 0.9;
                        else
                            modMultiplier *= 0.0;
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
                    top50[index, 0] = match.Groups[2].Value.Replace(",", (string)null);
                    top50[index, 1] = match.Groups[3].Value;
                    top50[index, 2] = this.ReorderMods(this.RemoveUselessMods(match.Groups[4].Value));
                    top50[index, 3] = match.Groups[1].Value;
                    ++index;
                }
                return top50;
            }

            public int GetBestScoreIndex(string[,] scores, string[] mods)
            {
                for (int bestScoreIndex = 0; bestScoreIndex < scores.GetLength(0); ++bestScoreIndex)
                {
                    if (this.IsSameMods(mods, this.GetMods(scores[bestScoreIndex, 2])))
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
                        if (!((IEnumerable<string>)mods2).Contains<string>("DT") && !((IEnumerable<string>)mods2).Contains<string>("NC"))
                            return false;
                    }
                    else if (!((IEnumerable<string>)mods2).Contains<string>(mods1[index]))
                        return false;
                }
                return true;
            }

            public string GetBeatmapPage(int mapID) => this.wc.DownloadString("https://old.ppy.sh/b/" + (object)mapID);

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
                int maxScore = this.CalculateMaxScore(beatmap, this.GetMods("HDNCHRFL"));
                string str = "HDNCHRFL";
                if (this.CalculateMaxScore(beatmap, this.GetMods("HDNCFL")) > maxScore)
                {
                    maxScore = this.CalculateMaxScore(beatmap, this.GetMods("HDNCFL"));
                    str = "HDNCFL";
                }
                if (this.CalculateMaxScore(beatmap, this.GetMods("HDHRFL")) > maxScore)
                {
                    maxScore = this.CalculateMaxScore(beatmap, this.GetMods("HDHRFL"));
                    str = "HDHRFL";
                }
                if (this.CalculateMaxScore(beatmap, this.GetMods("HDFL")) > maxScore)
                {
                    maxScore = this.CalculateMaxScore(beatmap, this.GetMods("HDFL"));
                    str = "HDFL";
                }
                if (this.CalculateMaxScore(beatmap, this.GetMods("EZHDNCFL")) > maxScore)
                {
                    maxScore = this.CalculateMaxScore(beatmap, this.GetMods("EZHDNCFL"));
                    str = "EZHDNCFL";
                }
                if (this.CalculateMaxScore(beatmap, this.GetMods("EZHDFL")) > maxScore)
                {
                    maxScore = this.CalculateMaxScore(beatmap, this.GetMods("EZHDFL"));
                    str = "EZHDFL";
                }
                if (this.CalculateMaxScore(beatmap, this.GetMods("EZHDHTFL")) > maxScore)
                {
                    maxScore = this.CalculateMaxScore(beatmap, this.GetMods("EZHDHTFL"));
                    str = "EZHDHTFL";
                }
                if (this.CalculateMaxScore(beatmap, this.GetMods("HDHTFL")) > maxScore)
                {
                    maxScore = this.CalculateMaxScore(beatmap, this.GetMods("HDHTFL"));
                    str = "HDHTFL";
                }
                if (this.CalculateMaxScore(beatmap, this.GetMods("HDHTHRFL")) > maxScore)
                {
                    maxScore = this.CalculateMaxScore(beatmap, this.GetMods("HDHTHRFL"));
                    str = "HDHTHRFL";
                }
                return new string[2] { maxScore.ToString(), str };
            }

            public void PrintTable(int beatmapID, string beatmap, string[] mods, bool leaderboardLookups)
            {
                string beatmapPage = (string)null;
                if (beatmapID > 0 && leaderboardLookups)
                    beatmapPage = this.GetBeatmapPage(beatmapID);
                bool flag = false;
                int maxScore;
                if (mods == null)
                {
                    string[] beatmapMaxScore = this.CalculateBeatmapMaxScore(beatmap);
                    maxScore = int.Parse(beatmapMaxScore[0]);
                    mods = this.GetMods(beatmapMaxScore[1]);
                    if (!this.IsSameMods(mods, this.GetMods("HDNCHRFL")))
                        flag = true;
                }
                else
                    maxScore = this.CalculateMaxScore(beatmap, mods);
                this.GetModsString(mods);
                List<int[]> spinners = this.GetSpinners(beatmap);
                float adjustTime = this.GetAdjustTime(mods);
                float od = this.GetOD(beatmap);
                int difficultyModifier = this.GetDifficultyModifier(mods);
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
                    float rotations = this.CalcRotations(length, adjustTime);
                    int rotReq = this.CalcRotReq(length, (double)od, difficultyModifier);
                    string str = this.CalcAmount((int)rotations, rotReq);
                    double num2 = this.CalcLeeway(length, adjustTime, (double)od, difficultyModifier);
                    consoleTable.AddRow((object)(index + 1), (object)length, (object)num1, (object)str, (object)string.Format("{0:0.00000}", (object)rotations), (object)string.Format("{0:0.00000}", (object)num2));
                }
                Console.WriteLine(" " + this.GetArtist(beatmap) + " - " + this.GetTitle(beatmap) + " (" + this.GetDifficultyName(beatmap) + ")");
                Console.Write(" Max Score: " + string.Format("{0:n0}", (object)maxScore) + " (");
                if (flag)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(this.GetModsString(mods));
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                    Console.Write(this.GetModsString(mods));
                Console.WriteLine(")");
                Console.WriteLine($" Best Mods: {BestModCombination(beatmap)}");
                if (this.HasLeaderboard(beatmapPage) && leaderboardLookups)
                {
                    string numberOne = GetNumberOneOnBeatmapPage(beatmapPage);
                    bool youAreNumberOne = numberOne == username;
                    int num3 = -1;
                    string str = (string)null;
                    double num4 = 2801770131.0;
                    if (flag)
                    {
                        string[,] top50 = this.GetTop50(beatmapPage);
                        num3 = int.Parse(top50[0, 0]);
                        str = top50[0, 2];
                        num4 = double.Parse(top50[0, 3], (IFormatProvider)CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        string[,] top50 = this.GetTop50(beatmapPage);
                        int bestScoreIndex = this.GetBestScoreIndex(top50, mods);
                        if (bestScoreIndex >= 0)
                        {
                            num3 = int.Parse(top50[bestScoreIndex, 0]);
                            str = top50[bestScoreIndex, 2];
                            num4 = double.Parse(top50[bestScoreIndex, 3], (IFormatProvider)CultureInfo.InvariantCulture);
                        }
                    }
                    if (num3 >= 0)
                    {
                        if (num3 == maxScore)
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
                                Console.WriteLine(": (" + str + ") #1: " + numberOne);
                            }
                        }
                        else if (num3 < maxScore)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(" Not Capped");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            if (youAreNumberOne)
                            {
                                Console.Write($": + {string.Format("{0:n0}", (object)(maxScore - num3))} ({str}) #1: {numberOne} ");
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("(That's you!)\n");
                                Console.ForegroundColor = ConsoleColor.Gray;
                            }
                            else
                            {
                                Console.WriteLine(": +" + string.Format("{0:n0}", (object)(maxScore - num3)) + " (" + str + ") #1: " + numberOne);
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write(" Over Cap");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write(": +" + string.Format("{0:n0}", (object)(num3 - maxScore)) + " (" + str + ") #1: " + numberOne + " ");
                            if (youAreNumberOne)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("(That's you!)\n");
                                Console.ForegroundColor = ConsoleColor.Gray;
                            }
                            if (num3 % 100 == maxScore % 100)
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
                            Console.WriteLine();
                        }
                    }
                }
                consoleTable.Options.EnableCount = false;
                consoleTable.Write();
            }
        }
    }
}
