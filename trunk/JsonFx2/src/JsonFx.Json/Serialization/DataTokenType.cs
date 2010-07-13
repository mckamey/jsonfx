﻿#region License
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

namespace JsonFx.Serialization
{
	/// <summary>
	/// General Lexical Analysis Tokens
	/// </summary>
	public enum DataTokenType
	{
		/// <summary>
		/// No token
		/// </summary>
		/// <remarks>
		/// The state before and after an object graph
		/// </remarks>
		None,

		/// <summary>
		/// Primitive and simple scalar values
		/// </summary>
		Value,

		/// <summary>
		/// Begin Array
		/// </summary>
		ArrayBegin,

		/// <summary>
		/// End Array
		/// </summary>
		ArrayEnd,

		/// <summary>
		/// Begin Object
		/// </summary>
		ObjectBegin,

		/// <summary>
		/// End Object
		/// </summary>
		ObjectEnd,

		/// <summary>
		/// Object property identifier
		/// </summary>
		PropertyKey,

		/// <summary>
		/// Object attribute identifier
		/// </summary>
		AttributeKey,

		/// <summary>
		/// Code comment value
		/// </summary>
		Comment
	}
}
