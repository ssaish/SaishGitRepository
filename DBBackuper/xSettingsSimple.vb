Option Strict On
Option Explicit On
Imports System.Collections.Generic

Public Class xSettingsSimple

    Dim _FileGroups As New System.Collections.Generic.List(Of xFileGroup)
    Dim _SettingsFileName As String
    Dim _FolderPath As String

    Private Class xFileGroup
        Public Title As String
        Public FilePos As String
        Public Items As System.Collections.Generic.List(Of String)
    End Class

    Public Sub New(ByVal Name As String)
        _SettingsFileName = Name

        ' For normal applications:
        _FolderPath = My.Application.Info.DirectoryPath

        'For web applications
        ' _FolderPath = HttpContext.Current.Server.MapPath(".")

        LoadSettings()
    End Sub

    Private Sub LoadSettings()
        Dim i As Integer
        Dim j As Integer
        Dim fg As New xFileGroup
        Dim dat As String
        Dim sa() As String

        ' open ini file
        ' if ini file not found this will automatically create new ini file
        Try
            If My.Computer.FileSystem.FileExists(_FolderPath & "\" & _SettingsFileName & ".ini") Then
                dat = My.Computer.FileSystem.ReadAllText(_FolderPath & "\" & _SettingsFileName & ".ini", System.Text.Encoding.GetEncoding(0))
            Else ' create
                dat = ""
            End If

        Catch ex As Exception
            dat = ""
        End Try

        dat = dat.Replace(Chr(10), "")
        sa = dat.Split(CChar(vbCrLf))

        ' load the groups
        For Each txt As String In sa
            i = InStr(1, txt, "[")
            j = InStr(1, txt, "]")
            If i = 1 And j > 0 And j > i Then
                fg = New xFileGroup
                fg.Title = Mid(txt, i + 1, j - i - 1)
                fg.FilePos = "9" ' goes to the bottom of the file if not used
                fg.Items = New System.Collections.Generic.List(Of String)
                _FileGroups.Add(fg)
            Else
                If txt <> "" Then
                    fg.Items.Add(txt)
                End If
            End If
        Next
        'Loaded = True
    End Sub

    Public Function GetS(ByVal Group As String, ByVal name As String, Optional ByVal DefaultValue As String = "") As String
        Dim i As Integer
        Dim s As String = ""
        Dim value As String = ""
        Dim fg As New System.Collections.Generic.List(Of String)
        Dim SettingFound As Boolean
        For i = 0 To _FileGroups.Count - 1
            If _FileGroups(i).Title = Group Then
                fg = _FileGroups(i).Items
                If fg.Count > 0 Then
                    For Each s In fg
                        If Left(s, s.IndexOf("=")).ToLower = name.ToLower Then
                            'Found setting
                            value = Trim(Right(s, Len(s) - s.IndexOf("=") - 1))
                            SettingFound = True
                            Exit For
                        End If
                    Next
                End If
            End If
        Next
        If Not SettingFound Then
            SetS(Group, name, DefaultValue)
            Return DefaultValue
        End If
        Return value
    End Function

    Public Sub SetS(ByVal Group As String, ByVal name As String, ByVal Value As String)
        Dim i As Integer
        Dim j As Integer
        Dim s As String = ""
        Dim s_new As String
        Dim fg As New System.Collections.Generic.List(Of String)
        Dim SettingFound As Boolean
        Dim GroupFound As Boolean
        s_new = name & "=" & Value
        For i = 0 To _FileGroups.Count - 1
            If _FileGroups(i).Title = Group Then
                GroupFound = True
                j = 0
                fg = _FileGroups(i).Items
                If fg.Count > 0 Then
                    For Each s In fg
                        If Left(s, s.IndexOf("=")).ToLower = name.ToLower Then
                            'Found setting
                            fg(j) = s_new
                            SettingFound = True
                            Exit For
                        End If
                        j += 1
                    Next
                End If
                If Not SettingFound Then
                    fg.Add(s_new)
                End If
                Exit For
            End If
        Next
        If Not GroupFound Then
            Dim fgnew As New xFileGroup
            fgnew = New xFileGroup
            fgnew.Title = Group
            fgnew.FilePos = "9" ' goes to the bottom of the file if not used
            fgnew.Items = New System.Collections.Generic.List(Of String)
            fgnew.Items.Add(s_new)
            _FileGroups.Add(fgnew)
        End If
        CommitChanges()
    End Sub

    Private Sub CommitChanges()
        Dim i As Integer
        Dim j As Integer
        Dim fg As New System.Collections.Generic.List(Of String)
        Dim sb As New System.Text.StringBuilder(10000)
        For i = 0 To _FileGroups.Count - 1
            fg = _FileGroups(i).Items
            If fg.Count > 0 Then
                sb.Append("[" & _FileGroups(i).Title & "]" & vbCrLf)
                For j = 0 To fg.Count - 1
                    sb.Append(fg(j) & vbCrLf)
                Next
                sb.Append(vbCrLf)
            End If
        Next
        Try
            My.Computer.FileSystem.WriteAllText(_FolderPath & "\" & _SettingsFileName & ".ini.new", sb.ToString, False, System.Text.Encoding.GetEncoding(0))
            If My.Computer.FileSystem.FileExists(_FolderPath & "\" & _SettingsFileName & ".ini") Then
                My.Computer.FileSystem.DeleteFile(_FolderPath & "\" & _SettingsFileName & ".ini")
            End If
            My.Computer.FileSystem.RenameFile(_FolderPath & "\" & _SettingsFileName & ".ini.new", _SettingsFileName & ".ini")
        Catch ex As Exception

        End Try
    End Sub
End Class

