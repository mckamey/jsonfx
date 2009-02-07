using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;

using JsonFx.Json;
using JsonFx.Client;

namespace JsonFx.UI.Jbst
{
	/// <summary>
	/// Allows easily attaching JBST controls and JSON data to an ASP.NET page.
	/// </summary>
	[ToolboxData("<{0}:Control runat=\"server\"></{0}:Control>")]
	public class Control : System.Web.UI.Control
	{
		#region Constants

		private const string EmptyJsonObject = "{}";

		#endregion Constants

		#region Fields

		private string name;
		private string data;
		private int? index;
		private ScriptDataBlock dataBlock;

		#endregion Fields

		#region Properties

		/// <summary>
		/// Gets and sets the script variable name of the JBST control to be bound.
		/// </summary>
		[DefaultValue("")]
		public virtual string Name
		{
			get
			{
				if (this.name == null)
				{
					return String.Empty;
				}
				return this.name;
			}
			set { this.name = JsonWriter.EnsureValidIdentifier(value, true); }
		}

		/// <summary>
		/// Gets and sets the script variable name of the JSON data to be bound.
		/// </summary>
		[DefaultValue(EmptyJsonObject)]
		public virtual string Data
		{
			get
			{
				if (String.IsNullOrEmpty(this.data))
				{
					return EmptyJsonObject;
				}
				return this.data;
			}
			set { this.data = value; }
		}

		/// <summary>
		/// Gets and sets the index to pass in when binding the data and JBST.
		/// </summary>
		[DefaultValue(-1)]
		public virtual int Index
		{
			get
			{
				if (!this.index.HasValue)
				{
					return -1;
				}
				return this.index.Value;
			}
			set
			{
				if (value < 0)
				{
					this.index = null;
					return;
				}
				this.index = value;
			}
		}

		/// <summary>
		/// Gets a dictionary of Data to emit to the page.
		/// </summary>
		public IDictionary<string, object> DataItems
		{
			get
			{
				if (this.dataBlock == null)
				{
					this.dataBlock = new ScriptDataBlock();
					this.Controls.Add(this.dataBlock);
				}
				return this.dataBlock.DataItems;
			}
		}

		///// <summary>
		///// Gets and sets if should render as a debuggable ("Pretty-Print") block.
		///// </summary>
		//[DefaultValue(false)]
		//public bool IsDebug
		//{
		//    get { return this.isDebug; }
		//    set { this.isDebug = value; }
		//}

		#endregion Properties

		#region Page Event Handlers

		/// <summary>
		/// Renders the JBST control reference and any stored data to be used.
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(HtmlTextWriter writer)
		{
			writer.BeginRender();
			try
			{
				if (String.IsNullOrEmpty(this.Name))
				{
					throw new ArgumentNullException("jbst:Control Name cannot be empty.");
				}

				string hook = "_"+Guid.NewGuid().ToString("N");

				// render the placeholder hook
				writer.Write("<div class=\"");
				writer.Write(hook);
				writer.Write("\">");

				writer.BeginRender();
				try
				{
					// render out any children as loading/error markup
					base.RenderChildren(writer);
				}
				finally
				{
					writer.EndRender();
				}

				writer.Write("</div>");

				// render the binding script
				writer.Write("<script type=\"text/javascript\">JsonFx.Bindings.register(\"div\",\"");
				writer.Write(hook);
				writer.Write("\",function(elem){return JsonFx.UI.bind((");
				writer.Write(this.Name);
				writer.Write("),(");
				writer.Write(this.Data);
				if (this.Index >= 0)
				{
					writer.Write("),(");
					writer.Write(this.Index);
				}
				writer.Write("))||elem;});</script>");
			}
			finally
			{
				writer.EndRender();
			}
		}

		#endregion Page Event Handlers
	}
}
