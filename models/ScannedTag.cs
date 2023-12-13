
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UFormKit.Models
{
    public class ScannedTag
    {
        public string Type { get; set; }
        public string BaseType { get; set; }
        public string RawName { get; set; }
        public string Name { get; set; }
        public List<string> Options { get; set; }
        public List<string> RawValues { get; set; }
        public List<string> Values { get; set; }
        public List<string> Labels { get; set; }
        public string Attr { get; set; }
        public string Content { get; set; }

        public bool IsRequired()
        {
            return Type.EndsWith("*");
        }

        public bool HasOption(string optionName)
        {
            string pattern = string.Format(@"^{0}(:.+)?$", Regex.Escape(optionName));
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return Options.Any(regex.IsMatch);
        }

        public object GetOption(string optionName, string pattern = "", bool single = false)
        {

            Dictionary<string, string> presetPatterns = new Dictionary<string, string>
            {
                { "date", @"[0-9]{4}-[0-9]{2}-[0-9]{2}" },
                { "int", @"[0-9]+" },
                { "signed_int", @"[-]?[0-9]+" },
                { "num", @"(?:[0-9]+|(?:[0-9]+)?[.][0-9]+)" },
                { "signed_num", @"[-]?(?:[0-9]+|(?:[0-9]+)?[.][0.9]+)" },
                { "class", @"[-0-9a-zA-Z_]+" },
                { "id", @"[-0-9a-zA-Z_]+" }
            };

            if (presetPatterns.ContainsKey(pattern))
            {
                pattern = presetPatterns[pattern];
            }

            if (string.IsNullOrEmpty(pattern))
            {
                pattern = ".+";
            }

            string regexPattern = string.Format(@"^{0}:{1}$", Regex.Escape(optionName), pattern);


            if (single)
            {
                string match = getFirstMatchOption(regexPattern);
                return string.IsNullOrEmpty(match) ? null : match.Substring(optionName.Length + 1);
            }
            else
            {
                List<string> matches = getAllMatchOptions(regexPattern);
                return matches.Count == 0 ? null : matches.Select(match => match.Substring(optionName.Length + 1)).ToList();
            }
        }

        public string GetIdOption()
        {
            string option = (string)GetOption("id", "id", true);

            return option;
        }

        public List<string> GetClassOption()
        {
            var options = (List<string>)GetOption("class");

            return options;
        }

        public object GetSizeOption(int defaultValue = 40)
        {
            string option = (string)GetOption("size", "int", true);

            if (!string.IsNullOrEmpty(option))
            {
                return option;
            }

            List<string> matches = getAllMatchOptions(@"^([0-9]*)/[0-9]*");

            if (matches.Count > 1 && !string.IsNullOrEmpty(matches[1]) && int.TryParse(matches[1], out int size))
            {
                return size.ToString();
            }


            return defaultValue;
        }

        public object GetMaxLengthOption(bool defaultValue = false)
        {
            string option = (string)GetOption("maxlength", "int", true);

            if (!string.IsNullOrEmpty(option))
            {
                return option;
            }

            List<string> matches = getAllMatchOptions(@"%^(?:[0-9]*x?[0-9]*)?/([0-9]+)$%");

            if (matches.Count > 1 && !string.IsNullOrEmpty(matches[1]) && int.TryParse(matches[1], out int maxLength))
            {
                return maxLength.ToString();
            }


            return defaultValue;
        }

        public object GetMinLengthOption(bool defaultValue = false)
        {
            string option = (string)GetOption("minlength", "int", true);

            if (!string.IsNullOrEmpty(option))
            {
                return option;
            }


            return defaultValue;
        }

        public object GetColsOption(int defaultValue = 40)
        {
            string option = (string)GetOption("cols", "int", true);

            if (!string.IsNullOrEmpty(option))
            {
                return option;
            }

            List<string> matches = getAllMatchOptions(@"%^([0-9]*)x([0-9]*)(?:/[0-9]+)?$%");

            if (matches.Count > 1 && !string.IsNullOrEmpty(matches[1]) && int.TryParse(matches[1], out int cols))
            {
                return cols.ToString();
            }

            return defaultValue;
        }

        public object GetRowsOption(int defaultValue = 10)
        {
            string option = (string)GetOption("rows", "int", true);

            if (!string.IsNullOrEmpty(option))
            {
                return option;
            }

            List<string> matches = getAllMatchOptions(@"%^([0-9]*)x([0-9]*)(?:/[0-9]+)?$%");

            if (matches.Count > 2 && !string.IsNullOrEmpty(matches[2]) && int.TryParse(matches[2], out int rows))
            {
                return rows.ToString();
            }

            return defaultValue;
        }
        public string GetDateOption(string optionName)
        {
            string option = (string)GetOption(optionName, "", true);
            if (!string.IsNullOrEmpty(option))
            {

                DateTime dateTime;
                if (DateTime.TryParse(option.Replace("_", " "), out dateTime))
                {
                    return dateTime.ToString("yyyy-MM-dd");
                }
            }
            return option;
        }


        public int GetLimitOption(int defaultValue = 1048576)
        {
            string pattern = @"^limit:([1-9][0-9]*)([kKmM]?[bB])?";

            string[] matches = getFirstMatchOptions(pattern);

            if (matches != null)
            {
                int size = int.Parse(matches[1]);

                if (!string.IsNullOrEmpty(matches[2]))
                {
                    string kbmb = matches[2].ToLower();

                    if (kbmb == "kb")
                    {
                        size *= 1024;
                    }
                    else if (kbmb == "mb")
                    {
                        size *= 1048576;
                    }
                }

                return size;
            }

            return (int)defaultValue;
        }


        private List<string> getAllMatchOptions(string pattern)
        {
            List<string> result = new List<string>();

            foreach (string option in Options)
            {
                Match match = Regex.Match(option, pattern);
                if (match.Success)
                    result.Add(match.Value);

            }

            return result;
        }

        private string[] getFirstMatchOptions(string pattern)
        {
            foreach (string option in Options)
            {
                Match match = Regex.Match(option, pattern);

                if (match.Success)
                {
                    string[] matches = new string[match.Groups.Count];

                    for (int i = 0; i < match.Groups.Count; i++)
                    {
                        matches[i] = match.Groups[i].Value;
                    }

                    return matches;
                }
            }

            return null;
        }
        private string getFirstMatchOption(string pattern)
        {
            foreach (string option in Options)
            {
                Match match = Regex.Match(option, pattern);
                if (match.Success)
                {
                    return match.Value;
                }

            }

            return null;
        }

    }
}