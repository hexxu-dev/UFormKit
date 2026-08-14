var f = (e) => {
  throw TypeError(e);
};
var h = (e, t, r) => t.has(e) || f("Cannot " + r);
var o = (e, t, r) => (h(e, t, "read from private field"), r ? r.call(e) : t.get(e)), l = (e, t, r) => t.has(e) ? f("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, r), d = (e, t, r, i) => (h(e, t, "write to private field"), i ? i.call(e, r) : t.set(e, r), r), c = (e, t, r) => (h(e, t, "access private method"), r);
import { UmbControllerBase as I } from "@umbraco-cms/backoffice/class-api";
import { observeMultiple as g } from "@umbraco-cms/backoffice/observable-api";
import { UMB_CONTENT_WORKSPACE_CONTEXT as p } from "@umbraco-cms/backoffice/content";
const v = "uFormHexxu", E = `<div> Your name\r
    [text* your-name] </div>\r
\r
<div> Your email\r
    [email* your-email] </div>\r
\r
<div> Subject\r
    [text* your-subject] </div>\r
\r
<div> Your message (optional)\r
    [textarea your-message] </div>\r
\r
[submit "Submit"]`, A = {
  sendersMessageSuccess: "Thank you for your message. It has been sent.",
  sendersMessageFailedToSend: "There was an error trying to send your message. Please try again later.",
  validationErrorsOccurred: "One or more fields have an error. Please check and try again.",
  thereAreTermsThatTheSenderMustAccept: "You must accept the terms and conditions before sending your message.",
  thereIsAFieldThatTheSenderMustFillIn: "The field is required.",
  thereIsAFieldWithInputThatIsLongerThanTheMaximumAllowedLength: "The field is too long.",
  thereIsAFieldWithInputThatIsShorterThanTheMinimumAllowedLength: "The field is too short.",
  uploadingAFileFailsForAnyReason: "There was an unknown error uploading the file.",
  uploadedFileIsNotAllowedForFileType: "You are not allowed to upload files of this type.",
  uploadedFileIsTooLarge: "The file is too big.",
  dateFormatThatTheSenderEnteredIsInvalid: "The date format is incorrect.",
  dateIsEarlierThanMinimumLimit: "The date is before the earliest one allowed.",
  dateIsLaterThanMaximumLimit: "The date is after the latest one allowed.",
  numberFormatThatTheSenderEnteredIsInvalid: "The number format is invalid.",
  numberIsSmallerThanMinimumLimit: "The number is smaller than the minimum allowed.",
  numberIsLargerThanMaximumLimit: "The number is larger than the maximum allowed.",
  emailAddressThatTheSenderEnteredIsInvalid: "The e-mail address entered is invalid.",
  uRLThatTheSenderEnteredIsInvalid: "The URL is invalid.",
  telephoneNumberThatTheSenderEnteredIsInvalid: "The telephone number is invalid.",
  form: E,
  subject: "[your-subject]",
  replyTo: "[your-email]",
  messageBody: `From: [your-name] [your-email]\r
Subject: [your-subject]\r
\r
Message Body:\r
[your-message]\r
`
}, y = Symbol("uformkit-not-seeded");
var n, s, u, b;
class x extends I {
  constructor(r) {
    super(r);
    l(this, u);
    l(this, n);
    /**
     * The `unique` of the document this instance last seeded.
     *
     * Keyed on the document, NOT on a plain boolean: the backoffice reuses one
     * workspace context across successive documents, so a "have I run yet" flag
     * stays true forever and only the first new form gets its defaults. Every new
     * node gets a client-side `unique` up front (`createScaffold` -> `setUnique`),
     * so comparing against it re-arms exactly once per document.
     */
    l(this, s, y);
    this.consumeContext(p, (i) => {
      o(this, n) !== i && (d(this, n, i ?? void 0), d(this, s, y), i && this.observe(
        g([
          i.isNew,
          i.structure.ownerContentTypeAlias,
          i.unique
        ]),
        ([m, a, T]) => {
          !m || a !== v || T !== o(this, s) && (d(this, s, T), c(this, u, b).call(this));
        },
        "uformkitDefaultsObserver"
      ));
    });
  }
}
n = new WeakMap(), s = new WeakMap(), u = new WeakSet(), b = function() {
  const r = o(this, n);
  if (r)
    for (const [i, m] of Object.entries(A)) {
      const a = r.getPropertyValue(i);
      (a == null || a === "") && r.setPropertyValue(i, m);
    }
};
export {
  x as UFormKitDefaultsWorkspaceContext,
  x as api,
  x as default
};
//# sourceMappingURL=uformkit-defaults.context.js.map
