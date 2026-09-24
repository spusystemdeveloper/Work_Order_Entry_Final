Imports System.Drawing
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class QueueOrderAgeRulesTests

    <TestMethod>
    Public Sub GivenUnderTenMinutes_WhenPriorityResolved_ThenReturnsNormal()
        Assert.AreEqual(QueuePriorityLevel.Normal, QueueOrderAgeRules.GetPriorityLevel(0))
        Assert.AreEqual(QueuePriorityLevel.Normal, QueueOrderAgeRules.GetPriorityLevel(5.5))
        Assert.AreEqual(QueuePriorityLevel.Normal, QueueOrderAgeRules.GetPriorityLevel(9.9))
    End Sub

    <TestMethod>
    Public Sub GivenBetweenTenAndTwentyMinutes_WhenPriorityResolved_ThenReturnsWarning()
        Assert.AreEqual(QueuePriorityLevel.Warning, QueueOrderAgeRules.GetPriorityLevel(10.0))
        Assert.AreEqual(QueuePriorityLevel.Warning, QueueOrderAgeRules.GetPriorityLevel(15.0))
        Assert.AreEqual(QueuePriorityLevel.Warning, QueueOrderAgeRules.GetPriorityLevel(19.99))
    End Sub

    <TestMethod>
    Public Sub GivenOverTwentyMinutes_WhenPriorityResolved_ThenReturnsAlert()
        Assert.AreEqual(QueuePriorityLevel.Alert, QueueOrderAgeRules.GetPriorityLevel(20.0))
        Assert.AreEqual(QueuePriorityLevel.Alert, QueueOrderAgeRules.GetPriorityLevel(45.0))
    End Sub

    <TestMethod>
    Public Sub GivenNegativeElapsedMinutes_WhenFormatted_ThenReturnsJustNow()
        Assert.AreEqual("Just now", QueueOrderAgeRules.FormatElapsedText(-1.0))
    End Sub

    <TestMethod>
    Public Sub GivenSubMinuteElapsed_WhenFormatted_ThenReturnsLessThanOneMin()
        Assert.AreEqual("< 1m", QueueOrderAgeRules.FormatElapsedText(0.4))
    End Sub

    <TestMethod>
    Public Sub GivenMinutesElapsed_WhenFormatted_ThenReturnsFormattedMinutes()
        Assert.AreEqual("8m", QueueOrderAgeRules.FormatElapsedText(8.2))
        Assert.AreEqual("19m", QueueOrderAgeRules.FormatElapsedText(19.0))
    End Sub

    <TestMethod>
    Public Sub GivenOverOneHourElapsed_WhenFormatted_ThenReturnsHoursAndMinutes()
        Assert.AreEqual("1h 15m", QueueOrderAgeRules.FormatElapsedText(75.0))
        Assert.AreEqual("2h 0m", QueueOrderAgeRules.FormatElapsedText(120.0))
    End Sub

    <TestMethod>
    Public Sub GivenDisabledAlerts_WhenHeaderColorResolved_ThenReturnsControlDarkDark()
        Dim color As Color = QueueOrderAgeRules.GetHeaderColor(QueuePriorityLevel.Alert, False)
        Assert.AreEqual(SystemColors.ControlDarkDark, color)
    End Sub

    <TestMethod>
    Public Sub GivenEnabledAlerts_WhenPriorityColorResolved_ThenReturnsExpectedColors()
        Dim normalColor As Color = QueueOrderAgeRules.GetHeaderColor(QueuePriorityLevel.Normal, True)
        Dim warningColor As Color = QueueOrderAgeRules.GetHeaderColor(QueuePriorityLevel.Warning, True)
        Dim alertColor As Color = QueueOrderAgeRules.GetHeaderColor(QueuePriorityLevel.Alert, True)

        Assert.AreEqual(Color.FromArgb(46, 125, 50), normalColor)
        Assert.AreEqual(Color.FromArgb(217, 119, 6), warningColor)
        Assert.AreEqual(Color.FromArgb(198, 40, 40), alertColor)
    End Sub

    <TestMethod>
    Public Sub GivenPriority_WhenBadgeTextBuilt_ThenReturnsAppropriateIconAndText()
        Assert.AreEqual("⏱️ 5m", QueueOrderAgeRules.GetStatusBadgeText(QueuePriorityLevel.Normal, 5.0))
        Assert.AreEqual("⚠️ 15m", QueueOrderAgeRules.GetStatusBadgeText(QueuePriorityLevel.Warning, 15.0))
        Assert.AreEqual("🔥 25m", QueueOrderAgeRules.GetStatusBadgeText(QueuePriorityLevel.Alert, 25.0))
    End Sub

End Class
