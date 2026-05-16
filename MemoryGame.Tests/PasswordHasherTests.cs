using Xunit;
using MemoryGame.Security;

namespace MemoryGame.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ReturnsThreePartFormat()
    {
        var stored = PasswordHasher.Hash("mypassword");
        var parts = stored.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public void Hash_IterationCountMatchesDefault()
    {
        var stored = PasswordHasher.Hash("mypassword");
        var iterations = int.Parse(stored.Split('.')[0]);
        Assert.Equal(100_000, iterations);
    }

    [Fact]
    public void Verify_ReturnsTrueForCorrectPassword()
    {
        var stored = PasswordHasher.Hash("correct-horse");
        Assert.True(PasswordHasher.Verify("correct-horse", stored));
    }

    [Fact]
    public void Verify_ReturnsFalseForWrongPassword()
    {
        var stored = PasswordHasher.Hash("correct-horse");
        Assert.False(PasswordHasher.Verify("wrong-password", stored));
    }

    [Fact]
    public void Verify_ReturnsFalseForMalformedStoredString()
    {
        Assert.False(PasswordHasher.Verify("anything", "not-a-valid-hash"));
    }

    [Fact]
    public void Verify_ReturnsFalseForEmptyStoredString()
    {
        Assert.False(PasswordHasher.Verify("anything", ""));
    }

    [Fact]
    public void Hash_ProducesDifferentSaltsEachCall()
    {
        var hash1 = PasswordHasher.Hash("same-password");
        var hash2 = PasswordHasher.Hash("same-password");
        // Salts (part[1]) must differ because they are randomly generated
        Assert.NotEqual(hash1.Split('.')[1], hash2.Split('.')[1]);
    }

    [Fact]
    public void Verify_WorksWithCustomIterationCount()
    {
        var stored = PasswordHasher.Hash("pass", iterations: 1_000);
        Assert.True(PasswordHasher.Verify("pass", stored));
    }
}
