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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtBackup = New System.Windows.Forms.Button()
        Me.btnRefresh = New System.Windows.Forms.Button()
        Me.lstDatabases = New System.Windows.Forms.ListBox()
        Me.chkSelectAll = New System.Windows.Forms.CheckBox()
        Me.chkDeselectAll = New System.Windows.Forms.CheckBox()
        Me.txtBackupFolder = New System.Windows.Forms.TextBox()
        Me.lblBackupToFolder = New System.Windows.Forms.Label()
        Me.txtLog = New System.Windows.Forms.TextBox()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.lstFiles = New System.Windows.Forms.ListBox()
        Me.btnRefreshFiles = New System.Windows.Forms.Button()
        Me.btnRestore = New System.Windows.Forms.Button()
        Me.chkDeselectAllFiles = New System.Windows.Forms.CheckBox()
        Me.chkSelectAllFiles = New System.Windows.Forms.CheckBox()
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
        Me.btnFolderSelect = New System.Windows.Forms.Button()
        Me.lstServers = New System.Windows.Forms.ListBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.txtServer = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtFile = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.RadioButton1 = New System.Windows.Forms.RadioButton()
        Me.RadioButton2 = New System.Windows.Forms.RadioButton()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.btnBackupSingle = New System.Windows.Forms.Button()
        Me.btnRestoreSingle = New System.Windows.Forms.Button()
        Me.btnGetFile = New System.Windows.Forms.Button()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(22, 319)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(38, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Server"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(21, 155)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(58, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Databases"
        '
        'txtBackup
        '
        Me.txtBackup.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.txtBackup.Location = New System.Drawing.Point(25, 511)
        Me.txtBackup.Name = "txtBackup"
        Me.txtBackup.Size = New System.Drawing.Size(302, 25)
        Me.txtBackup.TabIndex = 4
        Me.txtBackup.Text = "Backup Selected"
        Me.txtBackup.UseVisualStyleBackColor = False
        '
        'btnRefresh
        '
        Me.btnRefresh.Location = New System.Drawing.Point(238, 145)
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.Size = New System.Drawing.Size(89, 22)
        Me.btnRefresh.TabIndex = 5
        Me.btnRefresh.Text = "Get Databases"
        Me.btnRefresh.UseVisualStyleBackColor = True
        '
        'lstDatabases
        '
        Me.lstDatabases.FormattingEnabled = True
        Me.lstDatabases.Location = New System.Drawing.Point(25, 170)
        Me.lstDatabases.Name = "lstDatabases"
        Me.lstDatabases.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple
        Me.lstDatabases.Size = New System.Drawing.Size(302, 95)
        Me.lstDatabases.TabIndex = 6
        '
        'chkSelectAll
        '
        Me.chkSelectAll.AutoSize = True
        Me.chkSelectAll.Location = New System.Drawing.Point(26, 271)
        Me.chkSelectAll.Name = "chkSelectAll"
        Me.chkSelectAll.Size = New System.Drawing.Size(70, 17)
        Me.chkSelectAll.TabIndex = 7
        Me.chkSelectAll.Text = "Select All"
        Me.chkSelectAll.UseVisualStyleBackColor = True
        '
        'chkDeselectAll
        '
        Me.chkDeselectAll.AutoSize = True
        Me.chkDeselectAll.Location = New System.Drawing.Point(245, 271)
        Me.chkDeselectAll.Name = "chkDeselectAll"
        Me.chkDeselectAll.Size = New System.Drawing.Size(82, 17)
        Me.chkDeselectAll.TabIndex = 8
        Me.chkDeselectAll.Text = "Deselect All"
        Me.chkDeselectAll.UseVisualStyleBackColor = True
        '
        'txtBackupFolder
        '
        Me.txtBackupFolder.Location = New System.Drawing.Point(26, 34)
        Me.txtBackupFolder.Name = "txtBackupFolder"
        Me.txtBackupFolder.Size = New System.Drawing.Size(271, 20)
        Me.txtBackupFolder.TabIndex = 9
        Me.txtBackupFolder.Text = "d:\DBBackup\"
        '
        'lblBackupToFolder
        '
        Me.lblBackupToFolder.AutoSize = True
        Me.lblBackupToFolder.Location = New System.Drawing.Point(23, 15)
        Me.lblBackupToFolder.Name = "lblBackupToFolder"
        Me.lblBackupToFolder.Size = New System.Drawing.Size(79, 13)
        Me.lblBackupToFolder.TabIndex = 10
        Me.lblBackupToFolder.Text = "Backup Folder:"
        '
        'txtLog
        '
        Me.txtLog.BackColor = System.Drawing.SystemColors.Control
        Me.txtLog.Location = New System.Drawing.Point(25, 378)
        Me.txtLog.Multiline = True
        Me.txtLog.Name = "txtLog"
        Me.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtLog.Size = New System.Drawing.Size(302, 97)
        Me.txtLog.TabIndex = 11
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(25, 481)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(302, 24)
        Me.ProgressBar1.TabIndex = 12
        Me.ProgressBar1.Visible = False
        '
        'lstFiles
        '
        Me.lstFiles.FormattingEnabled = True
        Me.lstFiles.Location = New System.Drawing.Point(26, 85)
        Me.lstFiles.Name = "lstFiles"
        Me.lstFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple
        Me.lstFiles.Size = New System.Drawing.Size(302, 264)
        Me.lstFiles.TabIndex = 13
        '
        'btnRefreshFiles
        '
        Me.btnRefreshFiles.Location = New System.Drawing.Point(238, 60)
        Me.btnRefreshFiles.Name = "btnRefreshFiles"
        Me.btnRefreshFiles.Size = New System.Drawing.Size(89, 22)
        Me.btnRefreshFiles.TabIndex = 14
        Me.btnRefreshFiles.Text = "Refresh"
        Me.btnRefreshFiles.UseVisualStyleBackColor = True
        '
        'btnRestore
        '
        Me.btnRestore.BackColor = System.Drawing.Color.Red
        Me.btnRestore.ForeColor = System.Drawing.Color.White
        Me.btnRestore.Location = New System.Drawing.Point(25, 542)
        Me.btnRestore.Name = "btnRestore"
        Me.btnRestore.Size = New System.Drawing.Size(302, 26)
        Me.btnRestore.TabIndex = 15
        Me.btnRestore.Text = "Restore Selected"
        Me.btnRestore.UseVisualStyleBackColor = False
        '
        'chkDeselectAllFiles
        '
        Me.chkDeselectAllFiles.AutoSize = True
        Me.chkDeselectAllFiles.Location = New System.Drawing.Point(246, 355)
        Me.chkDeselectAllFiles.Name = "chkDeselectAllFiles"
        Me.chkDeselectAllFiles.Size = New System.Drawing.Size(82, 17)
        Me.chkDeselectAllFiles.TabIndex = 17
        Me.chkDeselectAllFiles.Text = "Deselect All"
        Me.chkDeselectAllFiles.UseVisualStyleBackColor = True
        '
        'chkSelectAllFiles
        '
        Me.chkSelectAllFiles.AutoSize = True
        Me.chkSelectAllFiles.Location = New System.Drawing.Point(26, 355)
        Me.chkSelectAllFiles.Name = "chkSelectAllFiles"
        Me.chkSelectAllFiles.Size = New System.Drawing.Size(70, 17)
        Me.chkSelectAllFiles.TabIndex = 16
        Me.chkSelectAllFiles.Text = "Select All"
        Me.chkSelectAllFiles.UseVisualStyleBackColor = True
        '
        'btnFolderSelect
        '
        Me.btnFolderSelect.Location = New System.Drawing.Point(303, 33)
        Me.btnFolderSelect.Name = "btnFolderSelect"
        Me.btnFolderSelect.Size = New System.Drawing.Size(24, 20)
        Me.btnFolderSelect.TabIndex = 18
        Me.btnFolderSelect.Text = "..."
        Me.btnFolderSelect.UseVisualStyleBackColor = True
        '
        'lstServers
        '
        Me.lstServers.FormattingEnabled = True
        Me.lstServers.Location = New System.Drawing.Point(22, 34)
        Me.lstServers.Name = "lstServers"
        Me.lstServers.Size = New System.Drawing.Size(302, 95)
        Me.lstServers.TabIndex = 19
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(176, 10)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(146, 22)
        Me.Button1.TabIndex = 20
        Me.Button1.Text = "Get Servers"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'txtServer
        '
        Me.txtServer.Location = New System.Drawing.Point(22, 335)
        Me.txtServer.Name = "txtServer"
        Me.txtServer.Size = New System.Drawing.Size(196, 20)
        Me.txtServer.TabIndex = 21
        Me.txtServer.Text = "127.0.0.1"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(17, 18)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(43, 13)
        Me.Label3.TabIndex = 22
        Me.Label3.Text = "Servers"
        '
        'txtFile
        '
        Me.txtFile.Location = New System.Drawing.Point(22, 387)
        Me.txtFile.Name = "txtFile"
        Me.txtFile.Size = New System.Drawing.Size(272, 20)
        Me.txtFile.TabIndex = 25
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(22, 371)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(23, 13)
        Me.Label5.TabIndex = 26
        Me.Label5.Text = "File"
        '
        'RadioButton1
        '
        Me.RadioButton1.AutoSize = True
        Me.RadioButton1.Location = New System.Drawing.Point(48, 559)
        Me.RadioButton1.Name = "RadioButton1"
        Me.RadioButton1.Size = New System.Drawing.Size(101, 17)
        Me.RadioButton1.TabIndex = 27
        Me.RadioButton1.Text = "Single database"
        Me.RadioButton1.UseVisualStyleBackColor = True
        '
        'RadioButton2
        '
        Me.RadioButton2.AutoSize = True
        Me.RadioButton2.Checked = True
        Me.RadioButton2.Location = New System.Drawing.Point(176, 559)
        Me.RadioButton2.Name = "RadioButton2"
        Me.RadioButton2.Size = New System.Drawing.Size(93, 17)
        Me.RadioButton2.TabIndex = 28
        Me.RadioButton2.TabStop = True
        Me.RadioButton2.Text = "Batch process"
        Me.RadioButton2.UseVisualStyleBackColor = True
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.txtBackup)
        Me.Panel1.Controls.Add(Me.txtBackupFolder)
        Me.Panel1.Controls.Add(Me.lblBackupToFolder)
        Me.Panel1.Controls.Add(Me.lstFiles)
        Me.Panel1.Controls.Add(Me.btnRefreshFiles)
        Me.Panel1.Controls.Add(Me.btnRestore)
        Me.Panel1.Controls.Add(Me.btnFolderSelect)
        Me.Panel1.Controls.Add(Me.chkSelectAllFiles)
        Me.Panel1.Controls.Add(Me.chkDeselectAllFiles)
        Me.Panel1.Controls.Add(Me.ProgressBar1)
        Me.Panel1.Controls.Add(Me.txtLog)
        Me.Panel1.Location = New System.Drawing.Point(345, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(348, 576)
        Me.Panel1.TabIndex = 29
        '
        'btnBackupSingle
        '
        Me.btnBackupSingle.Location = New System.Drawing.Point(89, 436)
        Me.btnBackupSingle.Name = "btnBackupSingle"
        Me.btnBackupSingle.Size = New System.Drawing.Size(152, 35)
        Me.btnBackupSingle.TabIndex = 31
        Me.btnBackupSingle.Text = "Backup Database"
        Me.btnBackupSingle.UseVisualStyleBackColor = True
        '
        'btnRestoreSingle
        '
        Me.btnRestoreSingle.Location = New System.Drawing.Point(89, 492)
        Me.btnRestoreSingle.Name = "btnRestoreSingle"
        Me.btnRestoreSingle.Size = New System.Drawing.Size(152, 35)
        Me.btnRestoreSingle.TabIndex = 32
        Me.btnRestoreSingle.Text = "Restore Database"
        Me.btnRestoreSingle.UseVisualStyleBackColor = True
        '
        'btnGetFile
        '
        Me.btnGetFile.Location = New System.Drawing.Point(300, 386)
        Me.btnGetFile.Name = "btnGetFile"
        Me.btnGetFile.Size = New System.Drawing.Size(24, 20)
        Me.btnGetFile.TabIndex = 19
        Me.btnGetFile.Text = "..."
        Me.btnGetFile.UseVisualStyleBackColor = True
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.DefaultExt = "bak"
        Me.OpenFileDialog1.Filter = "*.bak|"
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoScroll = True
        Me.ClientSize = New System.Drawing.Size(694, 588)
        Me.Controls.Add(Me.lstDatabases)
        Me.Controls.Add(Me.chkSelectAll)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.chkDeselectAll)
        Me.Controls.Add(Me.btnGetFile)
        Me.Controls.Add(Me.btnRestoreSingle)
        Me.Controls.Add(Me.btnRefresh)
        Me.Controls.Add(Me.btnBackupSingle)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.RadioButton2)
        Me.Controls.Add(Me.RadioButton1)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.txtFile)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtServer)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.lstServers)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "Form1"
        Me.Text = "DB Backuper v 0.3"
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtBackup As System.Windows.Forms.Button
    Friend WithEvents btnRefresh As System.Windows.Forms.Button
    Friend WithEvents lstDatabases As System.Windows.Forms.ListBox
    Friend WithEvents chkSelectAll As System.Windows.Forms.CheckBox
    Friend WithEvents chkDeselectAll As System.Windows.Forms.CheckBox
    Friend WithEvents txtBackupFolder As System.Windows.Forms.TextBox
    Friend WithEvents lblBackupToFolder As System.Windows.Forms.Label
    Friend WithEvents txtLog As System.Windows.Forms.TextBox
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents lstFiles As System.Windows.Forms.ListBox
    Friend WithEvents btnRefreshFiles As System.Windows.Forms.Button
    Friend WithEvents btnRestore As System.Windows.Forms.Button
    Friend WithEvents chkDeselectAllFiles As System.Windows.Forms.CheckBox
    Friend WithEvents chkSelectAllFiles As System.Windows.Forms.CheckBox
    Friend WithEvents FolderBrowserDialog1 As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents btnFolderSelect As System.Windows.Forms.Button
    Friend WithEvents lstServers As System.Windows.Forms.ListBox
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents txtServer As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents txtFile As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents RadioButton1 As System.Windows.Forms.RadioButton
    Friend WithEvents RadioButton2 As System.Windows.Forms.RadioButton
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents btnBackupSingle As System.Windows.Forms.Button
    Friend WithEvents btnRestoreSingle As System.Windows.Forms.Button
    Friend WithEvents btnGetFile As System.Windows.Forms.Button
    Friend WithEvents OpenFileDialog1 As OpenFileDialog
End Class
