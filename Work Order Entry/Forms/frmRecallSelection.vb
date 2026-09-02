'Public Class frmRecallSelection
'    Public _mode As String ' Variable to store mode (e.g., "recall" or "save")

'    ' Constructor to set mode
'    Public Sub New(mode As String)
'        InitializeComponent() ' Load the form components
'        _mode = mode
'    End Sub

'    Public SelectedOption As String

'    Public Sub frmRecallSelection_Load(sender As Object, e As EventArgs) Handles MyBase.Load
'        Selection()
'        Me.AcceptButton = btnOK ' Pressing Enter triggers btnOK.Click
'        Me.KeyPreview = True
'    End Sub

'    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
'        If listBoxSelection.SelectedIndex <> -1 Then
'            SelectedOption = listBoxSelection.SelectedItem.ToString()
'            Me.DialogResult = DialogResult.OK
'            Me.Close()
'        Else
'            MessageBox.Show("Please select an option.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
'        End If
'    End Sub

'    Public Sub Selection()

'        listBoxSelection.Items.Clear() ' Clear previous items

'        If _mode = "Recall" Then
'            ' Recall scenario
'            listBoxSelection.Items.Add("Recall a quote")
'            listBoxSelection.Items.Add("Recall a work order")
'        ElseIf _mode = "SaveToQuote" Then
'            ' Save scenario
'            listBoxSelection.Items.Add("Save Changes")
'            listBoxSelection.Items.Add("Convert to a work order")
'        ElseIf _mode = "SaveToWorkOrder" Then
'            listBoxSelection.Items.Add("Convert to a work order")
'        End If

'        listBoxSelection.SelectedIndex = 0

'    End Sub

'    Private Sub listBoxSelection_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles listBoxSelection.MouseDoubleClick
'        If listBoxSelection.SelectedIndex <> -1 Then
'            SelectedOption = listBoxSelection.SelectedItem.ToString()
'            Me.DialogResult = DialogResult.OK
'            Me.Close()
'        Else
'            MessageBox.Show("Please select an option.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
'        End If
'    End Sub

'    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
'        Me.Close()
'        frmRecall.Close()
'    End Sub

'    Private Sub frmRecallSelection_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
'        If e.KeyCode = Keys.Enter Then
'            btnOK.Focus()
'            e.Handled = True
'        End If
'    End Sub
'End Class

Public Class frmRecallSelection
    Public _mode As String ' Variable to store mode (e.g., "recall" or "save")
    Public SelectedOption As String

    ' Default constructor (no arguments)
    Public Sub New()
        InitializeComponent()
        _mode = "Recall" ' Default mode
    End Sub

    ' Constructor with parameter
    Public Sub New(mode As String)
        InitializeComponent()
        _mode = mode
    End Sub

    Private Sub frmRecallSelection_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Selection()
        Me.AcceptButton = btnOK ' Pressing Enter triggers btnOK.Click
        Me.CancelButton = Button1
        Me.KeyPreview = True
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        If listBoxSelection.SelectedIndex <> -1 Then
            SelectedOption = listBoxSelection.SelectedItem.ToString()
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Else
            MessageBox.Show("Please select an option.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Public Sub Selection()
        listBoxSelection.Items.Clear() ' Clear previous items

        If _mode = "Recall" Then
            listBoxSelection.Items.Add("Recall a quote")
            listBoxSelection.Items.Add("Recall a work order")
        ElseIf _mode = "SaveToQuote" Then
            listBoxSelection.Items.Add("Save Changes")
            listBoxSelection.Items.Add("Convert to a work order")
        ElseIf _mode = "SaveToWorkOrder" Then
            listBoxSelection.Items.Add("Convert to a work order")
        End If

        If listBoxSelection.Items.Count > 0 Then
            listBoxSelection.SelectedIndex = 0
        End If
    End Sub

    Private Sub listBoxSelection_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles listBoxSelection.MouseDoubleClick
        If listBoxSelection.SelectedIndex <> -1 Then
            SelectedOption = listBoxSelection.SelectedItem.ToString()
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Else
            MessageBox.Show("Please select an option.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        SelectedOption = String.Empty
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub frmRecallSelection_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Enter Then
            btnOK.Focus()
            e.Handled = True
        End If
    End Sub
End Class


