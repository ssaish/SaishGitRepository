Public Class xUISettingsServer

    Dim _FileHeader As String
    Dim _Groups As New System.Collections.Generic.List(Of xUISettingsGroup)
    Dim _FileGroups As New System.Collections.Generic.List(Of xUIFileGroup)
    Dim _LastSavedSettings As String
    Dim _SettingsName As String

    Public Enum GroupCategory
        WindowsService
        Main
        Design
        Data
        Config
        Line
        Device
        Other
    End Enum

    Public Sub New(ByVal SettingsName As String)
        _SettingsName = SettingsName
        LoadSettings()
    End Sub

    Public Function GetGroup(ByVal GroupName As String, ByVal GroupCategory As GroupCategory) As xUISettingsGroup
        Dim ImportanceLevel As Integer = GroupCategory
        For Each g In _Groups
            If g.GroupName = GroupName Then
                Return g
            End If
        Next

        Dim FileGroup As xUIFileGroup = Nothing
        For Each fg In _FileGroups
            If fg.Title = GroupName Then
                FileGroup = fg
            End If
        Next
        If FileGroup Is Nothing Then
            FileGroup = New xUIFileGroup With {.Title = GroupName, .ImportanceLevel = ImportanceLevel}
            _FileGroups.Add(FileGroup)
        End If
        Dim NewGroup = New xUISettingsGroup(ImportanceLevel, FileGroup)
        _Groups.Add(NewGroup)

        _FileGroups.Sort(AddressOf CompareFileGroup)

        Return NewGroup
    End Function

    Private Shared Function CompareFileGroup(ByVal x As xUIFileGroup, ByVal y As xUIFileGroup) As Integer
        If x.ImportanceLevel >= y.ImportanceLevel Then Return 0 Else Return -1
    End Function

    Private Sub LoadSettings()
        Dim i As Integer
        Dim j As Integer
        Dim fg As xUIFileGroup = Nothing
        Dim dat As String
        Dim sa() As String

        ' open ini file
        ' if ini file not found this will automatically create new ini file
        Try
            If My.Computer.FileSystem.FileExists(My.Application.Info.DirectoryPath & "\" & _SettingsName & ".ini") Then
                dat = My.Computer.FileSystem.ReadAllText(My.Application.Info.DirectoryPath & "\" & _SettingsName & ".ini", System.Text.Encoding.GetEncoding(0))
            ElseIf My.Computer.FileSystem.FileExists(My.Application.Info.DirectoryPath & "\" & _SettingsName & ".ini.new") Then
                dat = My.Computer.FileSystem.ReadAllText(My.Application.Info.DirectoryPath & "\" & _SettingsName & ".ini.new", System.Text.Encoding.GetEncoding(0))
            Else ' create
                dat = ""
            End If

        Catch ex As Exception
            dat = ""
        End Try
        _LastSavedSettings = dat

        dat = dat.Replace(Chr(10), "")

        If dat = "" Then Exit Sub

        _FileGroups.Clear()
        sa = dat.Split(CChar(vbCrLf))

        ' load the groups
        _FileHeader = ""
        For Each txt As String In sa
            i = InStr(1, txt, "[")
            j = InStr(1, txt, "]")
            If i = 1 And j > 0 And j > i Then
                fg = New xUIFileGroup
                fg.Title = Mid(txt, i + 1, j - i - 1)
                Dim import As Integer = 9
                Integer.TryParse(Mid(txt, j + 1), import)
                If i < 0 Or i > 9 Then i = 9
                fg.ImportanceLevel = import
                fg.Items = New List(Of xUISettingsServer.xUIFileItem)
                _FileGroups.Add(fg)
            Else
                If fg Is Nothing Then ' allow for comment
                    _FileHeader += txt & vbCrLf
                ElseIf txt.Length = 0 Then ' allow for comment
                    ' don't add blank lines
                ElseIf txt.Substring(0, 1) = ";" Or txt.Substring(0, 1) = "'" Then ' allow for comment
                    fg.Items.Add(New xUIFileItem With {.Filler = True, .Value = txt})
                ElseIf txt.Contains("=") Then
                    i = txt.IndexOf("=")
                    fg.Items.Add(New xUIFileItem With {.Name = txt.Substring(0, i), .Value = txt.Substring(i + 1)})
                Else
                    fg.Items.Add(New xUIFileItem With {.Filler = True, .Value = txt})
                End If
            End If
        Next

        _FileGroups.Sort(AddressOf CompareFileGroup)

    End Sub

    Public Sub CommitChanges()
        Dim sb As New System.Text.StringBuilder(10000)

        sb.Append(_FileHeader)
        For Each fg In _FileGroups
            sb.Append("[" & fg.Title & "] " & CStr(fg.ImportanceLevel) & vbCrLf)
            For Each fi In fg.Items
                If fi.Filler Then
                    sb.Append(fi.Value & vbCrLf)
                Else
                    sb.Append(fi.Name & "=" & fi.Value & vbCrLf)
                End If
            Next
            sb.Append(vbCrLf)
        Next

        Dim NewSaved = sb.ToString
        If NewSaved = _LastSavedSettings Then Exit Sub
        _LastSavedSettings = NewSaved

        Try
            My.Computer.FileSystem.WriteAllText(My.Application.Info.DirectoryPath & "\" & _SettingsName & ".ini.new", sb.ToString, False, System.Text.Encoding.GetEncoding(0))
            If My.Computer.FileSystem.FileExists(My.Application.Info.DirectoryPath & "\" & _SettingsName & ".ini") Then
                My.Computer.FileSystem.DeleteFile(My.Application.Info.DirectoryPath & "\" & _SettingsName & ".ini")
            End If
            My.Computer.FileSystem.RenameFile(My.Application.Info.DirectoryPath & "\" & _SettingsName & ".ini.new", _SettingsName & ".ini")
        Catch ex As Exception

        End Try

    End Sub

    Public Class xUIFileGroup
        Public Title As String
        Public ImportanceLevel As Integer
        Public Items As New List(Of xUIFileItem)
    End Class

    Public Class xUIFileItem
        Public Name As String
        Public Value As String
        Public Filler As Boolean
    End Class
End Class

Public Class xUISettingsGroup
    Dim _Items As New System.Collections.Generic.List(Of xUISettingData)
    Dim _FileGroup As xUISettingsServer.xUIFileGroup
    Dim _GroupImportanceLevel As Integer ' 0= lowest importance, 1=medium importance, 2=most important (more important groups are listed first in the ini file - makes it more readable over time)

    Public Sub New(ByVal ImportanceLevel As Integer, ByVal FileGroup As xUISettingsServer.xUIFileGroup)
        _GroupImportanceLevel = ImportanceLevel
        _FileGroup = FileGroup
        _FileGroup.ImportanceLevel = ImportanceLevel ' set this now
    End Sub

    Public Function [Get](ByVal SettingName As String, ByVal DefaultValue As Size) As xUISettingData
        Return [Get](SettingName, CStr(DefaultValue.Width) & "," & CStr(DefaultValue.Height))
    End Function

    Public Function [Get](ByVal SettingName As String, ByVal DefaultValue As Integer) As xUISettingData
        Return [Get](SettingName, CStr(DefaultValue))
    End Function

    ''' <summary>
    ''' Gets of Creates a setting with a default value
    ''' </summary>
    ''' <param name="SettingName"></param>
    ''' <param name="DefaultValue">The Value of the setting is set to the default value when the setting did not exist in the setting file.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function [Get](ByVal SettingName As String, ByVal DefaultValue As String) As xUISettingData

        ' check not already exist
        For Each s As xUISettingData In _Items
            If s.Name = SettingName Then ' already exists so do not duplicate
                Return s
            End If
        Next

        ' find/create file entry
        Dim FileItem As xUISettingsServer.xUIFileItem = Nothing
        For Each fs In _FileGroup.Items
            If Not fs.Filler AndAlso fs.Name = SettingName Then
                FileItem = fs
            End If
        Next
        If FileItem Is Nothing Then
            FileItem = New xUISettingsServer.xUIFileItem With {.Name = SettingName, .Value = DefaultValue}
            _FileGroup.Items.Add(FileItem)
        End If
        Dim v As New xUISettingData(FileItem, DefaultValue)
        _Items.Add(v)

        Return v
    End Function


    ''' <summary>
    ''' Gets and existing setting.  Tyically used to get the setting in order to set the Value property of the setting.
    ''' </summary>
    ''' <param name="SettingName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function [Get](ByVal SettingName As String) As xUISettingData

        ' check not already exist
        For Each s As xUISettingData In _Items
            If s.Name = SettingName Then ' already exists so do not duplicate
                Return s
            End If
        Next

        Return Nothing ' do not know the default so can not create - UI needs to create setting using the default first
    End Function

    Public ReadOnly Property GroupName As String
        Get
            Return _FileGroup.Title
        End Get
    End Property
End Class

Public Class xUISettingData
    Dim _DefaultValue As String
    Dim _FileItem As xUISettingsServer.xUIFileItem

    Public Sub New(ByVal FileItem As xUISettingsServer.xUIFileItem, ByVal DefaultValue As String)
        _FileItem = FileItem
        _DefaultValue = DefaultValue
    End Sub

    Public Property Value() As String
        Get
            Return _FileItem.Value
        End Get
        Set(ByVal value As String)
            _FileItem.Value = value
        End Set
    End Property

    Public ReadOnly Property Name As String
        Get
            Return _FileItem.Name
        End Get
    End Property

    Public Property ValueAsInt As Integer
        Get
            Dim i As Integer
            If Integer.TryParse(_FileItem.Value, i) Then
                Return i
            Else
                Integer.TryParse(_DefaultValue, i)
                Return i
            End If
        End Get
        Set(ByVal value As Integer)
            _FileItem.Value = CStr(value)
        End Set
    End Property

    Public Property ValueAsSize As Size
        Get
            Dim x As Integer
            Dim y As Integer
            Dim ar = _FileItem.Value.Split(","c)
            If ar.Length <> 2 Then
                ar = _DefaultValue.Split(","c)
            End If
            Integer.TryParse(ar(0), x)
            Integer.TryParse(ar(1), y)
            Return New Size(x, y)
        End Get
        Set(ByVal value As Size)
            _FileItem.Value = CStr(value.Width) & "," & CStr(value.Height)
        End Set
    End Property
End Class

