using Umbraco.Cms.Core.PropertyEditors;

namespace UFormKit
{

	/// <summary>
	///     Represents a property editor for label properties.
	/// </summary>

	[DataEditor(
	   alias: "UFormKitTemplate",
	   name: "UFormKit form template",
	   view: "~/App_Plugins/UFormKit/backoffice/form/form.html",
	   Group = "Common",
	   Icon = "icon-list")]
	public class UFormKitTemplate : DataEditor
	{
		public UFormKitTemplate(IDataValueEditorFactory dataValueEditorFactory)
			: base(dataValueEditorFactory)
		{
		}
	}
}
