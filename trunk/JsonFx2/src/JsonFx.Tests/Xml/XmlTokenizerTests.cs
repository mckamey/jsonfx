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

using JsonFx.Serialization;
using JsonFx.Serialization.Resolvers;
using JsonFx.Xml.Stax;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Xml
{
	public class XmlTokenizerTests
	{
		#region Constants

		private const string TraitName = "XML";
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
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenPrefixBegin("", "http://example.com/schema"),
			        StaxGrammar.TokenElementBegin(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenElementEnd(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.com/schema"),
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenPrefixBegin("prefix", "http://example.com/schema"),
			        StaxGrammar.TokenElementBegin(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenElementEnd(new DataName("root", "http://example.com/schema")),
			        StaxGrammar.TokenPrefixEnd("prefix", "http://example.com/schema"),
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenElementBegin(new DataName("foo")),
			        StaxGrammar.TokenPrefixBegin("", "http://example.com/schema"),
			        StaxGrammar.TokenElementBegin(new DataName("child", "http://example.com/schema")),
			        StaxGrammar.TokenText("value"),
			        StaxGrammar.TokenElementEnd(new DataName("child", "http://example.com/schema")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.com/schema"),
			        StaxGrammar.TokenElementEnd(new DataName("foo"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenPrefixBegin("", "http://example.org"),
			        StaxGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenElementBegin(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenText("value"),
			        StaxGrammar.TokenElementEnd(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.org")
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenPrefixBegin("bar", "http://example.org"),
			        StaxGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenElementBegin(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenText("value"),
			        StaxGrammar.TokenElementEnd(new DataName("child", "http://example.org")),
			        StaxGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenPrefixEnd("bar", "http://example.org")
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenPrefixBegin("", "http://json.org"),
			        StaxGrammar.TokenElementBegin(new DataName("foo", "http://json.org")),
			        StaxGrammar.TokenPrefixBegin("", "http://jsonfx.net"),
			        StaxGrammar.TokenElementBegin(new DataName("child", "http://jsonfx.net")),
			        StaxGrammar.TokenText("text value"),
			        StaxGrammar.TokenElementEnd(new DataName("child", "http://jsonfx.net")),
			        StaxGrammar.TokenPrefixEnd("", "http://jsonfx.net"),
			        StaxGrammar.TokenElementEnd(new DataName("foo", "http://json.org")),
			        StaxGrammar.TokenPrefixEnd("", "http://json.org")
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenPrefixBegin("", "http://example.org"),
			        StaxGrammar.TokenPrefixBegin("blah", "http://example.org"),
			        StaxGrammar.TokenElementBegin(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenAttribute(new DataName("key", "http://example.org")),
			        StaxGrammar.TokenText("value"),
			        StaxGrammar.TokenElementEnd(new DataName("foo", "http://example.org")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.org"),
			        StaxGrammar.TokenPrefixEnd("blah", "http://example.org")
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenPrefixBegin("", "http://example.org/outer"),
			        StaxGrammar.TokenElementBegin(new DataName("outer", "http://example.org/outer")),
			        StaxGrammar.TokenPrefixBegin("", "http://example.org/inner"),
			        StaxGrammar.TokenElementBegin(new DataName("middle-1", "http://example.org/inner")),
			        StaxGrammar.TokenElementBegin(new DataName("inner", "http://example.org/inner")),
			        StaxGrammar.TokenText("this should be inner"),
			        StaxGrammar.TokenElementEnd(new DataName("inner", "http://example.org/inner")),
			        StaxGrammar.TokenElementEnd(new DataName("middle-1", "http://example.org/inner")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.org/inner"),
			        StaxGrammar.TokenElementBegin(new DataName("middle-2", "http://example.org/outer")),
			        StaxGrammar.TokenText("this should be outer"),
			        StaxGrammar.TokenElementEnd(new DataName("middle-2", "http://example.org/outer")),
			        StaxGrammar.TokenElementEnd(new DataName("outer", "http://example.org/outer")),
			        StaxGrammar.TokenPrefixEnd("", "http://example.org/outer")
			    };

			var tokenizer = new XmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_UndeclaredPrefixes_ThrowsDeserializationException()
		{
			const string input = @"<a:one><b:two><c:three></d:three></e:two></f:one>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("one")),
			        StaxGrammar.TokenElementBegin(new DataName("two")),
			        StaxGrammar.TokenElementBegin(new DataName("three")),
			        StaxGrammar.TokenElementEnd(new DataName("three")),
			        StaxGrammar.TokenElementEnd(new DataName("two")),
			        StaxGrammar.TokenElementEnd(new DataName("one"))
			    };

			var tokenizer = new XmlTokenizer();

			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate()
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			Assert.Equal(2, ex.Index);
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
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("attrName")),
			        StaxGrammar.TokenText("attrValue"),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("noValue")),
			        StaxGrammar.TokenText(String.Empty),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("emptyValue")),
			        StaxGrammar.TokenText(String.Empty),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("white")),
			        StaxGrammar.TokenText(" extra whitespace around quote delims "),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("whitespace")),
			        StaxGrammar.TokenText(" this contains whitespace "),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_MultipleAttributes_ReturnsSequence()
		{
			const string input = @"<root no-value="""" whitespace="" this contains whitespace "" anyQuotedText="""+"/\\\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\n\r\t`1~!@#$%^&amp;*()_+-=[]{}|;:',./&lt;&gt;?"+@"""></root>";
			var expected = new[]
			    {
			        StaxGrammar.TokenElementBegin(new DataName("root")),
			        StaxGrammar.TokenAttribute(new DataName("anyQuotedText")),
			        StaxGrammar.TokenText("/\\\uCAFE\uBABE\uAB98\uFCDE\uBCDA\uEF4A   `1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
			        StaxGrammar.TokenAttribute(new DataName("no-value")),
			        StaxGrammar.TokenText(String.Empty),
			        StaxGrammar.TokenAttribute(new DataName("whitespace")),
			        StaxGrammar.TokenText(" this contains whitespace "),
			        StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenText("<")
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenText("B")
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenText("7")
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenText("\uABCD")
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenText("\uabcd")
			    };

			var tokenizer = new XmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_HtmlEntityEuro_ReturnsSequence()
		{
			const string input = @"&#x20AC;";
			var expected = new[]
			    {
			        StaxGrammar.TokenText("€")
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenText("leading&")
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenText("&trailing")
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenText(@"there should <b>e decoded chars & inside this text")
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenElementBegin(new DataName("div")),
			        StaxGrammar.TokenAttribute(new DataName("class")),
			        StaxGrammar.TokenText("content"),
			        StaxGrammar.TokenElementBegin(new DataName("p")),
			        StaxGrammar.TokenAttribute(new DataName("style")),
			        StaxGrammar.TokenText("color:red"),
			        StaxGrammar.TokenElementBegin(new DataName("strong")),
			        StaxGrammar.TokenText("Lorem ipsum"),
			        StaxGrammar.TokenElementEnd(new DataName("strong")),
			        StaxGrammar.TokenText(" dolor sit amet, "),
			        StaxGrammar.TokenElementBegin(new DataName("i")),
			        StaxGrammar.TokenText("consectetur"),
			        StaxGrammar.TokenElementEnd(new DataName("i")),
			        StaxGrammar.TokenText(" adipiscing elit."),
			        StaxGrammar.TokenElementEnd(new DataName("p")),
					StaxGrammar.TokenElementEnd(new DataName("div")),
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenElementBegin(new DataName("div")),
			        StaxGrammar.TokenAttribute(new DataName("class")),
			        StaxGrammar.TokenText("content"),
			        StaxGrammar.TokenWhitespace("\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("p")),
			        StaxGrammar.TokenAttribute(new DataName("style")),
			        StaxGrammar.TokenText("color:red"),
			        StaxGrammar.TokenWhitespace("\n\t\t"),
			        StaxGrammar.TokenElementBegin(new DataName("strong")),
			        StaxGrammar.TokenText("Lorem ipsum"),
			        StaxGrammar.TokenElementEnd(new DataName("strong")),
			        StaxGrammar.TokenText(" dolor sit amet, "),
			        StaxGrammar.TokenElementBegin(new DataName("i")),
			        StaxGrammar.TokenText("consectetur"),
			        StaxGrammar.TokenElementEnd(new DataName("i")),
			        StaxGrammar.TokenText(" adipiscing elit.\n\t"),
			        StaxGrammar.TokenElementEnd(new DataName("p")),
			        StaxGrammar.TokenWhitespace("\n"),
					StaxGrammar.TokenElementEnd(new DataName("div")),
			    };

			var tokenizer = new XmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Mixed Content Tests

		#region Unparsed Block Tests Tests

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_XmlDeclaration_ReturnsUnparsed()
		{
			const string input = @"<?xml version=""1.0""?>";
			var expected = new[]
			    {
			        StaxGrammar.TokenUnparsed("?{0}?", @"xml version=""1.0""")
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenUnparsed("!--{0}--", @" a quick note ")
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenText(@"value>""0"" && value<""10"" ?""valid"":""error""")
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenElementBegin(new DataName("p")),
			        StaxGrammar.TokenText(@"You can add a string to a number, but this stringifies the number:"),
			        StaxGrammar.TokenElementEnd(new DataName("p")),
			        StaxGrammar.TokenWhitespace("\n"),
			        StaxGrammar.TokenElementBegin(new DataName("math")),
			        StaxGrammar.TokenWhitespace("\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("ms")),
			        StaxGrammar.TokenText(@"x<y"),
			        StaxGrammar.TokenElementEnd(new DataName("ms")),
			        StaxGrammar.TokenWhitespace("\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("mo")),
			        StaxGrammar.TokenText(@"+"),
			        StaxGrammar.TokenElementEnd(new DataName("mo")),
			        StaxGrammar.TokenWhitespace("\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("mn")),
			        StaxGrammar.TokenText(@"3"),
			        StaxGrammar.TokenElementEnd(new DataName("mn")),
			        StaxGrammar.TokenWhitespace("\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("mo")),
			        StaxGrammar.TokenText(@"="),
			        StaxGrammar.TokenElementEnd(new DataName("mo")),
			        StaxGrammar.TokenWhitespace("\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("ms")),
			        StaxGrammar.TokenText(@"x<y3"),
			        StaxGrammar.TokenElementEnd(new DataName("ms")),
			        StaxGrammar.TokenWhitespace("\n"),
			        StaxGrammar.TokenElementEnd(new DataName("math")),
			    };

			var tokenizer = new XmlTokenizer();
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
	""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">
<root />";

			var tokenizer = new XmlTokenizer();
			DeserializationException ex = Assert.Throws<DeserializationException>(
				delegate()
				{
					var actual = tokenizer.GetTokens(input).ToArray();
				});

			Assert.Equal(0, ex.Index);
		}

		//[Fact(Skip="Embedded DOCTYPE not supported")]
		public void GetTokens_XmlDocTypeLocal_ReturnsUnparsed()
		{
			const string input =
@"<!DOCTYPE doc [
	<!ATTLIST normId id ID #IMPLIED>
	<!ATTLIST normNames attr NMTOKENS #IMPLIED>
]>
<root />";
			var expected = new[]
			    {
			        StaxGrammar.TokenUnparsed("!{0}",
@"DOCTYPE doc [
	<!ATTLIST normId id ID #IMPLIED>
	<!ATTLIST normNames attr NMTOKENS #IMPLIED>
]"),
					StaxGrammar.TokenElementBegin(new DataName("root")),
					StaxGrammar.TokenElementEnd(new DataName("root"))
			    };

			var tokenizer = new XmlTokenizer();
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
			        StaxGrammar.TokenElementBegin(new DataName("html")),
			        StaxGrammar.TokenWhitespace("\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("head")),
			        StaxGrammar.TokenWhitespace("\n\t\t"),
			        StaxGrammar.TokenElementBegin(new DataName("title")),
			        StaxGrammar.TokenText("PHP Test"),
			        StaxGrammar.TokenElementEnd(new DataName("title")),
			        StaxGrammar.TokenWhitespace("\n\t"),
			        StaxGrammar.TokenElementEnd(new DataName("head")),
			        StaxGrammar.TokenWhitespace("\n\t"),
			        StaxGrammar.TokenElementBegin(new DataName("body")),
			        StaxGrammar.TokenWhitespace("\n\t\t"),
			        StaxGrammar.TokenUnparsed("?{0}?", @"php echo '<p>Hello World</p>'; "),
			        StaxGrammar.TokenWhitespace("\n\t"),
			        StaxGrammar.TokenElementEnd(new DataName("body")),
			        StaxGrammar.TokenWhitespace("\n"),
			        StaxGrammar.TokenElementEnd(new DataName("html")),
			    };

			var tokenizer = new XmlTokenizer();
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
			var expected = new Token<StaxTokenType>[0];

			var tokenizer = new XmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		[Fact]
		[Trait(TraitName, TraitValue)]
		public void GetTokens_EmptyInput_ReturnsEmptySequence()
		{
			const string input = "";
			var expected = new Token<StaxTokenType>[0];

			var tokenizer = new XmlTokenizer();
			var actual = tokenizer.GetTokens(input).ToArray();

			Assert.Equal(expected, actual);
		}

		#endregion Input Edge Case Tests
	}
}
