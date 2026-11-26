Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class CustomerWithholdingSuggestionsTests
    Private Shared Function Line(marker As String, gross As Decimal) As WithholdingItemLine
        Return New WithholdingItemLine With {.ItemID = 1, .Notes = marker, .GrossAmount = gross}
    End Function

    <TestMethod>
    Public Sub MixedGoodsServicesUseTheirOwnVatExclusiveBases()
        Dim result = ItemWithholdingSuggestions.Calculate({Line("[WTAX:GOODS]", 1120D), Line("[WTAX:SERVICES]", 560D)}, 1500D, True)
        Assert.AreEqual(1000D, result.Bases(1))
        Assert.AreEqual(500D, result.Bases(2))
        Assert.AreEqual(10D, result.Amount(1))
        Assert.AreEqual(10D, result.Amount(2))
        Assert.AreEqual(75D, result.Amount(5))
        Assert.AreEqual(0D, result.Amount(15))
        Assert.AreEqual(0, result.ReviewLines)
    End Sub

    <TestMethod>
    Public Sub CustomerVat5UsesOrderSubtotalWhileIncomeClassesStaySeparate()
        Dim result = ItemWithholdingSuggestions.Calculate({Line("ordinary notes [wtax:goods] [WTAX:VAT5]", 1120D), Line("[WTAX:EWT15]", 224D), Line("[WTAX:GIFT_CERT]", 336D)}, 1500D, True)
        Assert.AreEqual(10D, result.Amount(1))
        Assert.AreEqual(75D, result.Amount(5))
        Assert.AreEqual(30D, result.Amount(15))
        Assert.AreEqual(0D, result.Amount(2))
        Assert.AreEqual(0, result.ReviewLines)
    End Sub

    <TestMethod>
    Public Sub BlankOrdinaryAndRetiredReviewNotesDefaultToGoods()
        For Each notes In New String() {Nothing, "", "  ", "Deliver to reception", "[WTAX:REVIEW]", "notes [wtax:review]"}
            Dim result = ItemWithholdingSuggestions.Calculate({Line(notes, 1120D)}, 1000D, True)
            Assert.AreEqual(10D, result.Amount(1))
            Assert.AreEqual(0D, result.Amount(2))
            Assert.AreEqual(0D, result.Amount(15))
            Assert.AreEqual(0, result.ReviewLines)
        Next
    End Sub

    <TestMethod>
    Public Sub ExplicitClassificationsTakePriorityOverRetiredReviewText()
        For Each classification In {"SERVICES", "EWT15", "GIFT_CERT"}
            Dim notes = "[WTAX:" & classification & "] [WTAX:REVIEW]"
            Dim result = ItemWithholdingSuggestions.Calculate({Line(notes, 100D)}, 100D, False)
            Assert.AreEqual(0D, result.Amount(1))
            Assert.AreEqual(If(classification = "SERVICES", 2D, 0D), result.Amount(2))
            Assert.AreEqual(If(classification = "EWT15", 15D, 0D), result.Amount(15))
            Assert.AreEqual(0, result.ReviewLines)
        Next
    End Sub

    <TestMethod>
    Public Sub CustomerVat5DoesNotRequireOrDoubleCountItemMarkers()
        For Each marker In New String() {Nothing, "[WTAX:GOODS]", "[WTAX:SERVICES]", "[WTAX:GOODS] [WTAX:VAT5]", "[WTAX:GIFT_CERT]", "[WTAX:REVIEW]"}
            Dim result = ItemWithholdingSuggestions.Calculate({Line(marker, 1120D)}, 1000D, True)
            Assert.AreEqual(50D, result.Amount(5))
        Next
        Assert.AreEqual(0, CustomerWithholdingSuggestions.EnabledRates(0, 0, 0, "").Count)
        CollectionAssert.AreEqual(New Integer() {5}, CustomerWithholdingSuggestions.EnabledRates(0, 0, 5, "").ToArray())
    End Sub

    <TestMethod>
    Public Sub CustomerVat5UsesActualSubtotalAndExistingSignedRounding()
        Dim result = ItemWithholdingSuggestions.Calculate({Line("[WTAX:GOODS]", 1120D)}, 900D, True)
        Assert.AreEqual(45D, result.Amount(5))
        result = ItemWithholdingSuggestions.Calculate({Line("[WTAX:GOODS]", -112D)}, -100D, True)
        Assert.AreEqual(-5D, result.Amount(5))
        result = ItemWithholdingSuggestions.Calculate({Line("[WTAX:GOODS]", 0.112D)}, 0.1D, True)
        Assert.AreEqual(0.01D, result.Amount(5))
    End Sub

    <TestMethod>
    Public Sub VatExemptAmountsStillAllowIncomeWithholdingButNotVat5()
        Dim result = ItemWithholdingSuggestions.Calculate({Line("[WTAX:GOODS] [WTAX:VAT5]", 1000D), Line("[WTAX:SERVICES]", 500D)}, 1500D, False)
        Assert.AreEqual(10D, result.Amount(1))
        Assert.AreEqual(10D, result.Amount(2))
        Assert.AreEqual(0D, result.Amount(5))
    End Sub

    <TestMethod>
    Public Sub UnknownAndConflictingMarkersNeverBecomeGoodsOrWholeOrderBases()
        Dim result = ItemWithholdingSuggestions.Calculate({Line(Nothing, 100D), Line("(TAX1%)", 100D), Line("[WTAX:GOODS] [WTAX:SERVICES]", 100D), Line("[WTAX:REVIEW]", 100D), Line("[WTAX:VAT5]", 100D), Line("[WTAX:GIFT_CERT] [WTAX:VAT5]", 100D), Line("[WTAX:UNKNOWN]", 100D)}, 700D, False)
        Assert.AreEqual(4, result.ReviewLines)
        Assert.AreEqual(3D, result.Amount(1))
        For Each rate As Integer In {2, 5, 15}
            Assert.AreEqual(0D, result.Amount(rate))
        Next
        StringAssert.Contains(result.ReviewMessage(), "4 item line(s) excluded")
    End Sub

    <TestMethod>
    Public Sub UnmarkedGoodsReceiveOnlyTheirOwnShareAndGiftRemainsExcluded()
        Dim result = ItemWithholdingSuggestions.Calculate({Line("[WTAX:GOODS]", 112D), Line("[WTAX:GIFT_CERT]", 112D), Line("unclassified", 112D)}, 300D, True)
        Assert.AreEqual(200D, result.Bases(1))
        Assert.AreEqual(2D, result.Amount(1))
        Assert.AreEqual(0, result.ReviewLines)
    End Sub

    <TestMethod>
    Public Sub FractionalSignedAndDuplicateMarkersRetainCentRounding()
        Dim result = ItemWithholdingSuggestions.Calculate({Line("[WTAX:EWT15] [WTAX:EWT15]", -0.1D)}, -0.1D, False)
        Assert.AreEqual(-0.02D, result.Amount(15))
        Assert.AreEqual(0, result.ReviewLines)
        result = ItemWithholdingSuggestions.Calculate({Line("[WTAX:SERVICES]", 0.25D)}, 0.25D, False)
        Assert.AreEqual(0.01D, result.Amount(2))
    End Sub

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
    Public Sub WalkInSelectionsPreserveNotesAndReplaceOnlyTheirOwnMarker()
        Dim notes = "Deliver to reception" & vbCrLf & "Keep upright [OTHER:VALUE]"
        Dim stored = WorkOrderWithholdingReference.WriteWalkInRates(notes, {2, 1, 2, 5})
        Assert.AreEqual(notes & Environment.NewLine & "[WO-WTAX:v1;RATES=1,2,5]", stored)
        CollectionAssert.AreEqual(New Integer() {1, 2, 5}, WorkOrderWithholdingReference.ReadWalkInRates(stored).ToArray())
        Dim revised = WorkOrderWithholdingReference.WriteWalkInRates(stored, {15})
        Assert.AreEqual(notes & Environment.NewLine & "[WO-WTAX:v1;RATES=15]", revised)
        Assert.AreEqual(revised, WorkOrderWithholdingReference.WriteWalkInRates(revised, {15}))
    End Sub

    <TestMethod>
    Public Sub WalkInEmptySelectionIsDifferentFromMissingSelection()
        Assert.IsNull(WorkOrderWithholdingReference.ReadWalkInRates("Regular shipping notes"))
        Dim empty = WorkOrderWithholdingReference.WriteWalkInRates("", New Integer() {})
        Assert.AreEqual("[WO-WTAX:v1;RATES=]", empty)
        Assert.AreEqual(0, WorkOrderWithholdingReference.ReadWalkInRates(empty).Count)
        Assert.AreEqual("", WorkOrderWithholdingReference.WriteWalkInRates(empty, Nothing))
        Assert.IsNull(WorkOrderWithholdingReference.ReadWalkInRates("[WO-WTAX:v1;RATES=3]"))
    End Sub

    <TestMethod>
    <ExpectedException(GetType(ArgumentException))>
    Public Sub WalkInUnsupportedRateCannotBeWritten()
        WorkOrderWithholdingReference.WriteWalkInRates("Preserve me", {3})
    End Sub

    <TestMethod>
    <ExpectedException(GetType(ArgumentOutOfRangeException))>
    Public Sub UnsupportedRatesAreRejected()
        CustomerWithholdingSuggestions.Amount(1100D, 3)
    End Sub
End Class
