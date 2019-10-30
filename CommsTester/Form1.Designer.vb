<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtAddress = New System.Windows.Forms.TextBox()
        Me.btnOpen = New System.Windows.Forms.Button()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.txtData = New System.Windows.Forms.TextBox()
        Me.txtSend = New System.Windows.Forms.TextBox()
        Me.btnSend = New System.Windows.Forms.Button()
        Me.cmbRuleTX = New System.Windows.Forms.ComboBox()
        Me.cmbDisplayType = New System.Windows.Forms.ComboBox()
        Me.txtRS232Settings = New System.Windows.Forms.TextBox()
        Me.cmbRuleRX = New System.Windows.Forms.ComboBox()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.chkRaw = New System.Windows.Forms.CheckBox()
        Me.chkCom = New System.Windows.Forms.CheckBox()
        Me.btnSendFile = New System.Windows.Forms.Button()
        Me.lstSentItems = New System.Windows.Forms.ListBox()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.chkShowLog = New System.Windows.Forms.CheckBox()
        Me.lblFile = New System.Windows.Forms.Label()
        Me.btnChooseFile = New System.Windows.Forms.Button()
        Me.chkSendCTRL = New System.Windows.Forms.CheckBox()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 8)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(82, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Comms Address"
        '
        'txtAddress
        '
        Me.txtAddress.Location = New System.Drawing.Point(100, 4)
        Me.txtAddress.Name = "txtAddress"
        Me.txtAddress.Size = New System.Drawing.Size(144, 20)
        Me.txtAddress.TabIndex = 1
        Me.txtAddress.Text = "4"
        '
        'btnOpen
        '
        Me.btnOpen.Location = New System.Drawing.Point(344, 4)
        Me.btnOpen.Name = "btnOpen"
        Me.btnOpen.Size = New System.Drawing.Size(72, 24)
        Me.btnOpen.TabIndex = 2
        Me.btnOpen.Text = "&Open"
        Me.btnOpen.UseVisualStyleBackColor = True
        '
        'lblStatus
        '
        Me.lblStatus.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatus.Location = New System.Drawing.Point(432, 4)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(210, 25)
        Me.lblStatus.TabIndex = 3
        '
        'txtData
        '
        Me.txtData.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtData.Font = New System.Drawing.Font("Microsoft Sans Serif", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtData.Location = New System.Drawing.Point(8, 4)
        Me.txtData.Multiline = True
        Me.txtData.Name = "txtData"
        Me.txtData.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtData.Size = New System.Drawing.Size(806, 149)
        Me.txtData.TabIndex = 4
        '
        'txtSend
        '
        Me.txtSend.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtSend.Location = New System.Drawing.Point(8, 52)
        Me.txtSend.Name = "txtSend"
        Me.txtSend.Size = New System.Drawing.Size(742, 20)
        Me.txtSend.TabIndex = 5
        '
        'btnSend
        '
        Me.btnSend.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnSend.Location = New System.Drawing.Point(758, 52)
        Me.btnSend.Name = "btnSend"
        Me.btnSend.Size = New System.Drawing.Size(56, 24)
        Me.btnSend.TabIndex = 6
        Me.btnSend.Text = "&Send"
        Me.btnSend.UseVisualStyleBackColor = True
        '
        'cmbRuleTX
        '
        Me.cmbRuleTX.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbRuleTX.FormattingEnabled = True
        Me.cmbRuleTX.Items.AddRange(New Object() {"None", "CR", "LF", "ESC", "ESC1", "STX/ETX", "ESC/ETB"})
        Me.cmbRuleTX.Location = New System.Drawing.Point(8, 28)
        Me.cmbRuleTX.Name = "cmbRuleTX"
        Me.cmbRuleTX.Size = New System.Drawing.Size(88, 21)
        Me.cmbRuleTX.TabIndex = 7
        '
        'cmbDisplayType
        '
        Me.cmbDisplayType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbDisplayType.FormattingEnabled = True
        Me.cmbDisplayType.Items.AddRange(New Object() {"Raw", "Ctrl", "Hex", "HexAsc"})
        Me.cmbDisplayType.Location = New System.Drawing.Point(216, 28)
        Me.cmbDisplayType.Name = "cmbDisplayType"
        Me.cmbDisplayType.Size = New System.Drawing.Size(112, 21)
        Me.cmbDisplayType.TabIndex = 8
        '
        'txtRS232Settings
        '
        Me.txtRS232Settings.Location = New System.Drawing.Point(248, 4)
        Me.txtRS232Settings.Name = "txtRS232Settings"
        Me.txtRS232Settings.Size = New System.Drawing.Size(92, 20)
        Me.txtRS232Settings.TabIndex = 9
        Me.txtRS232Settings.Text = "9600,n,8,1"
        '
        'cmbRuleRX
        '
        Me.cmbRuleRX.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbRuleRX.FormattingEnabled = True
        Me.cmbRuleRX.Items.AddRange(New Object() {"None", "CR", "LF", "ESC", "ESC1", "STX/ETX", "ESC/ETB"})
        Me.cmbRuleRX.Location = New System.Drawing.Point(100, 28)
        Me.cmbRuleRX.Name = "cmbRuleRX"
        Me.cmbRuleRX.Size = New System.Drawing.Size(88, 21)
        Me.cmbRuleRX.TabIndex = 10
        '
        'Timer1
        '
        Me.Timer1.Enabled = True
        '
        'chkRaw
        '
        Me.chkRaw.AutoSize = True
        Me.chkRaw.Checked = True
        Me.chkRaw.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkRaw.Location = New System.Drawing.Point(348, 32)
        Me.chkRaw.Name = "chkRaw"
        Me.chkRaw.Size = New System.Drawing.Size(48, 17)
        Me.chkRaw.TabIndex = 11
        Me.chkRaw.Text = "Raw"
        Me.chkRaw.UseVisualStyleBackColor = True
        '
        'chkCom
        '
        Me.chkCom.AutoSize = True
        Me.chkCom.Checked = True
        Me.chkCom.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkCom.Location = New System.Drawing.Point(396, 32)
        Me.chkCom.Name = "chkCom"
        Me.chkCom.Size = New System.Drawing.Size(47, 17)
        Me.chkCom.TabIndex = 12
        Me.chkCom.Text = "Com"
        Me.chkCom.UseVisualStyleBackColor = True
        '
        'btnSendFile
        '
        Me.btnSendFile.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnSendFile.Location = New System.Drawing.Point(726, 24)
        Me.btnSendFile.Name = "btnSendFile"
        Me.btnSendFile.Size = New System.Drawing.Size(88, 24)
        Me.btnSendFile.TabIndex = 13
        Me.btnSendFile.Text = "Send &File"
        Me.btnSendFile.UseVisualStyleBackColor = True
        '
        'lstSentItems
        '
        Me.lstSentItems.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstSentItems.Font = New System.Drawing.Font("Microsoft Sans Serif", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lstSentItems.FormattingEnabled = True
        Me.lstSentItems.ItemHeight = 18
        Me.lstSentItems.Location = New System.Drawing.Point(8, 4)
        Me.lstSentItems.Name = "lstSentItems"
        Me.lstSentItems.Size = New System.Drawing.Size(806, 112)
        Me.lstSentItems.TabIndex = 14
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 76)
        Me.SplitContainer1.Name = "SplitContainer1"
        Me.SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.lstSentItems)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.txtData)
        Me.SplitContainer1.Size = New System.Drawing.Size(818, 288)
        Me.SplitContainer1.SplitterDistance = 123
        Me.SplitContainer1.TabIndex = 15
        '
        'chkShowLog
        '
        Me.chkShowLog.AutoSize = True
        Me.chkShowLog.Checked = True
        Me.chkShowLog.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkShowLog.Location = New System.Drawing.Point(452, 32)
        Me.chkShowLog.Name = "chkShowLog"
        Me.chkShowLog.Size = New System.Drawing.Size(71, 17)
        Me.chkShowLog.TabIndex = 16
        Me.chkShowLog.Text = "ShowLog"
        Me.chkShowLog.UseVisualStyleBackColor = True
        '
        'lblFile
        '
        Me.lblFile.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblFile.Location = New System.Drawing.Point(529, 28)
        Me.lblFile.Name = "lblFile"
        Me.lblFile.Size = New System.Drawing.Size(113, 21)
        Me.lblFile.TabIndex = 17
        Me.lblFile.Text = "Label2"
        '
        'btnChooseFile
        '
        Me.btnChooseFile.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnChooseFile.Location = New System.Drawing.Point(691, 24)
        Me.btnChooseFile.Name = "btnChooseFile"
        Me.btnChooseFile.Size = New System.Drawing.Size(29, 22)
        Me.btnChooseFile.TabIndex = 18
        Me.btnChooseFile.Text = "..."
        Me.btnChooseFile.UseVisualStyleBackColor = True
        '
        'chkSendCTRL
        '
        Me.chkSendCTRL.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.chkSendCTRL.AutoSize = True
        Me.chkSendCTRL.Location = New System.Drawing.Point(648, 28)
        Me.chkSendCTRL.Name = "chkSendCTRL"
        Me.chkSendCTRL.Size = New System.Drawing.Size(37, 17)
        Me.chkSendCTRL.TabIndex = 19
        Me.chkSendCTRL.Text = "ctl"
        Me.chkSendCTRL.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(820, 364)
        Me.Controls.Add(Me.chkSendCTRL)
        Me.Controls.Add(Me.btnChooseFile)
        Me.Controls.Add(Me.lblFile)
        Me.Controls.Add(Me.chkShowLog)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Controls.Add(Me.btnSendFile)
        Me.Controls.Add(Me.chkCom)
        Me.Controls.Add(Me.chkRaw)
        Me.Controls.Add(Me.cmbRuleRX)
        Me.Controls.Add(Me.txtRS232Settings)
        Me.Controls.Add(Me.cmbDisplayType)
        Me.Controls.Add(Me.cmbRuleTX)
        Me.Controls.Add(Me.btnSend)
        Me.Controls.Add(Me.txtSend)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.btnOpen)
        Me.Controls.Add(Me.txtAddress)
        Me.Controls.Add(Me.Label1)
        Me.Name = "Form1"
        Me.Text = "CommsTester"
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        Me.SplitContainer1.Panel2.PerformLayout()
        Me.SplitContainer1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtAddress As System.Windows.Forms.TextBox
    Friend WithEvents btnOpen As System.Windows.Forms.Button
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents txtData As System.Windows.Forms.TextBox
    Friend WithEvents txtSend As System.Windows.Forms.TextBox
    Friend WithEvents btnSend As System.Windows.Forms.Button
    Friend WithEvents cmbRuleTX As System.Windows.Forms.ComboBox
    Friend WithEvents cmbDisplayType As System.Windows.Forms.ComboBox
    Friend WithEvents txtRS232Settings As System.Windows.Forms.TextBox
    Friend WithEvents cmbRuleRX As System.Windows.Forms.ComboBox
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents chkRaw As System.Windows.Forms.CheckBox
    Friend WithEvents chkCom As System.Windows.Forms.CheckBox
    Friend WithEvents btnSendFile As System.Windows.Forms.Button
    Friend WithEvents lstSentItems As System.Windows.Forms.ListBox
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents chkShowLog As System.Windows.Forms.CheckBox
    Friend WithEvents lblFile As System.Windows.Forms.Label
    Friend WithEvents btnChooseFile As System.Windows.Forms.Button
    Friend WithEvents chkSendCTRL As System.Windows.Forms.CheckBox

End Class
