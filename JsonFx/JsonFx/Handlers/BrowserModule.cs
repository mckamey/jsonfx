using System;
using System.Web;

using JsonFx.UI;

namespace JsonFx.Handlers
{
	/// <summary>
	/// HttpModule which augments HttpRequest.Browser object to show
	/// Request is being made from asynchronous JsonFx call.
	/// </summary>
	public class BrowserModule : IHttpModule
	{
		#region IHttpModule Members

		void IHttpModule.Dispose()
		{
		}

		void IHttpModule.Init(HttpApplication application)
		{
			application.BeginRequest += new EventHandler(application_BeginRequest);
			application.PostRequestHandlerExecute += new EventHandler(application_PostRequestHandlerExecute);
		}

		#endregion IHttpModule Members

		#region Application Events

		private void application_BeginRequest(object sender, EventArgs e)
		{
			HttpApplication application = sender as HttpApplication;
			if (application == null || application.Request == null)
			{
				return;
			}

			if (JsonFxBrowserCapabilities.IsJsonFx(application.Request))
			{
				application.Request.Browser = new JsonFxBrowserCapabilities(application.Request.Browser);
			}
		}

		private void application_PostRequestHandlerExecute(object sender, EventArgs e)
		{
			HttpApplication application = sender as HttpApplication;
			if (application == null || application.Request == null)
			{
				return;
			}

			application.Response.AddHeader("X-JsonFx-Version", JsonFx.About.Version.ToString());
		}

		#endregion Application Events
	}
}
