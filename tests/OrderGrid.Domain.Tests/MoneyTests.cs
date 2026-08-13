using OrderGrid.Domain.Common;
using OrderGrid.Domain.ValueObjects;
namespace OrderGrid.Domain.Tests;
public sealed class MoneyTests
{
    [Fact] public void Rounds_and_normalizes_currency()
    { var money = new Money(10.125m, "jpy"); Assert.Equal(10.13m, money.Amount); Assert.Equal("JPY", money.Currency); }
    [Fact] public void Rejects_negative_amount() => Assert.Throws<DomainException>(() => new Money(-1, "JPY"));
    [Fact] public void Rejects_cross_currency_addition() => Assert.Throws<DomainException>(
        () => new Money(1, "JPY").Add(new Money(1, "USD")));
}
