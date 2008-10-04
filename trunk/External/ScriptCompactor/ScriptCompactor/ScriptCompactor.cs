#region BuildTools License
/*---------------------------------------------------------------------------------*\

	BuildTools distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2007 Stephen M. McKamey

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
#endregion BuildTools License

using System;
using System.IO;
using System.Collections.Generic;

using BuildTools.IO;

namespace BuildTools.ScriptCompactor
{
	public static class ScriptCompactor
	{
		#region ScriptCompactor.Options

		[Flags]
		public enum Options
		{
			None = 0x00,
			Overwrite = 0x01
		}

		#endregion ScriptCompactor.Options

		#region Public Methods

		public static List<ParseException> Compact(string inputFile, string outputFile, string copyright, string timeStamp, ScriptCompactor.Options options)
		{
			if (!File.Exists(inputFile))
			{
				throw new FileNotFoundException(String.Format("File (\"{0}\") not found.", inputFile), inputFile);
			}

			if ((options&ScriptCompactor.Options.Overwrite) == 0x0 && File.Exists(outputFile))
			{
				throw new AccessViolationException(String.Format("File (\"{0}\") already exists.", outputFile));
			}

			if (inputFile.Equals(outputFile, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new ApplicationException("Input and output file are set to the same path.");
			}

			FileUtility.PrepSavePath(outputFile);
			using (TextWriter output = File.CreateText(outputFile))
			{
				return ScriptCompactor.Compact(inputFile, null, output, copyright, timeStamp, options);
			}
		}

		public static List<ParseException> Compact(string inputFile, string inputSource, TextWriter output, string copyright, string timeStamp, ScriptCompactor.Options options)
		{
			if (output == null)
			{
				throw new NullReferenceException("Output TextWriter was null.");
			}

			// write out header with copyright and timestamp
			ScriptCompactor.WriteHeader(output, copyright, timeStamp);

			// verify
			JSLint jslint = new JSLint();
			jslint.Run(inputFile, inputSource);

			// compact and write out results
			try
			{
				JSMin jsmin = new JSMin();
				if (String.IsNullOrEmpty(inputSource))
				{
					jsmin.Run(File.OpenText(inputFile), output);
				}
				else
				{
					jsmin.Run(inputSource, output);
				}
			}
			catch (Exception ex)
			{
				// a bad JSLint error could cause JSMin to choke
				jslint.Errors.Add(new ParseError(ex.Message, inputFile, -1, -1, ex));
			}

			// return any errors
			return jslint.Errors;
		}

		#endregion Public Methods

		#region Private Methods

		private static void WriteHeader(TextWriter writer, string copyright, string timeStamp)
		{
			if (!String.IsNullOrEmpty(copyright) || !String.IsNullOrEmpty(timeStamp))
			{
				int width = 6;
				if (!String.IsNullOrEmpty(copyright))
				{
					copyright = copyright.Replace("*/", "");// make sure not to nest commments
					width = Math.Max(copyright.Length+6, width);
				}
				if (!String.IsNullOrEmpty(timeStamp))
				{
					timeStamp = DateTime.Now.ToString(timeStamp).Replace("*/", "");// make sure not to nest commments
					width = Math.Max(timeStamp.Length+6, width);
				}

				writer.WriteLine("/*".PadRight(width, '-')+"*\\");

				if (!String.IsNullOrEmpty(copyright))
				{
					writer.WriteLine("\t"+copyright);
				}

				if (!String.IsNullOrEmpty(timeStamp))
				{
					writer.WriteLine("\t"+timeStamp);
				}

				writer.WriteLine("\\*".PadRight(width, '-')+"*/");
			}
		}

		#endregion Private Methods
	}
}