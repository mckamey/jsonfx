#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2009 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion License

#region JSLint License
/*---------------------------------------------------------------------------------*\

	Copyright (c) 2002 Douglas Crockford  (www.JSLint.com)

	Permission is hereby granted, free of charge, to any person obtaining a copy of
	this software and associated documentation files (the "Software"), to deal in
	the Software without restriction, including without limitation the rights to
	use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
	of the Software, and to permit persons to whom the Software is furnished to do
	so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all
	copies or substantial portions of the Software.

	The Software shall be used for Good, not Evil.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion JSLint License

#if __MonoCS__
// remove JSLINT for Mono Framework
#undef JSLINT
#endif

#if JSLINT

using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.InteropServices;

using JsonFx.BuildTools;

namespace JsonFx.BuildTools.ScriptCompactor
{
	/// <summary>
	/// Implements .NET wrapper for JSLint
	/// </summary>
	public class JSLint
	{
		#region JSLint.Options

		[ComVisible(true)]
		public class Options
		{
			/// <summary>
			/// true if ADsafe should be enforced
			/// </summary>
			[Description("IsAdSafe")]
			public bool adsafe;

			/// <summary>
			/// true if bitwise operators should not be allowed
			/// </summary>
			[Description("DisallowBitwise")]
			public bool bitwise;

			/// <summary>
			/// true if the standard browser globals should be predefined
			/// </summary>
			[Description("IsBrowser")]
			public bool browser;

			/// <summary>
			/// true if upper case HTML should be allowed
			/// </summary>
			[Description("AllowUpperCaseHtml")]
			public bool cap;

			/// <summary>
			/// true if CSS workarounds should be tolerated
			/// </summary>
			[Description("AllowCSSWorkarounds")]
			public bool css;

			/// <summary>
			/// true if debugger statements should be allowed
			/// </summary>
			[Description("AllowDebugger")]
			public bool debug;

			/// <summary>
			/// true if === should be required
			/// </summary>
			[Description("RequireStrictEquals")]
			public bool eqeqeq;

			/// <summary>
			/// true if eval should be allowed
			/// </summary>
			[Description("AllowEval")]
			public bool evil;

			/// <summary>
			/// true if for...in statements must filter
			/// </summary>
			[Description("AllowForIn")]
			public bool forin;

			/// <summary>
			/// true if HTML fragments should be allowed
			/// </summary>
			[Description("AllowHtmlFragments")]
			public bool fragment;

			/// <summary>
			/// true if line breaks should not be checked
			/// </summary>
			[Description("AllowLaxNewLines")]
			public bool laxbreak;

			/// <summary>
			/// true if constructor names must be capitalized
			/// </summary>
			[Description("CtorCapitalized")]
			public bool newcap;

			/// <summary>
			/// true if names should be checked
			/// </summary>
			[Description("CheckIdentifiers")]
			public bool nomen;

			/// <summary>
			/// true if HTML event handlers should be allowed
			/// </summary>
			[Description("AllowEventHandlers")]
			public bool on;

			/// <summary>
			/// true if only one var statement per function should be allowed
			/// </summary>
			[Description("OneVar")]
			public bool onevar;

			/// <summary>
			/// true if the scan should stop on first error
			/// </summary>
			[Description("StopOnFirstError")]
			public bool passfail;

			/// <summary>
			/// true if increment/decrement should not be allowed
			/// </summary>
			[Description("NoIncDec")]
			public bool plusplus;

			/// <summary>
			/// true if the . should not be allowed in regexp literals
			/// </summary>
			[Description("CheckRegExp")]
			public bool regexp;

			/// <summary>
			/// true if the Rhino environment globals should be predefined
			/// </summary>
			[Description("IsRhino")]
			public bool rhino;

			/// <summary>
			/// true if variables should be declared before used
			/// </summary>
			[Description("NoUndefVars")]
			public bool undef;

			/// <summary>
			/// true if use of some browser features should be restricted
			/// </summary>
			[Description("SafeBrowser")]
			public bool safe;

			/// <summary>
			/// true if the Windows Siderbar Gadget globals should be predefined
			/// </summary>
			[Description("IsSidebar")]
			public bool sidebar;

			/// <summary>
			/// true if requires the "use strict"; pragma
			/// </summary>
			[Description("RequireStrict")]
			public bool strict;

			/// <summary>
			/// true if all forms of subscript notation are tolerated
			/// </summary>
			[Description("Subscript")]
			public bool sub;

			/// <summary>
			/// true if strict whitespace rules apply
			/// </summary>
			[Description("StrictWhitespace")]
			public bool white;

			/// <summary>
			/// true if the Yahoo Widgets globals should be predefined
			/// </summary>
			[Description("IsYahooWidget")]
			public bool widget;

			/// <summary>
			/// number of spaces to force indention
			/// </summary>
			/// <remarks>
			/// JSLINT writes to this property so it has to be here
			/// otherwise a vague "Object does not support this property or method"
			/// </remarks>
			public int indent;
		}

		#endregion JSLint.Options

		#region Constants

		private const string JSLintScript = "JsonFx.BuildTools.ScriptCompactor.jslint.js";
		private const string MSScriptError =
			"JSLint is disabled.\r\n"+
			"Syntax checking requires MSScriptControl COM component be registered:\r\n"+
			"http://help.jsonfx.net/instructions";

		#endregion Constants

		#region Fields

		private static string JSLintSource = null;
		private static bool isDisabled = (Type.GetTypeFromCLSID(new Guid("0E59F1D5-1FBE-11D0-8FF2-00A0D10038BC")) == null);
		private JSLint.Options options = new JSLint.Options();
		private List<ParseException> errors = new List<ParseException>();

		#endregion Fields

		#region Properties

		/// <summary>
		/// List of warnings and errors found
		/// </summary>
		public List<ParseException> Errors
		{
			get { return this.errors; }
		}

		/// <summary>
		/// true if ADsafe should be enforced
		/// </summary>
		public bool IsAdSafe
		{
			get { return this.options.adsafe; }
			set { this.options.adsafe = value; }
		}

		/// <summary>
		/// true if bitwise operators should not be allowed
		/// </summary>
		public bool DisallowBitwise
		{
			get { return this.options.bitwise; }
			set { this.options.bitwise = value; }
		}

		/// <summary>
		/// true if the standard browser globals should be predefined
		/// </summary>
		public bool IsBrowser
		{
			get { return this.options.browser; }
			set { this.options.browser = value; }
		}

		/// <summary>
		/// true if upper case HTML should be allowed
		/// </summary>
		public bool AllowUpperCaseHtml
		{
			get { return this.options.cap; }
			set { this.options.cap = value; }
		}

		/// <summary>
		/// true if CSS workarounds should be tolerated
		/// </summary>
		public bool AllowCSSWorkarounds
		{
			get { return this.options.css; }
			set { this.options.css = value; }
		}

		/// <summary>
		/// true if debugger statements should be allowed
		/// </summary>
		public bool AllowDebugger
		{
			get { return this.options.debug; }
			set { this.options.debug = value; }
		}

		/// <summary>
		/// true if === should be required
		/// </summary>
		public bool RequireStrictEquals
		{
			get { return this.options.eqeqeq; }
			set { this.options.eqeqeq = value; }
		}

		/// <summary>
		/// true if eval should be allowed
		/// </summary>
		public bool AllowEval
		{
			get { return this.options.evil; }
			set { this.options.evil = value; }
		}

		/// <summary>
		/// true if for...in statements must filter
		/// </summary>
		public bool AllowForIn
		{
			get { return this.options.forin; }
			set { this.options.forin = value; }
		}

		/// <summary>
		/// true if HTML fragments should be allowed
		/// </summary>
		public bool AllowHtmlFragments
		{
			get { return this.options.fragment; }
			set { this.options.fragment = value; }
		}

		/// <summary>
		/// true if line breaks should not be checked
		/// </summary>
		public bool AllowLaxNewLines
		{
			get { return this.options.laxbreak; }
			set { this.options.laxbreak = value; }
		}

		/// <summary>
		/// true if constructor names must be capitalized
		/// </summary>
		public bool CtorCapitalized
		{
			get { return this.options.newcap; }
			set { this.options.newcap = value; }
		}

		/// <summary>
		/// true if names should be checked
		/// </summary>
		public bool CheckIdentifiers
		{
			get { return this.options.nomen; }
			set { this.options.nomen = value; }
		}

		/// <summary>
		/// true if HTML event handlers should be allowed
		/// </summary>
		public bool AllowEventHandlers
		{
			get { return this.options.on; }
			set { this.options.on = value; }
		}

		/// <summary>
		/// true if only one var statement per function should be allowed
		/// </summary>
		public bool OneVar
		{
			get { return this.options.onevar; }
			set { this.options.onevar = value; }
		}

		/// <summary>
		/// true if the scan should stop on first error
		/// </summary>
		public bool StopOnFirstError
		{
			get { return this.options.passfail; }
			set { this.options.passfail = value; }
		}

		/// <summary>
		/// true if increment/decrement should not be allowed
		/// </summary>
		public bool NoIncDec
		{
			get { return this.options.plusplus; }
			set { this.options.plusplus = value; }
		}

		/// <summary>
		/// true if the . should not be allowed in regexp literals
		/// </summary>
		public bool CheckRegExp
		{
			get { return this.options.regexp; }
			set { this.options.regexp = value; }
		}

		/// <summary>
		/// true if the Rhino environment globals should be predefined
		/// </summary>
		public bool IsRhino
		{
			get { return this.options.rhino; }
			set { this.options.rhino = value; }
		}

		/// <summary>
		/// true if undefined variables are errors
		/// </summary>
		public bool NoUndefVars
		{
			get { return this.options.undef; }
			set { this.options.undef = value; }
		}

		/// <summary>
		/// true if use of some browser features should be restricted
		/// </summary>
		public bool SafeBrowser
		{
			get { return this.options.safe; }
			set { this.options.safe = value; }
		}

		/// <summary>
		/// true if the System object should be predefined
		/// </summary>
		public bool IsSidebar
		{
			get { return this.options.sidebar; }
			set { this.options.sidebar = value; }
		}

		/// <summary>
		/// true if requires the "use strict"; pragma
		/// </summary>
		public bool RequireStrict
		{
			get { return this.options.strict; }
			set { this.options.strict = value; }
		}

		/// <summary>
		/// true if all forms of subscript notation are tolerated
		/// </summary>
		public bool Subscript
		{
			get { return this.options.sub; }
			set { this.options.sub = value; }
		}

		/// <summary>
		/// true if strict whitespace rules apply
		/// </summary>
		public bool StrictWhitespace
		{
			get { return this.options.white; }
			set { this.options.white = value; }
		}

		/// <summary>
		/// true if the Yahoo Widgets globals should be predefined
		/// </summary>
		public bool IsYahooWidget
		{
			get { return this.options.widget; }
			set { this.options.widget = value; }
		}

		#endregion Properties

		#region Methods

		public void Run(string filename, string script)
		{
			if (JSLint.isDisabled)
			{
				// shortcut if previously failed
				return;
			}

			Assembly assembly = Assembly.GetAssembly(typeof(JSLint));

			#region Microsoft Script Control References

			// Microsoft Script Control 1.0
			// http://weblogs.asp.net/rosherove/pages/DotNetScripting.aspx
			// http://msdn2.microsoft.com/en-us/library/ms974577.aspx
			// http://msdn.microsoft.com/msdnmag/issues/02/08/VisualStudioforApplications/
			// http://msdn2.microsoft.com/en-us/library/ms974586.aspx
			// http://msdn2.microsoft.com/en-us/library/ms950396.aspx

			// Must be built with x86 as Target (not Any CPU/x64/Itanium)
			// http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=1406892&SiteID=1
			// http://social.msdn.microsoft.com/Forums/en-US/netfx64bit/thread/338bb030-98d6-4b97-a182-562d985395d9/
			// http://www.devarticles.com/c/a/C-Sharp/Using-Late-Bound-COM-Objects/2/

			#endregion Microsoft Script Control References

			if (String.IsNullOrEmpty(script))
			{
				if (String.IsNullOrEmpty(filename) || !File.Exists(filename))
				{
					throw new FileNotFoundException("Cannot find the script file.", filename);
				}
				script = File.ReadAllText(filename);
			}

			try
			{
				this.EnsureJSLint(assembly);

				MSScriptControl.ScriptControlClass sc = new MSScriptControl.ScriptControlClass();
				sc.Language = "JScript";
				sc.AllowUI = false;
				sc.AddCode(JSLint.JSLintSource);

				object[] p = new object[] { script, this.options };

				bool result = false;
				try
				{
					result = (bool)sc.Run("JSLINT", ref p);
				}
				catch (Exception ex)
				{
					// IMPORTANT!
					// JSLINT writes to the options object so it must have all properties predefined
					// otherwise it throws a vague "Object does not support this property or method"

					string message = ex.Message;
					int	line = -1,
						column = -1;

					try
					{
						line = ((MSScriptControl.ErrorClass)(sc.Error)).Line;
						column = ((MSScriptControl.ErrorClass)(sc.Error)).Column;
						message = ((MSScriptControl.ErrorClass)(sc.Error)).Description;
					}
					catch {}

					this.errors.Add(new ParseWarning(message, JSLint.JSLintScript, line, column, ex));
				}

				if (!result)
				{
					// Alternatively this could also import json2.js
					// but then it would need to parse on the C# side
					// Just looping through with Eval is simpler/encapsulated
					int length = (int)sc.Eval("JSLINT.errors.length");
					for (int i=0; i<length; i++)
					{
						// if JSLint finds a fatal error it adds null to the end
						bool fatal = (i == length-1) && (bool)sc.Eval("!JSLINT.errors["+i+"]");
						if (!fatal)
						{
							int line = 1+(int)sc.Eval("JSLINT.errors["+i+"].line");
							int col = 1+(int)sc.Eval("JSLINT.errors["+i+"].character");
							string reason = sc.Eval("JSLINT.errors["+i+"].reason") as string;
							//string evidence = sc.Eval("JSLINT.errors["+i+"].evidence") as string;

							// could throw to set stack dump too but
							// just adding to errors collection here since
							// throwing adds a performance hit
							this.errors.Add(new ParseWarning(reason, filename, line, col));
						}
					}
				}
			}
			catch (Exception ex)
			{
				JSLint.isDisabled = true;

				this.errors.Add(new ParseWarning(
					JSLint.MSScriptError+"\r\n"+ex.GetType().Name+": "+ex.Message,
					assembly.GetName().Name,
					0,
					0,
					ex));
				return;
			}
		}

		public void Run(TextReader reader, string filename)
		{
			if (reader == null)
			{
				throw new NullReferenceException("Input StreamReader was null");
			}

			// read input file into memory
			string scriptText = reader.ReadToEnd();

			this.Run(filename, scriptText);
		}

		public void Run(string filename)
		{
			if (!File.Exists(filename))
			{
				throw new FileNotFoundException(String.Format("File not found: \"{0}\"", filename), filename);
			}

			using (StreamReader reader = new StreamReader(filename))
			{
				this.Run(reader, filename);
			}
		}

		public void Run(Stream input, string filename)
		{
			if (input == null)
				throw new NullReferenceException("Input Stream was null");

			// read input file into memory
			using (StreamReader reader = new StreamReader(input))
			{
				this.Run(reader, filename);
			}
		}

		private void EnsureJSLint(Assembly assembly)
		{
			if (JSLint.JSLintSource == null)
			{
				// jslint.js stored as a resource file
				string resourceName = JSLint.JSLintScript;
				if (assembly.GetManifestResourceInfo(resourceName) == null)
				{
					throw new FileNotFoundException("Cannot find the JSLint script file.", resourceName);
				}

				// output next to assembly
				using (Stream input = assembly.GetManifestResourceStream(resourceName))
				{
					using (StreamReader reader = new StreamReader(input))
					{
						JSLint.JSLintSource = reader.ReadToEnd();
					}
				}
			}
		}

		#endregion Methods
	}
}
#endif