using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using NPoco;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using UFormKit.Helpers;
using UFormKit.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace UFormKit.Components
{
    public class UFormViewComponent : ViewComponent
    {
        private IContentService ctService;
        private IContent content;
        private UFormSettings uformSettings;
        private string requiredMessage;
        private ILogger<UFormViewComponent> logger;
        private IScopeProvider scopeProvider;
        private List<ScannedTag> scannedTags = new List<ScannedTag>();



        public UFormViewComponent(IContentService ctService, ILogger<UFormViewComponent> logger, IScopeProvider scopeProvider, IOptions<UFormSettings> uformSettings)
        {
            this.ctService = ctService;
            this.logger = logger;
            this.scopeProvider = scopeProvider;
            this.uformSettings = uformSettings.Value;
        }

        public IViewComponentResult Invoke(int id)
        {
            var builder = new StringBuilder();
            var siteKey = "";
            var useRecaptcha = false;

            try
            {
                content = ctService.GetById(id);

                var template = content.GetValue<string>("form");
                useRecaptcha = content.GetValue<bool>("useRecaptcha");
                siteKey = useRecaptcha && uformSettings.Recaptcha != null ? uformSettings.Recaptcha.SiteKey : "";
                requiredMessage = content.GetValue<string>("thereIsAFieldThatTheSenderMustFillIn");

                Regex regex = new Regex(TagScanner.tagRegex());
                var tags = regex.Matches(template);

                var tagsGroup = new Dictionary<string, ScannedTag>();

                foreach (Match tag in tags)
                {
                    var scannedTad = TagScanner.PopulateTag(tag);
                    scannedTags.Add(scannedTad);
                    tagsGroup.Add(tag.Value, scannedTad);
                }

                foreach (var item in tagsGroup.Keys)
                {
                    template = template.Replace(item, tagReplace(tagsGroup[item]));
                }
                builder.Append($"<input type=\"hidden\" name=\"formId\" value=\"{id}\"/>");
                if (useRecaptcha)
                {
                    builder.Append("<input type=\"hidden\" name=\"g-recaptcha-response\" id=\"g-recaptcha-response\"  />");
                }
                builder.Append(template);

            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Something failed while creating custom form: {ex.Message}.");
                ModelState.AddModelError("", "Problem displaying form. Try again!");
            }
            return View(new UFormModel { Template = new HtmlString(builder.ToString()), SiteKey = siteKey, UseRecaptcha = useRecaptcha });

        }

        private string tagReplace(ScannedTag scannedTag)
        {
            var result = "";

            var defaultValue = scannedTag.GetDefaultValue(Request.Query, Request.HasFormContentType ? Request.Form : null);

            if (!string.IsNullOrEmpty(defaultValue))
            {
                scannedTag.Values.Insert(0, defaultValue);
            }

            switch (scannedTag.BaseType)
            {
                case TagType.Text:
                case TagType.Email:
                case TagType.Tel:
                case TagType.Url:
                    {
                        result = textField(scannedTag, scannedTag.BaseType);
                        break;
                    }
                case TagType.Number:
                case TagType.Range:
                    {
                        result = numberField(scannedTag, scannedTag.BaseType);
                        break;
                    }
                case TagType.Date:
                    {
                        result = dateField(scannedTag);
                        break;
                    }
                case TagType.Textarea:
                    {
                        result = textareaField(scannedTag);
                        break;
                    }
                case TagType.Count:
                    {
                        result = countField(scannedTag);
                        break;
                    }
                case TagType.Submit:
                    {
                        result = submitField(scannedTag);
                        break;
                    }
                case TagType.Select:
                    {
                        result = selectField(scannedTag);
                        break;
                    }
                case TagType.Checkbox:
                case TagType.Radio:
                    {
                        result = checkboxField(scannedTag, scannedTag.BaseType);
                        break;
                    }
                case TagType.File:
                    {
                        result = fileField(scannedTag);
                        break;
                    }
                case TagType.Acceptance:
                    result = acceptanceField(scannedTag);
                    break;
                case TagType.Hidden:
                    result = hiddenField(scannedTag);
                    break;
                default:
                    break;
            }

            return result;
        }


        private string textField(ScannedTag tag, string type)
        {
            var builder = new StringBuilder();
            builder.Append($"<input type=\"{type}\" name=\"{tag.RawName}\" data-val=\"true\"");
            if (type == TagType.Email)
            {
                builder.Append($" data-val-email=\"{content.GetValue<string>("emailAddressThatTheSenderEnteredIsInvalid")}\"");
            }
            else if (type == TagType.Url)
            {
                builder.Append($" data-val-url=\"{content.GetValue<string>("uRLThatTheSenderEnteredIsInvalid")}\"");
            }

            HtmlBuilder.AppendRequired(tag, builder, requiredMessage);
            HtmlBuilder.AppendClassAndId(tag, builder);
            HtmlBuilder.AppendPlaceholder(tag, builder);
            HtmlBuilder.AppendSize(tag, builder);
            HtmlBuilder.AppendMinMaxLength(tag, builder, content.GetValue<string>("thereIsAFieldWithInputThatIsShorterThanTheMinimumAllowedLength"), content.GetValue<string>("thereIsAFieldWithInputThatIsLongerThanTheMaximumAllowedLength"));
            HtmlBuilder.AppendAutocomplete(tag, builder);
            builder.Append("/>");
            HtmlBuilder.AppendValidationSpan(tag, builder);
            return builder.ToString();
        }

        private string numberField(ScannedTag tag, string type)
        {
            var builder = new StringBuilder();
            builder.Append($"<input type=\"{type}\" name=\"{tag.RawName}\" data-val=\"true\" data-val-number=\"{content.GetValue<string>("numberFormatThatTheSenderEnteredIsInvalid")}\"");
            HtmlBuilder.AppendRequired(tag, builder, requiredMessage);
            HtmlBuilder.AppendClassAndId(tag, builder);
            HtmlBuilder.AppendPlaceholder(tag, builder);
            HtmlBuilder.AppendAutocomplete(tag, builder);

            var minValue = tag.GetOption("min", "signed_num", true);
            var maxValue = tag.GetOption("max", "signed_num", true);

            if (type == "range")
            {
                if (minValue == null) minValue = 0;
                if (maxValue == null) maxValue = 100;
            }

            HtmlBuilder.AppendMinMax(tag, builder, minValue, maxValue);
            HtmlBuilder.AppendStep(tag, builder);

            builder.Append("/>");
            HtmlBuilder.AppendValidationSpan(tag, builder);
            return builder.ToString();
        }

        private string dateField(ScannedTag tag)
        {
            var builder = new StringBuilder();
            builder.Append($"<input type=\"date\" name=\"{tag.RawName}\" data-val=\"true\"");
            HtmlBuilder.AppendRequired(tag, builder, requiredMessage);
            HtmlBuilder.AppendClassAndId(tag, builder);
            HtmlBuilder.AppendPlaceholder(tag, builder);

            var minValue = tag.GetOption("min", "date", true);
            var maxValue = tag.GetOption("max", "date", true);
            HtmlBuilder.AppendMinMax(tag, builder, minValue, maxValue);
            HtmlBuilder.AppendAutocomplete(tag, builder);

            builder.Append("/>");
            HtmlBuilder.AppendValidationSpan(tag, builder);
            return builder.ToString();
        }

        private string textareaField(ScannedTag tag)
        {
            var builder = new StringBuilder();
            builder.Append($"<textarea name=\"{tag.RawName}\" data-val=\"true\"");

            var cols = tag.GetColsOption();
            var rows = tag.GetRowsOption();

            if (cols != null)
            {
                builder.Append($" cols={cols}");
            }

            if (rows != null)
            {
                builder.Append($" rows={rows}");
            }

            HtmlBuilder.AppendRequired(tag, builder, requiredMessage);
            HtmlBuilder.AppendClassAndId(tag, builder);
            HtmlBuilder.AppendPlaceholder(tag, builder);
            HtmlBuilder.AppendMinMaxLength(tag, builder, content.GetValue<string>("thereIsAFieldWithInputThatIsShorterThanTheMinimumAllowedLength"), content.GetValue<string>("thereIsAFieldWithInputThatIsLongerThanTheMaximumAllowedLength"));
            HtmlBuilder.AppendAutocomplete(tag, builder);

            var countField = scannedTags.FirstOrDefault(x => x.Type == TagType.Count && x.RawName == tag.RawName);
            if (countField != null)
            {
                var isDown = countField.HasOption("down") ? "true" : "false";
                builder.Append($" onkeyup=\"characterCount(this,{isDown})\"");
            }
            builder.Append("></textarea>");
            HtmlBuilder.AppendValidationSpan(tag, builder);

            return builder.ToString();
        }

        private string submitField(ScannedTag tag)
        {
            var builder = new StringBuilder();
            builder.Append(string.Format("<input type=\"submit\""));
            var value = tag.Labels.Any() ? tag.Labels.FirstOrDefault() : "Submit";
            builder.Append($" value=\"{value}\"");
            HtmlBuilder.AppendClassAndId(tag, builder);
            builder.Append("/>");
            return builder.ToString();
        }

        private string selectField(ScannedTag tag)
        {
            var builder = new StringBuilder();
            var isMultiple = tag.HasOption("multiple");
            var isFirstOptionBlank = tag.HasOption("include_blank");
            var defaultValue = tag.GetOption("default", "", true);

            builder.Append($"<select name=\"{tag.RawName}\" data-val=\"true\"");
            HtmlBuilder.AppendRequired(tag, builder, requiredMessage);
            HtmlBuilder.AppendClassAndId(tag, builder);

            if (isMultiple)
            {
                builder.Append(" multiple");
            }
            builder.Append(">");
            if (tag.Labels.Any())
            {
                getOptionsFromDatabaseIfQuery(tag);
                if (isFirstOptionBlank)
                    builder.Append($"<option value=\"\">{(object)"Please choose an option"}</option>");

                var counter = 1;
                var selectedStr = "";
                foreach (var label in tag.Labels)
                {
                    if (defaultValue != null)
                    {
                        if (((string)defaultValue).Split("_").Contains(counter.ToString()))
                        {
                            selectedStr = " selected";
                        }
                    }
                    builder.Append($"<option value=\"{label}\" {selectedStr}>{label}</option>");
                    counter++;
                    selectedStr = "";
                }
            }
            builder.Append("</select>");
            HtmlBuilder.AppendValidationSpan(tag, builder);
            return builder.ToString();
        }

        private string checkboxField(ScannedTag tag, string type)
        {
            var builder = new StringBuilder();
            builder.Append("<span ");
            HtmlBuilder.AppendClassAndId(tag, builder);
            builder.Append("/>");
            var useLabelElement = tag.HasOption("use_label_element");

            var requiredStr = "";

            if (tag.IsRequired())
            {
                requiredStr = string.Format(" data-rule-required=\"true\" data-msg-required=\"{0}\" ", content.GetValue<string>("thereIsAFieldThatTheSenderMustFillIn"));
            }

            var checkedStr = "";

            var defaultValue = tag.GetOption("default", "", true);

            var exclusiveStr = "";
            if (tag.HasOption("exclusive"))
            {
                exclusiveStr = " onChange=\"makeExclusive(this)\"";
            }

            if (tag.Labels.Any())
            {
                getOptionsFromDatabaseIfQuery(tag);
                var counter = 1;
                foreach (var label in tag.Labels)
                {
                    string checkbox;

                    if (defaultValue != null)
                    {
                        if (((string)defaultValue).Split("_").Contains(counter.ToString()))
                        {
                            checkedStr = " checked";
                        }
                    }

                    if (!tag.HasOption("label_first"))
                    {
                        checkbox = $"<input  data-val=\"true\" type=\"{type}\" name=\"{tag.RawName}\" value=\"{label}\" {requiredStr} {checkedStr} {exclusiveStr}><span>{label}</span>";
                    }
                    else
                    {
                        checkbox = $"<span>{label}</span><input onChange=\"makeExclusive(this)\" data-val=\"true\" type=\"{type}\" name=\"{tag.RawName}\"  value=\"{label}\" {requiredStr} {checkedStr} {exclusiveStr}>";
                    }
                    if (useLabelElement)
                    {
                        checkbox = $"<label>{checkbox}</label>";
                    }

                    builder.Append(checkbox);

                    counter++;
                    checkedStr = "";
                }
            }
            builder.Append("</span>");
            HtmlBuilder.AppendValidationSpan(tag, builder);
            return builder.ToString();
        }

        private string fileField(ScannedTag tag)
        {
            var builder = new StringBuilder();
            builder.Append($"<input type=\"file\" name=\"{tag.RawName}\" data-val=\"true\"");
            HtmlBuilder.AppendRequired(tag, builder, requiredMessage);
            HtmlBuilder.AppendClassAndId(tag, builder);
            HtmlBuilder.AppendSize(tag, builder);

            var filetypes = tag.GetOption("filetypes", "", true);
            if (filetypes != null)
            {
                var accept = ((string)filetypes).Split("|").Select(x => x.Contains(".") ? x : "." + x);

                builder.Append($" accept=\"{string.Join(",", accept)}\"");
            }

            var limit = tag.GetLimitOption();
            builder.Append("/>");
            HtmlBuilder.AppendValidationSpan(tag, builder);
            return builder.ToString();
        }

        private string acceptanceField(ScannedTag tag)
        {
            var builder = new StringBuilder();
            builder.Append($"<label><input type=\"checkbox\" name=\"{tag.RawName}\" value=\"1\" data-val=\"true\"");

            var isOptional = tag.HasOption("optional");
            if (!isOptional)
                builder.Append($" data-rule-required=\"true\" data-msg-required=\"{content.GetValue<string>("thereAreTermsThatTheSenderMustAccept")}\" ");
            builder.Append("/>");
            builder.Append($"<span>{tag.Content}</span>");
            HtmlBuilder.AppendValidationSpan(tag, builder);
            return builder.ToString();
        }

        private string hiddenField(ScannedTag tag)
        {
            var builder = new StringBuilder();
            builder.Append($"<input type=\"hidden\" name=\"{tag.RawName}\" value=\"{tag.Values.First()}\"");
            HtmlBuilder.AppendClassAndId(tag, builder);
            builder.Append("/>");
            return builder.ToString();
        }

        private string countField(ScannedTag tag)
        {
            var builder = new StringBuilder();
            var isDown = tag.HasOption("down");
            var start = 0;

            if (isDown)
            {
                var target = scannedTags.FirstOrDefault(x => x.Type != TagType.Count && x.RawName == tag.RawName);
                if (target != null && target.GetMaxLengthOption != null)
                {
                    start = int.Parse(target.GetMaxLengthOption().ToString());
                }
            }

            builder.Append($"<span data-target-name=\"{tag.RawName}\">{start}</span>");
            return builder.ToString();
        }

        private void getOptionsFromDatabaseIfQuery(ScannedTag tag)
        {
            if (tag.Labels.Count == 1 && tag.Labels.First().Contains("query:"))
            {
                var query = tag.Labels.First().Replace("query:", "");
                var options = new List<string>();
                try
                {
                    using (var scope = scopeProvider.CreateScope())
                    {
                        options = scope.Database.Query<string>(Sql.Builder.Append(query)).ToList();
                        scope.Complete();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Something failed while executing query: {ex.Message}.");
                }
                tag.Labels = options;
            }
        }


    }
}
