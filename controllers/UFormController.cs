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

namespace UFormKit.Controller
{
	[PluginController("UFormKit")]
	public class UFormController : SurfaceController
	{

		private IContentService ctService;
		private IEmailSender emailSender;
		private CaptchaVerificationService verificationService;
		private ILogger<UFormController> logger;

		public UFormController(ILogger<UFormController> logger, CaptchaVerificationService verificationService, IEmailSender emailSender, IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, IContentService ctService) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
		{
			this.ctService = ctService;
			this.emailSender = emailSender;
			this.verificationService = verificationService;
			this.logger = logger;
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

			var fileList = new List<string>();
			if (!string.IsNullOrEmpty(filesAttach))
			{
				fileList = filesAttach.Split("\n").ToList();
			}

			try
			{
				Regex regex = new Regex(TagScanner.tagRegex());
				var tags = regex.Matches(template);

				foreach (Match tag in tags)
				{
					scannedTags.Add(TagScanner.PopulateTag(tag));
				}


                foreach (var item in scannedTags)
                {
                    var tag = $"[{item.RawName}]";
                    var value = form.ContainsKey(item.RawName) ? form[item.RawName].ToString() : "";
                    if (body.Contains(tag))
                    {
                        if (string.IsNullOrEmpty(value) && excludeBlankMailTags)
                        {
                            var bodyLines = body.Split("\n").ToList();
                            var linesWithTag = bodyLines.Select(x => x).Where(x => x.Contains(tag)).ToList();
                            foreach (var line in linesWithTag)
                            {
                                body = body.Replace(line + "\n", "");
                            }
                        }

                        body = body.Replace(tag, value);
                    }

                    if (subject != null && subject.Contains(tag))
                    {
                        subject = subject.Replace(tag, value);
                    }

                    if (mailFrom != null && mailFrom.Contains(tag))
                    {
                        mailFrom = mailFrom.Replace(tag, value);
                    }

                    if (mailTo != null && mailTo.Contains(tag))
                    {
                        mailTo = mailTo.Replace(tag, value);
                    }

                    if (replyTo != null && replyTo.Contains(tag))
                    {
                        replyTo = replyTo.Replace(tag, value);
                    }
                    if (cc != null && cc.Contains(tag))
                    {
                        cc = cc.Replace(tag, value);
                    }
                    if (bcc != null && bcc.Contains(tag))
                    {
                        bcc = bcc.Replace(tag, value);
                    }

                }
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

				TempData["success"] = content.GetValue<string>("sendersMessageSuccess");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, $"Error happend while sending email: {ex.Message}.");
				ModelState.AddModelError("", content.GetValue<string>("sendersMessageFailedToSend"));
				return CurrentUmbracoPage();
			}
			return RedirectToCurrentUmbracoPage();
		}



	}
}
