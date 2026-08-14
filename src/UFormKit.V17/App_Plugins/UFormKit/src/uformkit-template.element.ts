import { LitElement, css, html } from "@umbraco-cms/backoffice/external/lit";
import { customElement, property, state, query } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import type {
  UmbPropertyEditorUiElement,
  UmbPropertyEditorConfigCollection,
} from "@umbraco-cms/backoffice/property-editor";
import {
  TAG_DEFINITIONS,
  TagDefinition,
  TagFormValues,
  buildTagString,
  makeInitialValues,
} from "./tag-definitions";

@customElement("uformkit-template-editor")
export class UFormKitTemplateEditorElement
  extends UmbLitElement
  implements UmbPropertyEditorUiElement
{
  @property({ type: String })
  value: string = "";

  @property({ attribute: false })
  config?: UmbPropertyEditorConfigCollection;

  @state()
  private _activeTag: TagDefinition | null = null;

  @state()
  private _form: TagFormValues = makeInitialValues("text");

  @state()
  private _preview: string = "";

  @query("#template-textarea")
  private _textarea?: HTMLTextAreaElement;

  private _onValueChange(e: Event) {
    const next = (e.target as HTMLTextAreaElement).value;
    this.value = next;
    this.dispatchEvent(new CustomEvent("change", { bubbles: true, composed: true }));
  }

  private _openBuilder(tag: TagDefinition) {
    this._activeTag = tag;
    this._form = makeInitialValues(tag.type);
    this._preview = buildTagString(this._form);
  }

  private _closeBuilder() {
    this._activeTag = null;
  }

  private _updateForm<K extends keyof TagFormValues>(key: K, value: TagFormValues[K]) {
    this._form = { ...this._form, [key]: value };
    this._preview = buildTagString(this._form);
  }

  private _insertTag() {
    if (!this._textarea) {
      return;
    }
    const current = this.value ?? "";
    const start = this._textarea.selectionStart ?? current.length;
    const end = this._textarea.selectionEnd ?? start;
    const before = current.slice(0, start);
    const after = current.slice(end);
    this.value = `${before}${this._preview}${after}`;
    this.dispatchEvent(new CustomEvent("change", { bubbles: true, composed: true }));
    this._closeBuilder();
  }

  private _renderField(field: string) {
    const f = this._form;
    switch (field) {
      case "name":
        return html`<label>Name <input .value=${f.name} @input=${(e: InputEvent) => this._updateForm("name", (e.target as HTMLInputElement).value)} /></label>`;
      case "required":
        return html`<label><input type="checkbox" .checked=${f.required} @change=${(e: Event) => this._updateForm("required", (e.target as HTMLInputElement).checked)} /> Required</label>`;
      case "id":
        return html`<label>ID <input .value=${f.id} @input=${(e: InputEvent) => this._updateForm("id", (e.target as HTMLInputElement).value)} /></label>`;
      case "class":
        return html`<label>Class <input .value=${f.className} @input=${(e: InputEvent) => this._updateForm("className", (e.target as HTMLInputElement).value)} /></label>`;
      case "placeholder":
        return html`<label><input type="checkbox" .checked=${f.placeholder} @change=${(e: Event) => this._updateForm("placeholder", (e.target as HTMLInputElement).checked)} /> Use as placeholder</label>`;
      case "value":
        return html`<label>Default value <input .value=${f.value} @input=${(e: InputEvent) => this._updateForm("value", (e.target as HTMLInputElement).value)} /></label>`;
      case "size":
        return html`<label>Size <input type="number" .value=${String(f.size)} @input=${(e: InputEvent) => this._updateForm("size", Number((e.target as HTMLInputElement).value))} /></label>`;
      case "minNum":
        return html`<label>Min <input type="number" .value=${f.minNum?.toString() ?? ""} @input=${(e: InputEvent) => this._updateForm("minNum", (e.target as HTMLInputElement).value ? Number((e.target as HTMLInputElement).value) : null)} /></label>`;
      case "maxNum":
        return html`<label>Max <input type="number" .value=${f.maxNum?.toString() ?? ""} @input=${(e: InputEvent) => this._updateForm("maxNum", (e.target as HTMLInputElement).value ? Number((e.target as HTMLInputElement).value) : null)} /></label>`;
      case "minDate":
        return html`<label>Min date <input type="date" .value=${f.minDate ?? ""} @input=${(e: InputEvent) => this._updateForm("minDate", (e.target as HTMLInputElement).value || null)} /></label>`;
      case "maxDate":
        return html`<label>Max date <input type="date" .value=${f.maxDate ?? ""} @input=${(e: InputEvent) => this._updateForm("maxDate", (e.target as HTMLInputElement).value || null)} /></label>`;
      case "limit":
        return html`<label>Limit (bytes) <input type="number" .value=${f.limit?.toString() ?? ""} @input=${(e: InputEvent) => this._updateForm("limit", (e.target as HTMLInputElement).value ? Number((e.target as HTMLInputElement).value) : null)} /></label>`;
      case "filetypes":
        return html`<label>File types <input .value=${f.filetypes} placeholder="jpg|png|pdf" @input=${(e: InputEvent) => this._updateForm("filetypes", (e.target as HTMLInputElement).value)} /></label>`;
      case "rows":
        return html`<label>Rows <input type="number" .value=${String(f.rows)} @input=${(e: InputEvent) => this._updateForm("rows", Number((e.target as HTMLInputElement).value))} /></label>`;
      case "cols":
        return html`<label>Cols <input type="number" .value=${String(f.cols)} @input=${(e: InputEvent) => this._updateForm("cols", Number((e.target as HTMLInputElement).value))} /></label>`;
      case "options":
        return html`<label>Options (one per line) <textarea rows="5" .value=${f.options} @input=${(e: InputEvent) => this._updateForm("options", (e.target as HTMLTextAreaElement).value)}></textarea></label>`;
      case "multiple":
        return html`<label><input type="checkbox" .checked=${f.multiple} @change=${(e: Event) => this._updateForm("multiple", (e.target as HTMLInputElement).checked)} /> Multiple</label>`;
      case "includeBlank":
        return html`<label><input type="checkbox" .checked=${f.includeBlank} @change=${(e: Event) => this._updateForm("includeBlank", (e.target as HTMLInputElement).checked)} /> Include blank option</label>`;
      case "labelFirst":
        return html`<label><input type="checkbox" .checked=${f.labelFirst} @change=${(e: Event) => this._updateForm("labelFirst", (e.target as HTMLInputElement).checked)} /> Label first</label>`;
      case "useLabelElement":
        return html`<label><input type="checkbox" .checked=${f.useLabelElement} @change=${(e: Event) => this._updateForm("useLabelElement", (e.target as HTMLInputElement).checked)} /> Wrap in <code>&lt;label&gt;</code></label>`;
      case "exclusive":
        return html`<label><input type="checkbox" .checked=${f.exclusive} @change=${(e: Event) => this._updateForm("exclusive", (e.target as HTMLInputElement).checked)} /> Exclusive</label>`;
      case "labelSub":
        return html`<label>Button / link label <input .value=${f.labelSub} @input=${(e: InputEvent) => this._updateForm("labelSub", (e.target as HTMLInputElement).value)} /></label>`;
      case "condition":
        return html`<label>Acceptance condition <input .value=${f.condition} @input=${(e: InputEvent) => this._updateForm("condition", (e.target as HTMLInputElement).value)} /></label>`;
      case "optional":
        return html`<label><input type="checkbox" .checked=${f.optional} @change=${(e: Event) => this._updateForm("optional", (e.target as HTMLInputElement).checked)} /> Optional</label>`;
      case "numType":
        return html`<label>Sub-type
            <select .value=${f.numType} @change=${(e: Event) => this._updateForm("numType", (e.target as HTMLSelectElement).value)}>
              <option value="">number</option>
              <option value="range">range</option>
            </select>
          </label>`;
      case "query":
        return html`<label>SQL query (for select/checkbox/radio) <input .value=${f.query} @input=${(e: InputEvent) => this._updateForm("query", (e.target as HTMLInputElement).value)} /></label>`;
      default:
        return null;
    }
  }

  private _renderBuilder() {
    if (!this._activeTag) {
      return null;
    }
    return html`
      <div class="overlay" @click=${this._closeBuilder}>
        <div class="dialog" @click=${(e: Event) => e.stopPropagation()}>
          <header>
            <h3>Form-tag Generator: ${this._activeTag.name}</h3>
            <button type="button" @click=${this._closeBuilder}>×</button>
          </header>
          <div class="fields">
            ${this._activeTag.fields.map((f) => this._renderField(f))}
          </div>
          <pre class="preview">${this._preview}</pre>
          <footer>
            <button type="button" @click=${this._closeBuilder}>Cancel</button>
            <button type="button" class="primary" @click=${this._insertTag}>Insert tag</button>
          </footer>
        </div>
      </div>
    `;
  }

  render() {
    return html`
      <div class="tag-list">
        ${TAG_DEFINITIONS.map(
          (tag) => html`<button type="button" class="tag-chip" @click=${() => this._openBuilder(tag)}>${tag.name}</button>`,
        )}
      </div>
      <textarea
        id="template-textarea"
        rows="20"
        .value=${this.value ?? ""}
        @input=${this._onValueChange}
      ></textarea>
      ${this._renderBuilder()}
    `;
  }

  static styles = css`
    :host {
      display: block;
    }
    .tag-list {
      display: flex;
      flex-wrap: wrap;
      gap: 4px;
      margin-bottom: 8px;
    }
    .tag-chip {
      background: var(--uui-color-surface-alt, #f3f3f5);
      border: 1px solid var(--uui-color-border, #d8d7d9);
      border-radius: 3px;
      padding: 4px 8px;
      cursor: pointer;
      font: inherit;
    }
    .tag-chip:hover {
      background: var(--uui-color-selected, #1b264f);
      color: #fff;
    }
    textarea {
      width: 100%;
      box-sizing: border-box;
      font-family: var(--uui-font-monospace, monospace);
      padding: 8px;
      border: 1px solid var(--uui-color-border, #d8d7d9);
      border-radius: 3px;
    }
    .overlay {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.45);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 100;
    }
    .dialog {
      background: var(--uui-color-surface, #fff);
      border-radius: 6px;
      width: min(600px, 92vw);
      max-height: 86vh;
      overflow: auto;
      padding: 16px;
      display: flex;
      flex-direction: column;
      gap: 12px;
    }
    .dialog header {
      display: flex;
      align-items: center;
      justify-content: space-between;
    }
    .dialog header button {
      background: none;
      border: none;
      font-size: 24px;
      cursor: pointer;
    }
    .dialog .fields {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }
    .dialog .fields label {
      display: flex;
      flex-direction: column;
      font-size: 13px;
      gap: 4px;
    }
    /* Column layout is right for "Label above input", but puts a checkbox on its own
       line above its text. Lay those out inline instead. */
    .dialog .fields label:has(> input[type="checkbox"]) {
      flex-direction: row;
      align-items: center;
      gap: 8px;
    }
    .dialog .preview {
      background: var(--uui-color-surface-alt, #f3f3f5);
      padding: 8px;
      border-radius: 3px;
      white-space: pre-wrap;
      word-break: break-all;
    }
    .dialog footer {
      display: flex;
      justify-content: flex-end;
      gap: 8px;
    }
    .dialog footer button {
      padding: 6px 12px;
      border-radius: 3px;
      border: 1px solid var(--uui-color-border, #d8d7d9);
      background: var(--uui-color-surface, #fff);
      cursor: pointer;
    }
    .dialog footer button.primary {
      background: var(--uui-color-selected, #1b264f);
      color: #fff;
      border-color: transparent;
    }
  `;
}

declare global {
  interface HTMLElementTagNameMap {
    "uformkit-template-editor": UFormKitTemplateEditorElement;
  }
}

export default UFormKitTemplateEditorElement;
