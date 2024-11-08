using Xunit;
using OllamaSqlClr.Helpers;

namespace OllamaSqlClr.Tests.Helpers
{
    public class QueryValidatorTests
    {
        [Theory]
        [InlineData("SELECT * FROM test", true)]
        [InlineData("INSERT INTO test VALUES (1)", false)]
        [InlineData("DELETE FROM test", false)]
        public void IsSafeQuery_ShouldReturnExpectedResults(string query, bool expected)
        {
            // Arrange
            var validator = new QueryValidator();

            // Act
            var result = validator.IsSafeQuery(query);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("SELECT * FROM test", false)]
        [InlineData("INSERT INTO test VALUES (1)", false)]
        [InlineData("I can make no reply at all.", true)]
        public void IsNoReply_ShouldReturnExpectedResults(string query, bool expected)
        {
            // Arrange
            var validator = new QueryValidator();

            // Act
            var result = validator.IsNoReply(query);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}

