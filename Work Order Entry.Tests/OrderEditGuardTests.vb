Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class OrderEditGuardTests
    <TestMethod>
    Public Sub GivenSameRecalledState_WhenValidated_ThenSavingIsAllowed()
        OrderEditGuard.EnsureUnchanged(New OrderEditSnapshot(12, "server|store", "same"),
                                      New OrderEditSnapshot(12, "SERVER|STORE", "same"))
    End Sub

    <TestMethod, ExpectedException(GetType(OrderEditConflictException))>
    Public Sub GivenChangedState_WhenValidated_ThenSavingIsBlocked()
        OrderEditGuard.EnsureUnchanged(New OrderEditSnapshot(12, "server|store", "before"),
                                      New OrderEditSnapshot(12, "server|store", "after"))
    End Sub

    <TestMethod, ExpectedException(GetType(OrderEditConflictException))>
    Public Sub GivenNoRecallBaseline_WhenValidated_ThenSavingIsBlocked()
        OrderEditGuard.EnsureUnchanged(Nothing, New OrderEditSnapshot(12, "server|store", "same"))
    End Sub

    <TestMethod, ExpectedException(GetType(OrderEditConflictException))>
    Public Sub GivenDifferentStore_WhenValidated_ThenSavingIsBlocked()
        OrderEditGuard.EnsureUnchanged(New OrderEditSnapshot(12, "server|store1", "same"),
                                      New OrderEditSnapshot(12, "server|store2", "same"))
    End Sub

    <TestMethod, ExpectedException(GetType(OrderEditConflictException))>
    Public Sub GivenDifferentOrder_WhenValidated_ThenSavingIsBlocked()
        OrderEditGuard.EnsureUnchanged(New OrderEditSnapshot(12, "server|store", "same"),
                                      New OrderEditSnapshot(13, "server|store", "same"))
    End Sub

    <TestMethod, ExpectedException(GetType(OrderEditConflictException))>
    Public Sub GivenEmptyFingerprint_WhenValidated_ThenSavingIsBlocked()
        OrderEditGuard.EnsureUnchanged(New OrderEditSnapshot(12, "server|store", ""),
                                      New OrderEditSnapshot(12, "server|store", ""))
    End Sub
End Class
