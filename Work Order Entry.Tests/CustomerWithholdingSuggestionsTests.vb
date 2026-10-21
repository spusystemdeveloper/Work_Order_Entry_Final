Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class CustomerWithholdingSuggestionsTests
    <TestMethod>
    Public Sub OnlyTheExactCustomerFlagsEnableTheirRespectiveRates()
        CollectionAssert.AreEqual(New Integer() {1, 2, 5, 15},
            CustomerWithholdingSuggestions.EnabledRates(1, 2, 5, "15").ToArray())
        Assert.AreEqual(0, CustomerWithholdingSuggestions.EnabledRates(2, 1, 15, "5").Count)
        Assert.AreEqual(0, CustomerWithholdingSuggestions.EnabledRates(0, 0, 0, Nothing).Count)
        CollectionAssert.AreEqual(New Integer() {2, 15},
            CustomerWithholdingSuggestions.EnabledRates(0, 2, 0, " 15 ").ToArray())
    End Sub

    <TestMethod>
    Public Sub EachRateUsesTheSuppliedVatExclusiveAmount()
        Assert.AreEqual(9.82D, CustomerWithholdingSuggestions.Amount(982.14D, 1))
        Assert.AreEqual(19.64D, CustomerWithholdingSuggestions.Amount(982.14D, 2))
        Assert.AreEqual(49.11D, CustomerWithholdingSuggestions.Amount(982.14D, 5))
        Assert.AreEqual(147.32D, CustomerWithholdingSuggestions.Amount(982.14D, 15))
        ' An actual tax-exempt subtotal of 1,100 must not be divided by 1.12.
        Assert.AreEqual(11D, CustomerWithholdingSuggestions.Amount(1100D, 1))
    End Sub

    <TestMethod>
    Public Sub RoundingUsesTwoDecimalsAndPreservesSignedAmounts()
        Assert.AreEqual(0.02D, CustomerWithholdingSuggestions.Amount(0.10D, 15))
        Assert.AreEqual(-0.02D, CustomerWithholdingSuggestions.Amount(-0.10D, 15))
        Assert.AreEqual(0D, CustomerWithholdingSuggestions.Amount(0D, 15))
        Assert.AreEqual(18518518.35D,
                        CustomerWithholdingSuggestions.Amount(123456789D, 15))
    End Sub

    <TestMethod>
    Public Sub OtherCustomCaptionMeaningsAreNotUsedAsWithholdingFlags()
        Assert.IsTrue(CustomerWithholdingSuggestions.CaptionMatches(" W/Tax 1%: ", 1))
        Assert.IsTrue(CustomerWithholdingSuggestions.CaptionMatches("w/tax 15%", 15))
        Assert.IsFalse(CustomerWithholdingSuggestions.CaptionMatches("Credit Terms:", 2))
        Assert.IsFalse(CustomerWithholdingSuggestions.CaptionMatches("W/Tax 2%", 1))
        Assert.IsFalse(CustomerWithholdingSuggestions.CaptionMatches(Nothing, 5))
    End Sub

    <TestMethod>
    <ExpectedException(GetType(ArgumentOutOfRangeException))>
    Public Sub UnsupportedRatesAreRejected()
        CustomerWithholdingSuggestions.Amount(1100D, 3)
    End Sub
End Class
