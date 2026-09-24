<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.chk_Panels = New System.Windows.Forms.Timer(Me.components)
        Me.FlowLayoutPanel2 = New System.Windows.Forms.FlowLayoutPanel()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblTotalInQueue = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.lblForProcess = New System.Windows.Forms.Label()
        Me.picBoxForProcess = New System.Windows.Forms.PictureBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.lblProcessing = New System.Windows.Forms.Label()
        Me.picBoxProcessing = New System.Windows.Forms.PictureBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Panel4 = New System.Windows.Forms.Panel()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.lblPrepared = New System.Windows.Forms.Label()
        Me.picBoxPrepared = New System.Windows.Forms.PictureBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.Panel5 = New System.Windows.Forms.Panel()
        Me.picBoxForInvoice = New System.Windows.Forms.PictureBox()
        Me.lblForInvoice = New System.Windows.Forms.Label()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.PanelOverdue = New System.Windows.Forms.Panel()
        Me.picBoxOverdue = New System.Windows.Forms.PictureBox()
        Me.lblOverdue = New System.Windows.Forms.Label()
        Me.LabelOverdueTitle = New System.Windows.Forms.Label()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.PanelCompletedToday = New System.Windows.Forms.Panel()
        Me.picBoxCompletedToday = New System.Windows.Forms.PictureBox()
        Me.LabelCompletedTodayTitle = New System.Windows.Forms.Label()
        Me.lblCompletedToday = New System.Windows.Forms.Label()
        Me.lblSearchPrompt = New System.Windows.Forms.Label()
        Me.txtQueueSearch = New System.Windows.Forms.TextBox()
        Me.btnClearSearch = New System.Windows.Forms.Button()
        Me.btnBatchPick = New System.Windows.Forms.Button()
        Me.btnTvDisplay = New System.Windows.Forms.Button()
        Me.btnPickerSummary = New System.Windows.Forms.Button()
        Me.pnlBot = New System.Windows.Forms.Panel()
        Me.flowLyout_bot = New System.Windows.Forms.FlowLayoutPanel()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.lblServer = New System.Windows.Forms.Label()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.lblDatabase = New System.Windows.Forms.Label()
        Me.flowPnl_Orders = New System.Windows.Forms.FlowLayoutPanel()
        Me.txtReleaseType = New System.Windows.Forms.Label()
        Me.pnlTop = New System.Windows.Forms.Panel()
        Me.btnReload = New System.Windows.Forms.Button()
        Me.btnSet = New System.Windows.Forms.Button()
        Me.pnlContainer = New System.Windows.Forms.Panel()
        Me.delay = New System.Windows.Forms.Timer(Me.components)
        Me.FlowLayoutPanel2.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.Panel1.SuspendLayout()
        CType(Me.picBoxForProcess, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel3.SuspendLayout()
        CType(Me.picBoxProcessing, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel4.SuspendLayout()
        CType(Me.picBoxPrepared, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel5.SuspendLayout()
        CType(Me.picBoxForInvoice, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.PanelOverdue.SuspendLayout()
        CType(Me.picBoxOverdue, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlBot.SuspendLayout()
        Me.flowLyout_bot.SuspendLayout()
        Me.pnlTop.SuspendLayout()
        Me.pnlContainer.SuspendLayout()
        Me.SuspendLayout()
        '
        'chk_Panels
        '
        Me.chk_Panels.Interval = 5000
        '
        'FlowLayoutPanel2
        '
        Me.FlowLayoutPanel2.AutoSize = True
        Me.FlowLayoutPanel2.BackColor = System.Drawing.Color.Ivory
        Me.FlowLayoutPanel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.FlowLayoutPanel2.Controls.Add(Me.Panel2)
        Me.FlowLayoutPanel2.Controls.Add(Me.Label2)
        Me.FlowLayoutPanel2.Controls.Add(Me.Panel1)
        Me.FlowLayoutPanel2.Controls.Add(Me.Label5)
        Me.FlowLayoutPanel2.Controls.Add(Me.Panel3)
        Me.FlowLayoutPanel2.Controls.Add(Me.Label8)
        Me.FlowLayoutPanel2.Controls.Add(Me.Panel4)
        Me.FlowLayoutPanel2.Controls.Add(Me.Label11)
        Me.FlowLayoutPanel2.Controls.Add(Me.Panel5)
        Me.FlowLayoutPanel2.Controls.Add(Me.Label7)
        Me.FlowLayoutPanel2.Controls.Add(Me.PanelOverdue)
        Me.FlowLayoutPanel2.Controls.Add(Me.Label13)
        Me.FlowLayoutPanel2.Controls.Add(Me.PanelCompletedToday)
        Me.FlowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top
        Me.FlowLayoutPanel2.Location = New System.Drawing.Point(0, 0)
        Me.FlowLayoutPanel2.Name = "FlowLayoutPanel2"
        Me.FlowLayoutPanel2.Padding = New System.Windows.Forms.Padding(10)
        Me.FlowLayoutPanel2.Size = New System.Drawing.Size(1294, 50)
        Me.FlowLayoutPanel2.TabIndex = 2
        '
        'Panel2
        '
        Me.Panel2.AutoSize = True
        Me.Panel2.BackColor = System.Drawing.Color.Ivory
        Me.Panel2.Controls.Add(Me.Label1)
        Me.Panel2.Controls.Add(Me.lblTotalInQueue)
        Me.Panel2.Location = New System.Drawing.Point(13, 13)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(165, 19)
        Me.Panel2.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(-2, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(128, 19)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Total In Queue :"
        '
        'lblTotalInQueue
        '
        Me.lblTotalInQueue.AutoSize = True
        Me.lblTotalInQueue.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTotalInQueue.Location = New System.Drawing.Point(132, 0)
        Me.lblTotalInQueue.Name = "lblTotalInQueue"
        Me.lblTotalInQueue.Size = New System.Drawing.Size(30, 19)
        Me.lblTotalInQueue.TabIndex = 1
        Me.lblTotalInQueue.Text = "---"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(184, 10)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(33, 19)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "      "
        '
        'Panel1
        '
        Me.Panel1.AutoSize = True
        Me.Panel1.BackColor = System.Drawing.Color.Ivory
        Me.Panel1.Controls.Add(Me.Label3)
        Me.Panel1.Controls.Add(Me.lblForProcess)
        Me.Panel1.Controls.Add(Me.picBoxForProcess)
        Me.Panel1.Location = New System.Drawing.Point(223, 13)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(165, 22)
        Me.Panel1.TabIndex = 0
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(26, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(100, 19)
        Me.Label3.TabIndex = 3
        Me.Label3.Text = "For Process :"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblForProcess
        '
        Me.lblForProcess.AutoSize = True
        Me.lblForProcess.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblForProcess.Location = New System.Drawing.Point(132, 0)
        Me.lblForProcess.Name = "lblForProcess"
        Me.lblForProcess.Size = New System.Drawing.Size(30, 19)
        Me.lblForProcess.TabIndex = 4
        Me.lblForProcess.Text = "---"
        '
        'picBoxForProcess
        '
        Me.picBoxForProcess.BackColor = System.Drawing.Color.OldLace
        Me.picBoxForProcess.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picBoxForProcess.Location = New System.Drawing.Point(3, 2)
        Me.picBoxForProcess.Name = "picBoxForProcess"
        Me.picBoxForProcess.Padding = New System.Windows.Forms.Padding(0, 0, 0, 10)
        Me.picBoxForProcess.Size = New System.Drawing.Size(17, 17)
        Me.picBoxForProcess.TabIndex = 15
        Me.picBoxForProcess.TabStop = False
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.Location = New System.Drawing.Point(394, 10)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(25, 19)
        Me.Label5.TabIndex = 5
        Me.Label5.Text = "    "
        '
        'Panel3
        '
        Me.Panel3.AutoSize = True
        Me.Panel3.BackColor = System.Drawing.Color.Ivory
        Me.Panel3.Controls.Add(Me.Label6)
        Me.Panel3.Controls.Add(Me.lblProcessing)
        Me.Panel3.Controls.Add(Me.picBoxProcessing)
        Me.Panel3.Location = New System.Drawing.Point(425, 13)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(164, 22)
        Me.Panel3.TabIndex = 2
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.Location = New System.Drawing.Point(27, 0)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(98, 19)
        Me.Label6.TabIndex = 6
        Me.Label6.Text = "Processing :"
        '
        'lblProcessing
        '
        Me.lblProcessing.AutoSize = True
        Me.lblProcessing.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblProcessing.Location = New System.Drawing.Point(131, 0)
        Me.lblProcessing.Name = "lblProcessing"
        Me.lblProcessing.Size = New System.Drawing.Size(30, 19)
        Me.lblProcessing.TabIndex = 7
        Me.lblProcessing.Text = "---"
        '
        'picBoxProcessing
        '
        Me.picBoxProcessing.BackColor = System.Drawing.Color.DarkSalmon
        Me.picBoxProcessing.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picBoxProcessing.Location = New System.Drawing.Point(3, 2)
        Me.picBoxProcessing.Name = "picBoxProcessing"
        Me.picBoxProcessing.Size = New System.Drawing.Size(17, 17)
        Me.picBoxProcessing.TabIndex = 16
        Me.picBoxProcessing.TabStop = False
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label8.Location = New System.Drawing.Point(595, 10)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(25, 19)
        Me.Label8.TabIndex = 8
        Me.Label8.Text = "    "
        '
        'Panel4
        '
        Me.Panel4.AutoSize = True
        Me.Panel4.BackColor = System.Drawing.Color.Ivory
        Me.Panel4.Controls.Add(Me.Label9)
        Me.Panel4.Controls.Add(Me.lblPrepared)
        Me.Panel4.Controls.Add(Me.picBoxPrepared)
        Me.Panel4.Location = New System.Drawing.Point(626, 13)
        Me.Panel4.Name = "Panel4"
        Me.Panel4.Size = New System.Drawing.Size(155, 22)
        Me.Panel4.TabIndex = 3
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label9.Location = New System.Drawing.Point(27, 0)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(89, 19)
        Me.Label9.TabIndex = 9
        Me.Label9.Text = "Prepared :"
        '
        'lblPrepared
        '
        Me.lblPrepared.AutoSize = True
        Me.lblPrepared.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPrepared.Location = New System.Drawing.Point(122, 0)
        Me.lblPrepared.Name = "lblPrepared"
        Me.lblPrepared.Size = New System.Drawing.Size(30, 19)
        Me.lblPrepared.TabIndex = 10
        Me.lblPrepared.Text = "---"
        '
        'picBoxPrepared
        '
        Me.picBoxPrepared.BackColor = System.Drawing.Color.PaleGreen
        Me.picBoxPrepared.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picBoxPrepared.Location = New System.Drawing.Point(3, 2)
        Me.picBoxPrepared.Name = "picBoxPrepared"
        Me.picBoxPrepared.Size = New System.Drawing.Size(17, 17)
        Me.picBoxPrepared.TabIndex = 17
        Me.picBoxPrepared.TabStop = False
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label11.Location = New System.Drawing.Point(787, 10)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(25, 19)
        Me.Label11.TabIndex = 11
        Me.Label11.Text = "    "
        '
        'Panel5
        '
        Me.Panel5.AutoSize = True
        Me.Panel5.BackColor = System.Drawing.Color.Ivory
        Me.Panel5.Controls.Add(Me.picBoxForInvoice)
        Me.Panel5.Controls.Add(Me.lblForInvoice)
        Me.Panel5.Controls.Add(Me.Label12)
        Me.Panel5.Location = New System.Drawing.Point(818, 13)
        Me.Panel5.Name = "Panel5"
        Me.Panel5.Size = New System.Drawing.Size(167, 22)
        Me.Panel5.TabIndex = 4
        '
        'picBoxForInvoice
        '
        Me.picBoxForInvoice.BackColor = System.Drawing.Color.CornflowerBlue
        Me.picBoxForInvoice.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picBoxForInvoice.Location = New System.Drawing.Point(3, 2)
        Me.picBoxForInvoice.Name = "picBoxForInvoice"
        Me.picBoxForInvoice.Size = New System.Drawing.Size(17, 17)
        Me.picBoxForInvoice.TabIndex = 18
        Me.picBoxForInvoice.TabStop = False
        '
        'lblForInvoice
        '
        Me.lblForInvoice.AutoSize = True
        Me.lblForInvoice.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblForInvoice.Location = New System.Drawing.Point(134, 0)
        Me.lblForInvoice.Name = "lblForInvoice"
        Me.lblForInvoice.Size = New System.Drawing.Size(30, 19)
        Me.lblForInvoice.TabIndex = 13
        Me.lblForInvoice.Text = "---"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label12.Location = New System.Drawing.Point(27, 0)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(101, 19)
        Me.Label12.TabIndex = 12
        Me.Label12.Text = "For Invoice :"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.Location = New System.Drawing.Point(991, 10)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(25, 19)
        Me.Label7.TabIndex = 22
        Me.Label7.Text = "    "
        '
        'PanelOverdue
        '
        Me.PanelOverdue.AutoSize = True
        Me.PanelOverdue.BackColor = System.Drawing.Color.Ivory
        Me.PanelOverdue.Controls.Add(Me.picBoxOverdue)
        Me.PanelOverdue.Controls.Add(Me.lblOverdue)
        Me.PanelOverdue.Controls.Add(Me.LabelOverdueTitle)
        Me.PanelOverdue.Location = New System.Drawing.Point(1022, 13)
        Me.PanelOverdue.Name = "PanelOverdue"
        Me.PanelOverdue.Size = New System.Drawing.Size(200, 22)
        Me.PanelOverdue.TabIndex = 5
        '
        'picBoxOverdue
        '
        Me.picBoxOverdue.BackColor = System.Drawing.Color.FromArgb(198, 40, 40)
        Me.picBoxOverdue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picBoxOverdue.Location = New System.Drawing.Point(3, 2)
        Me.picBoxOverdue.Name = "picBoxOverdue"
        Me.picBoxOverdue.Size = New System.Drawing.Size(17, 17)
        Me.picBoxOverdue.TabIndex = 19
        Me.picBoxOverdue.TabStop = False
        '
        'lblOverdue
        '
        Me.lblOverdue.AutoSize = True
        Me.lblOverdue.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblOverdue.Location = New System.Drawing.Point(165, 0)
        Me.lblOverdue.Name = "lblOverdue"
        Me.lblOverdue.Size = New System.Drawing.Size(30, 19)
        Me.lblOverdue.TabIndex = 15
        Me.lblOverdue.Text = "---"
        '
        'LabelOverdueTitle
        '
        Me.LabelOverdueTitle.AutoSize = True
        Me.LabelOverdueTitle.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelOverdueTitle.Location = New System.Drawing.Point(27, 0)
        Me.LabelOverdueTitle.Name = "LabelOverdueTitle"
        Me.LabelOverdueTitle.Size = New System.Drawing.Size(135, 19)
        Me.LabelOverdueTitle.TabIndex = 14
        Me.LabelOverdueTitle.Text = "Overdue (>20m) :"
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label13.Location = New System.Drawing.Point(1225, 10)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(25, 19)
        Me.Label13.TabIndex = 23
        Me.Label13.Text = "    "
        '
        'PanelCompletedToday
        '
        Me.PanelCompletedToday.AutoSize = True
        Me.PanelCompletedToday.BackColor = System.Drawing.Color.Ivory
        Me.PanelCompletedToday.Controls.Add(Me.picBoxCompletedToday)
        Me.PanelCompletedToday.Controls.Add(Me.lblCompletedToday)
        Me.PanelCompletedToday.Controls.Add(Me.LabelCompletedTodayTitle)
        Me.PanelCompletedToday.Location = New System.Drawing.Point(1256, 13)
        Me.PanelCompletedToday.Name = "PanelCompletedToday"
        Me.PanelCompletedToday.Size = New System.Drawing.Size(225, 22)
        Me.PanelCompletedToday.TabIndex = 6
        '
        'picBoxCompletedToday
        '
        Me.picBoxCompletedToday.BackColor = System.Drawing.Color.FromArgb(16, 185, 129)
        Me.picBoxCompletedToday.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picBoxCompletedToday.Location = New System.Drawing.Point(3, 2)
        Me.picBoxCompletedToday.Name = "picBoxCompletedToday"
        Me.picBoxCompletedToday.Size = New System.Drawing.Size(17, 17)
        Me.picBoxCompletedToday.TabIndex = 24
        Me.picBoxCompletedToday.TabStop = False
        '
        'lblCompletedToday
        '
        Me.lblCompletedToday.AutoSize = True
        Me.lblCompletedToday.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCompletedToday.Location = New System.Drawing.Point(185, 0)
        Me.lblCompletedToday.Name = "lblCompletedToday"
        Me.lblCompletedToday.Size = New System.Drawing.Size(30, 19)
        Me.lblCompletedToday.TabIndex = 17
        Me.lblCompletedToday.Text = "---"
        '
        'LabelCompletedTodayTitle
        '
        Me.LabelCompletedTodayTitle.AutoSize = True
        Me.LabelCompletedTodayTitle.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelCompletedTodayTitle.Location = New System.Drawing.Point(27, 0)
        Me.LabelCompletedTodayTitle.Name = "LabelCompletedTodayTitle"
        Me.LabelCompletedTodayTitle.Size = New System.Drawing.Size(155, 19)
        Me.LabelCompletedTodayTitle.TabIndex = 16
        Me.LabelCompletedTodayTitle.Text = "Completed Today :"
        '
        'pnlBot
        '
        Me.pnlBot.BackColor = System.Drawing.Color.Ivory
        Me.pnlBot.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlBot.Controls.Add(Me.flowLyout_bot)
        Me.pnlBot.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlBot.Location = New System.Drawing.Point(0, 700)
        Me.pnlBot.Name = "pnlBot"
        Me.pnlBot.Size = New System.Drawing.Size(1294, 29)
        Me.pnlBot.TabIndex = 3
        '
        'flowLyout_bot
        '
        Me.flowLyout_bot.BackColor = System.Drawing.Color.Ivory
        Me.flowLyout_bot.Controls.Add(Me.Label10)
        Me.flowLyout_bot.Controls.Add(Me.lblServer)
        Me.flowLyout_bot.Controls.Add(Me.Label14)
        Me.flowLyout_bot.Controls.Add(Me.Label15)
        Me.flowLyout_bot.Controls.Add(Me.lblDatabase)
        Me.flowLyout_bot.Dock = System.Windows.Forms.DockStyle.Fill
        Me.flowLyout_bot.Location = New System.Drawing.Point(0, 0)
        Me.flowLyout_bot.Name = "flowLyout_bot"
        Me.flowLyout_bot.Padding = New System.Windows.Forms.Padding(0, 5, 0, 0)
        Me.flowLyout_bot.Size = New System.Drawing.Size(1292, 27)
        Me.flowLyout_bot.TabIndex = 70
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Font = New System.Drawing.Font("Century Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label10.Location = New System.Drawing.Point(3, 5)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(56, 16)
        Me.Label10.TabIndex = 33
        Me.Label10.Text = "Server :"
        '
        'lblServer
        '
        Me.lblServer.AutoSize = True
        Me.lblServer.Font = New System.Drawing.Font("Century Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblServer.Location = New System.Drawing.Point(65, 5)
        Me.lblServer.Name = "lblServer"
        Me.lblServer.Size = New System.Drawing.Size(23, 16)
        Me.lblServer.TabIndex = 34
        Me.lblServer.Text = "---"
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Font = New System.Drawing.Font("Century Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label14.Location = New System.Drawing.Point(94, 5)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(24, 16)
        Me.Label14.TabIndex = 35
        Me.Label14.Text = "    "
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Font = New System.Drawing.Font("Century Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label15.Location = New System.Drawing.Point(124, 5)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(79, 16)
        Me.Label15.TabIndex = 36
        Me.Label15.Text = "Database :"
        '
        'lblDatabase
        '
        Me.lblDatabase.AutoSize = True
        Me.lblDatabase.Font = New System.Drawing.Font("Century Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDatabase.Location = New System.Drawing.Point(209, 5)
        Me.lblDatabase.Name = "lblDatabase"
        Me.lblDatabase.Size = New System.Drawing.Size(23, 16)
        Me.lblDatabase.TabIndex = 37
        Me.lblDatabase.Text = "---"
        '
        'flowPnl_Orders
        '
        Me.flowPnl_Orders.AutoScroll = True
        Me.flowPnl_Orders.BackColor = System.Drawing.SystemColors.GrayText
        Me.flowPnl_Orders.Dock = System.Windows.Forms.DockStyle.Fill
        Me.flowPnl_Orders.Location = New System.Drawing.Point(0, 50)
        Me.flowPnl_Orders.Margin = New System.Windows.Forms.Padding(10)
        Me.flowPnl_Orders.Name = "flowPnl_Orders"
        Me.flowPnl_Orders.Padding = New System.Windows.Forms.Padding(20, 20, 20, 100)
        Me.flowPnl_Orders.Size = New System.Drawing.Size(1294, 612)
        Me.flowPnl_Orders.TabIndex = 4
        '
        'txtReleaseType
        '
        Me.txtReleaseType.AutoSize = True
        Me.txtReleaseType.Font = New System.Drawing.Font("Century Gothic", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtReleaseType.Location = New System.Drawing.Point(14, 9)
        Me.txtReleaseType.Name = "txtReleaseType"
        Me.txtReleaseType.Size = New System.Drawing.Size(51, 19)
        Me.txtReleaseType.TabIndex = 4
        Me.txtReleaseType.Text = "------"
        '
        'lblSearchPrompt
        '
        Me.lblSearchPrompt.AutoSize = True
        Me.lblSearchPrompt.Font = New System.Drawing.Font("Century Gothic", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSearchPrompt.Location = New System.Drawing.Point(440, 10)
        Me.lblSearchPrompt.Name = "lblSearchPrompt"
        Me.lblSearchPrompt.Size = New System.Drawing.Size(60, 16)
        Me.lblSearchPrompt.TabIndex = 115
        Me.lblSearchPrompt.Text = "Search :"
        '
        'txtQueueSearch
        '
        Me.txtQueueSearch.Font = New System.Drawing.Font("Century Gothic", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtQueueSearch.Location = New System.Drawing.Point(505, 7)
        Me.txtQueueSearch.Name = "txtQueueSearch"
        Me.txtQueueSearch.Size = New System.Drawing.Size(260, 23)
        Me.txtQueueSearch.TabIndex = 116
        '
        'btnClearSearch
        '
        Me.btnClearSearch.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.btnClearSearch.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray
        Me.btnClearSearch.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control
        Me.btnClearSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnClearSearch.Font = New System.Drawing.Font("Century Gothic", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnClearSearch.Location = New System.Drawing.Point(768, 6)
        Me.btnClearSearch.Margin = New System.Windows.Forms.Padding(0)
        Me.btnClearSearch.Name = "btnClearSearch"
        Me.btnClearSearch.Size = New System.Drawing.Size(26, 25)
        Me.btnClearSearch.TabIndex = 117
        Me.btnClearSearch.Text = "✕"
        Me.btnClearSearch.UseVisualStyleBackColor = True
        '
        'pnlTop
        '
        Me.pnlTop.BackColor = System.Drawing.Color.Ivory
        Me.pnlTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlTop.Controls.Add(Me.lblSearchPrompt)
        Me.pnlTop.Controls.Add(Me.txtQueueSearch)
        Me.pnlTop.Controls.Add(Me.btnClearSearch)
        Me.pnlTop.Controls.Add(Me.btnBatchPick)
        Me.pnlTop.Controls.Add(Me.btnTvDisplay)
        Me.pnlTop.Controls.Add(Me.btnPickerSummary)
        Me.pnlTop.Controls.Add(Me.btnReload)
        Me.pnlTop.Controls.Add(Me.btnSet)
        Me.pnlTop.Controls.Add(Me.txtReleaseType)
        Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTop.Location = New System.Drawing.Point(0, 0)
        Me.pnlTop.Name = "pnlTop"
        Me.pnlTop.Size = New System.Drawing.Size(1294, 38)
        Me.pnlTop.TabIndex = 5
        '
        'btnBatchPick
        '
        Me.btnBatchPick.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnBatchPick.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.btnBatchPick.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray
        Me.btnBatchPick.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control
        Me.btnBatchPick.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnBatchPick.Font = New System.Drawing.Font("Century Gothic", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBatchPick.Location = New System.Drawing.Point(806, 4)
        Me.btnBatchPick.Margin = New System.Windows.Forms.Padding(0)
        Me.btnBatchPick.Name = "btnBatchPick"
        Me.btnBatchPick.Size = New System.Drawing.Size(108, 27)
        Me.btnBatchPick.TabIndex = 119
        Me.btnBatchPick.Text = "🌊 Batch Pick"
        Me.btnBatchPick.UseVisualStyleBackColor = True
        '
        'btnTvDisplay
        '
        Me.btnTvDisplay.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnTvDisplay.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.btnTvDisplay.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray
        Me.btnTvDisplay.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control
        Me.btnTvDisplay.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnTvDisplay.Font = New System.Drawing.Font("Century Gothic", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnTvDisplay.Location = New System.Drawing.Point(918, 4)
        Me.btnTvDisplay.Margin = New System.Windows.Forms.Padding(0)
        Me.btnTvDisplay.Name = "btnTvDisplay"
        Me.btnTvDisplay.Size = New System.Drawing.Size(106, 27)
        Me.btnTvDisplay.TabIndex = 120
        Me.btnTvDisplay.Text = "📺 TV Display"
        Me.btnTvDisplay.UseVisualStyleBackColor = True
        '
        'btnPickerSummary
        '
        Me.btnPickerSummary.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnPickerSummary.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.btnPickerSummary.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray
        Me.btnPickerSummary.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control
        Me.btnPickerSummary.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnPickerSummary.Font = New System.Drawing.Font("Century Gothic", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnPickerSummary.Location = New System.Drawing.Point(1030, 4)
        Me.btnPickerSummary.Margin = New System.Windows.Forms.Padding(0)
        Me.btnPickerSummary.Name = "btnPickerSummary"
        Me.btnPickerSummary.Size = New System.Drawing.Size(90, 27)
        Me.btnPickerSummary.TabIndex = 118
        Me.btnPickerSummary.Text = "👥 Pickers"
        Me.btnPickerSummary.UseVisualStyleBackColor = True
        '
        'btnReload
        '
        Me.btnReload.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnReload.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.btnReload.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray
        Me.btnReload.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control
        Me.btnReload.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnReload.Font = New System.Drawing.Font("Century Gothic", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnReload.Location = New System.Drawing.Point(1127, 4)
        Me.btnReload.Margin = New System.Windows.Forms.Padding(0)
        Me.btnReload.Name = "btnReload"
        Me.btnReload.Size = New System.Drawing.Size(67, 27)
        Me.btnReload.TabIndex = 114
        Me.btnReload.Text = "Reload"
        Me.btnReload.UseVisualStyleBackColor = True
        '
        'btnSet
        '
        Me.btnSet.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnSet.FlatAppearance.BorderColor = System.Drawing.Color.Black
        Me.btnSet.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray
        Me.btnSet.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control
        Me.btnSet.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnSet.Font = New System.Drawing.Font("Century Gothic", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnSet.Location = New System.Drawing.Point(1201, 4)
        Me.btnSet.Margin = New System.Windows.Forms.Padding(0)
        Me.btnSet.Name = "btnSet"
        Me.btnSet.Size = New System.Drawing.Size(83, 27)
        Me.btnSet.TabIndex = 113
        Me.btnSet.Text = "Settings"
        Me.btnSet.UseVisualStyleBackColor = True
        '
        'pnlContainer
        '
        Me.pnlContainer.Controls.Add(Me.flowPnl_Orders)
        Me.pnlContainer.Controls.Add(Me.FlowLayoutPanel2)
        Me.pnlContainer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlContainer.Location = New System.Drawing.Point(0, 38)
        Me.pnlContainer.Name = "pnlContainer"
        Me.pnlContainer.Size = New System.Drawing.Size(1294, 662)
        Me.pnlContainer.TabIndex = 6
        '
        'delay
        '
        Me.delay.Interval = 1000
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.ClientSize = New System.Drawing.Size(1294, 729)
        Me.Controls.Add(Me.pnlContainer)
        Me.Controls.Add(Me.pnlTop)
        Me.Controls.Add(Me.pnlBot)
        Me.DoubleBuffered = True
        Me.KeyPreview = True
        Me.Name = "frmMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Queueing"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.FlowLayoutPanel2.ResumeLayout(False)
        Me.FlowLayoutPanel2.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        CType(Me.picBoxForProcess, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        CType(Me.picBoxProcessing, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel4.ResumeLayout(False)
        Me.Panel4.PerformLayout()
        CType(Me.picBoxPrepared, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel5.ResumeLayout(False)
        Me.Panel5.PerformLayout()
        CType(Me.picBoxForInvoice, System.ComponentModel.ISupportInitialize).EndInit()
        Me.PanelOverdue.ResumeLayout(False)
        Me.PanelOverdue.PerformLayout()
        CType(Me.picBoxOverdue, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlBot.ResumeLayout(False)
        Me.flowLyout_bot.ResumeLayout(False)
        Me.flowLyout_bot.PerformLayout()
        Me.pnlTop.ResumeLayout(False)
        Me.pnlTop.PerformLayout()
        Me.pnlContainer.ResumeLayout(False)
        Me.pnlContainer.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents chk_Panels As System.Windows.Forms.Timer
    Friend WithEvents pnlBot As System.Windows.Forms.Panel
    Friend WithEvents flowPnl_Orders As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents FlowLayoutPanel2 As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lblTotalInQueue As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents lblForProcess As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents lblProcessing As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents lblPrepared As System.Windows.Forms.Label
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents lblForInvoice As System.Windows.Forms.Label
    Friend WithEvents picBoxForProcess As System.Windows.Forms.PictureBox
    Friend WithEvents picBoxProcessing As System.Windows.Forms.PictureBox
    Friend WithEvents picBoxPrepared As System.Windows.Forms.PictureBox
    Friend WithEvents picBoxForInvoice As System.Windows.Forms.PictureBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents PanelOverdue As System.Windows.Forms.Panel
    Friend WithEvents picBoxOverdue As System.Windows.Forms.PictureBox
    Friend WithEvents lblOverdue As System.Windows.Forms.Label
    Friend WithEvents LabelOverdueTitle As System.Windows.Forms.Label
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents lblSearchPrompt As System.Windows.Forms.Label
    Friend WithEvents txtQueueSearch As System.Windows.Forms.TextBox
    Friend WithEvents btnClearSearch As System.Windows.Forms.Button
    Friend WithEvents txtReleaseType As System.Windows.Forms.Label
    Friend WithEvents pnlTop As System.Windows.Forms.Panel
    Friend WithEvents pnlContainer As System.Windows.Forms.Panel
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents Panel3 As System.Windows.Forms.Panel
    Friend WithEvents Panel4 As System.Windows.Forms.Panel
    Friend WithEvents Panel5 As System.Windows.Forms.Panel
    Friend WithEvents flowLyout_bot As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents lblServer As System.Windows.Forms.Label
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents Label15 As System.Windows.Forms.Label
    Friend WithEvents lblDatabase As System.Windows.Forms.Label
    Friend WithEvents btnSet As System.Windows.Forms.Button
    Friend WithEvents delay As System.Windows.Forms.Timer
    Friend WithEvents btnReload As System.Windows.Forms.Button
    Friend WithEvents btnPickerSummary As System.Windows.Forms.Button
    Friend WithEvents btnBatchPick As System.Windows.Forms.Button
    Friend WithEvents btnTvDisplay As System.Windows.Forms.Button
    Friend WithEvents PanelCompletedToday As System.Windows.Forms.Panel
    Friend WithEvents picBoxCompletedToday As System.Windows.Forms.PictureBox
    Friend WithEvents lblCompletedToday As System.Windows.Forms.Label
    Friend WithEvents LabelCompletedTodayTitle As System.Windows.Forms.Label
End Class
