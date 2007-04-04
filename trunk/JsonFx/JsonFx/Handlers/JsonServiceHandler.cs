using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Compilation;

using JsonFx.Serialization;
using JsonFx.Services;
using JsonFx.Services.Discovery;
using JsonFx.Services.Proxy;

namespace JsonFx.Handlers
{
	internal class JsonServiceHandler : System.Web.IHttpHandler
	{
		#region Constants

		protected const string DescriptionMethodName = "system.describe";
		public const string JsonContentType = "application/json";
		public const string JsonFileExtension = ".json";

		#endregion Constants

		#region Fields

		private readonly JsonServiceInfo ServiceInfo;
		private object service = null;
		private string serviceUrl = null;
		private Exception error = null;

		#endregion Fields

		#region Init

		internal JsonServiceHandler(JsonServiceInfo serviceInfo, string serviceUrl)
		{
			try
			{
				this.ServiceInfo = serviceInfo;
				this.serviceUrl = serviceUrl;
			}
			catch (Exception ex)
			{
				this.error = ex;
			}
		}

		#endregion Init

		#region Properties

		protected object Service
		{
			get
			{
				if (this.service == null)
				{
					this.service = this.ServiceInfo.CreateService();
				}
				return this.service;
			}
		}

		#endregion Properties

		#region Methods

		private JsonRequest BuildRequestFromGet(System.Web.HttpContext context)
		{
			JsonRequest request = new JsonRequest();

			if (!String.IsNullOrEmpty(context.Request.PathInfo))
			{
				request.Method = context.Request.PathInfo.Substring(1);
			}

			JsonObject parameters = new JsonObject();
			foreach (string key in context.Request.QueryString.Keys)
			{
				parameters[key] = context.Request.QueryString[key];
			}

			request.NamedParams = parameters;

			return request;
		}

		private JsonRequest BuildRequestFromPost(System.Web.HttpContext context)
		{
			JsonReader reader = new JsonReader(context.Request.InputStream);
			return (JsonRequest)reader.Deserialize(typeof(JsonRequest));
		}

		private void HandleRequest(System.Web.HttpContext context, JsonRequest request, ref JsonResponse response)
		{
			context.Response.Clear();
			string browserEtc = context.Request.Browser["etc"];
			if (browserEtc != null && browserEtc.Contains("opera/8"))
			{
				// this is a specific fix for Opera 8.x
				// it can only handle "text/plain" & "text/html"
				// otherwise the content encoding is whacked
				context.Response.ContentType = "text/plain";
			}
			else
			{
				context.Response.ContentType = JsonServiceHandler.JsonContentType;
			}
			context.Response.ContentEncoding = System.Text.Encoding.UTF8;
			context.Response.AddHeader("Content-Disposition", "inline;filename=JsonResponse"+JsonServiceHandler.JsonFileExtension);

			response.ID = request.ID;

			System.Reflection.MethodInfo method = this.ServiceInfo.ResolveMethodName(request.Method);

			if (JsonServiceHandler.DescriptionMethodName.Equals(request.Method, StringComparison.InvariantCulture))
			{
				response.Result = new JsonServiceDescription(this.ServiceInfo.ServiceType, this.serviceUrl);
			}
			else if (method != null)
			{
				Object[] positionalParams = null;
				if (request.NamedParams != null)
				{
					String[] paramMap = this.ServiceInfo.GetMethodParams(method.Name);
					positionalParams = new Object[paramMap.Length];
					for (int i=0; i<paramMap.Length; i++)
					{
						// initially map name to position
						positionalParams[i] = request.NamedParams[paramMap[i]];
						if (positionalParams[i] == null)
						{
							// try to get as named positional param
							positionalParams[i] = request.NamedParams[i.ToString()];
						}
					}
				}
				else if (request.PositionalParams != null)
				{
					positionalParams = new object[request.PositionalParams.Length];
					request.PositionalParams.CopyTo(positionalParams, 0);
				}
				try
				{
#warning This does not check the parameters for type compatibility.  e.g. using Double for Int32 throws exception
					response.Result = method.Invoke(this.Service, positionalParams);
				}
				catch (TargetParameterCountException ex)
				{
					throw new JsonServiceException(
						String.Format(
							"Method \"{0}\" expects {1} parameters, {2} provided",
							method.Name,
							method.GetParameters().Length, positionalParams.Length),
						ex);
				}
				catch (TargetInvocationException ex)
				{
					if (ex.InnerException != null)
					{
						throw new JsonServiceException(ex.InnerException.Message, ex.InnerException);
					}
					throw new JsonServiceException(ex.Message, ex);
				}
				catch (Exception ex)
				{
					throw new JsonServiceException(ex.Message, ex);
				}
			}
			else
			{
				throw new JsonServiceException("Invalid method name.");
			}
		}

		#endregion Methods

		#region IHttpHandler Members

		void System.Web.IHttpHandler.ProcessRequest(System.Web.HttpContext context)
		{
			JsonResponse response = new JsonResponse();
			try
			{
				if (this.error != null)
					throw this.error;

				JsonRequest request = null;

				if ("GET".Equals(context.Request.HttpMethod, StringComparison.InvariantCultureIgnoreCase))
				{
					request = this.BuildRequestFromGet(context);
				}
				else
				{
					request = this.BuildRequestFromPost(context);
				}

				if (request == null)
					throw new JsonServiceException("The JSON-RPC Request was empty.");

				this.HandleRequest(context, request, ref response);
			}
			catch (Exception ex)
			{
				context.Response.ClearContent();
				response.Result = null;
				response.Error = new JsonError(ex is JsonServiceException ? ex.InnerException : ex);
			}
			finally
			{
				try
				{
					using (JsonWriter writer = new JsonWriter(context.Response.Output))
					{
						writer.Write(response);
					}
				}
				catch (TargetInvocationException ex)
				{
					context.Response.ClearContent();
					response.Result = null;
					response.Error = new JsonError(ex.InnerException);

					using (JsonWriter writer = new JsonWriter(context.Response.Output))
					{
						writer.Write(response);
					}
				}
				catch (Exception ex)
				{
					context.Response.ClearContent();
					response.Result = null;
					response.Error = new JsonError(ex);

					using (JsonWriter writer = new JsonWriter(context.Response.Output))
					{
						writer.Write(response);
					}
				}
			}
		}

		bool System.Web.IHttpHandler.IsReusable
		{
			get { return true; }
		}

		#endregion IHttpHandler Members
	}
}