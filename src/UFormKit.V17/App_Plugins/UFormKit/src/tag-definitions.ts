export type TagFieldKind =
  | "name"
  | "required"
  | "id"
  | "class"
  | "placeholder"
  | "size"
  | "minNum"
  | "maxNum"
  | "minDate"
  | "maxDate"
  | "limit"
  | "filetypes"
  | "rows"
  | "cols"
  | "options"
  | "multiple"
  | "includeBlank"
  | "labelFirst"
  | "useLabelElement"
  | "exclusive"
  | "labelSub"
  | "condition"
  | "optional"
  | "value"
  | "numType"
  | "query";

export interface TagDefinition {
  name: string;
  type: string;
  fields: TagFieldKind[];
}

export const TAG_DEFINITIONS: TagDefinition[] = [
  {
    name: "text",
    type: "text",
    fields: ["name", "required", "id", "class", "placeholder", "size", "value"],
  },
  {
    name: "email",
    type: "email",
    fields: ["name", "required", "id", "class", "placeholder", "size", "value"],
  },
  {
    name: "URL",
    type: "url",
    fields: ["name", "required", "id", "class", "placeholder", "size", "value"],
  },
  {
    name: "tel",
    type: "tel",
    fields: ["name", "required", "id", "class", "placeholder", "size", "value"],
  },
  {
    name: "number",
    type: "number",
    fields: ["name", "required", "id", "class", "placeholder", "minNum", "maxNum", "numType"],
  },
  {
    name: "date",
    type: "date",
    fields: ["name", "required", "id", "class", "minDate", "maxDate"],
  },
  {
    name: "text area",
    type: "textarea",
    fields: ["name", "required", "id", "class", "placeholder", "rows", "cols", "value"],
  },
  {
    name: "drop-down menu",
    type: "select",
    fields: ["name", "required", "id", "class", "multiple", "includeBlank", "options", "query"],
  },
  {
    name: "checkboxes",
    type: "checkbox",
    fields: ["name", "required", "id", "class", "options", "labelFirst", "useLabelElement", "exclusive", "query"],
  },
  {
    name: "radio buttons",
    type: "radio",
    fields: ["name", "required", "id", "class", "options", "labelFirst", "useLabelElement", "query"],
  },
  {
    name: "acceptance",
    type: "acceptance",
    fields: ["name", "id", "class", "optional", "labelSub", "condition"],
  },
  {
    name: "file",
    type: "file",
    fields: ["name", "required", "id", "class", "limit", "filetypes"],
  },
  {
    name: "submit",
    type: "submit",
    fields: ["id", "class", "labelSub"],
  },
  {
    name: "hidden",
    type: "hidden",
    fields: ["name", "id", "class", "value"],
  },
];

export interface TagFormValues {
  type: string;
  numType: string;
  required: boolean;
  name: string;
  id: string;
  className: string;
  values: string;
  placeholder: boolean;
  value: string;
  minNum: number | null;
  maxNum: number | null;
  minDate: string | null;
  maxDate: string | null;
  options: string;
  multiple: boolean;
  includeBlank: boolean;
  labelFirst: boolean;
  useLabelElement: boolean;
  exclusive: boolean;
  condition: string;
  optional: boolean;
  limit: number | null;
  filetypes: string;
  labelSub: string;
  size: number;
  cols: number;
  rows: number;
  query: string;
}

export function makeInitialValues(type: string): TagFormValues {
  return {
    type,
    numType: "",
    required: false,
    name: `${type}-${Math.floor(Math.random() * 1000)}`,
    id: "",
    className: "",
    values: "",
    placeholder: false,
    value: "",
    minNum: null,
    maxNum: null,
    minDate: null,
    maxDate: null,
    options: "",
    multiple: false,
    includeBlank: false,
    labelFirst: false,
    useLabelElement: true,
    exclusive: false,
    condition: "",
    optional: true,
    limit: null,
    filetypes: "",
    labelSub: "",
    size: 40,
    cols: 40,
    rows: 10,
    query: "",
  };
}

export function buildTagString(v: TagFormValues): string {
  let tag = v.type;
  if (v.numType) tag = v.numType;

  let s = `[${tag}`;
  if (v.required) s += "*";
  if (tag !== "submit") s += ` ${v.name}`;
  if (v.minNum !== null) s += ` min:${v.minNum}`;
  if (v.maxNum !== null) s += ` max:${v.maxNum}`;
  if (v.minDate) s += ` min:${v.minDate}`;
  if (v.maxDate) s += ` max:${v.maxDate}`;
  if (v.limit !== null) s += ` limit:${v.limit}`;
  if (v.filetypes) s += ` filetypes:${v.filetypes.replace(/[^0-9a-z.]/gi, "|")}`;
  if (v.id) s += ` id:${v.id}`;
  if (v.className) {
    for (const c of v.className.split(" ")) {
      if (c) s += ` class:${c}`;
    }
  }
  if (v.size && ["text", "email", "url", "tel"].includes(tag)) s += ` size:${v.size}`;
  if (v.rows && tag === "textarea") s += ` rows:${v.rows}`;
  if (v.cols && tag === "textarea") s += ` cols:${v.cols}`;
  if (v.labelSub) s += ` "${v.labelSub}"`;
  if (v.placeholder) s += ` placeholder`;
  if (v.value) s += ` "${v.value}"`;
  if (v.multiple) s += ` multiple `;
  if (v.includeBlank) s += ` include_blank`;
  if (v.labelFirst) s += ` label_first  `;
  if (v.useLabelElement && (tag === "radio" || tag === "checkbox")) s += ` use_label_element `;
  if (v.exclusive) s += ` exclusive `;
  if (tag === "radio") s += ` default:1`;
  if (v.optional && tag === "acceptance") s += ` optional`;
  if (v.options) {
    for (const line of v.options.split(/\n/)) {
      if (line) s += ` "${line}"`;
    }
  }
  if (v.query) s += ` "query:${v.query}"`;
  s += "]";
  if (v.condition && tag === "acceptance") s += ` ${v.condition} [/acceptance]`;
  return s;
}
