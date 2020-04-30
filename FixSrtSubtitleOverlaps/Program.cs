using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace FixSrtSubtitleOverlaps
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args is null || args.Length == 0)
            {
                args = new string[1];
                args[0] = @"F:\Clip\Music\A Simple Guide to Working _ Learning From Home_ HOW TO ADJUST - English (auto-generated)_old.srt";
            }

            foreach (string file in args)
            {
                FixSubtitle(file);
            }
        }

        private static void FixSubtitle(string file)
        {
            string subtitle = string.Empty;
            string path = Path.GetDirectoryName(file);
            string fileName = Path.GetFileNameWithoutExtension(file);
            string fileExt = Path.GetExtension(file);
            if (fileExt.ToLower() != ".srt")
            {
                ShowAlert("File extension should be '.srt' !!!");
                return;
            }
            List<SubTimes> subTimesList = new List<SubTimes>();
            List<string> subtitlesList = new List<string>();

            subtitle = FileReader(file);
            subtitle = subtitle.Replace("\r\n", "\n").Replace("\r", "\n");

            List<String> subtitleParts = new List<string>(Regex.Split(subtitle, @"^.*(\d+):(\d{2}):(\d{2}),(\d{3}) --> (\d+):(\d{2}):(\d{2}),(\d{3})", RegexOptions.Multiline));
            subtitleParts.RemoveAt(0);

            subTimesList = ExtractTimesToList(subtitleParts);
            subtitlesList = ExtractSubsToList(subtitleParts);
            JustifyTimes(subTimesList);
            string outSubtitle = GenerateSubtitle(subTimesList, subtitlesList);
            FileWriter(outSubtitle, fileName + "_OUT", "srt", path);
        }

        private static string GenerateSubtitle(List<SubTimes> subTimesList, List<string> subtitlesList)
        {
            string body = string.Empty;
            int counter = 1;
            for (int i = 0; i < subtitlesList.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(subtitlesList[i])) { continue; }
                body += counter.ToString().AddNewLine();
                body += PrintTimeStamp(subTimesList[i]).AddNewLine();
                body += subtitlesList[i].AddNewLine().AddNewLine();
                counter++;
            }
            return body;
        }

        private static string PrintTimeStamp(SubTimes subTimes)
        {
            string timestamp = string.Empty;
            timestamp = subTimes.startTime.Hours.ToString("00") + ":"
                        + subTimes.startTime.Minutes.ToString("00") + ":"
                        + subTimes.startTime.Seconds.ToString("00") + ","
                        + subTimes.startTime.Milliseconds.ToString("000").Substring(0, 3)
                        + " --> "
                        + subTimes.endTime.Hours.ToString("00") + ":"
                        + subTimes.endTime.Minutes.ToString("00") + ":"
                        + subTimes.endTime.Seconds.ToString("00") + ","
                        + subTimes.endTime.Milliseconds.ToString("000").Substring(0, 3);

            return timestamp;
        }

        private static string TrimSubtitle(string subtitle)
        {
            if (subtitle.IndexOf("\n") < 0) { return subtitle; }
            string[] subParts = subtitle.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            subParts[subParts.Length - 1] = string.Empty;
            return string.Join("\n", subParts);
        }

        private static void JustifyTimes(List<SubTimes> subTimesList)
        {
            for (int i = 0; i < subTimesList.Count; i++)
            {
                if (i >= subTimesList.Count - 1) { break; }
                if (subTimesList[i].endTime >= subTimesList[i + 1].startTime)
                {
                    subTimesList[i].endTime = subTimesList[i + 1].startTime.Add(new TimeSpan(-20000));  //MINUS 2 MILLISECONDS
                }
            }
        }

        private static List<string> ExtractSubsToList(List<string> arrSub)
        {
            List<string> subtitleList = new List<string>();
            for (int i = 0; i < arrSub.Count; i += 9)
            {
                string subtitle = i + 9 < arrSub.Count ? TrimSubtitle(arrSub[i + 8]) : arrSub[i + 8];
                subtitleList.Add(subtitle.Trim('\n'));
            }

            return subtitleList;
        }

        private static List<SubTimes> ExtractTimesToList(List<string> subtitleParts)
        {
            List<SubTimes> subTimesList = new List<SubTimes>();
            for (int i = 0; i < subtitleParts.Count; i += 9)
            {
                SubTimes subTimes = new SubTimes();
                subTimes.startTime = new TimeSpan(0, subtitleParts[i].ToInt(), subtitleParts[i + 1].ToInt(), subtitleParts[i + 2].ToInt(), subtitleParts[i + 3].ToInt());
                subTimes.endTime = new TimeSpan(0, subtitleParts[i + 4].ToInt(), subtitleParts[i + 5].ToInt(), subtitleParts[i + 6].ToInt(), subtitleParts[i + 7].ToInt());
                subTimesList.Add(subTimes);
            }
            return subTimesList;
        }

        private static void FileWriter(string boddy, string fileName, string extension, string address)
        {
            extension = extension.IndexOf(".") == 0 ? extension : "." + extension;
            using (StreamWriter wr = new StreamWriter(address + "\\" + fileName + extension, false, Encoding.UTF8))
            {
                wr.Write(boddy);
                wr.Flush();
            }
        }

        private static string FileReader(string fileName)
        {
            string outString = string.Empty;
            using (StreamReader reader = new StreamReader(fileName))
            {
                outString = reader.ReadToEnd();
            }
            return outString;
        }

        private static void ShowAlert(string alrt)
        {
            //Console.Clear();
            for (int i = 0; i < 10; i++)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(alrt);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(alrt);
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\nPress any key to continue ...");
            Console.ReadKey();
        }

    }

    class SubTimes
    {
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
    }
    public static class Extensions
    {
        public static int ToInt(this string str)
        {
            return Convert.ToInt32(str);
        }
        public static string AddNewLine(this string str)
        {
            return str + "\n";
        }
    }
}
