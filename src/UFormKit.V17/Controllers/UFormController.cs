using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;
using UFormKit.Helpers;
using UFormKit.Models;
using UFormKit.Notifications;


namespace UFormKit.Controller;

[PluginController("UFormKit")]
public class UFormController : SurfaceController
{
    private readonly IContentService _ctService;
    private readonly IEmailSender _emailSender;
    private readonly CaptchaVerificationService _verificationService;
    private readonly ILogger<UFormController> _logger;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IUmbracoContextAccessor _contextAccessor;
    private readonly IEventAggregator _eventAggregator;

    public UFormController(
        IUmbracoContextAccessor contextAccessor,
        IHostingEnvironment hostingEnvironment,
        ILogger<UFormController> logger,
        CaptchaVerificationService verificationService,
        IEmailSender emailSender,
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IContentService ctService,
        IEventAggregator eventAggregator)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _ctService = ctService;
        _emailSender = emailSender;
        _verificationService = verificationService;
        _logger = logger;
        _hostingEnvironment = hostingEnvironment;
        _contextAccessor = contextAccessor;
        _eventAggregator = eventAggregator;
    }

    [HttpPost]
    public async Task<IActionResult> SendAsync(IFormCollection form)
    {
        var formId = int.Parse(form["formId"].ToString());
        var content = _ctService.GetById(formId);
        if (content is null)
        {
            ModelState.AddModelError(string.Empty, "Form not found.");
            return CurrentUmbracoPage();
        }

        var ctx = BuildEmailContext(content);

        if (ctx.UseHoneypot && !string.IsNullOrEmpty(form["extra-user-code"]))
        {
            ModelState.AddModelError(string.Empty, "There was an issue submitting the form. Please try again.");
            return CurrentUmbracoPage();
        }

        if (ctx.UseRecaptcha)
        {
            var recaptcha = form["g-recaptcha-response"].ToString();
            if (string.IsNullOrEmpty(recaptcha) || !await _verificationService.IsCaptchaValid(recaptcha))
            {
                ModelState.AddModelError(string.Empty, "Problem while validating recaptcha. Try again!");
                return CurrentUmbracoPage();
            }
        }

        if (ctx.UseCloudflareTurnstile)
        {
            var cfValue = form["cf-turnstile-response"].ToString();
            if (string.IsNullOrEmpty(cfValue) || !await _verificationService.IsCftsValid(cfValue))
            {
                ModelState.AddModelError(string.Empty, "Verification failed. Please try again");
                return CurrentUmbracoPage();
            }
        }

        if (!string.IsNullOrEmpty(ctx.BlockedIPs))
        {
            var blockedList = ctx.BlockedIPs.Split(",").Select(s => s.Trim()).ToList();
            var userIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (userIp != null && IsIPBlocked(userIp, blockedList))
            {
                ModelState.AddModelError(string.Empty, "Access denied");
                return CurrentUmbracoPage();
            }
        }

        try
        {
            if (!ctx.Demo)
            {
                var prepared = PrepareMessage(ctx, form);
                var attachmentResult = TryBuildAttachments(content, prepared.ScannedTags, ctx.FilesAttach);
                if (!attachmentResult.IsSuccess)
                {
                    ModelState.AddModelError(string.Empty, attachmentResult.ErrorMessage);
                    return CurrentUmbracoPage();
                }

                var message = new EmailMessage(
                    prepared.MailFrom,
                    prepared.MailTo?.Split(","),
                    prepared.Cc?.Split(","),
                    prepared.Bcc?.Split(","),
                    prepared.ReplyTo?.Split(","),
                    prepared.Subject,
                    prepared.Body,
                    ctx.IsHtml,
                    attachmentResult.Attachments);

                await _emailSender.SendAsync(message, "");
                _eventAggregator.Publish(new UFormEmailSentNotification(form, prepared.Body));

                if (ctx.StoreData)
                {
                    try
                    {
                        InsertSubmission(prepared.Body, content.GetUdi().Guid);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error happend while inserting submission: {Message}.", ex.Message);
                    }
                }
            }

            TempData["success"] = content.GetValue<string>("sendersMessageSuccess");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error happend while sending email: {Message}.", ex.Message);
            ModelState.AddModelError(string.Empty, content.GetValue<string>("sendersMessageFailedToSend") ?? "Failed to send.");
            return CurrentUmbracoPage();
        }

        if (ctx.RedirectUrl is GuidUdi guidUdi)
        {
            var umbracoContext = _contextAccessor.GetRequiredUmbracoContext();
            var page = umbracoContext.Content?.GetById(guidUdi.Guid);
            if (page != null)
            {
                return RedirectToUmbracoPage(page);
            }
        }

        return RedirectToCurrentUmbracoPage();
    }

    [HttpPost]
    public async Task<JsonResult> SendJson(IFormCollection form)
    {
        var formId = int.Parse(form["formId"].ToString());
        var content = _ctService.GetById(formId);
        if (content is null)
        {
            return new JsonResult(new { Success = false, Message = "Form not found." }) { StatusCode = StatusCodes.Status404NotFound };
        }

        var ctx = BuildEmailContext(content);

        if (ctx.UseHoneypot && !string.IsNullOrEmpty(form["extra-user-code"]))
        {
            return new JsonResult(new { Success = false, Message = "There was an issue submitting the form. Please try again." }) { StatusCode = StatusCodes.Status400BadRequest };
        }

        if (ctx.UseRecaptcha)
        {
            var recaptcha = form["g-recaptcha-response"].ToString();
            if (string.IsNullOrEmpty(recaptcha) || !await _verificationService.IsCaptchaValid(recaptcha))
            {
                return new JsonResult(new { Success = false, Message = "Problem while validating recaptcha. Try again!" }) { StatusCode = StatusCodes.Status400BadRequest };
            }
        }

        if (!string.IsNullOrEmpty(ctx.BlockedIPs))
        {
            var blockedList = ctx.BlockedIPs.Split(",").Select(s => s.Trim()).ToList();
            var userIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (userIp != null && IsIPBlocked(userIp, blockedList))
            {
                return new JsonResult(new { Success = false, Message = "Access denied" }) { StatusCode = StatusCodes.Status400BadRequest };
            }
        }

        try
        {
            if (!ctx.Demo)
            {
                var prepared = PrepareMessage(ctx, form);
                var attachmentResult = TryBuildAttachments(content, prepared.ScannedTags, ctx.FilesAttach);
                if (!attachmentResult.IsSuccess)
                {
                    return new JsonResult(new { Success = false, Message = attachmentResult.ErrorMessage }) { StatusCode = StatusCodes.Status400BadRequest };
                }

                var message = new EmailMessage(
                    prepared.MailFrom,
                    prepared.MailTo?.Split(","),
                    prepared.Cc?.Split(","),
                    prepared.Bcc?.Split(","),
                    prepared.ReplyTo?.Split(","),
                    prepared.Subject,
                    prepared.Body,
                    ctx.IsHtml,
                    attachmentResult.Attachments);

                await _emailSender.SendAsync(message, "");
                _eventAggregator.Publish(new UFormEmailSentNotification(form, prepared.Body));

                if (ctx.StoreData)
                {
                    try
                    {
                        InsertSubmission(prepared.Body, content.GetUdi().Guid);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error happend while inserting submission: {Message}.", ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error happend while sending email: {Message}.", ex.Message);
            return new JsonResult(new { Success = false, Message = content.GetValue<string>("sendersMessageFailedToSend") }) { StatusCode = StatusCodes.Status500InternalServerError };
        }

        return new JsonResult(new { Success = true, Message = content.GetValue<string>("sendersMessageSuccess") }) { StatusCode = StatusCodes.Status200OK };
    }

    private record EmailContext(
        string? Template,
        string? FilesAttach,
        string? Subject,
        string? MailFrom,
        string? MailTo,
        string? ReplyTo,
        string? Cc,
        string? Bcc,
        string? Body,
        bool IsHtml,
        bool ExcludeBlankMailTags,
        bool ExcludeLinks,
        bool Demo,
        bool StoreData,
        Udi? RedirectUrl,
        bool UseRecaptcha,
        bool UseCloudflareTurnstile,
        bool UseHoneypot,
        string? BlockedIPs);

    private static EmailContext BuildEmailContext(global::Umbraco.Cms.Core.Models.IContent content) => new(
        Template: content.GetValue<string>("form"),
        FilesAttach: content.GetValue<string>("fileAttachments"),
        Subject: content.GetValue<string>("subject"),
        MailFrom: content.GetValue<string>("from"),
        MailTo: content.GetValue<string>("to"),
        ReplyTo: content.GetValue<string>("replyTo"),
        Cc: content.GetValue<string>("cc"),
        Bcc: content.GetValue<string>("bcc"),
        Body: content.GetValue<string>("messageBody"),
        IsHtml: content.GetValue<bool>("useHTMLContentType"),
        ExcludeBlankMailTags: content.GetValue<bool>("excludeLinesWithBlankMailTagsFromOutput"),
        ExcludeLinks: content.GetValue<bool>("excludeLinks"),
        Demo: content.GetValue<bool>("demoMode"),
        StoreData: !content.GetValue<bool>("doNotStoreData"),
        RedirectUrl: content.GetValue<Udi>("redirectToPage"),
        UseRecaptcha: content.GetValue<bool>("useRecaptcha"),
        UseCloudflareTurnstile: content.GetValue<bool>("useCloudflareTurnstile"),
        UseHoneypot: content.GetValue<bool>("useHoneypot"),
        BlockedIPs: content.GetValue<string>("blockIPAddresses"));

    private record PreparedMessage(
        string Body,
        string? Subject,
        string? MailFrom,
        string? MailTo,
        string? ReplyTo,
        string? Cc,
        string? Bcc,
        List<ScannedTag> ScannedTags);

    private PreparedMessage PrepareMessage(EmailContext ctx, IFormCollection form)
    {
        var scannedTags = new List<ScannedTag>();
        var template = ctx.Template ?? string.Empty;
        var body = ctx.Body ?? string.Empty;
        var subject = ctx.Subject;
        var mailFrom = ctx.MailFrom;
        var mailTo = ctx.MailTo;
        var replyTo = ctx.ReplyTo;
        var cc = ctx.Cc;
        var bcc = ctx.Bcc;

        var regex = new Regex(TagScanner.tagRegex());
        foreach (Match tag in regex.Matches(template))
        {
            scannedTags.Add(TagScanner.PopulateTag(tag));
        }

        foreach (var key in form.Keys)
        {
            var tag = $"[{key}]";
            var value = form[key].ToString();

            if (body.Contains(tag))
            {
                if (string.IsNullOrEmpty(value) && ctx.ExcludeBlankMailTags)
                {
                    var bodyLines = body.Split("\n").ToList();
                    var linesWithTag = bodyLines.Where(x => x.Contains(tag)).ToList();
                    foreach (var line in linesWithTag)
                    {
                        body = body.Replace(line + "\n", "");
                    }
                }
                body = body.Replace(tag, value);
            }

            subject = subject?.Replace(tag, value);
            mailFrom = mailFrom?.Replace(tag, value);
            mailTo = mailTo?.Replace(tag, value);
            replyTo = replyTo?.Replace(tag, value);
            cc = cc?.Replace(tag, value);
            bcc = bcc?.Replace(tag, value);
        }

        body = ReplaceSpecialMailTags(body);

        if (ctx.ExcludeLinks)
        {
            body = Regex.Replace(body, @"https?://[^\s]+", "");
        }

        return new PreparedMessage(body, subject, mailFrom, mailTo, replyTo, cc, bcc, scannedTags);
    }

    private record AttachmentBuildResult(bool IsSuccess, string ErrorMessage, List<EmailMessageAttachment>? Attachments);

    private AttachmentBuildResult TryBuildAttachments(global::Umbraco.Cms.Core.Models.IContent content, List<ScannedTag> scannedTags, string? filesAttach)
    {
        var fileList = string.IsNullOrEmpty(filesAttach)
            ? new List<string>()
            : filesAttach.Split("\n").ToList();

        if (!fileList.Any())
        {
            return new AttachmentBuildResult(true, string.Empty, null);
        }

        var attachments = new List<EmailMessageAttachment>();
        try
        {
            var files = Request.Form.Files;
            foreach (var file in files)
            {
                if (!fileList.Contains($"[{file.Name}]"))
                {
                    continue;
                }

                fileList.Remove($"[{file.Name}]");
                var scannedTag = scannedTags.FirstOrDefault(x => x.RawName == file.Name);
                if (scannedTag == null)
                {
                    continue;
                }

                var allowedTypes = scannedTag.GetOption("filetypes", "", true);
                var fileExtension = Path.GetExtension(file.FileName).Replace(".", "");
                if (allowedTypes != null && !((string)allowedTypes).Contains(fileExtension))
                {
                    return new AttachmentBuildResult(false, content.GetValue<string>("uploadedFileIsNotAllowedForFileType") ?? "Disallowed file type.", null);
                }

                var fileLimit = scannedTag.GetLimitOption();
                if (file.Length > fileLimit)
                {
                    return new AttachmentBuildResult(false, content.GetValue<string>("uploadedFileIsTooLarge") ?? "File too large.", null);
                }

                using var ms = new MemoryStream();
                file.CopyTo(ms);
                attachments.Add(new EmailMessageAttachment(new MemoryStream(ms.ToArray()), file.FileName));
            }

            foreach (var item in fileList)
            {
                var filePath = Path.IsPathFullyQualified(item) ? item : _hostingEnvironment.MapPathWebRoot(item);
                if (System.IO.File.Exists(filePath))
                {
                    attachments.Add(new EmailMessageAttachment(new MemoryStream(System.IO.File.ReadAllBytes(filePath)), Path.GetFileName(filePath)));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error happend while uploading file: {Message}.", ex.Message);
            return new AttachmentBuildResult(false, content.GetValue<string>("uploadingAFileFailsForAnyReason") ?? "File upload failed.", null);
        }

        return new AttachmentBuildResult(true, string.Empty, attachments);
    }

    private string ReplaceSpecialMailTags(string text) =>
        text.Replace("[_remote_ip]", HttpContext.Connection.RemoteIpAddress?.ToString())
            .Replace("[_user_agent]", Request.Headers["User-Agent"].ToString())
            .Replace("[_date]", DateTime.Now.Date.ToString("yyyy-MM-dd"))
            .Replace("[_time]", DateTime.Now.ToString("hh:mm:ss"))
            .Replace("[_url]", Request.GetDisplayUrl());

    private void InsertSubmission(string message, Guid parentGuid)
    {
        var submission = _ctService.Create(Guid.NewGuid().ToString(), parentGuid, "uFormSubmission");
        submission.SetValue("message", message.Replace("\r\n", "<br />").Replace("\n", "<br />"));
        submission.SetValue("dateAndTimeOfAMessage", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        _ctService.Save(submission);
        _ctService.Publish(submission, Array.Empty<string>());
    }

    private static bool IsIPBlocked(string currentIP, List<string> blockedIPs)
    {
        foreach (var blockedIP in blockedIPs)
        {
            if (blockedIP.Contains('*'))
            {
                var pattern = "^" + blockedIP.Replace(".", "\\.").Replace("*", ".*") + "$";
                if (Regex.IsMatch(currentIP, pattern))
                {
                    return true;
                }
            }
            else if (currentIP == blockedIP)
            {
                return true;
            }
        }
        return false;
    }
}
