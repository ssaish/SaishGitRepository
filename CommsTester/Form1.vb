Imports CommsTester.xCore

Public Class Form1

    Dim _Comm As xComm
    Dim _Area As xAreaBase
    Dim _Device As xDevice
    Dim _UnloadList As New xPublisher
    Dim _LogHighCmdFormat As Integer
    Dim _LogAllCmdFormat As Integer
    Dim _SuppressEvents As Boolean
    Dim _SettingsSrv As xUISettingsServer
    Dim _Settings As xUISettingsGroup
    Dim _LogServer As New xLogServer
    Dim _RX As String

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        _Area = New xAreaBase With {.Settings = New xSettingsGroup, .LogServerObj = _LogServer}
        _Device = New xDevice
        _Comm = New xComm(_Area, _Device, AddressOf StatusUpdated, _UnloadList, _LogHighCmdFormat, _LogAllCmdFormat)
        _LogServer.List(0).Detail = xLogDetail.High
        _LogServer.List(1).Detail = xLogDetail.High
        _SettingsSrv = New xUISettingsServer("CommsTester")
        _Settings = _SettingsSrv.GetGroup("Main", xUISettingsServer.GroupCategory.Main)
        txtRS232Settings.Text = _Settings.Get("Settings", txtRS232Settings.Text).Value
        txtAddress.Text = _Settings.Get("Port", txtAddress.Text).Value
        cmbRuleTX.SelectedIndex = 1
        cmbRuleRX.SelectedIndex = 1
        cmbDisplayType.SelectedIndex = 0

        If IO.File.Exists(Application.StartupPath & "\SentItems.txt") Then
            For Each s In IO.File.ReadAllLines(Application.StartupPath & "\SentItems.txt")
                s = s.Trim
                If s.Length = 0 Then Continue For
                lstSentItems.Items.Insert(0, s)
            Next
        End If
    End Sub

    Private Sub StatusUpdated(ByVal Status As xCommStatus)
        If Me.InvokeRequired Then
            Me.Invoke(New xEventProcCommStatus(AddressOf StatusUpdated), New Object() {Status}) ' marshal to UI thread
            Exit Sub
        End If
        lblStatus.Text = _Comm.StatusMessage
        If Status <> xCommStatus.Closed Then
            btnOpen.Text = "&Close"
            Text = "CommsTester - " & txtAddress.Text
        Else
            btnOpen.Text = "&Open"
            Text = "CommsTester"
        End If
        UpdateLogText()
    End Sub

    Private Sub btnOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpen.Click
        If _Comm.Status <> xCommStatus.Closed Then
            _Comm.Close()
        Else
            _Comm.RS232PortSettingsDefault = txtRS232Settings.Text
            _Comm.Connect(txtAddress.Text)
            _Settings.Get("Settings").Value = txtRS232Settings.Text
            _Settings.Get("Port").Value = txtAddress.Text
            _SettingsSrv.CommitChanges()
        End If
    End Sub

    Private Sub ReceivedData(ByVal Data As String)
        If Me.InvokeRequired Then
            Me.Invoke(New xEventProcStr(AddressOf ReceivedData), New Object() {Data}) ' marshal to UI thread
            Exit Sub
        Else
            _Comm.Send("PacketAck", "", 10)
        End If

        LogPckt("RX", Data)
        UpdateLogText()
    End Sub

    Private Sub UpdateLogText()
        If chkShowLog.Checked Then
            txtData.Text = _LogServer.LogData
        Else
            txtData.Text = _RX
        End If
        txtData.SelectionStart = txtData.Text.Length
        txtData.ScrollToCaret()
    End Sub

    Private Sub cmbRule_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbRuleTX.SelectedIndexChanged
        Select Case cmbRuleTX.SelectedIndex
            Case 0
                _Comm.TXRule = New xCommPacketNone(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
            Case 1
                _Comm.TXRule = New xCommPacketCR(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
            Case 2
                _Comm.TXRule = New xCommPacketLF(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
            Case 3
                _Comm.TXRule = New xCommPacketESC(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
            Case 4
                _Comm.TXRule = New xCommPacketESC1(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
            Case 5
                _Comm.TXRule = New xCommPacketSTXETX(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
            Case 6
                _Comm.TXRule = New xCommPacketESCETB(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
            Case Else
                _Comm.TXRule = New xCommPacketNone(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
        End Select
        _Comm.TXRule.SendCommandType = 10
    End Sub

    Private Sub cmbRuleRX_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbRuleRX.SelectedIndexChanged
        Select Case cmbRuleRX.SelectedIndex
            Case 0
                _Comm.RXRule = New xCommPacketNone(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
            Case 1
                _Comm.RXRule = New xCommPacketCR(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
            Case 2
                _Comm.RXRule = New xCommPacketLF(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
            Case 3
                _Comm.RXRule = New xCommPacketESC(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
            Case 4
                _Comm.RXRule = New xCommPacketESC1(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
            Case 5
                _Comm.RXRule = New xCommPacketSTXETX(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
            Case 6
                _Comm.RXRule = New xCommPacketESCETB(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
            Case Else
                _Comm.RXRule = New xCommPacketNone(AddressOf ReceivedData, _LogHighCmdFormat, _LogAllCmdFormat)
        End Select
        _Comm.RXRule.SendCommandType = 10

    End Sub

    Private Sub btnSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSend.Click
        Dim Data As String = ConvCtrlToBin(txtSend.Text)

        LogPckt("TX", Data)
        _Comm.Send(Data, "", 10)

        UpdateLogText

        lstSentItems.Items.Remove(txtSend.Text)
        lstSentItems.Items.Insert(0, txtSend.Text)
        Do While lstSentItems.Items.Count > 100
            lstSentItems.Items.RemoveAt(100)
        Loop
        Dim sb As String = ""
        For Each s In lstSentItems.Items
            sb = CStr(s) & vbCrLf & sb
        Next
        IO.File.WriteAllText(Application.StartupPath & "\SentItems.txt", sb)
    End Sub

    Private Sub LogPckt(Heading As String, text As String)
        Select Case cmbDisplayType.SelectedIndex
            Case 0
                _RX += Now.ToString("hh:MM:ss.fff") & " " & Heading & ": " & text & vbCrLf
            Case 1
                _RX += Now.ToString("hh:MM:ss.fff") & " " & Heading & ": " & ConvBinToCtrl(text) & vbCrLf
            Case 2
                _RX += Now.ToString("hh:MM:ss.fff") & " " & Heading & ": " & ConvBinToHex(text) & vbCrLf
            Case 3
                _RX += Now.ToString("hh:MM:ss.fff") & " " & Heading & ": " & ConvBinToHexAsc(text) & vbCrLf
        End Select
    End Sub

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        _UnloadList.Publish()
    End Sub

    Private Sub txtAddress_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtAddress.KeyPress
        If e.KeyChar = Chr(13) Then
            e.Handled = True
            btnOpen_Click(Nothing, Nothing)
        End If
    End Sub

    Private Sub txtSend_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtSend.KeyPress
        If e.KeyChar = Chr(13) Then
            e.Handled = True
            btnSend_Click(Nothing, Nothing)
        End If
    End Sub

    Private Sub txtData_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtData.KeyPress
        If e.KeyChar = Chr(27) Then
            _LogServer.LogData = ""
            txtData.Text = ""
            _RX = ""
        End If
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        If _Comm.PortOpen And txtData.Text <> _LogServer.LogData Then
            Dim NewText As String
            If chkShowLog.Checked Then
                NewText = _LogServer.LogData
            Else
                NewText = _RX
            End If
            If NewText <> txtData.Text Then
                NewText = txtData.Text
                txtData.SelectionStart = txtData.Text.Length
                txtData.ScrollToCaret()
            End If
        End If
    End Sub

    Private Sub chkRaw_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkRaw.CheckedChanged
        If _LogServer.List.Count <= 1 Then Exit Sub
        If chkRaw.Checked Then
            _LogServer.List(1).Detail = xLogDetail.High
        Else
            _LogServer.List(1).Detail = xLogDetail.Low
        End If
    End Sub

    Private Sub chkCom_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCom.CheckedChanged
        If _LogServer.List.Count <= 1 Then Exit Sub
        If chkCom.Checked Then
            _LogServer.List(0).Detail = xLogDetail.High
        Else
            _LogServer.List(0).Detail = xLogDetail.Low
        End If
    End Sub

    Private Sub lstSentItems_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstSentItems.Click
        txtSend.Text = lstSentItems.Text
    End Sub

    Private Sub lstSentItems_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstSentItems.DoubleClick
        txtSend.Text = lstSentItems.Text
        btnSend_Click(Nothing, Nothing)
    End Sub

    Private Sub chkShowLog_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkShowLog.CheckedChanged
        UpdateLogText()
    End Sub

    Private Sub btnSendFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSendFile.Click
        If _SendFile.Length = 0 Then MsgBox("Select a file first") : Exit Sub
        Dim s = IO.File.ReadAllLines(_SendFile)
        For Each Lin In s
            If chkSendCTRL.Checked Then
                Lin = ConvCtrlToBin(Lin)
            End If
            LogPckt("TX", Lin)
            _Comm.Send(Lin, "", 10)
        Next
        UpdateLogText()
    End Sub

    Private Sub cmbDisplayType_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbDisplayType.SelectedIndexChanged

    End Sub

    Dim _SendFile As String = ""
    Private Sub btnChooseFile_Click(sender As System.Object, e As System.EventArgs) Handles btnChooseFile.Click
        Dim sfd As New System.Windows.Forms.OpenFileDialog
        sfd.Title = "Send File"
        sfd.FilterIndex = 0
        Dim sdr As System.Windows.Forms.DialogResult = sfd.ShowDialog()
        If sdr = Windows.Forms.DialogResult.OK Then
            _SendFile = sfd.FileName
            lblFile.Text = _SendFile
        End If
    End Sub

    Private Sub txtData_TextChanged(sender As System.Object, e As System.EventArgs) Handles txtData.TextChanged

    End Sub
End Class


Public Interface xILogItemLite
    Sub Prn(ByRef txt As String)
End Interface

Public Interface xILogItem
    Inherits xILogItemLite
    Property Detail As xLogDetail
End Interface

Public Class xLogItem
    Implements xILogItem
    Dim _Detail As xLogDetail
    Dim _Name As String
    Dim _LogServer As xLogServer

    Public Sub New(ByRef Name As String, ByRef IDName As String, ByVal ParentID As String, ByVal UnloadList As xPublisher, ByVal LogServer As xLogServer, ByVal SubItem As Boolean)
        _Name = Name
        _LogServer = LogServer
        _LogServer.List.Add(Me)
    End Sub

    Public Sub prn(ByRef txt As String) Implements xILogItemLite.Prn
        _LogServer.LogData += Format(Now, "hh:mm:ss.fff") & ": " & _Name & ": " & txt & vbCrLf
    End Sub

    Public Property Detail As xLogDetail Implements xILogItem.Detail
        Get
            Return _Detail
        End Get
        Set(ByVal value As xLogDetail)
            _Detail = value
        End Set
    End Property
End Class

Public Class xLogServer
    Public LogData As String = ""
    Public List As New List(Of xLogItem)
End Class

Public Class xPublisher ' not intended to be thread safe - intended to be used within the same thread to pass events
    Private mList As New List(Of xEventProc)

    Public Sub Subscribe(ByVal EvProc As xEventProc)
        mList.Add(EvProc)
    End Sub

    Public Sub UnSubscribe(ByVal EvProc As xEventProc)
        mList.Remove(EvProc)
    End Sub

    Public Sub Publish()
        For Each proc As xEventProc In mList
            proc.Invoke()
        Next
    End Sub

    Public Sub Clear()
        mList.Clear()
    End Sub
End Class

Public Class xAreaBase
    Public Sub AddEvent(ByVal EventProc As xEventProcArg, ByVal EventArg As xEventArg)
        EventProc.Invoke(EventArg)
    End Sub

    Public AreaID As String
    Public VisibleAreaName As String
    Public Settings As xSettingsGroup
    Public LogServerObj As xLogServer
End Class

Public Class xDevice
    Public Area As xAreaBase
    Public ID As String
    Public VisibleFullID As String
End Class

Public Class xSettingsGroup
    Dim Data As New xSettingData
    Public Function Add(ByVal SettingName As String, ByVal DefaultValue As String, ByVal HelpText As String, ByVal RightsTag As xRightsTag, Optional ByVal DataType As xSettingDataType = xSettingDataType.StringValue, Optional ByVal OptionList() As String = Nothing) As xSettingData
        Return Data
    End Function

    Public Sub SetS(ByVal name As String, ByVal NewValue As String)
        Data.Value = NewValue
    End Sub
End Class

Public Class xSettingData
    Public Value As String = ""
End Class

Public Delegate Sub xEventProcArg(ByVal Arg As xEventArg)
Public Delegate Sub xEventProcStr(ByVal Data As String)
Public Delegate Sub xEventProcCommStatus(ByVal Status As xCommStatus)
Public Delegate Sub xEventProc()

Public Class xEventArg
End Class

Public Class xEventArgString
    Inherits xEventArg
    Public Str As String

    Public Sub New(ByVal Value As String)
        Str = Value
    End Sub
End Class

Public Enum xSettingDataType
    StringValue
    StringLength
    Int32
    Int32Range
    Bool
    Number
    NumberRange
    IndexList ' eg "Low|Medium|High  ' stores index 0 through 2 - displays Low, Medium, or High  - used when simple list great and options will not normally add or remove
    IDList ' eg "0;Low|1;Medium|4;High ' stores id 0, 1 or 4 - displays Low, Medium, or High - used when simple list great and options will normally add or remove
    TextList ' Low|Medium|High  ' stores Low, Medium, or High - displays Low, Medium, or High - used when no index available and options will normally add or remove
    EditList ' Low|Medium|High  ' stores Low, Medium, or High - displays Low, Medium, or High AND can type in own values - used when can enter values
    FormatResources ' defines all the resources (name, Field name, and query)
    Query ' a database query
End Enum

Public Enum xCommStatus
    Closed
    Connecting
    Listening
    Connected
    Closing
    ErrorState
    ClosedByPeer
    AlreadyOpenOtherApp
    AlreadyOpenOtherDev
    InvalidPort
    AccessDenied
    Disabled
End Enum

Public Enum xLogDetail
    All = 3 ' all details which will help in tracking what happened to find out why a desired outcome did not happen. 
    High = 2 ' all details which will help in tracking what happened to find out why a desired outcome did not happen.  Typically includes all events and data.
    Medium = 1 ' basic information on normal operation and errors.  Typically this includes state changes and error events
    Low = 0 ' Only critical errors, errors which are not expected and must be noted
End Enum

Public Enum xRightsTag
    CommPortFor
    DeviceConnectionStr
End Enum

Public Module xGeneral
    Public gCIMAware As Boolean = True
    Public Function ConvBinToHex(ByVal txt As String) As String
        ' shows a message box with all the characters in the string shown in hex
        Dim h As String
        Dim hf As String
        Dim i As Integer
        Dim t As String
        Dim p As Integer
        p = 0
        h = ""
        t = ""
        hf = ""
        For i = 0 To txt.Length - 1
            ' add hex value
            h = Hex(Asc(txt.Substring(i, 1))).PadLeft(2, "0"c)
            ' add space or hyphen
            p = p + 1
            If p = 8 Then
                hf = hf & h & "-"
                p = 0
            Else
                hf = hf & h & " "
            End If
        Next
        ConvBinToHex = hf
    End Function

    Public Function ConvBinToHexAsc(ByVal txt As String, Optional ByVal BytePerLine As Integer = 32) As String
        ' shows a message box with all the characters in the string shown in hex
        Dim h As String
        Dim hf As String = ""
        Dim i As Integer
        Dim t As String
        Dim p As Integer
        Dim c As Integer
        Dim a As String = ""
        Dim hl As String = ""
        p = 0
        h = ""
        t = ""
        hf = ""
        For i = 0 To txt.Length - 1
            ' add hex value
            c = Asc(txt.Substring(i, 1))
            h = Hex(c).PadLeft(2, "0"c)
            If c < 32 OrElse c > 126 Then
                c = 46 ' dot
            End If
            a += Chr(c)
            ' add space or hyphen
            p += 1
            If (p And 7) = 0 AndAlso p < BytePerLine Then
                hl &= h & "-"
            Else
                hl &= h & " "
            End If
            If p >= BytePerLine Then
                p = 0
                hf += hl & " " & a & vbCrLf
                a = ""
                hl = ""
            End If
        Next
        If txt.Length > BytePerLine Then hl = hl.PadRight(BytePerLine * 3)
        ConvBinToHexAsc = hf & hl & " " & a
    End Function

    Public Function ConvBinToInt16(ByVal str As String) As Integer
        ' converts a four byte binary integer as a string to an long integer
        Dim i As Integer
        If str.Length < 2 Then
            ConvBinToInt16 = 0
            Exit Function
        End If
        i = Asc(Mid$(str, 2))
        If i < 128 Then
            i = (i << 8) + Asc(Mid$(str, 1))
        Else
            i = i And &H7F
            i = (i << 8) + CInt(Asc(Mid$(str, 1)))
            i = 0 - i
            If i = 0 Then i = -32768
        End If
        ConvBinToInt16 = i
    End Function

    Public Function ConvBinToUInt16(ByVal str As String) As Integer
        ' converts a four byte binary integer as a string to an long integer
        Dim i As Integer
        If str.Length < 2 Then
            ConvBinToUInt16 = 0
            Exit Function
        End If
        i = Asc(Mid$(str, 2))
        i = (i << 8) + Asc(Mid$(str, 1))
        ConvBinToUInt16 = i
    End Function

    Public Function ConvInt16ToBin(ByVal l As Integer) As String
        ' converts an integer number into a 2 byte string
        Dim txt As String
        If l >= 0 Then
            txt = Chr(l And &HFF)
            txt = txt + Chr((l And &HFF00) \ &H100)
        Else
            txt = Chr(l And &HFF)
            txt = txt + Chr((l And &H7F00) \ &H100 + &H80)
        End If
        ConvInt16ToBin = txt
    End Function

    Public Function ConvBinToCtrl(ByVal txt As String) As String
        Dim i As Integer
        Dim t As String = ""
        Dim c As Integer
        For i = 0 To Len(txt) - 1
            c = Asc(txt.Substring(i, 1))
            'If c = 10 Or c = 13 Then
            '    t = t + Chr$(c)
            If c < 27 Then
                t = t + "^" + Chr((c + 64))
            ElseIf c < 32 Then
                t = t + "^" + Format(c, "000")
            ElseIf c = 94 Then ' ^
                t = t + "^094"
            ElseIf c >= 127 Then
                t = t + "^" + Format(c, "000")
            Else
                t = t + Chr(c)
            End If
        Next
        Return t

    End Function

    Public Function ConvCtrlToBin(ByVal intxt As String) As String
        Dim i As Integer
        Dim txt As String
        Dim c As Integer

        ' convert ascii control (with ^) to binary control chars
        txt = intxt
        Do While InStr(txt, "^") > 0
            i = InStr(txt, "^")
            c = Asc(UCase(Mid(txt, i + 1, 1)))
            If c >= 64 And c <= 95 Then
                txt = Left$(txt, i - 1) + Chr(c - Asc("A") + 1) + Right$(txt, Len(txt) - i - 1)
            ElseIf c >= 48 And c <= 57 Then
                txt = Left$(txt, i - 1) + Chr(CInt(Val(Mid(txt, i + 1, 3)))) + Mid$(txt, i + 4)
            End If
        Loop
        ConvCtrlToBin = txt

    End Function

    Public Function ConvBinToHex(ByVal ba() As Byte) As String
        ' shows a message box with all the characters in the string shown in hex
        Dim h As String
        Dim hf As String
        Dim i As Integer
        Dim t As String
        Dim p As Integer
        p = 0
        h = ""
        t = ""
        hf = ""
        For i = 0 To ba.Length - 1
            ' add hex value
            h = Hex(ba(i)).PadLeft(2, "0"c)
            ' add space or hyphen
            p = p + 1
            If p = 8 Then
                hf = hf & h & "-"
                p = 0
            Else
                hf = hf & h & " "
            End If
        Next
        ConvBinToHex = hf
    End Function

    Public Function CommsPortInfo(ByVal Address As String, ByVal Setting As String, ByVal CommsSettingsUserEditable As Boolean, ByVal DefaultRS232Settings As String) As xPortInfo
        Dim pi As xPortInfo
        If (Len(Address) <= 3 And Val(Address) > 0) Or Left(Address, 3) = "COM" Then ' RS232
            pi.Type = xCommPortType.RS232
            pi.Port = -1
            If Left(Address, 3) = "COM" Then
                Try
                    pi.Port = CShort(Mid(Address, 4))
                Catch ex As Exception

                End Try
            Else
                Try
                    pi.Port = CShort(Address)
                Catch ex As Exception

                End Try
            End If
            pi.Name = ""
            pi.IP = ""
        ElseIf InStr(Address, ":") > 0 Then ' TCP/IP ' future considerations for IPv6: '.' then ':' will identify IPv4, more than one ':' before '.' will identify IPv6
            pi.Type = xCommPortType.TCPIP
            pi.Name = ""
            pi.IP = ""
            pi.Port = 0
            Try
                Dim sa() As String = Address.Split(":"c)
                pi.IP = sa(0)
                pi.Port = CInt(sa(1))
                If sa.Count = 3 AndAlso sa(2).ToLower = "l" Then
                    pi.IPListener = True
                End If
            Catch ex As Exception ' ignore any problems and use existing
            End Try
        Else
            pi.Type = xCommPortType.None
            pi.Name = ""
            pi.IP = ""
        End If

        If CommsSettingsUserEditable Then
            Select Case pi.Type
                Case xCommPortType.RS232
                    pi.RS232PortSetting = Setting
                Case xCommPortType.TCPIP
                    If pi.Port >= 1288 And pi.Port <= 1310 Then
                        pi.RS232PortSetting = ""
                    Else
                        pi.RS232PortSetting = ""
                    End If
                Case Else
                    pi.RS232PortSetting = DefaultRS232Settings
            End Select
        Else
            pi.RS232PortSetting = DefaultRS232Settings
        End If

        ' control port only if RS232 settings defined (blank is undefined), and using TCP, and is a RS232 port on CIM, and system is CIM aware
        If pi.RS232PortSetting.Length > 0 And pi.Type = xCommPortType.TCPIP And (pi.Port And 255) < 32 And (pi.Port And 255) >= 8 And pi.Port > 1288 And pi.Port <= 1320 And gCIMAware And Not pi.IPListener Then
            pi.CtrlReq = True
        Else
            pi.CtrlReq = False
        End If
        Return pi
    End Function
End Module
