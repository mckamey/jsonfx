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
using System.Linq;

using JsonFx.Markup;
using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Html
{
	public class HtmlTokenizerTests
	{
		#region Constants

		private const string TraitName = "HTML";
		private const string TraitValue = "Deserialization";

		#endregion Constants

		#region Simple Single Element Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleOpenCloseTag_ReturnsSequence()
		{
			const string input = @"<root></root>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleVoidTag_ReturnsSequence()
		{
			const string input = @"<root />";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementVoid(new DataName("root"))
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Simple Single Element Tests

		#region Namespace Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_DefaultNamespaceTag_ReturnsSequence()
		{
			const string input = @"<root xmlns=""http://example.com/schema""></root>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root", String.Empty, "http://example.com/schema")),
			        MarkupGrammar.TokenElementEnd,
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NamespacePrefixTag_ReturnsSequence()
		{
			const string input = @"<prefix:root xmlns:prefix=""http://example.com/schema""></prefix:root>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root", "prefix", "http://example.com/schema")),
			        MarkupGrammar.TokenElementEnd,
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NamespacedChildTag_ReturnsSequence()
		{
			const string input = @"<foo><child xmlns=""http://example.com/schema"">value</child></foo>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://example.com/schema")),
			        MarkupGrammar.TokenValue("value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ParentAndChildShareDefaultNamespace_ReturnsSequence()
		{
			const string input = @"<foo xmlns=""http://example.org""><child>value</child></foo>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenValue("value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ParentAndChildSharePrefixedNamespace_ReturnsSequence()
		{
			const string input = @"<bar:foo xmlns:bar=""http://example.org""><bar:child>value</bar:child></bar:foo>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", "bar", "http://example.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", "bar", "http://example.org")),
			        MarkupGrammar.TokenValue("value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_ParentAndChildDifferentDefaultNamespaces_ReturnsSequence()
		{
			const string input = @"<foo xmlns=""http://json.org""><child xmlns=""http://jsonfx.net"">text value</child></foo>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("foo", String.Empty, "http://json.org")),
			        MarkupGrammar.TokenElementBegin(new DataName("child", String.Empty, "http://jsonfx.net")),
			        MarkupGrammar.TokenValue("text value"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_DifferentPrefixSameNamespace_ReturnsSequence()
		{
			const string input = @"<foo xmlns=""http://example.org"" xmlns:blah=""http://example.org"" blah:key=""value"" />";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementVoid(new DataName("foo", String.Empty, "http://example.org")),
			        MarkupGrammar.TokenAttribute(new DataName("key", "blah", "http://example.org")),
			        MarkupGrammar.TokenValue("value")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NestedDefaultNamespaces_ReturnsSequence()
		{
			const string input = @"<outer xmlns=""http://example.org/outer""><middle-1 xmlns=""http://example.org/inner""><inner>this should be inner</inner></middle-1><middle-2>this should be outer</middle-2></outer>";

			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("outer", String.Empty, "http://example.org/outer")),
			        MarkupGrammar.TokenElementBegin(new DataName("middle-1", String.Empty, "http://example.org/inner")),
			        MarkupGrammar.TokenElementBegin(new DataName("inner", String.Empty, "http://example.org/inner")),
			        MarkupGrammar.TokenValue("this should be inner"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementBegin(new DataName("middle-2", String.Empty, "http://example.org/outer")),
			        MarkupGrammar.TokenValue("this should be outer"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UndeclaredPrefixes_ReturnsDefault()
		{
			const string input = @"<a:one><b:two><c:three></d:three></e:two></f:one>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("one")),
			        MarkupGrammar.TokenElementBegin(new DataName("two")),
			        MarkupGrammar.TokenElementBegin(new DataName("three")),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Namespace Tests

		#region Simple Attribute Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleAttribute_ReturnsSequence()
		{
			const string input = @"<root attrName=""attrValue""></root>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("attrName")),
			        MarkupGrammar.TokenValue("attrValue"),
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleEmptyAttributeHtmlStyle_ReturnsSequence()
		{
			const string input = @"<root noValue></root>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("noValue")),
			        MarkupGrammar.TokenValue(String.Empty),
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleEmptyAttributeXmlStyle_ReturnsSequence()
		{
			const string input = @"<root noValue=""""></root>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("noValue")),
			        MarkupGrammar.TokenValue(String.Empty),
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleAttributeEmptyValue_ReturnsSequence()
		{
			const string input = @"<root emptyValue=""""></root>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("emptyValue")),
			        MarkupGrammar.TokenValue(String.Empty),
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_AttributeWhitespaceQuotDelims_ReturnsSequence()
		{
			const string input = @"<root white  =  "" extra whitespace around quote delims "" ></root>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("white")),
			        MarkupGrammar.TokenValue(" extra whitespace around quote delims "),
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_WhitespaceAttributeAposDelims_ReturnsSequence()
		{
			const string input = @"<root white  =  ' extra whitespace around apostrophe delims ' ></root>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("white")),
			        MarkupGrammar.TokenValue(" extra whitespace around apostrophe delims "),
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleAttributeWhitespace_ReturnsSequence()
		{
			const string input = @"<root whitespace="" this contains whitespace ""></root>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("whitespace")),
			        MarkupGrammar.TokenValue(" this contains whitespace "),
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleAttributeSingleQuoted_ReturnsSequence()
		{
			const string input = @"<root singleQuoted='apostrophe'></root>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("singleQuoted")),
			        MarkupGrammar.TokenValue("apostrophe"),
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_SingleAttributeSingleQuotedWhitespace_ReturnsSequence()
		{
			const string input = @"<root singleQuoted_whitespace=' apostrophe with whitespace '></root>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("singleQuoted_whitespace")),
			        MarkupGrammar.TokenValue(" apostrophe with whitespace "),
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MultipleAttributes_ReturnsSequence()
		{
			const string input = @"<root no-value whitespace="" this contains whitespace "" anyQuotedText="""+"/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"+@"""></root>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenAttribute(new DataName("no-value")),
			        MarkupGrammar.TokenValue(String.Empty),
			        MarkupGrammar.TokenAttribute(new DataName("whitespace")),
			        MarkupGrammar.TokenValue(" this contains whitespace "),
			        MarkupGrammar.TokenAttribute(new DataName("anyQuotedText")),
			        MarkupGrammar.TokenValue("/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Simple Attribute Tests

		#region Text Content Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlEntityLt_ReturnsSequence()
		{
			const string input = @"&lt;";
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue("<")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlEntityB_ReturnsSequence()
		{
			const string input = @"&#66;";
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue("B")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlEntityHexLowerX_ReturnsSequence()
		{
			const string input = @"&#x37;";
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue("7")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlEntityHexUpperX_ReturnsSequence()
		{
			const string input = @"&#X38;";
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue("8")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlEntityHexUpperCase_ReturnsSequence()
		{
			const string input = @"&#xABCD;";
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue("\uABCD")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlEntityHexLowerCase_ReturnsSequence()
		{
			const string input = @"&#xabcd;";
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue("\uabcd")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_HtmlEntityEuro_ReturnsSequence()
		{
			const string input = @"&euro;";
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue("€")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EntityWithLeadingText_ReturnsSequence()
		{
			const string input = @"leading&amp;";
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue("leading"),
			        MarkupGrammar.TokenValue("&")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EntityWithTrailingText_ReturnsSequence()
		{
			const string input = @"&amp;trailing";
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue("&"),
			        MarkupGrammar.TokenValue("trailing")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MixedEntities_ReturnsSequence()
		{
			const string input = @"there should &lt;b&gt;e decoded chars &amp; inside this text";
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue(@"there should "),
			        MarkupGrammar.TokenValue(@"<"),
			        MarkupGrammar.TokenValue(@"b"),
			        MarkupGrammar.TokenValue(@">"),
			        MarkupGrammar.TokenValue(@"e decoded chars "),
			        MarkupGrammar.TokenValue(@"&"),
			        MarkupGrammar.TokenValue(@" inside this text")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MixedEntitiesMalformed_ReturnsSequence()
		{
			const string input = @"there should &#xnot &Xltb&#gte decoded chars & inside this text";
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue(@"there should "),
			        MarkupGrammar.TokenValue(@"&#x"),
			        MarkupGrammar.TokenValue(@"not "),
			        MarkupGrammar.TokenValue(@"&Xltb"),
			        MarkupGrammar.TokenValue(@"&#"),
			        MarkupGrammar.TokenValue(@"gte decoded chars "),
			        MarkupGrammar.TokenValue(@"&"),
			        MarkupGrammar.TokenValue(@" inside this text")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Text Content Tests

		#region Mixed Content Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_HtmlContent_ReturnsSequence()
		{
			const string input = @"<div class=""content""><p style=""color:red""><strong>Lorem ipsum</strong> dolor sit amet, <i>consectetur</i> adipiscing elit.</p></div>";

			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("div")),
			        MarkupGrammar.TokenAttribute(new DataName("class")),
			        MarkupGrammar.TokenValue("content"),
			        MarkupGrammar.TokenElementBegin(new DataName("p")),
			        MarkupGrammar.TokenAttribute(new DataName("style")),
			        MarkupGrammar.TokenValue("color:red"),
			        MarkupGrammar.TokenElementBegin(new DataName("strong")),
			        MarkupGrammar.TokenValue("Lorem ipsum"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue(" dolor sit amet, "),
			        MarkupGrammar.TokenElementBegin(new DataName("i")),
			        MarkupGrammar.TokenValue("consectetur"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue(" adipiscing elit."),
			        MarkupGrammar.TokenElementEnd,
					MarkupGrammar.TokenElementEnd,
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_HtmlContentPrettyPrinted_ReturnsSequence()
		{
			const string input =
@"<div class=""content"">
	<p style=""color:red"">
		<strong>Lorem ipsum</strong> dolor sit amet, <i>consectetur</i> adipiscing elit.
	</p>
</div>";

			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("div")),
			        MarkupGrammar.TokenAttribute(new DataName("class")),
			        MarkupGrammar.TokenValue("content"),
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("p")),
			        MarkupGrammar.TokenAttribute(new DataName("style")),
			        MarkupGrammar.TokenValue("color:red"),
			        MarkupGrammar.TokenValue("\r\n\t\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("strong")),
			        MarkupGrammar.TokenValue("Lorem ipsum"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue(" dolor sit amet, "),
			        MarkupGrammar.TokenElementBegin(new DataName("i")),
			        MarkupGrammar.TokenValue("consectetur"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue(" adipiscing elit.\r\n\t"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n"),
					MarkupGrammar.TokenElementEnd,
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Mixed Content Tests

		#region Error Recovery Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UnclosedOpenTag_ReturnsSequence()
		{
			const string input = @"<root>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root"))
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UnclosedOpenTagAutoBalance_ReturnsSequence()
		{
			const string input = @"<root>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("root")),
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer { AutoBalanceTags=true };
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UnopenedCloseTag_ReturnsSequence()
		{
			const string input = @"</foo>";
			var expected = new []
				{
					MarkupGrammar.TokenElementEnd
				};

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UnopenedCloseTagAutoBalance_ReturnsSequence()
		{
			const string input = @"</foo>";
			var expected = new Token<MarkupTokenType>[0];

			var tokenizer = new HtmlTokenizer { AutoBalanceTags=true };
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_OverlappingTags_ReturnsSequenceAsIs()
		{
			const string input = @"<odd><auto-closed><even></odd></ignored></even>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("odd")),
			        MarkupGrammar.TokenElementBegin(new DataName("auto-closed")),
			        MarkupGrammar.TokenElementBegin(new DataName("even")),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_OverlappingTagsAutoBalancing_ReturnsSequenceRebalanced()
		{
			const string input = @"<odd><auto-closed><even></odd></ignored></even>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("odd")),
			        MarkupGrammar.TokenElementBegin(new DataName("auto-closed")),
			        MarkupGrammar.TokenElementBegin(new DataName("even")),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer { AutoBalanceTags=true };
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_OverlappingNamespacedTagsErrorRecovery_ReturnsSequenceAsIs()
		{
			const string input = @"<a:odd xmlns=""http://example.com/odd"" xmlns:a=""http://example.com/odd/a""><b:auto-closed xmlns=""http://example.com/auto-closed"" xmlns:b=""http://example.com/auto-closed/b""><c:even xmlns=""http://example.com/even"" xmlns:c=""http://example.com/even/c""></a:odd></d:ignored></c:even>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("odd", "a", "http://example.com/odd/a")),
			        MarkupGrammar.TokenElementBegin(new DataName("auto-closed", "b", "http://example.com/auto-closed/b")),
			        MarkupGrammar.TokenElementBegin(new DataName("even", "c", "http://example.com/even/c")),
			        MarkupGrammar.TokenElementEnd,
			        // NOTE: skips prefix ending for odd because can't know odd declared them
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_OverlappingNamespacedTagsErrorRecoveryAutoBalancing_ReturnsSequenceAsIs()
		{
			const string input = @"<a:odd xmlns=""http://example.com/odd"" xmlns:a=""http://example.com/odd/a""><b:auto-closed xmlns=""http://example.com/auto-closed"" xmlns:b=""http://example.com/auto-closed/b""><c:even xmlns=""http://example.com/even"" xmlns:c=""http://example.com/even/c""></a:odd></d:ignored></c:even>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("odd", "a", "http://example.com/odd/a")),
			        MarkupGrammar.TokenElementBegin(new DataName("auto-closed", "b", "http://example.com/auto-closed/b")),
			        MarkupGrammar.TokenElementBegin(new DataName("even", "c", "http://example.com/even/c")),
					MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenElementEnd
			    };

			var tokenizer = new HtmlTokenizer { AutoBalanceTags=true };
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Error Recovery Tests

		#region Unparsed Block Tests Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlDeclaration_ReturnsUnparsed()
		{
			const string input = @"<?xml version=""1.0""?>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenUnparsed("?{0}?", @"xml version=""1.0""")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlComment_ReturnsUnparsed()
		{
			const string input = @"<!-- a quick note -->";
			var expected = new[]
			    {
			        MarkupGrammar.TokenUnparsed("!--{0}--", @" a quick note ")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlCData_ReturnsTextValue()
		{
			const string input = @"<![CDATA[value>""0"" && value<""10"" ?""valid"":""error""]]>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenValue(@"value>""0"" && value<""10"" ?""valid"":""error""")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlCDataMixed_ReturnsTextValue()
		{
			const string input =
@"<p>You can add a string to a number, but this stringifies the number:</p>
<math>
	<ms><![CDATA[x<y]]></ms>
	<mo>+</mo>
	<mn>3</mn>
	<mo>=</mo>
	<ms><![CDATA[x<y3]]></ms>
</math>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("p")),
			        MarkupGrammar.TokenValue(@"You can add a string to a number, but this stringifies the number:"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n"),
			        MarkupGrammar.TokenElementBegin(new DataName("math")),
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("ms")),
			        MarkupGrammar.TokenValue(@"x<y"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("mo")),
			        MarkupGrammar.TokenValue(@"+"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("mn")),
			        MarkupGrammar.TokenValue(@"3"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("mo")),
			        MarkupGrammar.TokenValue(@"="),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("ms")),
			        MarkupGrammar.TokenValue(@"x<y3"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n"),
			        MarkupGrammar.TokenElementEnd,
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlDocTypeExternal_ReturnsUnparsed()
		{
			const string input =
@"<!DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">";

			var expected = new[]
			    {
			        MarkupGrammar.TokenUnparsed("!{0}",
@"DOCTYPE html PUBLIC
	""-//W3C//DTD XHTML 1.1//EN""
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd""")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		//[Fact(Skip="Embedded DOCTYPE not supported")]
		public void GetTokens_XmlDocTypeLocal_ReturnsUnparsed()
		{
			const string input =
@"<!DOCTYPE doc [
	<!ATTLIST normId id ID #IMPLIED>
	<!ATTLIST normNames attr NMTOKENS #IMPLIED>
]>";
			var expected = new[]
			    {
			        MarkupGrammar.TokenUnparsed("!{0}",
@"DOCTYPE doc [
	<!ATTLIST normId id ID #IMPLIED>
	<!ATTLIST normNames attr NMTOKENS #IMPLIED>
]")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_AspNetPageDeclaration_ReturnsUnparsed()
		{
			const string input = @"<%@ Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" %>";

			var expected = new[]
			    {
			        MarkupGrammar.TokenUnparsed("%@{0}%", @" Page Language=""C#"" AutoEventWireup=""true"" CodeBehind=""Default.aspx.cs"" Inherits=""Foo._Default"" ")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_PhpHelloWorld_ReturnsSequence()
		{
			const string input =
@"<html>
	<head>
		<title>PHP Test</title>
	</head>
	<body>
		<?php echo '<p>Hello World</p>'; ?>
	</body>
</html>";

			var expected = new[]
			    {
			        MarkupGrammar.TokenElementBegin(new DataName("html")),
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("head")),
			        MarkupGrammar.TokenValue("\r\n\t\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("title")),
			        MarkupGrammar.TokenValue("PHP Test"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementBegin(new DataName("body")),
			        MarkupGrammar.TokenValue("\r\n\t\t"),
			        MarkupGrammar.TokenUnparsed("?{0}?", @"php echo '<p>Hello World</p>'; "),
			        MarkupGrammar.TokenValue("\r\n\t"),
			        MarkupGrammar.TokenElementEnd,
			        MarkupGrammar.TokenValue("\r\n"),
			        MarkupGrammar.TokenElementEnd,
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_CodeCommentAroundMarkup_ReturnsSingleUnparsedBlock()
		{
			const string input =
@"<%--
<html>
	<body style=""color:lime"">
		<!-- not much to say here -->
	</body>
</html>
--%>";

			var expected = new[]
			    {
			        MarkupGrammar.TokenUnparsed("%--{0}--%",
@"
<html>
	<body style=""color:lime"">
		<!-- not much to say here -->
	</body>
</html>
")
			    };

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Unparsed Block Tests

		#region Input Edge Case Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_NullInput_ReturnsEmptySequence()
		{
			const string input = null;
			var expected = new Token<MarkupTokenType>[0];

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			const string input = "";
			var expected = new Token<MarkupTokenType>[0];

			var tokenizer = new HtmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
