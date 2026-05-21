using AGS.SmartShift.Domain.Common;
using Xunit;

namespace AGS.SmartShift.Domain.UnitTests.Common;

public sealed class PasswordPolicyTests
{
    [Fact]
    public void Dev_seed_password_meets_policy()
    {
        Assert.True(PasswordPolicy.TryValidate("Ags@Dev2026", out var error), error);
    }

    [Theory]
    [InlineData("short1!A")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWERCASE1!")]
    [InlineData("NoDigitsHere!")]
    [InlineData("NoSpecialChar1A")]
    public void Rejects_weak_passwords(string password)
    {
        Assert.False(PasswordPolicy.TryValidate(password, out _));
    }
}
