using Xunit;
using S1130.SystemObjects.Utility;
using System.Collections.Generic;

namespace UnitTests.S1130.SystemObjects.UtilityTests;
public class LexerTests
{
    private Lexer lexer = new Lexer();

    [Fact]
    public void Tokenize_NumberInput_ReturnsNumberToken()
    {
        var tokens = lexer.Tokenize("123");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal("123", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_IdentifierInput_ReturnsIdentifierToken()
    {
        var tokens = lexer.Tokenize("abc");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal("abc", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_OperatorInput_ReturnsOperatorToken()
    {
        var tokens = lexer.Tokenize("+");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Operator, tokens[0].Type);
        Assert.Equal("+", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_PunctuationInput_ReturnsPunctuationToken()
    {
        var tokens = lexer.Tokenize(";");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Punctuation, tokens[0].Type);
        Assert.Equal(";", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_UnknownInput_ReturnsUnknownToken()
    {
        var tokens = lexer.Tokenize("@");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Unknown, tokens[0].Type);
        Assert.Equal("@", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_MixedInput_ReturnsCorrectTokens()
    {
        var tokens = lexer.Tokenize("Acc == 4");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal("Acc", tokens[0].Value);
        Assert.Equal(TokenType.Operator, tokens[1].Type);
        Assert.Equal("==", tokens[1].Value);
        Assert.Equal(TokenType.Number, tokens[2].Type);
        Assert.Equal("4", tokens[2].Value);
    }
}