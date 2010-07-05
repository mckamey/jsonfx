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
using System.Collections.Generic;
using System.Linq;

using JsonFx.Serialization;
using Xunit;
using Assert=JsonFx.AssertPatched;

namespace JsonFx.Json.Filters
{
	public class DateIso8601FilterTests
	{
		#region TryRead Tests

		[Fact]
		public void TryRead_UtcStandardTimeZone_ReadsAsUtc()
		{
			var expected = new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc);

			var input = new[]
				{
					JsonGrammar.TokenString("2008-02-29T23:59:59.999Z")
				};

			DateTime actual;
			Assert.True(new DateIso8601Filter().TryRead(new DataReaderSettings(), input, out actual));

			Assert.Equal(expected.Kind, actual.Kind);
			Assert.Equal(expected.Ticks, actual.Ticks);
		}

		[Fact]
		public void TryRead_UnspecifiedStandardTimeZone_ReadsAsUnspecified()
		{
			var input = new[]
				{
					JsonGrammar.TokenString("2008-02-29T23:59:59.999")
				};

			var expected = new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Unspecified);

			DateTime actual;
			Assert.True(new DateIso8601Filter().TryRead(new DataReaderSettings(), input, out actual));

			Assert.Equal(expected.Kind, actual.Kind);
			Assert.Equal(expected.Ticks, actual.Ticks);
		}

		[Fact]
		public void TryRead_EasternStandardTimeZone_ReadsAsUtc()
		{
			var input = new[]
				{
					JsonGrammar.TokenString("2008-02-29T23:59:59.999-06:00")
				};

			var expected = new DateTime(2008, 3, 01, 05, 59, 59, 999, DateTimeKind.Utc);

			DateTime actual;
			Assert.True(new DateIso8601Filter().TryRead(new DataReaderSettings(), input, out actual));

			Assert.Equal(expected.Kind, actual.Kind);
			Assert.Equal(expected.Ticks, actual.Ticks);
		}

		[Fact]
		public void TryRead_UtcDaylightSavingsTimeZone_ReadsAsUtc()
		{
			var expected = new DateTime(2010, 7, 5, 10, 51, 17, 768, DateTimeKind.Utc);

			var input = new[]
				{
					JsonGrammar.TokenString("2010-07-05T10:51:17.768Z")
				};

			DateTime actual;
			Assert.True(new DateIso8601Filter().TryRead(new DataReaderSettings(), input, out actual));

			Assert.Equal(expected.Kind, actual.Kind);
			Assert.Equal(expected.Ticks, actual.Ticks);
		}

		[Fact]
		public void TryRead_UnspecifiedDaylightSavingsTimeZone_ReadsAsUnspecified()
		{
			var input = new[]
				{
					JsonGrammar.TokenString("2010-07-05T10:51:17.768")
				};

			var expected = new DateTime(2010, 7, 5, 10, 51, 17, 768, DateTimeKind.Unspecified);

			DateTime actual;
			Assert.True(new DateIso8601Filter().TryRead(new DataReaderSettings(), input, out actual));

			Assert.Equal(expected.Kind, actual.Kind);
			Assert.Equal(expected.Ticks, actual.Ticks);
		}

		[Fact]
		public void TryRead_EasternDaylightSavingsTimeZone_ReadsAsUtc()
		{
			var input = new[]
				{
					JsonGrammar.TokenString("2010-07-05T10:51:17.768-05:00")
				};

			var expected = new DateTime(2010, 7, 5, 15, 51, 17, 768, DateTimeKind.Utc);

			DateTime actual;
			Assert.True(new DateIso8601Filter().TryRead(new DataReaderSettings(), input, out actual));

			Assert.Equal(expected.Kind, actual.Kind);
			Assert.Equal(expected.Ticks, actual.Ticks);
		}

		#endregion TryRead Tests

		#region TryWrite Tests

		[Fact]
		public void TryWrite_UtcStandardTimeZone_DirectlyWritesAsUtc()
		{
			var input = new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc);

			var expected = new[]
				{
					JsonGrammar.TokenString("2008-02-29T23:59:59.999Z")
				};

			IEnumerable<Token<JsonTokenType>> actual;
			Assert.True(new DateIso8601Filter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		public void TryWrite_LocalStandardTimeZone_ConvertsToUtcAndWritesAsUtc()
		{
			var input = new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Local);

			var expected = new[]
				{
					JsonGrammar.TokenString("2008-03-01T07:59:59.999Z")
				};

			IEnumerable<Token<JsonTokenType>> actual;
			Assert.True(new DateIso8601Filter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		public void TryWrite_UnspecifiedStandardTimeZone_DirectlyWritesAsClientLocal()
		{
			var input = new DateTime(2008, 2, 29, 23, 59, 59, 999, DateTimeKind.Unspecified);

			var expected = new[]
				{
					JsonGrammar.TokenString("2008-02-29T23:59:59.999")
				};

			IEnumerable<Token<JsonTokenType>> actual;
			Assert.True(new DateIso8601Filter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		public void TryWrite_UtcDaylightSavingsTimeZone_DirectlyWritesAsUtc()
		{
			var input = new DateTime(2010, 7, 5, 10, 51, 17, 768, DateTimeKind.Utc);

			var expected = new[]
				{
					JsonGrammar.TokenString("2010-07-05T10:51:17.768Z")
				};

			IEnumerable<Token<JsonTokenType>> actual;
			Assert.True(new DateIso8601Filter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		public void TryWrite_LocalDaylightSavingsTimeZone_ConvertsToUtcAndWritesAsUtc()
		{
			var input = new DateTime(2010, 7, 5, 10, 51, 17, 768, DateTimeKind.Local);

			var expected = new[]
				{
					JsonGrammar.TokenString("2010-07-05T17:51:17.768Z")
				};

			IEnumerable<Token<JsonTokenType>> actual;
			Assert.True(new DateIso8601Filter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		public void TryWrite_UnspecifiedDaylightSavingsTimeZone_DirectlyWritesAsClientLocal()
		{
			var input = new DateTime(2010, 7, 5, 10, 51, 17, 768, DateTimeKind.Unspecified);

			var expected = new[]
				{
					JsonGrammar.TokenString("2010-07-05T10:51:17.768")
				};

			IEnumerable<Token<JsonTokenType>> actual;
			Assert.True(new DateIso8601Filter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		public void TryWrite_DateTimeMinValue_DirectlyWritesAsClientLocal()
		{
			var input = DateTime.MinValue;

			var expected = new[]
				{
					JsonGrammar.TokenString("0001-01-01T00:00:00.000")
				};

			IEnumerable<Token<JsonTokenType>> actual;
			Assert.True(new DateIso8601Filter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		[Fact]
		public void TryWrite_DateTimeMaxValue_DirectlyWritesAsClientLocal()
		{
			var input = DateTime.MaxValue;

			var expected = new[]
				{
					JsonGrammar.TokenString("9999-12-31T23:59:59.999")
				};

			IEnumerable<Token<JsonTokenType>> actual;
			Assert.True(new DateIso8601Filter().TryWrite(new DataWriterSettings(), input, out actual));

			Assert.NotNull(actual);
			Assert.Equal(expected.Count(), actual.Count());
			Assert.Equal(expected[0].TokenType, actual.First().TokenType);
			Assert.Equal(expected[0].Value, actual.First().Value);
		}

		#endregion TryWrite Tests
	}
}
