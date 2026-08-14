using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace UFormKit;

[DataEditor(
    alias: "UFormKitTemplate",
    ValueType = ValueTypes.Text,
    ValueEditorIsReusable = true)]
public class UFormKitTemplate : DataEditor
{
    public UFormKitTemplate(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
    {
    }

    protected override IDataValueEditor CreateValueEditor()
    {
        var editor = base.CreateValueEditor();
        editor.ValueType = ValueTypes.Text;
        return editor;
    }
}
