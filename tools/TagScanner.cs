using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UFormKit.Models;

namespace UFormKit.Helpers
{
    public static class TagScanner
    {
        public static string tagRegex()
        {
            var tagNames = new List<string> { "text", "email", "url", "tel", "number", "range", "date", "textarea", "select", "checkbox", "radio", "acceptance", "file", "submit","hidden" };

            var tagRegexp = string.Join("|", tagNames.Select(tagName => $"{Regex.Escape(tagName)}|{Regex.Escape(tagName + "*")}"));
            return "(\\[?)"
                + "\\[(" + tagRegexp + ")(?:[\\r\\n\\t ](.*?))?(?:[\\r\\n\\t ](\\/))?\\]"
                + "(?:([^[]*?)\\[\\/\\2\\])?"
                + "(\\]?)";

        }

        public static AttributeResult ParseAtts(string text)
        {
            text = Regex.Replace(text, "[\x00a0\x200b]+", " ", RegexOptions.Compiled | RegexOptions.Multiline);
            text = text.Trim();

            string pattern = @"^([-+*=0-9a-zA-Z:.!?#$&@_/|% \r\n\t]*?)((?:[\r\n\t ]*""[^""]*""|[\r\n\t ]*'[^']*')*)$";
            AttributeResult atts = new AttributeResult();

            Match match = Regex.Match(text, pattern, RegexOptions.Multiline);

            if (match.Success)
            {
                if (!string.IsNullOrEmpty(match.Groups[1].Value))
                {
                    atts.Options = new List<string>(Regex.Split(match.Groups[1].Value, "[\r\n\t ]+").Select(option => option.Trim()));
                }

                if (!string.IsNullOrEmpty(match.Groups[2].Value))
                {
                    List<string> matchedValues = new List<string>();
                    foreach (Match valueMatch in Regex.Matches(match.Groups[2].Value, @"""[^""]*""|'[^']*'"))
                    {
                        matchedValues.Add(valueMatch.Value);
                    }

                    foreach (var value in matchedValues)
                    {
                        atts.Values.Add(value.Substring(1, value.Length - 2));
                    }
                }
            }
            else
            {
                atts.Values.Add(text);
            }

            return atts;
        }

        public static ScannedTag PopulateTag (Match matches)
        {
            string tagType = matches.Groups[2].Value;
            string tagBasetype = tagType.Trim('*');
            var attr = ParseAtts(matches.Groups[3].Value);

            var scannedTag = new ScannedTag();
            scannedTag.Type = tagType;
            scannedTag.BaseType = tagBasetype;
            scannedTag.RawName = attr.Options.Any() ? attr.Options.First() : "";
            scannedTag.Name = scannedTag.RawName.Replace(".", "_");
            scannedTag.Options = attr.Options;
            scannedTag.RawValues = attr.Values;
            scannedTag.Values = scannedTag.RawValues;
            scannedTag.Labels = scannedTag.Values;
            scannedTag.Values = scannedTag.Values.Select(value => value.Trim()).ToList();
            scannedTag.Labels = scannedTag.Labels.Select(label => label.Trim()).ToList();
            string content = matches.Groups[5].Value.Trim();
            content = Regex.Replace(content, @"<br[\r\n\t ]*\/?>$", string.Empty, RegexOptions.Multiline);
            scannedTag.Content = content;

            return scannedTag;
        }
    }
}
