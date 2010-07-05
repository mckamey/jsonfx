#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2010 Stephen M. McKamey

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

using System;
using System.Collections.Generic;
using System.Globalization;

using JsonFx.Serialization;

namespace JsonFx.Json
{
	/// <summary>
	/// Defines a filter for JSON serialization of DateTime into ISO-8601
	/// </summary>
	/// <remarks>
	/// http://www.w3.org/TR/NOTE-datetime
	/// http://en.wikipedia.org/wiki/ISO_8601
	/// </remarks>
	public class DateIso8601Filter : JsonFilter<DateTime>
	{
		#region IDataFilter<JsonTokenType,DateTime> Members

		public override bool TryRead(IEnumerable<Token<JsonTokenType>> tokens, out DateTime value)
		{
			// TODO: determine MoveNext or not?
			Token<JsonTokenType> token = tokens.GetEnumerator().Current;

			string date = Convert.ToString(token.Value, CultureInfo.InvariantCulture);
			return this.TryParseISO8601(date, out value);
		}

		public override bool TryWrite(DateTime value, out IEnumerable<Token<JsonTokenType>> tokens)
		{
			tokens = new Token<JsonTokenType>[]
				{
					JsonGrammar.TokenString(this.FormatISO8601(value))
				};

			return true;
		}

		#endregion IDataFilter<JsonTokenType,DateTime> Members

		#region Utility Methods

		/// <summary>
		/// Converts a ISO-8601 string to the corresponding DateTime representation
		/// </summary>
		/// <param name="date">ISO-8601 conformant date</param>
		/// <param name="value"></param>
		/// <returns>true if parsing was successful</returns>
		private bool TryParseISO8601(string date, out DateTime value)
		{
			return DateTime.TryParse(
				date,
				CultureInfo.InvariantCulture,
				DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault,
				out value);
		}

		/// <summary>
		/// Converts a DateTime to the corresponding ISO-8601 string representation
		/// </summary>
		/// <param name="value"></param>
		/// <returns>ISO-8601 conformant date</returns>
		private string FormatISO8601(DateTime value)
		{
			switch (value.Kind)
			{
				case DateTimeKind.Local:
				{
					value = value.ToUniversalTime();
					goto case DateTimeKind.Utc;
				}
				case DateTimeKind.Utc:
				{
					// UTC DateTime in ISO-8601
					return value.ToString("s")+"Z";
				}
				default:
				{
					// DateTime in ISO-8601
					return value.ToString("s");
				}
			}
		}

		#endregion Utility Methods
	}
}
