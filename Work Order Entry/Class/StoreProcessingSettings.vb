Imports System.Configuration

Public NotInheritable Class StoreProcessingSettings

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property BranchSelectionEnabled As Boolean
        Get
            Return ReadBoolean("BranchSelectionEnabled", False)
        End Get
    End Property

    Public Shared ReadOnly Property ExpectedDatabaseName As String
        Get
            Dim value As String =
                ConfigurationManager.AppSettings("ExpectedDatabaseName")
            Return If(value, String.Empty).Trim()
        End Get
    End Property

    Public Shared ReadOnly Property QueueingEnabled As Boolean
        Get
            Return ReadBoolean("QueueingEnabled", True)
        End Get
    End Property

    Public Shared ReadOnly Property QueueingDefaultForNewWorkOrders As Boolean
        Get
            Return QueueingEnabled AndAlso
                   ReadBoolean("QueueingDefaultForNewWorkOrders", True)
        End Get
    End Property

    Public Shared ReadOnly Property AllowPerOrderQueueChoice As Boolean
        Get
            Return QueueingEnabled AndAlso
                   ReadBoolean("AllowPerOrderQueueChoice", True)
        End Get
    End Property

    Public Shared ReadOnly Property AllowOrderGrouping As Boolean
        Get
            Return QueueingEnabled AndAlso
                   ReadBoolean("AllowOrderGrouping", True)
        End Get
    End Property

    Public Shared ReadOnly Property ShowForInvoiceButton As Boolean
        Get
            Return QueueingEnabled AndAlso
                   ReadBoolean("ShowForInvoiceButton", True)
        End Get
    End Property

    Public Shared ReadOnly Property ShowImportButton As Boolean
        Get
            Return ReadBoolean("ShowImportButton", True)
        End Get
    End Property

    Public Shared ReadOnly Property LockCustomerAndSalesRepOnRecall As Boolean
        Get
            Return ReadBoolean("LockCustomerAndSalesRepOnRecall", False)
        End Get
    End Property

    Public Shared Sub SaveLockCustomerAndSalesRepOnRecall(ByVal isLocked As Boolean)
        Dim configuration = ConfigurationManager.OpenExeConfiguration(
            ConfigurationUserLevel.None)

        SetBoolean(configuration, "LockCustomerAndSalesRepOnRecall", isLocked)

        configuration.Save(ConfigurationSaveMode.Modified)
        ConfigurationManager.RefreshSection("appSettings")
    End Sub

    Public Shared ReadOnly Property LockSalesRepToCustomerDefault As Boolean
        Get
            Return ReadBoolean("LockSalesRepToCustomerDefault", False)
        End Get
    End Property

    Public Shared Sub SaveLockSalesRepToCustomerDefault(ByVal isLocked As Boolean)
        Dim configuration = ConfigurationManager.OpenExeConfiguration(
            ConfigurationUserLevel.None)

        SetBoolean(configuration, "LockSalesRepToCustomerDefault", isLocked)

        configuration.Save(ConfigurationSaveMode.Modified)
        ConfigurationManager.RefreshSection("appSettings")
    End Sub

    Public Shared ReadOnly Property AllowDuplicateItemEntry As Boolean
        Get
            Return ReadBoolean("AllowDuplicateItemEntry", False)
        End Get
    End Property

    Public Shared Sub SaveAllowDuplicateItemEntry(ByVal isAllowed As Boolean)
        Dim configuration = ConfigurationManager.OpenExeConfiguration(
            ConfigurationUserLevel.None)

        SetBoolean(configuration, "AllowDuplicateItemEntry", isAllowed)

        configuration.Save(ConfigurationSaveMode.Modified)
        ConfigurationManager.RefreshSection("appSettings")
    End Sub

    Public Shared Sub SaveShowForInvoiceButton(ByVal isVisible As Boolean)
        SaveBranchSettings(QueueingEnabled, AllowOrderGrouping, isVisible,
                           ShowImportButton)
    End Sub

    Public Shared Sub SaveBranchSettings(ByVal queueProcessingEnabled As Boolean,
                                         ByVal allowGrouping As Boolean,
                                         ByVal showForInvoice As Boolean,
                                         ByVal showImport As Boolean)
        SaveBranchSettings(
            queueProcessingEnabled,
            allowGrouping,
            showForInvoice,
            showImport,
            BranchSelectionEnabled,
            ExpectedDatabaseName)
    End Sub

    Public Shared Sub SaveBranchSettings(ByVal queueProcessingEnabled As Boolean,
                                         ByVal allowGrouping As Boolean,
                                         ByVal showForInvoice As Boolean,
                                         ByVal showImport As Boolean,
                                         ByVal branchSelectionEnabled As Boolean,
                                         ByVal expectedDatabaseName As String)
        Dim configuration = ConfigurationManager.OpenExeConfiguration(
            ConfigurationUserLevel.None)

        SetBoolean(configuration, "BranchSelectionEnabled",
                   branchSelectionEnabled)
        SetString(configuration, "ExpectedDatabaseName",
                  If(expectedDatabaseName, String.Empty).Trim())
        SetBoolean(configuration, "QueueingEnabled", queueProcessingEnabled)
        SetBoolean(configuration, "QueueingDefaultForNewWorkOrders",
                   queueProcessingEnabled)
        SetBoolean(configuration, "AllowPerOrderQueueChoice", False)
        SetBoolean(configuration, "AllowOrderGrouping",
                   queueProcessingEnabled AndAlso allowGrouping)
        SetBoolean(configuration, "ShowForInvoiceButton",
                   queueProcessingEnabled AndAlso showForInvoice)
        SetBoolean(configuration, "ShowImportButton", showImport)

        configuration.Save(ConfigurationSaveMode.Modified)
        ConfigurationManager.RefreshSection("appSettings")
    End Sub

    Private Shared Sub SetString(
                                 ByVal configuration As System.Configuration.Configuration,
                                 ByVal key As String,
                                 ByVal value As String)
        Dim setting = configuration.AppSettings.Settings(key)

        If setting Is Nothing Then
            configuration.AppSettings.Settings.Add(key, value)
        Else
            setting.Value = value
        End If
    End Sub

    Private Shared Sub SetBoolean(
                                  ByVal configuration As System.Configuration.Configuration,
                                  ByVal key As String,
                                  ByVal value As Boolean)
        Dim setting = configuration.AppSettings.Settings(key)
        Dim textValue As String = value.ToString().ToLowerInvariant()

        If setting Is Nothing Then
            configuration.AppSettings.Settings.Add(key, textValue)
        Else
            setting.Value = textValue
        End If
    End Sub

    Private Shared Function ReadBoolean(ByVal key As String,
                                        ByVal defaultValue As Boolean) As Boolean
        Dim rawValue As String = ConfigurationManager.AppSettings(key)
        Dim parsedValue As Boolean

        If Boolean.TryParse(rawValue, parsedValue) Then Return parsedValue
        Return defaultValue
    End Function

End Class
