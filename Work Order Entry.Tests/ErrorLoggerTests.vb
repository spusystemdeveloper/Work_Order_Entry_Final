Imports System
Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry

<TestClass>
Public Class ErrorLoggerTests

    <TestMethod>
    Public Sub GivenException_WhenLogged_ThenLogFileIsCreatedAndContainsMessage()
        ' Given
        Dim testEx As New InvalidOperationException("Test error message for graceful handling")

        ' When
        Dim logPath As String = ErrorLogger.LogError(testEx, "UnitTestContext")

        ' Then
        Assert.IsTrue(File.Exists(logPath), "Log file should exist on disk.")
        Dim contents As String = File.ReadAllText(logPath)
        StringAssert.Contains(contents, "Test error message for graceful handling")
        StringAssert.Contains(contents, "UnitTestContext")
        StringAssert.Contains(contents, "InvalidOperationException")
    End Sub

    <TestMethod>
    Public Sub GivenNestedException_WhenLogged_ThenInnerExceptionIsRecorded()
        ' Given
        Dim innerEx As New ArgumentException("Inner database argument problem")
        Dim outerEx As New InvalidOperationException("Outer wrapper problem", innerEx)

        ' When
        Dim logPath As String = ErrorLogger.LogError(outerEx, "NestedExceptionContext")

        ' Then
        Assert.IsTrue(File.Exists(logPath), "Log file should exist.")
        Dim contents As String = File.ReadAllText(logPath)
        StringAssert.Contains(contents, "Outer wrapper problem")
        StringAssert.Contains(contents, "Inner database argument problem")
        StringAssert.Contains(contents, "Inner Exception")
    End Sub

    <TestMethod>
    Public Sub GivenNullException_WhenLogged_ThenDoesNotThrow()
        ' Given & When
        Dim logPath As String = ErrorLogger.LogError(Nothing, "NullContext")

        ' Then
        Assert.IsTrue(File.Exists(logPath), "Log file should be produced even for null exception.")
        Dim contents As String = File.ReadAllText(logPath)
        StringAssert.Contains(contents, "NullContext")
    End Sub

End Class
