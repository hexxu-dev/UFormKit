using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Cache;
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
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.IO;
using System;
using Microsoft.AspNetCore.Http.Extensions;

namespace UFormKit.Controller
{
    [PluginController("UFormKit")]
    public class UFormController : SurfaceController
    {

        private IContentService ctService;
        private IEmailSender emailSender;
        private CaptchaVerificationService verificationService;
        private ILogger<UFormController> logger;
        private Umbraco.Cms.Core.Hosting.IHostingEnvironment hostingEnvironment;
        private IUmbracoContextAccessor contextAccessor;


        public UFormController(IUmbracoContextAccessor contextAccessor, Umbraco.Cms.Core.Hosting.IHostingEnvironment hostingEnvironment, ILogger<UFormController> logger, CaptchaVerificationService verificationService, IEmailSender emailSender, IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, IContentService ctService) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            this.ctService = ctService;
            this.emailSender = emailSender;
            this.verificationService = verificationService;
            this.logger = logger;
            this.hostingEnvironment = hostingEnvironment;
            this.contextAccessor = contextAccessor;
        }

        [HttpPost]
        public async Task<IActionResult> SendAsync(IFormCollection form)
        {
            var recaptcha = form["g-recaptcha-response"];

            if (!string.IsNullOrEmpty(recaptcha))
            {
                var isValid = await verificationService.IsCaptchaValid(recaptcha);
                if (!isValid)
                {
                    ModelState.AddModelError("", "Problem while validating recaptcha. Try again!");
                    return CurrentUmbracoPage();
                }
            }


            var scannedTags = new List<ScannedTag>();
            var formId = int.Parse(form["formId"]);

            var content = ctService.GetById(formId);
            var template = content.GetValue<string>("form");
            var filesAttach = content.GetValue<string>("fileAttachments");
            var subject = content.GetValue<string>("subject");
            var mailFrom = content.GetValue<string>("from");
            var mailTo = content.GetValue<string>("to");
            var replyTo = content.GetValue<string>("replyTo");
            var cc = content.GetValue<string>("cc");
            var bcc = content.GetValue<string>("bcc");
            var body = content.GetValue<string>("messageBody");
            var isHtml = content.GetValue<bool>("useHTMLContentType");
            var excludeBlankMailTags = content.GetValue<bool>("excludeLinesWithBlankMailTagsFromOutput");
            var demo = content.GetValue<bool>("demoMode");
            var redirectUrl = content.GetValue<Umbraco.Cms.Core.Udi>("redirectToPage");

            var fileList = new List<string>();
            if (!string.IsNullOrEmpty(filesAttach))
            {
                fileList = filesAttach.Split("\n").ToList();
            }

            try
            {
                if (!demo)
                {
                    Regex regex = new Regex(TagScanner.tagRegex());
                    var tags = regex.Matches(template);

                    foreach (Match tag in tags)
                    {
                        scannedTags.Add(TagScanner.PopulateTag(tag));
                    }


                    foreach (var key in form.Keys)
                    {
                        var tag = $"[{key}]";

                        if (body.Contains(tag))
                        {
                            var value = form[key];
                            if (string.IsNullOrEmpty(form[key]) && excludeBlankMailTags)
                            {
                                var bodyLines = body.Split("\n").ToList();
                                var linesWithTag = bodyLines.Select(x => x).Where(x => x.Contains(tag)).ToList();
                                foreach (var line in linesWithTag)
                                {
                                    body = body.Replace(line + "\n", "");
                                }
                            }

                            body = body.Replace(tag, form[key]);
                        }

                        if (subject != null)
                        {
                            subject = subject.Replace(tag, form[key]);
                        }

                        if (mailFrom != null)
                        {
                            mailFrom = mailFrom.Replace(tag, form[key]);
                        }

                        if (mailTo != null)
                        {
                            mailTo = mailTo.Replace(tag, form[key]);
                        }

                        if (replyTo != null)
                        {
                            replyTo = replyTo.Replace(tag, form[key]);
                        }
                        if (cc != null)
                        {
                            cc = cc.Replace(tag, form[key]);
                        }
                        if (bcc != null)
                        {
                            bcc = bcc.Replace(tag, form[key]);
                        }

                    }

                    body = replaceSpecialMailTags(body);
                    EmailMessage message;
                    List<EmailMessageAttachment> attachments = null;
                    var files = Request.Form.Files;
                    try
                    {
                        if (fileList.Any())
                        {
                            attachments = new List<EmailMessageAttachment>();
                            if (files.Any())
                            {
                                foreach (var file in files)
                                {
                                    if (fileList.Contains($"[{file.Name}]"))
                                    {
                                        fileList.Remove($"[{file.Name}]");
                                        var scannedTag = scannedTags.FirstOrDefault(x => x.RawName == file.Name);
                                        var alowedTypes = scannedTag.GetOption("filetypes", "", true);

                                        var fileExtension = Path.GetExtension(file.FileName).Replace(".", "");
                                        if (alowedTypes != null && !((string)alowedTypes).Contains(fileExtension))
                                        {
                                            ModelState.AddModelError(file.Name, content.GetValue<string>("uploadedFileIsNotAllowedForFileType"));
                                            return CurrentUmbracoPage();
                                        }

                                        var fileLimit = scannedTag.GetLimitOption();

                                        if (file.Length > fileLimit)
                                        {
                                            ModelState.AddModelError(file.Name, content.GetValue<string>("uploadedFileIsTooLarge"));
                                            return CurrentUmbracoPage();
                                        }

                                        using (var ms = new MemoryStream())
                                        {
                                            file.CopyTo(ms);
                                            var fileBytes = ms.ToArray();
                                            EmailMessageAttachment att = new EmailMessageAttachment(new MemoryStream(fileBytes), file.FileName);
                                            attachments.Add(att);
                                        }
                                    }
                                }
                            }

                            foreach (var item in fileList)
                            {
                                var filePath = Path.IsPathFullyQualified(item) ? item : hostingEnvironment.MapPathWebRoot(item);
                                if (System.IO.File.Exists(filePath))
                                {
                                    attachments.Add(new EmailMessageAttachment(new MemoryStream(System.IO.File.ReadAllBytes(filePath)), Path.GetFileName(filePath)));
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Error happend while uploading file: {ex.Message}.");
                        ModelState.AddModelError("", content.GetValue<string>("uploadingAFileFailsForAnyReason"));
                        return CurrentUmbracoPage();
                    }
                    message = new EmailMessage(mailFrom, mailTo?.Split(","), cc?.Split(","), bcc?.Split(","), replyTo?.Split(","), subject, body, isHtml, attachments);

                    await emailSender.SendAsync(message, "");
                }

                TempData["success"] = content.GetValue<string>("sendersMessageSuccess");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error happend while sending email: {ex.Message}.");
                ModelState.AddModelError("", content.GetValue<string>("sendersMessageFailedToSend"));
                return CurrentUmbracoPage();
            }

            if (redirectUrl != null && contextAccessor.GetRequiredUmbracoContext().Content != null)
            {
                var page = contextAccessor.GetRequiredUmbracoContext().Content.GetById(redirectUrl);
                if (page != null)
                {
                    return RedirectToUmbracoPage(page);
                }
            }

            return RedirectToCurrentUmbracoPage();

        }

        private string replaceSpecialMailTags(string text)
        {
            return text.Replace("[_remote_ip]", HttpContext.Connection.RemoteIpAddress?.ToString())
                .Replace("[_user_agent]", Request.Headers["User-Agent"].ToString())
                .Replace("[_date]", DateTime.Now.Date.ToString("yyyy-MM-dd"))
                .Replace("[_time]", DateTime.Now.ToString("hh:mm:ss"))
                .Replace("[_url]", Request.GetDisplayUrl());
        }



    }
}

