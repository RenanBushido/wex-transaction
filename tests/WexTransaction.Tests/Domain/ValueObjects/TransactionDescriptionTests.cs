namespace WexTransaction.Tests.Domain.ValueObjects;

public class TransactionDescriptionTests
{
    [Fact]
    public void Create_WithSingleCharacter_Succeeds()
    {
        var desc = new TransactionDescription("A");
        Assert.Equal("A", desc.Value);
    }

    [Fact]
    public void Create_WithExactly50Characters_Succeeds()
    {
        var value = new string('x', 50);
        var desc = new TransactionDescription(value);
        Assert.Equal(value, desc.Value);
    }

    [Fact]
    public void Create_WithNullDescription_ThrowsInvalidDescriptionException()
    {
        Assert.Throws<InvalidDescriptionException>(() => new TransactionDescription(null!));
    }

    [Fact]
    public void Create_WithEmptyDescription_ThrowsInvalidDescriptionException()
    {
        Assert.Throws<InvalidDescriptionException>(() => new TransactionDescription(string.Empty));
    }

    [Fact]
    public void Create_WithWhitespaceOnlyDescription_ThrowsInvalidDescriptionException()
    {
        Assert.Throws<InvalidDescriptionException>(() => new TransactionDescription("   "));
    }

    [Fact]
    public void Create_With51Characters_ThrowsInvalidDescriptionException()
    {
        var value = new string('x', 51);
        Assert.Throws<InvalidDescriptionException>(() => new TransactionDescription(value));
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        var desc = new TransactionDescription("test");
        string result = desc;
        Assert.Equal("test", result);
    }
}
