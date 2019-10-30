Imports System.Data.SqlClient

Public Class Form1

    Dim conn As SqlConnection


    Private Sub SetConnection()
        conn = Nothing
        conn = New SqlConnection()
        conn.ConnectionString = "Data Source=" & Me.txtServerDB.Text & ";Integrated Security=True"


    End Sub


    Private Sub BackupDatabase(ByVal DBname As String, ByVal Folder As String, ByRef er As String)

        SetConnection()

        If Not Folder.EndsWith("\") Then
            Folder = Folder & "\"
        End If

        Dim BackupFileName As String = Folder & DBname & Now.ToString("_MM_dd_yyyy") & ".bak"
        Dim BackupDescription As String = "Backup of " & DBname & Now.ToString(" MM dd yyyy") & ".bak"
        Dim SQL As String = "BACKUP DATABASE [" & DBname & "] TO DISK='" & BackupFileName & "' WITH INIT, DESCRIPTION='" & BackupDescription & "', MEDIADESCRIPTION='" & BackupDescription & "', NAME='" & BackupDescription & "',STATS=5"

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

    End Sub


    Private Function CheckDatabaseExist(ByVal DBname As String, ByRef er As String) As Boolean
        CheckDatabaseExist = False
        SetConnection()

        Dim SQL As String = "USE Master   " & vbCrLf & "select * from sysdatabases where name = '" & DBname & "'"


        'Dim cmd As New SqlCommand
        'cmd.CommandType = CommandType.Text
        'cmd.CommandText = SQL
        'cmd.Connection = conn


        'Dim rowCount As Integer
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


        'Dim BackupFileName As String = Folder & DBname & Now.ToString("_MM_dd_yyyy") & ".bak"
        'Dim BackupDescription As String = "Backup of " & DBname & Now.ToString(" MM dd yyyy") & ".bak"'
        'Dim SQL As String = "BACKUP DATABASE [" & DBname & "] TO DISK='" & BackupFileName & "' WITH INIT, DESCRIPTION='" & BackupDescription & "', MEDIADESCRIPTION='" & BackupDescription & "', NAME='" & BackupDescription & "',STATS=5"

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
                Dim SQLDA As New SqlDataAdapter("SELECT fileid, groupid, filename FROM SysFiles", conn)
                Dim SQLDS As New DataSet
                SQLDA.Fill(SQLDS)
                If SQLDS.Tables.Count > 0 AndAlso SQLDS.Tables(0).Rows.Count > 0 Then
                    Dim MasterDB As String = CStr(SQLDS.Tables(0).Rows(0)("filename"))
                    DBLocationFolder = MasterDB.Substring(0, MasterDB.LastIndexOf("\"))
                End If

                'get logical filename
                Dim LogicalDataFile As String = ""
                Dim LogicalLogFile As String = ""
                SQLDA = Nothing
                SQLDS = Nothing
                SQLDA = New SqlDataAdapter("RESTORE FILELISTONLY FROM DISK='" & FullFilename & "'", conn)
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


                If LogicalLogFile.Length > 0 And LogicalDataFile.Length > 0 Then
                    cmd.CommandText = "RESTORE DATABASE [" & DBname & "] FROM DISK='" & FullFilename & "' WITH MOVE '" & LogicalDataFile & "' TO '" & System.IO.Path.Combine(DBLocationFolder, DBname) & ".mdf' , MOVE '" & LogicalLogFile & "' TO '" & System.IO.Path.Combine(DBLocationFolder, DBname) & ".LDF', REPLACE,STATS=5 "
                ElseIf LogicalDataFile.Length > 0 Then
                    cmd.CommandText = "RESTORE DATABASE [" & DBname & "] FROM DISK='" & FullFilename & "' WITH MOVE '" & LogicalDataFile & "' TO '" & System.IO.Path.Combine(DBLocationFolder, DBname) & ".mdf',  REPLACE,STATS=5 "
                End If


                cmd.CommandTimeout = 60

                rowCount = cmd.ExecuteNonQuery()

                'all good

            Catch ex As Exception
                er = ex.Message
            Finally
                If previousConnectionState = ConnectionState.Closed Then
                    conn.Close()
                End If
            End Try



        End If



    End Sub


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

    End Sub

    Private Sub RetrieveDatabases()
        SetConnection()

        Dim SQL As String = "USE Master   " & vbCrLf & "select * from sysdatabases where sid <> 0x01 order by name"

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

    End Sub


    Private Sub btnRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRefresh.Click
        RetrieveDatabases()       
    End Sub

    Private Sub lstDatabases_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstDatabases.SelectedIndexChanged

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

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtLog.TextChanged

    End Sub

    Private Sub btnRefreshFiles_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRefreshFiles.Click
        Me.lstFiles.Items.Clear()



        Dim strFileSize As String = ""
        Dim di As New IO.DirectoryInfo(Me.txtBackupFolder.Text)
        Dim aryFi As IO.FileInfo() = di.GetFiles("*.bak")
        Dim fi As IO.FileInfo

        For Each fi In aryFi
            Me.lstFiles.Items.Add(fi.Name)
        Next



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



        Dim folder As String

        folder = Me.txtBackupFolder.Text


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
    End Sub
End Class
