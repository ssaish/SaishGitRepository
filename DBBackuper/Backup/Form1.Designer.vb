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
        Me.txtServerDB = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.txtBackup = New System.Windows.Forms.Button
        Me.btnRefresh = New System.Windows.Forms.Button
        Me.lstDatabases = New System.Windows.Forms.ListBox
        Me.chkSelectAll = New System.Windows.Forms.CheckBox
        Me.chkDeselectAll = New System.Windows.Forms.CheckBox
        Me.txtBackupFolder = New System.Windows.Forms.TextBox
        Me.lblBackupToFolder = New System.Windows.Forms.Label
        Me.txtLog = New System.Windows.Forms.TextBox
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar
        Me.lstFiles = New System.Windows.Forms.ListBox
        Me.btnRefreshFiles = New System.Windows.Forms.Button
        Me.btnRestore = New System.Windows.Forms.Button
        Me.chkDeselectAllFiles = New System.Windows.Forms.CheckBox
        Me.chkSelectAllFiles = New System.Windows.Forms.CheckBox
        Me.SuspendLayout()
        '
        'txtServerDB
        '
        Me.txtServerDB.Location = New System.Drawing.Point(7, 21)
        Me.txtServerDB.Name = "txtServerDB"
        Me.txtServerDB.Size = New System.Drawing.Size(245, 20)
        Me.txtServerDB.TabIndex = 0
        Me.txtServerDB.Text = "127.0.0.1"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(4, 5)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(38, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Server"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(4, 54)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(58, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Databases"
        '
        'txtBackup
        '
        Me.txtBackup.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.txtBackup.Location = New System.Drawing.Point(7, 364)
        Me.txtBackup.Name = "txtBackup"
        Me.txtBackup.Size = New System.Drawing.Size(245, 25)
        Me.txtBackup.TabIndex = 4
        Me.txtBackup.Text = "Backup Selected"
        Me.txtBackup.UseVisualStyleBackColor = False
        '
        'btnRefresh
        '
        Me.btnRefresh.Location = New System.Drawing.Point(163, 47)
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.Size = New System.Drawing.Size(89, 22)
        Me.btnRefresh.TabIndex = 5
        Me.btnRefresh.Text = "Refresh"
        Me.btnRefresh.UseVisualStyleBackColor = True
        '
        'lstDatabases
        '
        Me.lstDatabases.FormattingEnabled = True
        Me.lstDatabases.Location = New System.Drawing.Point(7, 70)
        Me.lstDatabases.Name = "lstDatabases"
        Me.lstDatabases.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple
        Me.lstDatabases.Size = New System.Drawing.Size(245, 264)
        Me.lstDatabases.TabIndex = 6
        '
        'chkSelectAll
        '
        Me.chkSelectAll.AutoSize = True
        Me.chkSelectAll.Location = New System.Drawing.Point(12, 340)
        Me.chkSelectAll.Name = "chkSelectAll"
        Me.chkSelectAll.Size = New System.Drawing.Size(70, 17)
        Me.chkSelectAll.TabIndex = 7
        Me.chkSelectAll.Text = "Select All"
        Me.chkSelectAll.UseVisualStyleBackColor = True
        '
        'chkDeselectAll
        '
        Me.chkDeselectAll.AutoSize = True
        Me.chkDeselectAll.Location = New System.Drawing.Point(173, 340)
        Me.chkDeselectAll.Name = "chkDeselectAll"
        Me.chkDeselectAll.Size = New System.Drawing.Size(82, 17)
        Me.chkDeselectAll.TabIndex = 8
        Me.chkDeselectAll.Text = "Deselect All"
        Me.chkDeselectAll.UseVisualStyleBackColor = True
        '
        'txtBackupFolder
        '
        Me.txtBackupFolder.Location = New System.Drawing.Point(267, 21)
        Me.txtBackupFolder.Name = "txtBackupFolder"
        Me.txtBackupFolder.Size = New System.Drawing.Size(262, 20)
        Me.txtBackupFolder.TabIndex = 9
        Me.txtBackupFolder.Text = "d:\DBBackup\"
        '
        'lblBackupToFolder
        '
        Me.lblBackupToFolder.AutoSize = True
        Me.lblBackupToFolder.Location = New System.Drawing.Point(264, 5)
        Me.lblBackupToFolder.Name = "lblBackupToFolder"
        Me.lblBackupToFolder.Size = New System.Drawing.Size(79, 13)
        Me.lblBackupToFolder.TabIndex = 10
        Me.lblBackupToFolder.Text = "Backup Folder:"
        '
        'txtLog
        '
        Me.txtLog.BackColor = System.Drawing.SystemColors.Control
        Me.txtLog.Location = New System.Drawing.Point(7, 422)
        Me.txtLog.Multiline = True
        Me.txtLog.Name = "txtLog"
        Me.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtLog.Size = New System.Drawing.Size(522, 182)
        Me.txtLog.TabIndex = 11
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(7, 392)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(522, 24)
        Me.ProgressBar1.TabIndex = 12
        Me.ProgressBar1.Visible = False
        '
        'lstFiles
        '
        Me.lstFiles.FormattingEnabled = True
        Me.lstFiles.Location = New System.Drawing.Point(267, 70)
        Me.lstFiles.Name = "lstFiles"
        Me.lstFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple
        Me.lstFiles.Size = New System.Drawing.Size(262, 264)
        Me.lstFiles.TabIndex = 13
        '
        'btnRefreshFiles
        '
        Me.btnRefreshFiles.Location = New System.Drawing.Point(440, 47)
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
        Me.btnRestore.Location = New System.Drawing.Point(267, 363)
        Me.btnRestore.Name = "btnRestore"
        Me.btnRestore.Size = New System.Drawing.Size(262, 26)
        Me.btnRestore.TabIndex = 15
        Me.btnRestore.Text = "Restore Selected"
        Me.btnRestore.UseVisualStyleBackColor = False
        '
        'chkDeselectAllFiles
        '
        Me.chkDeselectAllFiles.AutoSize = True
        Me.chkDeselectAllFiles.Location = New System.Drawing.Point(428, 340)
        Me.chkDeselectAllFiles.Name = "chkDeselectAllFiles"
        Me.chkDeselectAllFiles.Size = New System.Drawing.Size(82, 17)
        Me.chkDeselectAllFiles.TabIndex = 17
        Me.chkDeselectAllFiles.Text = "Deselect All"
        Me.chkDeselectAllFiles.UseVisualStyleBackColor = True
        '
        'chkSelectAllFiles
        '
        Me.chkSelectAllFiles.AutoSize = True
        Me.chkSelectAllFiles.Location = New System.Drawing.Point(267, 340)
        Me.chkSelectAllFiles.Name = "chkSelectAllFiles"
        Me.chkSelectAllFiles.Size = New System.Drawing.Size(70, 17)
        Me.chkSelectAllFiles.TabIndex = 16
        Me.chkSelectAllFiles.Text = "Select All"
        Me.chkSelectAllFiles.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoScroll = True
        Me.ClientSize = New System.Drawing.Size(541, 616)
        Me.Controls.Add(Me.chkDeselectAllFiles)
        Me.Controls.Add(Me.chkSelectAllFiles)
        Me.Controls.Add(Me.btnRestore)
        Me.Controls.Add(Me.btnRefreshFiles)
        Me.Controls.Add(Me.lstFiles)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.txtLog)
        Me.Controls.Add(Me.lblBackupToFolder)
        Me.Controls.Add(Me.txtBackupFolder)
        Me.Controls.Add(Me.chkDeselectAll)
        Me.Controls.Add(Me.chkSelectAll)
        Me.Controls.Add(Me.lstDatabases)
        Me.Controls.Add(Me.btnRefresh)
        Me.Controls.Add(Me.txtBackup)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtServerDB)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "Form1"
        Me.Text = "DB Backuper v 0.2"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtServerDB As System.Windows.Forms.TextBox
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

End Class
