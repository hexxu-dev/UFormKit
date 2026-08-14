import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import type { UmbApi } from "@umbraco-cms/backoffice/extension-api";
import { observeMultiple } from "@umbraco-cms/backoffice/observable-api";
import {
  UMB_CONTENT_WORKSPACE_CONTEXT,
  type UmbContentWorkspaceContext,
} from "@umbraco-cms/backoffice/content";

/**
 * Document type alias whose new nodes should be seeded with defaults.
 */
const CONTENT_TYPE_ALIAS = "uFormHexxu";

/**
 * The starter form template inserted into a brand-new form.
 */
const DEFAULT_TEMPLATE =
  '<div> Your name\r\n    [text* your-name] </div>\r\n\r\n<div> Your email\r\n    [email* your-email] </div>\r\n\r\n<div> Subject\r\n    [text* your-subject] </div>\r\n\r\n<div> Your message (optional)\r\n    [textarea your-message] </div>\r\n\r\n[submit "Submit"]';

/**
 * Property alias -> default value.
 *
 * `from` is intentionally omitted: it is derived from the server-side SMTP
 * settings, which are not available client-side. Umbraco's EmailSender falls
 * back to the configured SMTP From when the message From is empty, so an empty
 * `from` property still sends correctly.
 */
const DEFAULT_VALUES: Record<string, string> = {
  sendersMessageSuccess: "Thank you for your message. It has been sent.",
  sendersMessageFailedToSend:
    "There was an error trying to send your message. Please try again later.",
  validationErrorsOccurred:
    "One or more fields have an error. Please check and try again.",
  thereAreTermsThatTheSenderMustAccept:
    "You must accept the terms and conditions before sending your message.",
  thereIsAFieldThatTheSenderMustFillIn: "The field is required.",
  thereIsAFieldWithInputThatIsLongerThanTheMaximumAllowedLength:
    "The field is too long.",
  thereIsAFieldWithInputThatIsShorterThanTheMinimumAllowedLength:
    "The field is too short.",
  uploadingAFileFailsForAnyReason:
    "There was an unknown error uploading the file.",
  uploadedFileIsNotAllowedForFileType:
    "You are not allowed to upload files of this type.",
  uploadedFileIsTooLarge: "The file is too big.",
  dateFormatThatTheSenderEnteredIsInvalid: "The date format is incorrect.",
  dateIsEarlierThanMinimumLimit: "The date is before the earliest one allowed.",
  dateIsLaterThanMaximumLimit: "The date is after the latest one allowed.",
  numberFormatThatTheSenderEnteredIsInvalid: "The number format is invalid.",
  numberIsSmallerThanMinimumLimit:
    "The number is smaller than the minimum allowed.",
  numberIsLargerThanMaximumLimit:
    "The number is larger than the maximum allowed.",
  emailAddressThatTheSenderEnteredIsInvalid:
    "The e-mail address entered is invalid.",
  uRLThatTheSenderEnteredIsInvalid: "The URL is invalid.",
  telephoneNumberThatTheSenderEnteredIsInvalid:
    "The telephone number is invalid.",
  form: DEFAULT_TEMPLATE,
  subject: "[your-subject]",
  replyTo: "[your-email]",
  messageBody:
    "From: [your-name] [your-email]\r\nSubject: [your-subject]\r\n\r\nMessage Body:\r\n[your-message]\r\n",
};

/**
 * Workspace context that seeds default property values on a brand-new
 * `uFormHexxu` node, restoring the create-time defaults that the V13
 * `SendingContentNotification` handler used to provide (the notification was
 * removed in Umbraco 14+).
 */
/**
 * Marker for "this instance has not seeded any document yet". A symbol is used
 * rather than `undefined` so that it can never collide with a real `unique`.
 */
const NOT_SEEDED = Symbol("uformkit-not-seeded");

export class UFormKitDefaultsWorkspaceContext
  extends UmbControllerBase
  implements UmbApi
{
  #workspace?: UmbContentWorkspaceContext;

  /**
   * The `unique` of the document this instance last seeded.
   *
   * Keyed on the document, NOT on a plain boolean: the backoffice reuses one
   * workspace context across successive documents, so a "have I run yet" flag
   * stays true forever and only the first new form gets its defaults. Every new
   * node gets a client-side `unique` up front (`createScaffold` -> `setUnique`),
   * so comparing against it re-arms exactly once per document.
   */
  #seededUnique: string | null | undefined | typeof NOT_SEEDED = NOT_SEEDED;

  constructor(host: UmbControllerHost) {
    super(host);

    this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (workspace) => {
      // Re-arm for each new workspace instance the extension is attached to.
      if (this.#workspace === workspace) {
        return;
      }
      this.#workspace = workspace ?? undefined;
      this.#seededUnique = NOT_SEEDED;

      if (!workspace) {
        return;
      }

      // `unique` must be part of the observed tuple. Between one new form and
      // the next, `isNew` and the alias both stay unchanged (true / uFormHexxu),
      // so observing only those two never emits again for the second document.
      this.observe(
        observeMultiple([
          workspace.isNew,
          workspace.structure.ownerContentTypeAlias,
          workspace.unique,
        ]),
        ([isNew, alias, unique]) => {
          if (!isNew || alias !== CONTENT_TYPE_ALIAS) {
            return;
          }
          if (unique === this.#seededUnique) {
            return;
          }
          this.#seededUnique = unique;
          this.#applyDefaults();
        },
        "uformkitDefaultsObserver",
      );
    });
  }

  #applyDefaults() {
    const workspace = this.#workspace;
    if (!workspace) {
      return;
    }

    for (const [alias, value] of Object.entries(DEFAULT_VALUES)) {
      const current = workspace.getPropertyValue<string>(alias);
      if (current === undefined || current === null || current === "") {
        workspace.setPropertyValue(alias, value);
      }
    }
  }
}

export { UFormKitDefaultsWorkspaceContext as api };
export default UFormKitDefaultsWorkspaceContext;
