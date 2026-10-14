Imports System.Windows.Forms
Imports WorkOrderEntry

<TestClass()>
Public Class LoadingOverlayTests
    <TestMethod()>
    Public Sub IgnoredOwnerDoesNotAllowDuplicateNestedOverlays()
        Dim ignored = LoadingOverlay.Show(Nothing, "Ignored")
        ignored.Dispose()
        ignored.Dispose()
        AssertOnlyOneOuterOverlay()
    End Sub

    <TestMethod()>
    Public Sub DisposedOwnerDoesNotAllowDuplicateNestedOverlays()
        Dim owner As New Form()
        owner.Dispose()
        Using LoadingOverlay.Show(owner, "Disposed")
        End Using
        AssertOnlyOneOuterOverlay()
    End Sub

    <TestMethod()>
    Public Sub FailedOverlayCreationDoesNotReleaseAnotherScope()
        Using owner As New InvalidHandleForm()
            Using failed = LoadingOverlay.Show(owner, "Failed")
                Assert.IsFalse(TypeOf failed Is LoadingOverlay)
            End Using
        End Using
        AssertOnlyOneOuterOverlay()
    End Sub

    Private Shared Sub AssertOnlyOneOuterOverlay()
        ' Hidden synthetic forms: no real app startup or database connection.
        Using owner As New Form()
            Using outer = LoadingOverlay.Show(owner, "Outer")
                Assert.IsTrue(TypeOf outer Is LoadingOverlay)
                Dim nested = LoadingOverlay.Show(owner, "Nested")
                Assert.IsFalse(TypeOf nested Is LoadingOverlay, "Nested work must retain the outer overlay.")
                nested.Dispose()
                nested.Dispose()
                Using another = LoadingOverlay.Show(owner, "Still nested")
                    Assert.IsFalse(TypeOf another Is LoadingOverlay, "Disposing twice must not release the outer scope.")
                End Using
            End Using
        End Using
    End Sub

    Private NotInheritable Class InvalidHandleForm
        Inherits Form
        Protected Overrides Sub CreateHandle()
            Throw New InvalidOperationException("Synthetic handle creation failure")
        End Sub
    End Class
End Class
