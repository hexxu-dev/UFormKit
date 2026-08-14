import { css as h, property as v, customElement as _, html as c } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as f } from "@umbraco-cms/backoffice/lit-element";
var d = Object.defineProperty, w = Object.getOwnPropertyDescriptor, m = (e) => {
  throw TypeError(e);
}, l = (e, t, s, o) => {
  for (var r = o > 1 ? void 0 : o ? w(t, s) : t, i = e.length - 1, n; i >= 0; i--)
    (n = e[i]) && (r = (o ? n(t, s, r) : n(r)) || r);
  return o && r && d(t, s, r), r;
}, y = (e, t, s) => t.has(e) || m("Cannot " + s), g = (e, t, s) => t.has(e) ? m("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, s), b = (e, t, s) => (y(e, t, "access private method"), s), p, u;
let a = class extends f {
  constructor() {
    super(...arguments), g(this, p), this.value = "";
  }
  render() {
    const e = b(this, p, u).call(this, this.value);
    return c`<div class="value">${e.map((t, s) => s === 0 ? t : c`<br />${t}`)}</div>`;
  }
};
p = /* @__PURE__ */ new WeakSet();
u = function(e) {
  return (e ?? "").split(/<br\s*\/?>/gi);
};
a.styles = h`
    :host {
      display: block;
    }
    .value {
      white-space: pre-wrap;
      overflow-wrap: anywhere;
    }
  `;
l([
  v({ type: String })
], a.prototype, "value", 2);
l([
  v({ attribute: !1 })
], a.prototype, "config", 2);
a = l([
  _("uformkit-submission-message")
], a);
const C = a;
export {
  a as UFormKitSubmissionMessageElement,
  C as default
};
//# sourceMappingURL=uformkit-submission.element.js.map
