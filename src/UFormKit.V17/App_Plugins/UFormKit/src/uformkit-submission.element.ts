import { css, html } from "@umbraco-cms/backoffice/external/lit";
import { customElement, property } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import type {
  UmbPropertyEditorUiElement,
  UmbPropertyEditorConfigCollection,
} from "@umbraco-cms/backoffice/property-editor";

/**
 * Read-only display for a stored UForm submission.
 *
 * The submission body is the HTML mail body (see UFormController.InsertSubmission), so it
 * carries <br /> separators. Umbraco 13's label editor rendered it with ng-bind-html; the
 * v14+ Umb.PropertyEditorUi.Label escapes instead, which surfaces the tags as text.
 *
 * The body is written by an anonymous site visitor, so it is NOT rendered as HTML — that
 * would execute submitted markup in the backoffice. Everything is escaped and only line
 * breaks are honoured, which reproduces what the v13 editor showed without the injection path.
 */
@customElement("uformkit-submission-message")
export class UFormKitSubmissionMessageElement
  extends UmbLitElement
  implements UmbPropertyEditorUiElement
{
  @property({ type: String })
  value: string = "";

  @property({ attribute: false })
  config?: UmbPropertyEditorConfigCollection;

  #toLines(raw: string): string[] {
    // Only <br> is treated as markup; every other character is rendered verbatim by Lit.
    return (raw ?? "").split(/<br\s*\/?>/gi);
  }

  render() {
    const lines = this.#toLines(this.value);
    return html`<div class="value">${lines.map((line, i) => (i === 0 ? line : html`<br />${line}`))}</div>`;
  }

  static styles = css`
    :host {
      display: block;
    }
    .value {
      white-space: pre-wrap;
      overflow-wrap: anywhere;
    }
  `;
}

declare global {
  interface HTMLElementTagNameMap {
    "uformkit-submission-message": UFormKitSubmissionMessageElement;
  }
}

export default UFormKitSubmissionMessageElement;
