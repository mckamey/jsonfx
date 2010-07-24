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

using JsonFx.Serialization;
using JsonFx.Serialization.Resolvers;

namespace JsonFx.Xml
{
	/// <summary>
	/// Formal language of tokens and symbols for markup
	/// </summary>
	internal class MarkupGrammar
	{
		#region Operators

		public const char OperatorElementBegin = '<';
		public const char OperatorElementEnd = '>';
		public const char OperatorElementClose = '/';
		public const char OperatorValueDelim = ' ';
		public const char OperatorPairDelim = '=';
		public const char OperatorPrefixDelim = ':';

		public const char OperatorStringDelim = '"';
		public const char OperatorStringDelimAlt = '\'';

		public const char OperatorEntityBegin = '&';
		public const char OperatorEntityNum = '#';
		public const char OperatorEntityHex = 'x';
		public const char OperatorEntityHexAlt = 'X';
		public const char OperatorEntityEnd = ';';

		public const char OperatorComment = '!';
		public const char OperatorCommentDelim = '-';
		public const string OperatorCommentBegin = "--";
		public const string OperatorCommentEnd = "--";
		public const string OperatorCDataBegin = "[CDATA[";
		public const string OperatorCDataEnd = "]]";

		public const char OperatorProcessingInstruction = '?';
		public const string OperatorPhpExpressionBegin = "?=";
		public const string OperatorProcessingInstructionEnd = "?";

		public const char OperatorCode = '%';
		public const char OperatorCodeDirective = '@';
		public const char OperatorCodeExpression = '=';
		public const char OperatorCodeDeclaration = '!';
		public const char OperatorCodeDataBind = '#';
		public const char OperatorCodeExtension = '$';

		#endregion Operators

		#region Reusable Tokens

		public static readonly Token<MarkupTokenType> TokenNone = new Token<MarkupTokenType>(MarkupTokenType.None);

		public static Token<MarkupTokenType> TokenUnparsed(string name, string text)
		{
			return new Token<MarkupTokenType>(MarkupTokenType.UnparsedBlock, new DataName(name), text);
		}

		public static Token<MarkupTokenType> TokenElementBegin(DataName name)
		{
			return new Token<MarkupTokenType>(MarkupTokenType.ElementBegin, name, MarkupTagType.BeginTag);
		}

		public static Token<MarkupTokenType> TokenElementBegin(DataName name, MarkupTagType tagType)
		{
			return new Token<MarkupTokenType>(MarkupTokenType.ElementBegin, name, tagType);
		}

		public static Token<MarkupTokenType> TokenElementEnd(DataName name)
		{
			return new Token<MarkupTokenType>(MarkupTokenType.ElementEnd, name, MarkupTagType.EndTag);
		}

		public static Token<MarkupTokenType> TokenAttribute(DataName name)
		{
			return new Token<MarkupTokenType>(MarkupTokenType.Attribute, name);
		}

		public static Token<MarkupTokenType> TokenText(char ch)
		{
			return new Token<MarkupTokenType>(MarkupTokenType.TextValue, ch);
		}

		public static Token<MarkupTokenType> TokenText(string text)
		{
			return new Token<MarkupTokenType>(MarkupTokenType.TextValue, text);
		}

		public static Token<MarkupTokenType> TokenWhitespace(string text)
		{
			return new Token<MarkupTokenType>(MarkupTokenType.Whitespace, text);
		}

		#endregion Reusable Tokens
	}
}
