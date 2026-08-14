import { css as b, property as m, state as c, query as f, customElement as h, html as i } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as g } from "@umbraco-cms/backoffice/lit-element";
const x = [
  {
    name: "text",
    type: "text",
    fields: ["name", "required", "id", "class", "placeholder", "size", "value"]
  },
  {
    name: "email",
    type: "email",
    fields: ["name", "required", "id", "class", "placeholder", "size", "value"]
  },
  {
    name: "URL",
    type: "url",
    fields: ["name", "required", "id", "class", "placeholder", "size", "value"]
  },
  {
    name: "tel",
    type: "tel",
    fields: ["name", "required", "id", "class", "placeholder", "size", "value"]
  },
  {
    name: "number",
    type: "number",
    fields: ["name", "required", "id", "class", "placeholder", "minNum", "maxNum", "numType"]
  },
  {
    name: "date",
    type: "date",
    fields: ["name", "required", "id", "class", "minDate", "maxDate"]
  },
  {
    name: "text area",
    type: "textarea",
    fields: ["name", "required", "id", "class", "placeholder", "rows", "cols", "value"]
  },
  {
    name: "drop-down menu",
    type: "select",
    fields: ["name", "required", "id", "class", "multiple", "includeBlank", "options", "query"]
  },
  {
    name: "checkboxes",
    type: "checkbox",
    fields: ["name", "required", "id", "class", "options", "labelFirst", "useLabelElement", "exclusive", "query"]
  },
  {
    name: "radio buttons",
    type: "radio",
    fields: ["name", "required", "id", "class", "options", "labelFirst", "useLabelElement", "query"]
  },
  {
    name: "acceptance",
    type: "acceptance",
    fields: ["name", "id", "class", "optional", "labelSub", "condition"]
  },
  {
    name: "file",
    type: "file",
    fields: ["name", "required", "id", "class", "limit", "filetypes"]
  },
  {
    name: "submit",
    type: "submit",
    fields: ["id", "class", "labelSub"]
  },
  {
    name: "hidden",
    type: "hidden",
    fields: ["name", "id", "class", "value"]
  }
];
function p(e) {
  return {
    type: e,
    numType: "",
    required: !1,
    name: `${e}-${Math.floor(Math.random() * 1e3)}`,
    id: "",
    className: "",
    values: "",
    placeholder: !1,
    value: "",
    minNum: null,
    maxNum: null,
    minDate: null,
    maxDate: null,
    options: "",
    multiple: !1,
    includeBlank: !1,
    labelFirst: !1,
    useLabelElement: !0,
    exclusive: !1,
    condition: "",
    optional: !0,
    limit: null,
    filetypes: "",
    labelSub: "",
    size: 40,
    cols: 40,
    rows: 10,
    query: ""
  };
}
function d(e) {
  let a = e.type;
  e.numType && (a = e.numType);
  let l = `[${a}`;
  if (e.required && (l += "*"), a !== "submit" && (l += ` ${e.name}`), e.minNum !== null && (l += ` min:${e.minNum}`), e.maxNum !== null && (l += ` max:${e.maxNum}`), e.minDate && (l += ` min:${e.minDate}`), e.maxDate && (l += ` max:${e.maxDate}`), e.limit !== null && (l += ` limit:${e.limit}`), e.filetypes && (l += ` filetypes:${e.filetypes.replace(/[^0-9a-z.]/gi, "|")}`), e.id && (l += ` id:${e.id}`), e.className)
    for (const r of e.className.split(" "))
      r && (l += ` class:${r}`);
  if (e.size && ["text", "email", "url", "tel"].includes(a) && (l += ` size:${e.size}`), e.rows && a === "textarea" && (l += ` rows:${e.rows}`), e.cols && a === "textarea" && (l += ` cols:${e.cols}`), e.labelSub && (l += ` "${e.labelSub}"`), e.placeholder && (l += " placeholder"), e.value && (l += ` "${e.value}"`), e.multiple && (l += " multiple "), e.includeBlank && (l += " include_blank"), e.labelFirst && (l += " label_first  "), e.useLabelElement && (a === "radio" || a === "checkbox") && (l += " use_label_element "), e.exclusive && (l += " exclusive "), a === "radio" && (l += " default:1"), e.optional && a === "acceptance" && (l += " optional"), e.options)
    for (const r of e.options.split(/\n/))
      r && (l += ` "${r}"`);
  return e.query && (l += ` "query:${e.query}"`), l += "]", e.condition && a === "acceptance" && (l += ` ${e.condition} [/acceptance]`), l;
}
var y = Object.defineProperty, $ = Object.getOwnPropertyDescriptor, o = (e, a, l, r) => {
  for (var u = r > 1 ? void 0 : r ? $(a, l) : a, t = e.length - 1, s; t >= 0; t--)
    (s = e[t]) && (u = (r ? s(a, l, u) : s(u)) || u);
  return r && u && y(a, l, u), u;
};
let n = class extends g {
  constructor() {
    super(...arguments), this.value = "", this._activeTag = null, this._form = p("text"), this._preview = "";
  }
  _onValueChange(e) {
    const a = e.target.value;
    this.value = a, this.dispatchEvent(new CustomEvent("change", { bubbles: !0, composed: !0 }));
  }
  _openBuilder(e) {
    this._activeTag = e, this._form = p(e.type), this._preview = d(this._form);
  }
  _closeBuilder() {
    this._activeTag = null;
  }
  _updateForm(e, a) {
    this._form = { ...this._form, [e]: a }, this._preview = d(this._form);
  }
  _insertTag() {
    if (!this._textarea)
      return;
    const e = this.value ?? "", a = this._textarea.selectionStart ?? e.length, l = this._textarea.selectionEnd ?? a, r = e.slice(0, a), u = e.slice(l);
    this.value = `${r}${this._preview}${u}`, this.dispatchEvent(new CustomEvent("change", { bubbles: !0, composed: !0 })), this._closeBuilder();
  }
  _renderField(e) {
    var l, r, u;
    const a = this._form;
    switch (e) {
      case "name":
        return i`<label>Name <input .value=${a.name} @input=${(t) => this._updateForm("name", t.target.value)} /></label>`;
      case "required":
        return i`<label><input type="checkbox" .checked=${a.required} @change=${(t) => this._updateForm("required", t.target.checked)} /> Required</label>`;
      case "id":
        return i`<label>ID <input .value=${a.id} @input=${(t) => this._updateForm("id", t.target.value)} /></label>`;
      case "class":
        return i`<label>Class <input .value=${a.className} @input=${(t) => this._updateForm("className", t.target.value)} /></label>`;
      case "placeholder":
        return i`<label><input type="checkbox" .checked=${a.placeholder} @change=${(t) => this._updateForm("placeholder", t.target.checked)} /> Use as placeholder</label>`;
      case "value":
        return i`<label>Default value <input .value=${a.value} @input=${(t) => this._updateForm("value", t.target.value)} /></label>`;
      case "size":
        return i`<label>Size <input type="number" .value=${String(a.size)} @input=${(t) => this._updateForm("size", Number(t.target.value))} /></label>`;
      case "minNum":
        return i`<label>Min <input type="number" .value=${((l = a.minNum) == null ? void 0 : l.toString()) ?? ""} @input=${(t) => this._updateForm("minNum", t.target.value ? Number(t.target.value) : null)} /></label>`;
      case "maxNum":
        return i`<label>Max <input type="number" .value=${((r = a.maxNum) == null ? void 0 : r.toString()) ?? ""} @input=${(t) => this._updateForm("maxNum", t.target.value ? Number(t.target.value) : null)} /></label>`;
      case "minDate":
        return i`<label>Min date <input type="date" .value=${a.minDate ?? ""} @input=${(t) => this._updateForm("minDate", t.target.value || null)} /></label>`;
      case "maxDate":
        return i`<label>Max date <input type="date" .value=${a.maxDate ?? ""} @input=${(t) => this._updateForm("maxDate", t.target.value || null)} /></label>`;
      case "limit":
        return i`<label>Limit (bytes) <input type="number" .value=${((u = a.limit) == null ? void 0 : u.toString()) ?? ""} @input=${(t) => this._updateForm("limit", t.target.value ? Number(t.target.value) : null)} /></label>`;
      case "filetypes":
        return i`<label>File types <input .value=${a.filetypes} placeholder="jpg|png|pdf" @input=${(t) => this._updateForm("filetypes", t.target.value)} /></label>`;
      case "rows":
        return i`<label>Rows <input type="number" .value=${String(a.rows)} @input=${(t) => this._updateForm("rows", Number(t.target.value))} /></label>`;
      case "cols":
        return i`<label>Cols <input type="number" .value=${String(a.cols)} @input=${(t) => this._updateForm("cols", Number(t.target.value))} /></label>`;
      case "options":
        return i`<label>Options (one per line) <textarea rows="5" .value=${a.options} @input=${(t) => this._updateForm("options", t.target.value)}></textarea></label>`;
      case "multiple":
        return i`<label><input type="checkbox" .checked=${a.multiple} @change=${(t) => this._updateForm("multiple", t.target.checked)} /> Multiple</label>`;
      case "includeBlank":
        return i`<label><input type="checkbox" .checked=${a.includeBlank} @change=${(t) => this._updateForm("includeBlank", t.target.checked)} /> Include blank option</label>`;
      case "labelFirst":
        return i`<label><input type="checkbox" .checked=${a.labelFirst} @change=${(t) => this._updateForm("labelFirst", t.target.checked)} /> Label first</label>`;
      case "useLabelElement":
        return i`<label><input type="checkbox" .checked=${a.useLabelElement} @change=${(t) => this._updateForm("useLabelElement", t.target.checked)} /> Wrap in <code>&lt;label&gt;</code></label>`;
      case "exclusive":
        return i`<label><input type="checkbox" .checked=${a.exclusive} @change=${(t) => this._updateForm("exclusive", t.target.checked)} /> Exclusive</label>`;
      case "labelSub":
        return i`<label>Button / link label <input .value=${a.labelSub} @input=${(t) => this._updateForm("labelSub", t.target.value)} /></label>`;
      case "condition":
        return i`<label>Acceptance condition <input .value=${a.condition} @input=${(t) => this._updateForm("condition", t.target.value)} /></label>`;
      case "optional":
        return i`<label><input type="checkbox" .checked=${a.optional} @change=${(t) => this._updateForm("optional", t.target.checked)} /> Optional</label>`;
      case "numType":
        return i`<label>Sub-type
            <select .value=${a.numType} @change=${(t) => this._updateForm("numType", t.target.value)}>
              <option value="">number</option>
              <option value="range">range</option>
            </select>
          </label>`;
      case "query":
        return i`<label>SQL query (for select/checkbox/radio) <input .value=${a.query} @input=${(t) => this._updateForm("query", t.target.value)} /></label>`;
      default:
        return null;
    }
  }
  _renderBuilder() {
    return this._activeTag ? i`
      <div class="overlay" @click=${this._closeBuilder}>
        <div class="dialog" @click=${(e) => e.stopPropagation()}>
          <header>
            <h3>Form-tag Generator: ${this._activeTag.name}</h3>
            <button type="button" @click=${this._closeBuilder}>×</button>
          </header>
          <div class="fields">
            ${this._activeTag.fields.map((e) => this._renderField(e))}
          </div>
          <pre class="preview">${this._preview}</pre>
          <footer>
            <button type="button" @click=${this._closeBuilder}>Cancel</button>
            <button type="button" class="primary" @click=${this._insertTag}>Insert tag</button>
          </footer>
        </div>
      </div>
    ` : null;
  }
  render() {
    return i`
      <div class="tag-list">
        ${x.map(
      (e) => i`<button type="button" class="tag-chip" @click=${() => this._openBuilder(e)}>${e.name}</button>`
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
};
n.styles = b`
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
o([
  m({ type: String })
], n.prototype, "value", 2);
o([
  m({ attribute: !1 })
], n.prototype, "config", 2);
o([
  c()
], n.prototype, "_activeTag", 2);
o([
  c()
], n.prototype, "_form", 2);
o([
  c()
], n.prototype, "_preview", 2);
o([
  f("#template-textarea")
], n.prototype, "_textarea", 2);
n = o([
  h("uformkit-template-editor")
], n);
const k = n;
export {
  n as UFormKitTemplateEditorElement,
  k as default
};
//# sourceMappingURL=uformkit-template.element.js.map
