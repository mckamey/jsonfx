using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;

using JsonFx.Serialization;
using JsonFx.Services;

namespace JsonFx.Services.Discovery
{
	public class JsonServiceDescription : JsonDescriptionBase
	{
		#region Fields

		private string sdversion = "1.0";
		private string name = null;
		private string id = null;
		private string version = null;
		private string summary = null;
		private string help = null;
		private string address = null;
		private JsonMethodDescription[] procs = null;

		#endregion Fields

		#region Init

		public JsonServiceDescription() { }

		/// <summary>
		/// Ctor.
		/// </summary>
		internal JsonServiceDescription(Type serviceType, string serviceUrl)
		{
			//TODO: clean up JsonServiceDescription efficiency

			if (serviceType == null)
				return;

			if (!JsonServiceAttribute.IsJsonService(serviceType))
				throw new JsonServiceException("Specified type is not marked as a JsonService.");

			this.Name = JsonNameAttribute.GetJsonName(serviceType);
			if (String.IsNullOrEmpty(this.Name))
				this.name = serviceType.Name;

			MethodInfo[] infos = serviceType.GetMethods();
			List<JsonMethodDescription> methods = new List<JsonMethodDescription>(infos.Length);
			foreach (MethodInfo info in infos)
			{
				if (info.IsPublic && JsonMethodAttribute.IsJsonMethod(info))
				{
					methods.Add(new JsonMethodDescription(info));
				}
			}
			this.Methods = methods.ToArray();

			this.ID = "urn:uuid:"+serviceType.GUID;

			Version assemblyVersion = serviceType.Assembly.GetName().Version;
			this.Version = assemblyVersion.ToString(2);

			this.Address = serviceUrl;

			this.Help = JsonServiceAttribute.GetHelpUrl(serviceType);

			if (Attribute.IsDefined(serviceType, typeof(DescriptionAttribute)))
			{
				DescriptionAttribute description = Attribute.GetCustomAttribute(serviceType, typeof(DescriptionAttribute), true) as DescriptionAttribute;
				this.Summary = description.Description;
			}
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the version of service description to which this conforms.
		/// </summary>
		/// <remarks>
		/// REQUIRED. A String value that represents the version number of this object
		/// and MUST read "1.0" for conforming implementations.
		/// </remarks>
		[JsonName("sdversion")]
		public string SDVersion
		{
			get { return this.sdversion; }
			set { this.sdversion = value; }
		}

		/// <summary>
		/// Gets and sets a simple name for the method.
		/// </summary>
		/// <remarks>
		/// REQUIRED. A String value that provides a simple name for the method.
		/// </remarks>
		[JsonName("name")]
		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		/// <summary>
		/// Gets and sets a unique identifier for the service.
		/// </summary>
		/// <remarks>
		/// REQUIRED. A String value that uniquely and globally identifies the
		/// service. The string MUST use the URI Generic Syntax (RFC 3986). 
		/// </remarks>
		[JsonName("id")]
		public string ID
		{
			get { return this.id; }
			set { this.id = value; }
		}

		/// <summary>
		/// Gets and sets the version number of this service.
		/// </summary>
		/// <remarks>
		/// OPTIONAL. A String value that indicates version number of the
		/// service and MAY be used by the applications for checking compatibility.
		/// The version number, when present, MUST include a major and minor
		/// component separated by a period (U+002E or ASCII 46). The major
		/// and minor components MUST use decimal digits (0 to 9) only. For example,
		/// use "2.5" to mean a major version of 2 and a minor version of 5. The
		/// use and interpretation of the version number is left at the discretion
		/// of the applications treating the Service Description.
		/// </remarks>
		[JsonName("version")]
		public string Version
		{
			get { return this.version; }
			set { this.version = value; }
		}

		/// <summary>
		/// Gets and sets a summary of the purpose of the service.
		/// </summary>
		/// <remarks>
		/// OPTIONAL. A String value that summarizes the purpose of the service.
		/// This SHOULD be kept to a maximum of 5 sentences and often limited to a
		/// single phrase like, "The News Search service allows you to search the
		/// Internet for news stories."
		/// </remarks>
		[JsonName("summary")]
		public string Summary
		{
			get { return this.summary; }
			set { this.summary = value; }
		}

		/// <summary>
		/// Gets and sets a help documentation for the service.
		/// </summary>
		/// <remarks>
		/// OPTIONAL. A String value that is a URL from where human-readable
		/// documentation about the service may be obtained.
		/// </remarks>
		[JsonName("help")]
		public string Help
		{
			get { return this.help; }
			set { this.help = value; }
		}

		/// <summary>
		/// Gets and sets the URL of the service end-point to which the remote procedure calls can be targeted.
		/// </summary>
		/// <remarks>
		/// OPTIONAL. A String value that is the URL of the service end-point to
		/// which the remote procedure calls can be targeted. The protocol scheme
		/// of this URL SHOULD be http or https. Although this value is optional,
		/// it is highly RECOMMENDED that a service always publish its address so
		/// that a service description obtained indirectly can be used nonetheless
		/// to locate the service.
		/// </remarks>
		[JsonName("address")]
		public string Address 
		{
			get { return this.address; }
			set { this.address = value; }
		}

		/// <summary>
		/// Gets and sets a help documentation for the service.
		/// </summary>
		/// <remarks>
		/// OPTIONAL. A String value that is a URL from where human-readable
		/// documentation about the service may be obtained.
		/// </remarks>
		[JsonName("procs")]
		public JsonMethodDescription[] Methods
		{
			get { return this.procs; }
			set { this.procs = value; }
		}

		#endregion Properties
	}
}
