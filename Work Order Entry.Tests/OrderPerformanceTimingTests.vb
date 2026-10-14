Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports WorkOrderEntry
Imports System.IO
Imports System.Globalization
Imports System.Linq

<TestClass>
Public Class OrderPerformanceTimingTests
    <TestMethod>
    Public Sub DisabledTimingDoesNotCreateFiles()
        Dim directory = Path.Combine(Path.GetTempPath(), "OrderTimingTests-" & Guid.NewGuid().ToString("N"))
        Using timing As New OrderPerformanceTiming("UpdateDatabase", 123, 6, False, directory)
        End Using
        Assert.IsFalse(IO.Directory.Exists(directory))
    End Sub

    <TestMethod>
    Public Sub TimingRecordsOnceWithInvariantNumbersAndSanitizedFields()
        Dim directory = Path.Combine(Path.GetTempPath(), "OrderTimingTests-" & Guid.NewGuid().ToString("N"))
        Dim previousCulture = Globalization.CultureInfo.CurrentCulture
        Try
            Globalization.CultureInfo.CurrentCulture = New CultureInfo("fr-FR")
            Dim timing As New OrderPerformanceTiming("UpdateDatabase," & vbCrLf, 123, 6, True, directory)
            timing.Outcome = "Saved"
            timing.Dispose()
            timing.Dispose()
            Dim lines = File.ReadAllLines(IO.Directory.GetFiles(directory).Single())
            Assert.AreEqual(2, lines.Length)
            Dim fields = lines(1).Split(","c)
            Assert.AreEqual(6, fields.Length)
            Assert.AreEqual("123", fields(2))
            Assert.AreEqual("6", fields(3))
            Assert.AreEqual("Saved", fields(5))
            Assert.IsTrue(Double.Parse(fields(4), CultureInfo.InvariantCulture) >= 0)
        Finally
            Globalization.CultureInfo.CurrentCulture = previousCulture
            If IO.Directory.Exists(directory) Then IO.Directory.Delete(directory, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub ExcludedDialogTimeCannotMakeProcessingTimeNegative()
        Dim directory = Path.Combine(Path.GetTempPath(), "OrderTimingTests-" & Guid.NewGuid().ToString("N"))
        Try
            Using timing As New OrderPerformanceTiming("RecallTotalsCalculation", 123, 2, True, directory)
                timing.ExcludeElapsed(Double.MaxValue)
            End Using
            Dim fields = File.ReadAllLines(IO.Directory.GetFiles(directory).Single())(1).Split(","c)
            Assert.AreEqual("0.000", fields(4))
            Assert.AreEqual("Completed", fields(5))
        Finally
            If IO.Directory.Exists(directory) Then IO.Directory.Delete(directory, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub InvalidDialogDurationsCannotCorruptTheTimingRecord()
        Dim directory = Path.Combine(Path.GetTempPath(), "OrderTimingTests-" & Guid.NewGuid().ToString("N"))
        Try
            Using timing As New OrderPerformanceTiming("RecallReadAndPopulate", 123, 2, True, directory)
                timing.ExcludeElapsed(Double.NaN)
                timing.ExcludeElapsed(Double.PositiveInfinity)
                timing.ExcludeElapsed(-10)
            End Using
            Dim fields = File.ReadAllLines(IO.Directory.GetFiles(directory).Single())(1).Split(","c)
            Dim elapsed = Double.Parse(fields(4), CultureInfo.InvariantCulture)
            Assert.IsFalse(Double.IsNaN(elapsed) OrElse Double.IsInfinity(elapsed))
            Assert.IsTrue(elapsed >= 0)
        Finally
            If IO.Directory.Exists(directory) Then IO.Directory.Delete(directory, True)
        End Try
    End Sub

    <TestMethod>
    Public Sub AnUnwritableLogNeverThrowsIntoOrderProcessing()
        Dim directory = Path.Combine(Path.GetTempPath(), "OrderTimingTests-" & Guid.NewGuid().ToString("N"))
        File.WriteAllText(directory, "Occupied by a file")
        Try
            Using timing As New OrderPerformanceTiming("InsertDatabase", 0, 6, True, directory)
                timing.Outcome = "Saved"
            End Using
            Assert.AreEqual("Occupied by a file", File.ReadAllText(directory))
        Finally
            File.Delete(directory)
        End Try
    End Sub
End Class
