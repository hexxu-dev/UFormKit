using System.Linq;
using System.Text;
using UFormKit.Models;
using static Umbraco.Cms.Core.Diagnostics.MiniDump;

namespace UFormKit.Helpers
{
    public static class HtmlBuilder
    {
        public static void AppendClassAndId(ScannedTag tag, StringBuilder builder)
        {
            var idValue = tag.GetIdOption();
            var classValues = tag.GetClassOption();

            if (!string.IsNullOrEmpty(idValue))
            {
                builder.Append($" id=\"{idValue}\"");
            }
            if (classValues != null && classValues.Any())
            {
                builder.Append($" class=\"{string.Join(" ", classValues)}\"");
            }
        }

        public static void AppendValidationSpan(ScannedTag tag, StringBuilder builder)
        {
            builder.Append($"<span class=\"text-danger field-validation-valid\" data-valmsg-for=\"{tag.RawName}\" data-valmsg-replace=\"true\"></span>");
        }

        public static void AppendRequired(ScannedTag tag, StringBuilder builder, string message)
        {
            if (tag.IsRequired())
            {
                builder.Append($" data-val-required=\"{message}\" ");
            }
        }

        public static void AppendSize(ScannedTag tag, StringBuilder builder)
        {
            var size = tag.GetSizeOption();
            if (size != null)
            {
                builder.Append($" size=\"{size}\" ");
            }
        }

        public static void AppendMinMaxLength(ScannedTag tag, StringBuilder builder, string messageMin, string messageMax)
        {
            var min = tag.GetMinLengthOption();
            if (min != null)
            {
                builder.Append($" minlength=\"{min}\" data-val-minlength-min=\"{min}\" data-val-minlength=\"{messageMin}\" ");
            }
            var max = tag.GetMaxLengthOption();
            if (max != null)
            {
                builder.Append($" maxlength=\"{max}\" data-val-maxlength-max=\"{max}\" data-val-maxlength=\"{messageMax}\" ");
            }
        }

        public static void AppendPlaceholder(ScannedTag tag, StringBuilder builder)
        {
            if (tag.HasOption("placeholder") && tag.Values.Any())
            {
                builder.Append($" placeholder = \"{tag.Values.First()}\"");
            }
            else if (tag.Values.Any())
            {
                builder.Append($" value = \"{tag.Values.First()}\"");
            }
        }

        public static void AppendMinMax(ScannedTag tag, StringBuilder builder, object? minValue, object? maxValue)
        {
            if (minValue != null)
            {
                builder.Append($" min=\"{minValue}\"");
            }
            if (maxValue != null)
            {
                builder.Append($" max=\"{maxValue}\"");
            }
        }

        public static void AppendAutocomplete(ScannedTag tag, StringBuilder builder)
        {
            var autocomplete = tag.GetOption("autocomplete", "[-0-9a-zA-Z]+", true);
            if (autocomplete != null)
            {
                builder.Append($" autocomplete=\"{autocomplete}\" ");
            }
        }

        public static void AppendStep(ScannedTag tag, StringBuilder builder)
        {
            var step = tag.GetOption("step", "int", true);
            if (step != null)
            {
                builder.Append($" step=\"{step}\" ");
            }
        }



    }
}
