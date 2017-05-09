using System;
using System.Collections.Generic;
using Villermen.RuneScapeCacheTools.Extensions;
using Xunit;

namespace RuneScapeCacheToolsTests
{
    public class FormatterTests
    {
        public static IEnumerable<object[]> TestResultData = new[]
        {
            new object[] {"someString", "\"someString\""},
            new object[] {145, "145"},
            new object[] {145.23, "145.23"},
            new object[] {false, "FALSE"},
            new object[] {new object[] { 234, false, "someString"}, "[234,FALSE,\"someString\"]"},
            new object[] {new Tuple<int, string>(234, "someString"), "(234,\"someString\")"}
        };

        [Theory]
        [MemberData(nameof(FormatterTests.TestResultData))]
        public void TestResult(object value, string expectedRepresentation)
        {
            Assert.Equal(expectedRepresentation, Formatter.GetValueRepresentation(value));
        }
    }
}