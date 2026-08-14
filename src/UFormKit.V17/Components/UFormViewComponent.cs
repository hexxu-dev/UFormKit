using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using UFormKit.Helpers;
using UFormKit.Models;

namespace UFormKit.Components;

public class UFormViewComponent : ViewComponent
{
    private readonly IContentService _ctService;
    private readonly ILogger<UFormViewComponent> _logger;
    private readonly IScopeProvider _scopeProvider;
    private readonly UFormSettings _uformSettings;

    private global::Umbraco.Cms.Core.Models.IContent? _content;
    private string? _requiredMessage;
    private readonly List<ScannedTag> _scannedTags = new();

    public UFormViewComponent(
        IContentService ctService,
        ILogger<UFormViewComponent> logger,
        IScopeProvider scopeProvider,
        IOptions<UFormSettings> uformSettings)
    {
        _ctService = ctService;
        _logger = logger;
        _scopeProvider = scopeProvider;
        _uformSettings = uformSettings.Value;
    }

    /// <summary>
    ///     Renders a form. Accepts the form node's integer id or its GUID key, typed or as a string:
    ///     the Umbraco 14+ backoffice only surfaces the key, while templates carried over from
    ///     UFormKit 1.x pass the integer id. <paramref name="key" /> is an alias for the same value.
    /// </summary>
    /// <remarks>
    ///     Two parameters on purpose. With a single parameter, MVC binds the caller's whole anonymous
    ///     object to it whenever the parameter type is assignable from it (which <c>object</c> always
    ///     is), so <c>new { id = 1065 }</c> would arrive as the anonymous object instead of 1065.
    /// </remarks>
    public IViewComponentResult Invoke(object? id = null, object? key = null)
    {
        var builder = new StringBuilder();
        var siteKey = string.Empty;
        var cftsSitekey = string.Empty;
        var useRecaptcha = false;
        var useHoneypot = false;
        var useCloudflareTurnstile = false;

        try
        {
            _content = ResolveForm(id ?? key);
            if (_content is null)
            {
                ModelState.AddModelError(string.Empty, "Form not found.");
                return View(new UFormModel { Template = new HtmlString(string.Empty) });
            }

            var template = _content.GetValue<string>("form") ?? string.Empty;
            useRecaptcha = _content.GetValue<bool>("useRecaptcha");
            useCloudflareTurnstile = _content.GetValue<bool>("useCloudflareTurnstile");
            useHoneypot = _content.GetValue<bool>("useHoneypot");
            siteKey = useRecaptcha && _uformSettings.Recaptcha != null ? _uformSettings.Recaptcha.SiteKey : string.Empty;
            cftsSitekey = useCloudflareTurnstile && _uformSettings.CloudflareTurnstile != null ? _uformSettings.CloudflareTurnstile.SiteKey : string.Empty;
            _requiredMessage = _content.GetValue<string>("thereIsAFieldThatTheSenderMustFillIn");

            var regex = new Regex(TagScanner.tagRegex());
            var tags = regex.Matches(template);

            var tagsGroup = new Dictionary<string, ScannedTag>();
            foreach (Match tag in tags)
            {
                var scannedTag = TagScanner.PopulateTag(tag);
                _scannedTags.Add(scannedTag);
                tagsGroup[tag.Value] = scannedTag;
            }

            foreach (var item in tagsGroup.Keys)
            {
                template = template.Replace(item, TagReplace(tagsGroup[item]));
            }

            builder.Append($"<input type=\"hidden\" name=\"formId\" value=\"{_content.Id}\"/>");
            if (useRecaptcha)
            {
                builder.Append("<input type=\"hidden\" name=\"g-recaptcha-response\" id=\"g-recaptcha-response\"  />");
            }

            if (useHoneypot)
            {
                builder.Append("<div style=\"position:absolute; left:-9999px;\" aria-hidden=\"true\">");
                builder.Append("<input type=\"text\" name=\"extra-user-code\" value=\"\" autocomplete=\"off\" tabindex=\"-1\" />");
                builder.Append("</div>");
            }

            builder.Append(template);

            if (useCloudflareTurnstile)
            {
                builder.Append(" <script src=\"https://challenges.cloudflare.com/turnstile/v0/api.js?\" defer></script>");
                builder.Append("<div class=\"cf-turnstile\" data-sitekey=\"" + cftsSitekey + "\" data-callback=\"onTurnstileCallback\"></div>");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something failed while creating custom form: {Message}.", ex.Message);
            ModelState.AddModelError(string.Empty, "Problem displaying form. Try again!");
        }

        return View(new UFormModel
        {
            Template = new HtmlString(builder.ToString()),
            SiteKey = siteKey,
            UseRecaptcha = useRecaptcha,
            CftsKey = cftsSitekey,
            UseCfts = useCloudflareTurnstile,
        });
    }

    private global::Umbraco.Cms.Core.Models.IContent? ResolveForm(object? id) => id switch
    {
        int nodeId => _ctService.GetById(nodeId),
        Guid nodeKey => _ctService.GetById(nodeKey),
        string s when int.TryParse(s, out var nodeId) => _ctService.GetById(nodeId),
        string s when Guid.TryParse(s, out var nodeKey) => _ctService.GetById(nodeKey),
        _ => null,
    };

    private string TagReplace(ScannedTag scannedTag)
    {
        var defaultValue = scannedTag.GetDefaultValue(Request.Query, Request.HasFormContentType ? Request.Form : null);
        if (!string.IsNullOrEmpty(defaultValue))
        {
            scannedTag.Values.Insert(0, defaultValue);
        }

        return scannedTag.BaseType switch
        {
            TagType.Text or TagType.Email or TagType.Tel or TagType.Url => TextField(scannedTag, scannedTag.BaseType),
            TagType.Number or TagType.Range => NumberField(scannedTag, scannedTag.BaseType),
            TagType.Date => DateField(scannedTag),
            TagType.Textarea => TextareaField(scannedTag),
            TagType.Count => CountField(scannedTag),
            TagType.Submit => SubmitField(scannedTag),
            TagType.Select => SelectField(scannedTag),
            TagType.Checkbox or TagType.Radio => CheckboxField(scannedTag, scannedTag.BaseType),
            TagType.File => FileField(scannedTag),
            TagType.Acceptance => AcceptanceField(scannedTag),
            TagType.Hidden => HiddenField(scannedTag),
            _ => string.Empty,
        };
    }

    private string TextField(ScannedTag tag, string type)
    {
        var builder = new StringBuilder();
        builder.Append($"<input type=\"{type}\" name=\"{tag.RawName}\" data-val=\"true\"");
        if (type == TagType.Email)
        {
            builder.Append($" data-val-email=\"{_content!.GetValue<string>("emailAddressThatTheSenderEnteredIsInvalid")}\"");
        }
        else if (type == TagType.Url)
        {
            builder.Append($" data-val-url=\"{_content!.GetValue<string>("uRLThatTheSenderEnteredIsInvalid")}\"");
        }

        HtmlBuilder.AppendRequired(tag, builder, _requiredMessage!);
        HtmlBuilder.AppendClassAndId(tag, builder);
        HtmlBuilder.AppendPlaceholder(tag, builder);
        HtmlBuilder.AppendSize(tag, builder);
        HtmlBuilder.AppendMinMaxLength(tag, builder,
            _content!.GetValue<string>("thereIsAFieldWithInputThatIsShorterThanTheMinimumAllowedLength")!,
            _content!.GetValue<string>("thereIsAFieldWithInputThatIsLongerThanTheMaximumAllowedLength")!);
        HtmlBuilder.AppendAutocomplete(tag, builder);
        builder.Append("/>");
        HtmlBuilder.AppendValidationSpan(tag, builder);
        return builder.ToString();
    }

    private string NumberField(ScannedTag tag, string type)
    {
        var builder = new StringBuilder();
        builder.Append($"<input type=\"{type}\" name=\"{tag.RawName}\" data-val=\"true\" data-val-number=\"{_content!.GetValue<string>("numberFormatThatTheSenderEnteredIsInvalid")}\"");
        HtmlBuilder.AppendRequired(tag, builder, _requiredMessage!);
        HtmlBuilder.AppendClassAndId(tag, builder);
        HtmlBuilder.AppendPlaceholder(tag, builder);
        HtmlBuilder.AppendAutocomplete(tag, builder);

        var minValue = tag.GetOption("min", "signed_num", true);
        var maxValue = tag.GetOption("max", "signed_num", true);

        if (type == "range")
        {
            minValue ??= 0;
            maxValue ??= 100;
        }

        HtmlBuilder.AppendMinMax(tag, builder, minValue, maxValue);
        HtmlBuilder.AppendStep(tag, builder);

        builder.Append("/>");
        HtmlBuilder.AppendValidationSpan(tag, builder);
        return builder.ToString();
    }

    private string DateField(ScannedTag tag)
    {
        var builder = new StringBuilder();
        builder.Append($"<input type=\"date\" name=\"{tag.RawName}\" data-val=\"true\"");
        HtmlBuilder.AppendRequired(tag, builder, _requiredMessage!);
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

    private string TextareaField(ScannedTag tag)
    {
        var builder = new StringBuilder();
        builder.Append($"<textarea name=\"{tag.RawName}\" data-val=\"true\"");

        var cols = tag.GetColsOption();
        var rows = tag.GetRowsOption();

        if (cols != null) builder.Append($" cols={cols}");
        if (rows != null) builder.Append($" rows={rows}");

        HtmlBuilder.AppendRequired(tag, builder, _requiredMessage!);
        HtmlBuilder.AppendClassAndId(tag, builder);
        HtmlBuilder.AppendPlaceholder(tag, builder);
        HtmlBuilder.AppendMinMaxLength(tag, builder,
            _content!.GetValue<string>("thereIsAFieldWithInputThatIsShorterThanTheMinimumAllowedLength")!,
            _content!.GetValue<string>("thereIsAFieldWithInputThatIsLongerThanTheMaximumAllowedLength")!);
        HtmlBuilder.AppendAutocomplete(tag, builder);

        var countField = _scannedTags.FirstOrDefault(x => x.Type == TagType.Count && x.RawName == tag.RawName);
        if (countField != null)
        {
            var isDown = countField.HasOption("down") ? "true" : "false";
            builder.Append($" onkeyup=\"characterCount(this,{isDown})\"");
        }

        builder.Append("></textarea>");
        HtmlBuilder.AppendValidationSpan(tag, builder);
        return builder.ToString();
    }

    private string SubmitField(ScannedTag tag)
    {
        var builder = new StringBuilder();
        builder.Append("<input type=\"submit\"");
        var value = tag.Labels.Any() ? tag.Labels.FirstOrDefault() : "Submit";
        builder.Append($" value=\"{value}\"");
        HtmlBuilder.AppendClassAndId(tag, builder);
        builder.Append("/>");
        return builder.ToString();
    }

    private string SelectField(ScannedTag tag)
    {
        var builder = new StringBuilder();
        var isMultiple = tag.HasOption("multiple");
        var isFirstOptionBlank = tag.HasOption("include_blank");
        var defaultValue = tag.GetOption("default", "", true);

        builder.Append($"<select name=\"{tag.RawName}\" data-val=\"true\"");
        HtmlBuilder.AppendRequired(tag, builder, _requiredMessage!);
        HtmlBuilder.AppendClassAndId(tag, builder);
        if (isMultiple) builder.Append(" multiple");
        builder.Append(">");

        if (tag.Labels.Any())
        {
            GetOptionsFromDatabaseIfQuery(tag);
            if (isFirstOptionBlank) builder.Append("<option value=\"\">Please choose an option</option>");

            var counter = 1;
            var selectedStr = "";
            foreach (var label in tag.Labels)
            {
                if (defaultValue != null && ((string)defaultValue).Split("_").Contains(counter.ToString()))
                {
                    selectedStr = " selected";
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

    private string CheckboxField(ScannedTag tag, string type)
    {
        var builder = new StringBuilder();
        builder.Append("<span ");
        HtmlBuilder.AppendClassAndId(tag, builder);
        builder.Append("/>");
        var useLabelElement = tag.HasOption("use_label_element");

        var requiredStr = "";
        if (tag.IsRequired())
        {
            requiredStr = $" data-rule-required=\"true\" data-msg-required=\"{_content!.GetValue<string>("thereIsAFieldThatTheSenderMustFillIn")}\" ";
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
            GetOptionsFromDatabaseIfQuery(tag);
            var counter = 1;
            foreach (var label in tag.Labels)
            {
                string checkbox;

                if (defaultValue != null && ((string)defaultValue).Split("_").Contains(counter.ToString()))
                {
                    checkedStr = " checked";
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

    private string FileField(ScannedTag tag)
    {
        var builder = new StringBuilder();
        builder.Append($"<input type=\"file\" name=\"{tag.RawName}\" data-val=\"true\"");
        HtmlBuilder.AppendRequired(tag, builder, _requiredMessage!);
        HtmlBuilder.AppendClassAndId(tag, builder);
        HtmlBuilder.AppendSize(tag, builder);

        var filetypes = tag.GetOption("filetypes", "", true);
        if (filetypes != null)
        {
            var accept = ((string)filetypes).Split("|").Select(x => x.Contains(".") ? x : "." + x);
            builder.Append($" accept=\"{string.Join(",", accept)}\"");
        }

        builder.Append("/>");
        HtmlBuilder.AppendValidationSpan(tag, builder);
        return builder.ToString();
    }

    private string AcceptanceField(ScannedTag tag)
    {
        var builder = new StringBuilder();
        builder.Append($"<label><input type=\"checkbox\" name=\"{tag.RawName}\" value=\"1\" data-val=\"true\"");

        var isOptional = tag.HasOption("optional");
        if (!isOptional)
        {
            builder.Append($" data-rule-required=\"true\" data-msg-required=\"{_content!.GetValue<string>("thereAreTermsThatTheSenderMustAccept")}\" ");
        }
        builder.Append("/>");
        builder.Append($"<span>{tag.Content}</span>");
        HtmlBuilder.AppendValidationSpan(tag, builder);
        return builder.ToString();
    }

    private string HiddenField(ScannedTag tag)
    {
        var builder = new StringBuilder();
        builder.Append($"<input type=\"hidden\" name=\"{tag.RawName}\" value=\"{tag.Values.First()}\"");
        HtmlBuilder.AppendClassAndId(tag, builder);
        builder.Append("/>");
        return builder.ToString();
    }

    private string CountField(ScannedTag tag)
    {
        var builder = new StringBuilder();
        var isDown = tag.HasOption("down");
        var start = 0;

        if (isDown)
        {
            var target = _scannedTags.FirstOrDefault(x => x.Type != TagType.Count && x.RawName == tag.RawName);
            if (target != null && target.GetMaxLengthOption() != null)
            {
                start = int.Parse(target.GetMaxLengthOption()!.ToString()!);
            }
        }

        builder.Append($"<span data-target-name=\"{tag.RawName}\">{start}</span>");
        return builder.ToString();
    }

    private void GetOptionsFromDatabaseIfQuery(ScannedTag tag)
    {
        if (tag.Labels.Count != 1 || !tag.Labels.First().Contains("query:"))
        {
            return;
        }

        var query = tag.Labels.First().Replace("query:", "");
        var options = new List<string>();
        try
        {
            using var scope = _scopeProvider.CreateScope();
            options = scope.Database.Query<string>(Sql.Builder.Append(query)).ToList();
            scope.Complete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something failed while executing query: {Message}.", ex.Message);
        }
        tag.Labels = options;
    }
}
