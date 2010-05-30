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
using System.Text;

namespace JsonFx.Serialization
{
	/// <summary>
	/// Represents a single token in an input character sequence
	/// </summary>
	public sealed class Token<T>
	{
		#region Fields

		public readonly T TokenType;
		public readonly object Value;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="tokenType"></param>
		public Token(T tokenType)
			: this(tokenType, null)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="tokenType"></param>
		/// <param name="value"></param>
		public Token(T tokenType, object value)
		{
			this.TokenType = tokenType;
			this.Value = value;
		}

		#endregion Init

		#region Object Overrides

		/// <summary>
		/// Returns a string that represents the current token.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("{ ");
			builder.Append(this.TokenType);
			if (this.Value != null)
			{
				builder.Append("=");
				builder.Append(this.Value);
			}
			builder.Append(" }");

			return builder.ToString();
		}

		#endregion Object Overrides
	}
}