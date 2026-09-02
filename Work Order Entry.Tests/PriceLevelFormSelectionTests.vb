Imports System.Reflection
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class PriceLevelFormSelectionTests

    <TestMethod>
    Public Sub GivenSeveralPriceLevels_WhenSelectionIsInitialized_ThenTopRowIsHighlighted()
        ' Given
        Using form As New frmPriceLevel()
            Dim grid = DirectCast(
                form.Controls.Find("gridLevel", True)(0),
                DataGridView)

            grid.Rows.Add("PRICE (RETAIL)", 100)
            grid.Rows.Add("PRICE A (WHOLESALE)", 90)
            grid.Rows.Add("PRICE B (D1)", 80)
            grid.CurrentCell = grid.Rows(2).Cells(0)
            grid.Rows(2).Selected = True

            ' When
            Dim selectTop = GetType(frmPriceLevel).GetMethod(
                "SelectTopPriceLevel",
                BindingFlags.Instance Or BindingFlags.NonPublic)
            selectTop.Invoke(form, Nothing)

            ' Then
            Assert.AreEqual(0, grid.CurrentCell.RowIndex)
            Assert.IsTrue(grid.Rows(0).Selected)
            Assert.IsFalse(grid.Rows(2).Selected)
        End Using
    End Sub

End Class
