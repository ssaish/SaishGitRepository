Imports System.Data.SqlClient
Imports System.Data.Sql
Imports WinSCP

Public Class Form1

    Dim conn As SqlConnection

    Dim _LinuxBox As Boolean

    Private Sub SetConnection(Optional ByVal Server As String = "")
        conn = Nothing
        conn = New SqlConnection()
        If Server.Length = 0 Then Server = Me.txtServer.Text
        Dim ConString = "Data Source=" & Server & ";Integrated Security=True"
        Try
            ConString = _Settings.GetS("Main", "ConnectionString", "Data Source=" & Server & ";Integrated Security=True")
        Catch ex As Exception
            ConString = "Data Source=" & Server & ";Integrated Security=True"
        End Try
        If ConString.Contains("10.0.0.27") Then _LinuxBox = True
        conn.ConnectionString = ConString
    End Sub


    Private Sub BackupDatabase(ByVal DBname As String, ByVal Folder As String, ByRef er As String, Optional ByVal Server As String = "")

        SetConnection(Server)

        If Not Folder.EndsWith("\") Then
            Folder = Folder & "\"
        End If

        Dim BackupFileName As String = Folder & DBname & Now.ToString("_dd_MM_yyyy") & ".bak"
        Dim BackupDescription As String = "Backup of " & DBname & Now.ToString(" dd MM yyyy") & ".bak"
        Dim SQL As String = "BACKUP DATABASE [" & DBname & "] TO DISK='" & BackupFileName & "' WITH INIT, DESCRIPTION='" & BackupDescription & "', MEDIADESCRIPTION='" & BackupDescription & "', NAME='" & BackupDescription & "',STATS=5"
        Dim LinuxFilePath As String = ""
        If _LinuxBox Then
            'Linux box
            Try
                LinuxFilePath = "/home/ids/mssql/profiles/saishs/Desktop/" & BackupFileName.Substring(BackupFileName.LastIndexOf("\") + 1)
                SQL = "BACKUP DATABASE [" & DBname & "] TO DISK='" & LinuxFilePath & "' WITH INIT, DESCRIPTION='" & BackupDescription & "', MEDIADESCRIPTION='" & BackupDescription & "', NAME='" & BackupDescription & "',STATS=5"

            Catch ex As Exception
                er = ex.Message
            End Try
        End If

        Dim cmd As New SqlCommand
        cmd.CommandType = CommandType.Text
        cmd.CommandText = SQL
        cmd.Connection = conn


        Dim rowCount As Integer
        Dim previousConnectionState As ConnectionState
        previousConnectionState = conn.State
        Try
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If
            cmd.CommandTimeout = 60
            rowCount = cmd.ExecuteNonQuery()
        Catch ex As Exception
            er = ex.Message
        Finally
            If previousConnectionState = ConnectionState.Closed Then
                conn.Close()
            End If
        End Try
        If _LinuxBox Then
            WinSCPSessionCopy(BackupFileName, LinuxFilePath, True, True)
        End If

    End Sub


    Private Function CheckDatabaseExist(ByVal DBname As String, ByRef er As String) As Boolean
        CheckDatabaseExist = False
        SetConnection()

        Dim SQL As String = "USE Master   " & vbCrLf & "select * from sysdatabases where name = '" & DBname & "'"

        Dim previousConnectionState As ConnectionState
        previousConnectionState = conn.State
        Try
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If
            'rowCount = cmd.ExecuteReader

            Dim SQLDA As New SqlDataAdapter(SQL, conn)

            Dim SQLDS As New DataSet
            SQLDA.Fill(SQLDS)

            If SQLDS.Tables.Count > 0 AndAlso SQLDS.Tables(0).Rows.Count > 0 Then
                CheckDatabaseExist = True
            End If
        Catch ex As Exception
            er = ex.Message
        Finally
            If previousConnectionState = ConnectionState.Closed Then
                conn.Close()
            End If
        End Try

    End Function


    Private Sub RestoreDatabase(ByVal DBname As String, ByVal FullFilename As String, ByVal CreateNew As Boolean, ByRef er As String)

        SetConnection()
        'CreateNew = False
        If CreateNew Then
            Dim cmd As New SqlCommand
            cmd.CommandType = CommandType.Text
            cmd.CommandText = "CREATE DATABASE " & DBname
            cmd.Connection = conn

            Dim rowCount As Integer
            Dim previousConnectionState As ConnectionState
            previousConnectionState = conn.State
            Try
                If conn.State = ConnectionState.Closed Then
                    conn.Open()
                End If
                cmd.CommandTimeout = 60
                rowCount = cmd.ExecuteNonQuery()
            Catch ex As Exception
                er = ex.Message
            Finally
                If previousConnectionState = ConnectionState.Closed Then
                    conn.Close()
                End If
            End Try
        End If

        If er = "" Then
            Dim DBLocationFolder As String = ""

            Dim cmd As New SqlCommand
            cmd.CommandType = CommandType.Text

            cmd.Connection = conn

            Dim rowCount As Integer
            Dim previousConnectionState As ConnectionState
            previousConnectionState = conn.State
            Try
                If conn.State = ConnectionState.Closed Then
                    conn.Open()
                End If

                'get location of database files
                Dim LinuxBox As Boolean
                Dim SQLDA As New SqlDataAdapter("SELECT fileid, groupid, filename FROM SysFiles", conn)
                Dim SQLDS As New DataSet
                SQLDA.Fill(SQLDS)
                If SQLDS.Tables.Count > 0 AndAlso SQLDS.Tables(0).Rows.Count > 0 Then
                    Dim MasterDB As String = CStr(SQLDS.Tables(0).Rows(0)("filename"))
                    Try
                        DBLocationFolder = MasterDB.Substring(0, MasterDB.LastIndexOf("\"))
                    Catch ex As Exception
                        DBLocationFolder = MasterDB.Substring(0, MasterDB.LastIndexOf("/")) '"/home/ids/"
                        LinuxBox = True
                    End Try
                End If
                Dim LinuxFilePath As String = ""
                'Linux box
                If LinuxBox Then
                    Try
                        LinuxFilePath = "/home/ids/" & FullFilename.Substring(FullFilename.LastIndexOf("\") + 1)
                        WinSCPSession(LinuxFilePath, FullFilename, True, False)
                    Catch ex As Exception
                        er = ex.Message
                    End Try
                End If

                If er.Length = 0 Then
                    'get logical filename
                    Dim LogicalDataFile As String = ""
                    Dim LogicalLogFile As String = ""
                    SQLDA = Nothing
                    SQLDS = Nothing
                    If LinuxBox Then
                        SQLDA = New SqlDataAdapter("RESTORE FILELISTONLY FROM DISK='" & LinuxFilePath & "'", conn)
                    Else
                        SQLDA = New SqlDataAdapter("RESTORE FILELISTONLY FROM DISK='" & FullFilename & "'", conn)
                    End If

                    SQLDS = New DataSet
                    SQLDA.Fill(SQLDS)
                    If SQLDS.Tables.Count > 0 AndAlso SQLDS.Tables(0).Rows.Count > 0 Then

                        For Each r As DataRow In SQLDS.Tables(0).Rows
                            If CStr(r("Type")) = "D" Then
                                LogicalDataFile = CStr(r("LogicalName"))
                            ElseIf CStr(r("Type")) = "L" Then
                                LogicalLogFile = CStr(r("LogicalName"))
                            End If
                        Next
                    End If

                    If LinuxBox Then
                        If LogicalLogFile.Length > 0 And LogicalDataFile.Length > 0 Then
                            cmd.CommandText = "RESTORE DATABASE [" & DBname & "] FROM DISK='" & LinuxFilePath & "' WITH MOVE '" & LogicalDataFile & "' TO '" & System.IO.Path.Combine(DBLocationFolder, DBname) & ".mdf' , MOVE '" & LogicalLogFile & "' TO '" & System.IO.Path.Combine(DBLocationFolder, DBname) & ".LDF', REPLACE,STATS=5 "
                        ElseIf LogicalDataFile.Length > 0 Then
                            cmd.CommandText = "RESTORE DATABASE [" & DBname & "] FROM DISK='" & LinuxFilePath & "' WITH MOVE '" & LogicalDataFile & "' TO '" & System.IO.Path.Combine(DBLocationFolder, DBname) & ".mdf',  REPLACE,STATS=5 "
                        End If
                    Else
                        If LogicalLogFile.Length > 0 And LogicalDataFile.Length > 0 Then
                            cmd.CommandText = "RESTORE DATABASE [" & DBname & "] FROM DISK='" & FullFilename & "' WITH MOVE '" & LogicalDataFile & "' TO '" & System.IO.Path.Combine(DBLocationFolder, DBname) & ".mdf' , MOVE '" & LogicalLogFile & "' TO '" & System.IO.Path.Combine(DBLocationFolder, DBname) & ".LDF', REPLACE,STATS=5 "
                        ElseIf LogicalDataFile.Length > 0 Then
                            cmd.CommandText = "RESTORE DATABASE [" & DBname & "] FROM DISK='" & FullFilename & "' WITH MOVE '" & LogicalDataFile & "' TO '" & System.IO.Path.Combine(DBLocationFolder, DBname) & ".mdf',  REPLACE,STATS=5 "
                        End If
                    End If

                    cmd.CommandTimeout = 60

                    rowCount = cmd.ExecuteNonQuery()

                    'all good
                    If LinuxBox Then
                        Try
                            LinuxFilePath = "/home/ids/" & FullFilename.Substring(FullFilename.LastIndexOf("\") + 1)
                            WinSCPSession(LinuxFilePath, FullFilename, False, True)
                        Catch ex As Exception
                            er = ex.Message
                        End Try
                    End If
                End If
            Catch ex As Exception
                er = ex.Message
            Finally
                If previousConnectionState = ConnectionState.Closed Then
                    conn.Close()
                End If
            End Try
        End If
    End Sub

    Function WinSCPSessionCopy(FullFilename As String, LinuxFilePath As String, copy As Boolean, delete As Boolean) As Boolean

        Try
            ' Setup session options
            Dim sessionOptions As New SessionOptions
            With sessionOptions
                .Protocol = Protocol.Sftp
                .HostName = txtServer.Text
                .UserName = "ids"
                .Password = "mat123"
                .GiveUpSecurityAndAcceptAnySshHostKey = True
                '.SshHostKeyFingerprint = "ssh-ed25519 256 c7EA7H/C22Z39M/rKUwgnJ8Ekc3EE/1Xzi5QzMq+GNM=ssh-ed25519 256 63:46:f2:ad:9c:7a:b1:53:b6:76:f7:ac:83:1b:e0:60"
                '"ssh-rsa 2048 xxxxxxxxxxx...="
            End With

            Using session As New Session
                ' Connect
                session.Open(sessionOptions)
                Dim uploadComplete As Boolean
                If copy Then
                    ' Upload files
                    Dim transferOptions As New TransferOptions
                    transferOptions.TransferMode = TransferMode.Binary
                    Dim transferResult As TransferOperationResult
                    transferResult = session.GetFiles(LinuxFilePath, FullFilename, False, transferOptions)
                    ' Throw on any error
                    Do While True
                        transferResult.Check()
                        ' Print results
                        If transferResult.IsSuccess Then
                            For Each transfer In transferResult.Transfers
                                Me.txtLog.Text = Me.txtLog.Text & "  " & "Download of " & transfer.FileName & " succeeded"
                                uploadComplete = True
                            Next
                            Exit Do
                        End If

                    Loop
                End If
                If delete And uploadComplete Then
                    ' Upload files
                    Dim removeResults As RemovalOperationResult
                    removeResults = session.RemoveFiles(LinuxFilePath)
                    ' Throw on any error
                    Do While True
                        ' Throw on any error
                        removeResults.Check()
                        If removeResults.IsSuccess Then
                            ' Print results
                            For Each removefile In removeResults.Removals
                                Me.txtLog.Text = Me.txtLog.Text & "  " & "Removed file " & removefile.FileName & " succeeded"
                            Next
                            Exit Do
                        End If
                    Loop
                End If
                Return True
            End Using
        Catch ex As Exception
            Me.txtLog.Text = Me.txtLog.Text & "  " & ex.Message
            Return False
        End Try
    End Function

    Function WinSCPSession(LinuxFilePath As String, FullFilename As String, copy As Boolean, delete As Boolean) As Boolean

        Try
            ' Setup session options
            Dim sessionOptions As New SessionOptions
            With sessionOptions
                .Protocol = Protocol.Sftp
                .HostName = txtServer.Text
                .UserName = "ids"
                .Password = "mat123"
                .GiveUpSecurityAndAcceptAnySshHostKey = True
                '.SshHostKeyFingerprint = "ssh-ed25519 256 c7EA7H/C22Z39M/rKUwgnJ8Ekc3EE/1Xzi5QzMq+GNM=ssh-ed25519 256 63:46:f2:ad:9c:7a:b1:53:b6:76:f7:ac:83:1b:e0:60"
                '"ssh-rsa 2048 xxxxxxxxxxx...="
            End With

            Using session As New Session
                ' Connect
                session.Open(sessionOptions)
                If copy Then
                    ' Upload files
                    Dim transferOptions As New TransferOptions
                    transferOptions.TransferMode = TransferMode.Binary
                    Dim transferResult As TransferOperationResult
                    transferResult = session.PutFiles(FullFilename, LinuxFilePath, False, transferOptions)
                    ' Throw on any error
                    Do While True
                        transferResult.Check()
                        ' Print results
                        If transferResult.IsSuccess Then
                            For Each transfer In transferResult.Transfers
                                Me.txtLog.Text = Me.txtLog.Text & "  " & "Upload of " & transfer.FileName & " succeeded"
                            Next
                            Exit Do
                        End If

                    Loop
                End If
                If delete Then
                    ' Upload files
                    Dim removeResults As RemovalOperationResult
                    removeResults = session.RemoveFiles(LinuxFilePath)
                    ' Throw on any error
                    Do While True
                        ' Throw on any error
                        removeResults.Check()
                        If removeResults.IsSuccess Then
                            ' Print results
                            For Each removefile In removeResults.Removals
                                Me.txtLog.Text = Me.txtLog.Text & "  " & "Removed file " & removefile.FileName & " succeeded"
                            Next
                            Exit Do
                        End If
                    Loop
                End If
                Return True
            End Using
        Catch ex As Exception
            Me.txtLog.Text = Me.txtLog.Text & "  " & ex.Message
            Return False
        End Try
    End Function

    Private Sub txtBackup_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtBackup.Click


        Me.txtLog.Text = "Backup started..."

        Dim er As String = ""

        Dim folder As String

        If Me.txtBackupFolder.Text.Length > 0 Then

            folder = Me.txtBackupFolder.Text
            If Not My.Computer.FileSystem.DirectoryExists(folder) Then
                My.Computer.FileSystem.CreateDirectory(folder)
            End If

        Else
            'software location
            folder = My.Application.Info.DirectoryPath
        End If

        Me.ProgressBar1.Visible = True
        Me.ProgressBar1.Minimum = 0
        Me.ProgressBar1.Value = 0

        Dim i As Integer = 0
        Dim selectedCount As Integer = 0
        For Each dbname As String In Me.lstDatabases.Items

            If Me.lstDatabases.GetSelected(i) Then
                selectedCount += 1
            End If

            i += 1
        Next
        Me.ProgressBar1.Maximum = selectedCount * 2


        i = 0
        For Each dbname As String In Me.lstDatabases.Items
            If Me.lstDatabases.GetSelected(i) Then

                Me.txtLog.Text = Me.txtLog.Text & vbCrLf & "Backing up: " & dbname

                Me.ProgressBar1.Value += 1
                Application.DoEvents()
                BackupDatabase(dbname, folder, er)

                If er.Length > 0 Then
                    MessageBox.Show(er)
                    Me.txtLog.Text = Me.txtLog.Text & "  FAIL: " & er
                    Me.ProgressBar1.Visible = False
                    Exit Sub
                Else
                    Me.txtLog.Text = Me.txtLog.Text & "  success!"
                End If
                If Me.ProgressBar1.Value < Me.ProgressBar1.Maximum + 1 Then
                    Me.ProgressBar1.Value += 1
                End If

            End If

            Me.txtLog.ScrollToCaret()
            i += 1
        Next

        Me.txtLog.Text = Me.txtLog.Text & vbCrLf & "Back up finished"
        Me.ProgressBar1.Visible = False



        RefreshBUFolder()

    End Sub

    Private Sub RetrieveDatabases()
        Try
            SetConnection()

            Dim SQL As String = "USE Master   
                                select * from sysdatabases where name not in (
                                'master',
                                'tempdb',
                                'model',
                                'msdb'
                                ) order by name"

            Dim cmd As New SqlCommand
            cmd.CommandType = CommandType.Text
            cmd.CommandText = SQL
            cmd.Connection = conn
            Dim SQLDA As New SqlDataAdapter(SQL, conn)

            Dim SQLDS As New DataSet
            SQLDA.Fill(SQLDS)


            Me.lstDatabases.Items.Clear()
            If SQLDS.Tables.Count > 0 Then
                For Each r As DataRow In SQLDS.Tables(0).Rows
                    Me.lstDatabases.Items.Add(r("name"))
                Next
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub btnRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRefresh.Click
        RetrieveDatabases()
    End Sub

    Private Sub chkSelectAll_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSelectAll.CheckedChanged
        Me.chkDeselectAll.Checked = Not Me.chkSelectAll.Checked
        If Me.chkSelectAll.Checked Then
            For i As Integer = 0 To Me.lstDatabases.Items.Count - 1
                Me.lstDatabases.SetSelected(i, True)
            Next
        End If
    End Sub

    Private Sub chkDeselectAll_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDeselectAll.CheckedChanged
        Me.chkSelectAll.Checked = Not Me.chkDeselectAll.Checked
        If Me.chkDeselectAll.Checked Then
            For i As Integer = 0 To Me.lstDatabases.Items.Count - 1
                Me.lstDatabases.SetSelected(i, False)
            Next
        End If
    End Sub

    Private Sub RefreshBUFolder()
        Me.lstFiles.Items.Clear()

        Dim strFileSize As String = ""

        If My.Computer.FileSystem.DirectoryExists(Me.txtBackupFolder.Text) Then
            Dim di As New IO.DirectoryInfo(Me.txtBackupFolder.Text)
            Dim aryFi As IO.FileInfo() = di.GetFiles("*.bak")
            Dim fi As IO.FileInfo

            For Each fi In aryFi
                Me.lstFiles.Items.Add(fi.Name)
            Next
        End If
    End Sub

    Private Sub btnRefreshFiles_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRefreshFiles.Click
        RefreshBUFolder()
    End Sub



    Private Sub chkDeselectAllFiles_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDeselectAllFiles.CheckedChanged
        Me.chkSelectAllFiles.Checked = Not Me.chkDeselectAllFiles.Checked
        If Me.chkDeselectAllFiles.Checked Then
            For i As Integer = 0 To Me.lstFiles.Items.Count - 1
                Me.lstFiles.SetSelected(i, False)
            Next
        End If
    End Sub

    Private Sub chkSelectAllFiles_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSelectAllFiles.CheckedChanged
        Me.chkDeselectAllFiles.Checked = Not Me.chkSelectAllFiles.Checked
        If Me.chkSelectAllFiles.Checked Then
            For i As Integer = 0 To Me.lstFiles.Items.Count - 1
                Me.lstFiles.SetSelected(i, True)
            Next
        End If
    End Sub

    Private Sub btnRestore_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRestore.Click
        Dim er As String = ""
        Dim folder As String = Me.txtBackupFolder.Text
        If Me.lstFiles.SelectedItems.Count = 0 Then
            MsgBox("No selected files to restore" & vbCrLf & "Unable to continue")
            Exit Sub
        End If
        Me.txtLog.Text = "Restore started..."


        Me.ProgressBar1.Visible = True
        Me.ProgressBar1.Minimum = 0
        Me.ProgressBar1.Value = 0

        Dim i As Integer = 0
        Dim selectedCount As Integer = 0
        For Each dbname As String In Me.lstFiles.Items
            If Me.lstFiles.GetSelected(i) Then
                selectedCount += 1
            End If
            i += 1
        Next
        Me.ProgressBar1.Maximum = selectedCount * 2


        i = 0
        For Each FileName As String In Me.lstFiles.SelectedItems
            'If Me.lstDatabases.GetSelected(i) Then
            Dim DBname As String = ""

            If FileName.Length > 15 Then

                DBname = FileName.Substring(0, FileName.Length - 15)

                Me.txtLog.Text = Me.txtLog.Text & vbCrLf & "Restoring: " & DBname & " From: " & FileName

                Me.ProgressBar1.Value += 1
                Application.DoEvents()

                If CheckDatabaseExist(DBname, er) = True Then
                    'restore
                    RestoreDatabase(DBname, System.IO.Path.Combine(folder, FileName), False, er)
                Else
                    'create and restore
                    RestoreDatabase(DBname, System.IO.Path.Combine(folder, FileName), True, er)
                End If

                If er.Length > 0 Then
                    MessageBox.Show(er)
                    Me.txtLog.Text = Me.txtLog.Text & "  FAIL: " & er
                    Me.ProgressBar1.Visible = False
                    Exit Sub
                Else
                    Me.txtLog.Text = Me.txtLog.Text & "  success!"
                End If
                If Me.ProgressBar1.Value < Me.ProgressBar1.Maximum + 1 Then
                    Me.ProgressBar1.Value += 1
                End If

                'End If

                Me.txtLog.ScrollToCaret()



            Else
                'no db to work with
            End If


            i += 1
        Next

        Me.txtLog.Text = Me.txtLog.Text & vbCrLf & "Restore finished"
        Me.ProgressBar1.Visible = False

        'update list of databases
        RetrieveDatabases()

    End Sub



    Private Function GetSqlServers() As List(Of String)

        Dim listOfServers As New List(Of String)()


        Dim sqlEnumerator As SqlDataSourceEnumerator = SqlDataSourceEnumerator.Instance


        Dim sqlServersTable As DataTable = sqlEnumerator.GetDataSources()


        For Each rowOfData As DataRow In sqlServersTable.Rows

            Dim serverName As String = rowOfData("ServerName").ToString()

            Dim instanceName As String = rowOfData("InstanceName").ToString()


            If Not instanceName.Equals(String.Empty) Then
                serverName += String.Format("\{0}", instanceName)
            End If


            listOfServers.Add(serverName)
        Next


        listOfServers.Sort()

        Return listOfServers
    End Function



    Dim _Settings As xSettingsSimple

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        If txtBackupFolder.Text.Length > 0 Then RefreshBUFolder()
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        _Settings.SetS("Main", "Server", txtServer.Text)
        _Settings.SetS("Main", "BackupFolder", txtBackupFolder.Text)
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Dim args As String() = Environment.GetCommandLineArgs()

        'params eg: 
        'server:127.0.0.1\SQL2008 database:KCA path:D:\
        Dim Server As String = ""
        Dim Database As String = ""
        Dim Path As String = ""

        For Each s In args
            If s.Contains("server:") Then
                Server = s.Replace("server:", "")
            ElseIf s.Contains("database:") Then
                Database = s.Replace("database:", "")
            ElseIf s.Contains("path:") Then
                Path = s.Replace("path:", "")
            End If

        Next

        If Server.Length > 0 And Database.Length > 0 And Path.Length > 0 Then
            BackupDatabase(Database, Path, "", Server)
            Application.Exit()
        ElseIf Server.Length > 0 And Database.Length > 0 Then
            BackupDatabase(Database, Application.StartupPath, "", Server)
            Application.Exit()
        End If


        BatchSwitch()
    End Sub

    Private Sub BatchSwitch()
        If _Settings Is Nothing Then
            _Settings = New xSettingsSimple("DBBackuper")
        End If
        txtServer.Text = _Settings.GetS("Main", "Server", "127.0.0.1")
        txtBackupFolder.Text = _Settings.GetS("Main", "BackupFolder", "")
        If RadioButton1.Checked Then
            Me.Panel1.Visible = False
            Me.Width = 350
            Me.txtFile.Enabled = True
        Else
            Me.Panel1.Visible = True
            Me.Width = 700
            Me.txtFile.Enabled = False
        End If
    End Sub

    Private Sub btnFolderSelect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFolderSelect.Click
        Me.FolderBrowserDialog1.ShowDialog()
        If Me.FolderBrowserDialog1.SelectedPath.Length > 0 Then
            txtBackupFolder.Text = Me.FolderBrowserDialog1.SelectedPath
            RefreshBUFolder()
        End If

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        For Each s In GetSqlServers()
            lstServers.Items.Add(s)
        Next
    End Sub

    Private Sub lstServers_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstServers.SelectedIndexChanged
        Me.txtServer.Text = Me.lstServers.SelectedItem
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles RadioButton1.CheckedChanged
        BatchSwitch()
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles RadioButton2.CheckedChanged
        BatchSwitch()
    End Sub

    Private Sub btnGetFile_Click(sender As Object, e As EventArgs) Handles btnGetFile.Click
        If txtBackupFolder.Text.Length > 0 Then OpenFileDialog1.InitialDirectory = txtBackupFolder.Text
        Me.OpenFileDialog1.ShowDialog()
        If Me.OpenFileDialog1.SafeFileName.Length > 0 Then
            txtFile.Text = OpenFileDialog1.FileName
        End If
    End Sub

    Private Sub btnBackupSingle_Click(sender As Object, e As EventArgs) Handles btnBackupSingle.Click
        Me.FolderBrowserDialog1.ShowDialog()
        If Me.FolderBrowserDialog1.SelectedPath.Length > 0 Then
            Dim er As String = ""
            BackupDatabase(lstDatabases.SelectedItem.ToString, Me.FolderBrowserDialog1.SelectedPath, er)
            If er.Length = 0 Then
                MessageBox.Show("Backup Success")
            End If
        End If
    End Sub

    Private Sub btnRestoreSingle_Click(sender As Object, e As EventArgs) Handles btnRestoreSingle.Click
        Dim FileName As String = txtFile.Text.Trim
        Dim er = ""
        If FileName.Length > 15 Then

            Dim DBname = FileName.Substring(0, FileName.Length - 15)
            DBname = DBname.Substring(DBname.LastIndexOf("\") + 1)
            Me.txtLog.Text = Me.txtLog.Text & vbCrLf & "Restoring: " & DBname & " From: " & FileName

            Me.ProgressBar1.Value += 1
            Application.DoEvents()

            If CheckDatabaseExist(DBname, er) = True Then
                'restore
                RestoreDatabase(DBname, FileName, False, er)
            Else
                'create and restore
                RestoreDatabase(DBname, FileName, True, er)
            End If

            If er.Length > 0 Then
                MessageBox.Show(er)
                Me.txtLog.Text = Me.txtLog.Text & "  FAIL: " & er
                Me.ProgressBar1.Visible = False
                Exit Sub
            Else
                Me.txtLog.Text = Me.txtLog.Text & "  success!"
            End If
            If Me.ProgressBar1.Value < Me.ProgressBar1.Maximum + 1 Then
                Me.ProgressBar1.Value += 1
            End If

            'End If

            Me.txtLog.ScrollToCaret()
        Else
            'no db to work with
        End If
    End Sub
End Class
