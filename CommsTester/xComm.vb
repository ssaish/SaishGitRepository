Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.String
Imports System.Text

Public Structure xPortInfo
    Public Type As xCommPortType
    Public Port As Integer
    Public Name As String ' unresolved name (CIM name)
    Public IP As String
    Public RS232PortSetting As String
    Public CtrlReq As Boolean
    Public IPListener As Boolean
End Structure

Public Enum xCommPortType
    None = 0
    RS232 = 1
    TCPIP = 2
End Enum

Namespace xCore

    Public Class xComm
        Dim _Area As xAreaBase
        Dim _Device As xDevice
        Dim _CommID As String ' <DeviceID>
        Dim _AddressSetting As String ' address and disabled flag
        Dim _PortInfo As xPortInfo
        Dim _Address As String ' address tcp=x.x.x.x:p and rs232=p
        'Dim _Port As UInteger ' port number
        'Dim _IP As String ' ip address x.x.x.x (or ipname)
        Dim _PortType As xCommPortType
        Dim _CtrlReq As Boolean ' TCP control port is required for this connection
        Dim _CtrlStatusReqIter As Integer ' iterations of control port status request
        Dim _Disabled As Boolean ' the port is disabled
        Dim _OutOfService As Boolean ' Out Of Service - used only to write it as part of the comm port settings - has no effect on the operation of the comm port - used by the device to set the out of service value
        Dim _Simul As Boolean ' the selected port address is the second one (which is a simulation port)
        Dim _Message As String ' the message for this communications port
        Dim _Status As xCommStatus ' the status for this communications port
        Dim _RXBuffer As String = "" ' receive buffer where buffering of input is required
        Dim _RXRule As xCommPacket
        Dim _TXRule As xCommPacket
        Dim _Settings As xSettingsGroup
        Dim _PortOpen As Boolean
        Dim _Callback As AsyncCallback
        Dim _DataReceivedProc As xEventProcStr
        Dim _StatusChangedProc As xEventProcCommStatus
        Dim _ConnStateMessage() As String = {"Closed", "Connecting", "Connected", "Closing", "Error", "Disabled"}
        Dim _UnloadList As xPublisher
        Dim _RS232PortSettings As String = "9600,n,8,1"
        Dim _DefaultRS232Settings As String = "9600,n,8,1"
        Dim _CommsSettingsUserEditable As Boolean

        Dim _CtrlRXBuffer As String = "" ' receive buffer where buffering of input is required
        Dim _CtrlESCMode As Boolean
        Dim _CtrlESCRX As Boolean
        Shared _CIMVersionRequest() As Byte = {27, 2, 1, 27, 3}
        Shared _CIMBufUsageRequest() As Byte = {27, 2, 6, 27, 3}
        Shared _CIMTXBufSizeReqAndPortStatus() As Byte = {27, 2, 7, 27, 3, 27, 2, 4, 27, 3} ' request the TXBufferSize and the port status
        Dim _CIMSetThreshold() As Byte = {27, 2, 8, 0, 0, 27, 3} ' set the threshold value to CIMTXBufferSize-MinTXSize so CIM will send notification to PC when MinTXSize bytes free in TX buffer

        Dim _PortData As xPort
        Dim _PortDataParams As xParameterList
        Dim _PortDataState As xPort.State
        Dim _PortDataMessage As String = ""

        Dim _PortListener As xPortTCPServer
        Dim _PortListenerState As xPort.State
        Dim _PortListenerMessage As String = ""

        Dim _PortCtrl As xPort
        Dim _PortCtrlParams As xParameterList
        Dim _PortCtrlState As xPort.State
        Dim _PortCtrlMessage As String = ""

        Dim _CommErrorMsg As String
        Dim _DataRxd As Boolean

        Dim _TCPCtrlSetup As Boolean
        Dim _TCPCtrlErrorMsg As String
        Dim _TCPCtrlReconnectDataOnClose As Boolean
        Dim _CIMTXBufferCount As Integer
        Dim _CIMTCPTransitCount As Integer
        Dim _CIMTXBufferSize As Integer = 1500
        Dim _MinTXSize As Integer = 100
        Dim _TXQueue As String = ""
        Dim _CIMHandshaking As Byte
        Dim _CIMControlLines As Byte = 3

        Dim _LogHighCmdFormat As Integer '0=hex 1=ctrl
        Dim _LogAllCmdFormat As Integer '0=hex 1=ctrl

        Dim _Log As xILogItem
        Dim _LogLocal As xILogItem
        Dim _LogRaw As xLogItem
        Dim _LogCtrl As xLogItem

        Shared _OpenedPorts As New xSafeDictionary(Of String, xDevice)

        Public Sub New(ByRef Area As xAreaBase, ByVal Device As xDevice, ByVal StatusChangedProc As xEventProcCommStatus, ByVal UnloadList As xPublisher, ByVal LogHighCmdFormat As Integer, ByVal LogAllCmdFormat As Integer)
            _Area = Area
            _Device = Device
            _Settings = CType(_Area.Settings, xSettingsGroup)
            _CommID = Device.ID
            _Log = New xLogItem("Com", _Area.AreaID & "-" & _CommID + "-Com", _Area.AreaID & "-" & _CommID, UnloadList, Area.LogServerObj, True)
            _LogLocal = _Log
            _LogRaw = New xLogItem("Raw", _Area.AreaID & "-" & _CommID + "-Raw", _Area.AreaID & "-" & _CommID, UnloadList, Area.LogServerObj, True)
            _StatusChangedProc = StatusChangedProc
            _UnloadList = UnloadList
            _LogHighCmdFormat = LogHighCmdFormat
            _LogAllCmdFormat = LogAllCmdFormat

            _AddressSetting = _Settings.Add(_CommID & "-CommPortFor", "0,127.0.0.1:1288,1,", "Connection string for " & _CommID, xRightsTag.DeviceConnectionStr, xSettingDataType.StringValue).Value
            Dim a() As String = (_AddressSetting & ",,,,,").Split(","c)
            If a(3) <> "s" Then
                _Address = a(0)
                _Simul = False
            Else
                _Address = a(1)
                _Simul = True
            End If
            If Val(a(2)) = 1 Then
                _Disabled = True
                _Status = xCommStatus.Disabled
            End If
            If Val(a(5)) = 1 Then
                _OutOfService = True
            End If
            _RS232PortSettings = a(4).Replace("#", ",")

            _PortDataParams = New xParameterList("", ","c)
            ApplyRS232Settings(_RS232PortSettings)
        End Sub

        Public Sub SetLogger(ByVal Log As xILogItem)
            If Log Is Nothing Then
                _Log = _LogLocal
            Else
                _Log = Log
            End If
            _RXRule.InitByComm(_Log, AddressOf SendW)
            _TXRule.InitByComm(_Log, AddressOf SendW)
        End Sub

        Public Property CommsSettingsUserEditable() As Boolean
            Get
                Return _CommsSettingsUserEditable
            End Get
            Set(ByVal value As Boolean)
                _CommsSettingsUserEditable = value
            End Set
        End Property

        Public Property RS232PortSettingsDefault() As String
            Get
                Return _DefaultRS232Settings
            End Get
            Set(ByVal value As String)
                _DefaultRS232Settings = value
                If Not _CommsSettingsUserEditable Then ' if not editable then the default settings are automatically applied
                    RS232PortSettings = value
                End If
            End Set
        End Property

        Public Property RS232PortSettings() As String
            Get
                Return _RS232PortSettings
            End Get
            Set(ByVal value As String)
                If (_RS232PortSettings <> value) Then
                    If _PortType = xCommPortType.RS232 Then
                        _RS232PortSettings = value
                        ApplyRS232Settings(value)
                    ElseIf _PortType = xCommPortType.TCPIP Then
                        Dim PortInfo As xPortInfo
                        PortInfo = CommsPortInfo(_Address, value, _CommsSettingsUserEditable, _DefaultRS232Settings)
                        If PortInfo.CtrlReq <> _CtrlReq Then
                            _RS232PortSettings = value
                            Close()
                            Connect()
                        ElseIf _CtrlReq Then
                            ' apply the setting
                            _RS232PortSettings = value
                            If _PortCtrlState = xPort.State.Connected Then
                                SendCtrlRS232Config(value)
                            End If
                        Else
                            _RS232PortSettings = value
                        End If
                    End If
                End If
            End Set
        End Property

        Private Sub ApplyRS232Settings(ByVal value As String)
            Dim sa() As String
            sa = (value & ",,,,,").Split(","c)
            _PortDataParams.Update("BaudRate=" & sa(0))
            _PortDataParams.Update("Parity=" & sa(1))
            _PortDataParams.Update("DataBits=" & sa(2))
            _PortDataParams.Update("StopBits=" & sa(3))
            _PortDataParams.Update("FlowControl=" & sa(4))
        End Sub

        Private Sub UpdateINI()
            Dim s As String
            Dim a() As String
            a = (_AddressSetting & ",,,,,").Split(","c)
            If _Simul Then
                a(1) = _Address
            Else
                a(0) = _Address
            End If
            If _Disabled Then
                a(2) = "1"
            Else
                a(2) = "0"
            End If
            If _Simul Then
                a(3) = "s"
            Else
                a(3) = ""
            End If
            If _OutOfService Then
                a(5) = "1"
            Else
                a(5) = "0"
            End If
            s = a(0) & "," & a(1) & "," & a(2) & "," & a(3) & "," & _RS232PortSettings.Replace(",", "#") & "," & a(5)
            If s <> _AddressSetting Then
                _AddressSetting = s
                _Settings.SetS(_CommID & "-CommPortFor", _AddressSetting)
            End If
        End Sub

        Public Property Enabled() As Boolean
            Get
                Return Not _Disabled
            End Get
            Set(ByVal value As Boolean)
                _Disabled = Not value
                If _Disabled Then
                    _Status = xCommStatus.Disabled
                    _CommErrorMsg = ""
                    Close()
                    UpdateINI()
                    'UpdateState(Nothing)
                Else
                    _Status = xCommStatus.Closed
                    Connect()
                End If
            End Set
        End Property

        Public Property OutOfService() As Boolean
            Get
                Return _OutOfService
            End Get
            Set(ByVal value As Boolean)
                _OutOfService = value
                UpdateINI()
            End Set
        End Property

        Public Property RXRule() As xCommPacket
            Get
                Return _RXRule
            End Get
            Set(ByVal value As xCommPacket)
                _RXRule = value
                _RXRule.InitByComm(_Log, AddressOf SendW)
                If _TXRule Is Nothing Then
                    _TXRule = value
                End If
            End Set
        End Property

        Public Property TXRule() As xCommPacket
            Get
                Return _TXRule
            End Get
            Set(ByVal value As xCommPacket)
                _TXRule = value
                _TXRule.InitByComm(_Log, AddressOf SendW)
            End Set
        End Property

        Public ReadOnly Property Status() As xCommStatus
            Get
                Return _Status
            End Get
        End Property

        Public ReadOnly Property StatusMessage() As String
            Get
                If _PortType = xCommPortType.RS232 Then
                    Return _Message 'mConnStateMessage(mStatus)
                ElseIf _PortType = xCommPortType.TCPIP Then
                    Return _Message
                Else
                    Return "Invalid Address"
                End If
            End Get
        End Property

        Public Property Address() As String
            Get
                Return _Address
            End Get
            Set(ByVal value As String)
                Close()
                Connect(value)
            End Set
        End Property

        Public Property Simul() As Boolean
            Get
                Return _Simul
            End Get
            Set(ByVal value As Boolean)
                If _Simul <> value Then
                    Close()
                    _Simul = value
                    Dim NewAddr As String
                    If _Simul Then
                        NewAddr = (_AddressSetting & ",").Split(","c)(1)
                    Else
                        NewAddr = (_AddressSetting & ",").Split(","c)(0)
                    End If
                    Connect(NewAddr)
                End If
            End Set
        End Property

        Dim _ReservedAddress As String = ""
        Public Sub Connect(Optional ByVal NewAddress As String = "-")
            If NewAddress <> "-" And NewAddress <> _Address Then
                If _ReservedAddress IsNot Nothing AndAlso _ReservedAddress.Length > 0 Then _OpenedPorts.Remove(_Address, _Device) : _ReservedAddress = Nothing
                _Address = NewAddress
                _ShowLastError = False
            End If
            _PortInfo = CommsPortInfo(_Address, _RS232PortSettings, _CommsSettingsUserEditable, _DefaultRS232Settings)
            _RS232PortSettings = _PortInfo.RS232PortSetting
            UpdateINI()

            If _PortData IsNot Nothing Then _PortData.Close() : _PortData = Nothing
            If _PortListener IsNot Nothing Then _PortListener.Close() : _PortListener = Nothing
            If _PortCtrl IsNot Nothing Then _PortCtrl.Close() : _PortCtrl = Nothing

            If _Disabled Then
                _PortType = xCommPortType.None
                _Status = xCommStatus.Disabled
                _StatusChangedProc.Invoke(_Status)
                Exit Sub
            End If
            _PortType = _PortInfo.Type

            ' check port not already opened by another device
            Dim added = _OpenedPorts.Add(_Address, _Device)
            If Not ReferenceEquals(added, _Device) Then
                If added.Area.VisibleAreaName = _Device.Area.VisibleAreaName Then
                    If _PortType = xCommPortType.RS232 Then
                        _Message = "Port 'COM" & CStr(_PortInfo.Port) & "' already used by " & added.ID
                    Else
                        _Message = "Port '" & _PortInfo.IP & ":" & CStr(_PortInfo.Port) & "' already used by " & added.ID
                    End If
                Else
                    If _PortType = xCommPortType.RS232 Then
                        _Message = "Port 'COM" & CStr(_PortInfo.Port) & "' already used by " & added.VisibleFullID
                    Else
                        _Message = "Port '" & _PortInfo.IP & ":" & CStr(_PortInfo.Port) & "' already used by " & added.VisibleFullID
                    End If
                End If
                _Status = xCommStatus.AlreadyOpenOtherDev
                _StatusChangedProc.Invoke(_Status)
                Exit Sub
            End If
            _ReservedAddress = _Address

            ' open the port
            _Status = xCommStatus.Connecting
            Select Case _PortInfo.Type
                Case xCommPortType.RS232
                    _CtrlReq = False
                    ApplyRS232Settings(_RS232PortSettings)

                    _PortDataParams.Update("Port=" & "COM" & CStr(_PortInfo.Port))
                    _PortData = New xPortRS232(_LogRaw)
                    _PortData.AttachStatusDelegate(AddressOf DataPortStatusChanged)
                    _PortData.AttachReadDelegate(AddressOf DataPortRead)
                    _PortData.Open(_PortDataParams, True)
                Case xCommPortType.TCPIP
                    _PortDataParams.Update("IP=" & _PortInfo.IP & "," & "Port=" & CStr(_PortInfo.Port))
                    If _PortInfo.IPListener Then
                        _PortListener = New xPortTCPServer(_LogRaw)
                        _PortListener.AttachStatusDelegate(AddressOf ListenerPortStatusChanged)
                        _PortListener.AttachNewClientDelegate(AddressOf DataPortServerNewClient)
                        _PortListener.Open(_PortDataParams, True)
                    Else ' is client
                        _PortData = New xPortTCPClient(_LogRaw)
                        _PortData.AttachStatusDelegate(AddressOf DataPortStatusChanged)
                        _PortData.AttachReadDelegate(AddressOf DataPortRead)
                        _Log.Prn("open")
                        _PortDataState = xPort.State.Connecting
                        _PortDataMessage = "Connection Initiated"
                        _PortData.Open(_PortDataParams, True)
                        If _PortInfo.CtrlReq Then
                            _CtrlReq = True
                        Else
                            _CtrlReq = False
                        End If
                    End If
                Case Else
                    _PortType = xCommPortType.None
                    _Status = xCommStatus.Closed
                    _StatusChangedProc.Invoke(_Status)
            End Select
            _ShowLastError = False
            UpdateState(Nothing)
        End Sub

        Public Sub Close(Optional ByVal ShutdownThread As Boolean = False)
            If _ReservedAddress IsNot Nothing AndAlso _ReservedAddress.Length > 0 Then _OpenedPorts.Remove(_Address, _Device) : _ReservedAddress = Nothing

            If _LogRaw.Detail > xLogDetail.All Then _LogCtrl.prn("Port Closed by Line")
            If _PortDataState <> xPort.State.Closed Then
                _PortDataState = xPort.State.Closed
                _PortDataMessage = "Closed"
                UpdateState(Nothing)
            End If

            _RXRule.Clear()
            _TCPCtrlReconnectDataOnClose = False
            If _PortData IsNot Nothing Then _PortData.Close() : _PortData = Nothing
            If _PortCtrl IsNot Nothing Then _PortCtrl.Close() : _PortCtrl = Nothing
            If _PortListener IsNot Nothing Then _PortListener.Close() : _PortListener = Nothing
        End Sub

        Private Sub DataPortStatusChanged(ByVal ConnectionState As xPort.State, ByVal ConnectionMessage As String)
            If _LogRaw.Detail > xLogDetail.All Then _LogRaw.Prn("DataPortStatusChanged: State=" & ConnectionState.ToString & " Message=" & ConnectionMessage)
            If _PortDataState <> ConnectionState Or _PortDataMessage <> ConnectionMessage Then
                _PortDataState = ConnectionState
                _PortDataMessage = ConnectionMessage
                _Area.AddEvent(AddressOf UpdateState, New xEventArg())
                If _CtrlReq Then
                    If _PortDataState = xPort.State.Connected Then
                        If _LogCtrl Is Nothing Then ' create the ctrl logger
                            _LogCtrl = New xLogItem("Ctrl", _Area.AreaID & "-" & _CommID + "-Ctrl", _Area.AreaID & "-" & _CommID, _UnloadList, _Area.LogServerObj, True)
                        End If
                        _PortCtrl = New xPortTCPClient(_LogCtrl)
                        If _PortCtrlParams Is Nothing Then _PortCtrlParams = New xParameterList("", ","c)
                        _PortCtrlParams.Update("IP=" & _PortInfo.IP & "," & "Port=" & CStr(_PortInfo.Port + 1))
                        _PortCtrl.AttachStatusDelegate(AddressOf CtrlPortStatusChanged)
                        _PortCtrl.AttachReadDelegate(AddressOf CtrlPortRead)
                        _PortCtrlState = xPort.State.Connecting
                        _PortCtrlMessage = "Connection Initiated"
                        If _LogCtrl.Detail >= xLogDetail.High Then _LogCtrl.Prn("Connection Initiated")
                        _PortCtrl.Open(_PortCtrlParams, True)
                    ElseIf _PortDataState = xPort.State.Connecting Or _PortDataState = xPort.State.Listening Then
                    Else ' closed or error
                        If _PortCtrl IsNot Nothing Then
                            If _LogCtrl.Detail > xLogDetail.High Then _LogCtrl.Prn("Port Closed because Data port closed.")
                            If _PortCtrl IsNot Nothing Then _PortCtrl.Close() : _PortCtrl = Nothing
                        End If
                    End If
                   
                End If
            End If
        End Sub

        Private Sub CtrlPortStatusChanged(ByVal ConnectionState As xPort.State, ByVal ConnectionMessage As String)
            If _LogCtrl.Detail > xLogDetail.High Then _LogCtrl.Prn("CtrlPortStatusChanged: State=" & ConnectionState.ToString & " Message=" & ConnectionMessage)
            If _PortCtrlState <> ConnectionState Or _PortCtrlMessage <> ConnectionMessage Then
                If _PortCtrlState = xPort.State.Connecting And ConnectionState = xPort.State.Connected Then ' just connected so send data request
                    _PortCtrlState = ConnectionState
                    _PortCtrlMessage = ConnectionMessage
                    _CIMTCPTransitCount = 0
                    If _LogCtrl.Detail >= xLogDetail.Medium Then _LogCtrl.Prn("TX: " & ConvBinToHex(_CIMVersionRequest) & " Version Request")
                    _PortCtrl.Write(_CIMVersionRequest) ' send request for version
                Else
                    _PortCtrlMessage = ConnectionMessage
                    _PortCtrlState = ConnectionState
                End If
                _Area.AddEvent(AddressOf UpdateState, New xEventArg())
            End If
        End Sub

        Private Sub ListenerPortStatusChanged(ByVal ConnectionState As xPort.State, ByVal ConnectionMessage As String)
            If _LogRaw.Detail > xLogDetail.All Then _LogRaw.Prn("ListenerPortStatusChanged: State=" & ConnectionState.ToString & " Message=" & ConnectionMessage)
            If _PortListenerState <> ConnectionState Or _PortListenerMessage <> ConnectionMessage Then
                _PortListenerState = ConnectionState
                _PortListenerMessage = ConnectionMessage
                _Area.AddEvent(AddressOf UpdateState, New xEventArg())
            End If
        End Sub

        Private Sub DataPortServerNewClient(ByVal client As xCore.xPortTCPClient)
            If _PortData IsNot Nothing Then _PortData.Close()
            _PortData = client
            _PortData.AttachStatusDelegate(AddressOf DataPortStatusChanged)
            _PortData.AttachReadDelegate(AddressOf DataPortRead)
        End Sub

        Dim _ShowLastError As Boolean
        Private Sub UpdateState(ByVal Arg As xEventArg)
            Dim NewState As xCommStatus
            Dim NewMessage As String

            If _Disabled Then
                NewState = xCommStatus.Disabled
                NewMessage = "Disabled"
                _ShowLastError = False
            ElseIf _Status = xCommStatus.AlreadyOpenOtherDev Then
                NewState = _Status  ' must disable to remove this error
                NewMessage = _Message ' message text setup in Connect()
            ElseIf _PortDataState = xPort.State.Connecting Then
                NewState = xCommStatus.Connecting
                NewMessage = "Connecting"
                If _ShowLastError Then Exit Sub
            ElseIf _PortDataState = xPort.State.Connected And Not _CtrlReq Then
                NewState = xCommStatus.Connected
                NewMessage = "Connected"
                _ShowLastError = False
            ElseIf _PortDataState = xPort.State.Connected Then ' connected and control required
                If _PortCtrlState = xPort.State.Closed Then
                    NewState = xCommStatus.ErrorState
                    NewMessage = "Ctrl Closed"
                    If _ShowLastError Then Exit Sub
                ElseIf _PortCtrlState = xPort.State.Connecting Then
                    NewState = xCommStatus.Connecting
                    NewMessage = "Ctrl Connecting"
                    If _ShowLastError Then Exit Sub
                ElseIf _PortCtrlState = xPort.State.Connected Then
                    If Not _TCPCtrlSetup Then
                        NewState = xCommStatus.Connecting
                        NewMessage = "Ctrl Init"
                        If _ShowLastError Then Exit Sub
                    Else
                        NewState = xCommStatus.Connected
                        NewMessage = "Connected"
                        _ShowLastError = False
                    End If
                Else
                    NewState = xCommStatus.ErrorState
                    NewMessage = "Ctrl Error"
                End If
            ElseIf _PortDataState = xPort.State.ErrorAccessDenied Then
                NewState = xCommStatus.AccessDenied
                NewMessage = _PortDataMessage
                _ShowLastError = True
            ElseIf _PortDataState = xPort.State.ErrorAlreadyOpenOtherApp Then
                NewState = xCommStatus.AlreadyOpenOtherApp
                NewMessage = _PortDataMessage
                _ShowLastError = True
            ElseIf _PortDataState = xPort.State.ErrorInvalidPort Then
                NewState = xCommStatus.InvalidPort
                NewMessage = _PortDataMessage
                _ShowLastError = True
            ElseIf _PortListenerState = xPort.State.Listening Then
                NewState = xCommStatus.Listening
                NewMessage = "Listening"
            ElseIf _PortInfo.IPListener AndAlso _PortListenerState = xPort.State.Closed Then
                NewState = xCommStatus.Closed
                NewMessage = "Closed"
            ElseIf _PortDataState = xPort.State.Closed Then
                NewState = xCommStatus.Closed
                NewMessage = "Closed"
                _ShowLastError = False
            Else
                NewState = xCommStatus.ErrorState
                NewMessage = _PortDataMessage
                _ShowLastError = True
            End If
            If NewState <> _Status Or NewMessage <> _Message Then
                '_Log.Prn("Status: " & NewState.ToString & " Msg: " & NewMessage)
                _Status = NewState
                _Message = NewMessage
                _StatusChangedProc.Invoke(_Status)
            End If
        End Sub

        Public Sub RequestStatus()
            If _CtrlReq Then
                If _CtrlStatusReqIter > 3 Then
                    _CtrlStatusReqIter = 0
                    Close()
                    Connect()
                Else
                    If _PortCtrlState <> xPort.State.Connected Then
                        If _LogCtrl.Detail >= xLogDetail.All Then _LogCtrl.Prn("Port not open.  State=" & _PortCtrlMessage & " dumped bytes: " & ConvBinToHex(_CIMTXBufSizeReqAndPortStatus))
                    Else
                        If _LogCtrl.Detail >= xLogDetail.All Then _LogCtrl.Prn("TX: " & ConvBinToHex(_CIMBufUsageRequest) & " usage request")
                        _PortCtrl.Write(_CIMBufUsageRequest)
                        _CtrlStatusReqIter += 1
                    End If
                End If
            End If
        End Sub

        Public ReadOnly Property PortType() As xCommPortType
            Get
                Return _PortInfo.Type
            End Get
        End Property

        Public ReadOnly Property PortOpen() As Boolean
            Get
                If _Status = xCommStatus.Connected Then
                    Return True
                End If
            End Get
        End Property

        Private Sub DataPortRead(ByVal data() As Byte, ByVal length As Integer)
            _Area.AddEvent(AddressOf ProcessDataString, New xEventArgString(System.Text.Encoding.GetEncoding(0).GetString(data, 0, length)))
        End Sub

        Private Sub CtrlPortRead(ByVal data() As Byte, ByVal length As Integer)
            _Area.AddEvent(AddressOf ProcessNewCtrlString, New xEventArgString(System.Text.Encoding.GetEncoding(0).GetString(data, 0, length)))
        End Sub

        Public Sub Send(ByVal Data As String, ByVal Comment As String, ByVal CommandType As Integer)
            If _Log.Detail = xLogDetail.High Then
                If CommandType >= 10 Then
                    If _LogHighCmdFormat = 0 Then
                        If Comment.Length = 0 Then
                            _Log.Prn("TX=" & ConvBinToHexAsc(Data))
                        Else
                            _Log.Prn("TX=" & ConvBinToHexAsc(Data) & " (" & Comment & ")")
                        End If
                    Else
                        If Comment.Length = 0 Then
                            _Log.Prn("TX=" & ConvBinToCtrl(Data))
                        Else
                            _Log.Prn("TX=" & ConvBinToCtrl(Data) & " (" & Comment & ")")
                        End If
                    End If
                End If
            ElseIf _Log.Detail >= xLogDetail.All Then
                If _LogAllCmdFormat = 0 Then
                    If Comment.Length = 0 Then
                        _Log.Prn("TX=" & ConvBinToHexAsc(Data))
                    Else
                        _Log.Prn("TX=" & ConvBinToHexAsc(Data) & " (" & Comment & ")")
                    End If
                Else
                    If Comment.Length = 0 Then
                        _Log.Prn("TX=" & ConvBinToCtrl(Data))
                    Else
                        _Log.Prn("TX=" & ConvBinToCtrl(Data) & " (" & Comment & ")")
                    End If
                End If
            End If

            _TXRule.SendCommandType = CommandType
            _TXRule.SendPacket(Data)

            ' set the command type of the received data (used for logging level)
            _RXRule.SendCommandType = CommandType
        End Sub

        Private Sub SendW(ByVal Data As String)
            If _PortDataState <> xPort.State.Connected Then
                If _Log.Detail >= xLogDetail.High Then _Log.Prn("Port not open.  State=" & _PortDataMessage & " dumped TX bytes: " & ConvBinToHex(_CIMTXBufSizeReqAndPortStatus))
                Exit Sub
            End If

            If _PortData Is Nothing Then
                If _TXRule.SendCommandType >= 10 Then ' is not a status command
                    If _LogRaw.Detail >= xLogDetail.Medium Then
                        If _LogHighCmdFormat = 0 Then
                            _LogRaw.prn("* Closed TX=" & ConvBinToHexAsc(Data))
                        Else
                            _LogRaw.prn("* Closed TX=" & ConvBinToCtrl(Data))
                        End If
                    End If
                Else
                    If _LogRaw.Detail >= xLogDetail.High Then
                        If _LogHighCmdFormat = 0 Then
                            _LogRaw.prn("* Closed TX=" & ConvBinToHexAsc(Data))
                        Else
                            _LogRaw.prn("* Closed TX=" & ConvBinToCtrl(Data))
                        End If
                    End If
                End If
                Exit Sub
            End If
            If _PortType = xCommPortType.RS232 Then
                If _TXRule.SendCommandType >= 10 Then ' is not a status command
                    If _LogRaw.Detail >= xLogDetail.Medium Then
                        If _LogHighCmdFormat = 0 Then
                            _LogRaw.prn("TX=" & ConvBinToHexAsc(Data))
                        Else
                            _LogRaw.prn("TX=" & ConvBinToCtrl(Data))
                        End If
                    End If
                Else
                    If _LogRaw.Detail >= xLogDetail.High Then
                        If _LogHighCmdFormat = 0 Then
                            _LogRaw.prn("TX=" & ConvBinToHexAsc(Data))
                        Else
                            _LogRaw.prn("TX=" & ConvBinToCtrl(Data))
                        End If
                    End If
                End If

                Dim DataBA = System.Text.Encoding.GetEncoding(0).GetBytes(Data.ToCharArray)
                _PortData.Write(DataBA)
            ElseIf _PortType = xCommPortType.TCPIP Then ' is TCP
                If _CtrlReq Then
                    _TXQueue += Data
                    AttemptManagedSend()
                Else
                    If _TXRule.SendCommandType >= 10 Then ' is not a status command
                        If _LogRaw.Detail >= xLogDetail.Medium Then
                            If _LogHighCmdFormat = 0 Then
                                _LogRaw.prn("TX=" & ConvBinToHexAsc(Data))
                            Else
                                _LogRaw.prn("TX=" & ConvBinToCtrl(Data))
                            End If
                        End If
                    Else
                        If _LogRaw.Detail >= xLogDetail.High Then
                            If _LogHighCmdFormat = 0 Then
                                _LogRaw.prn("TX=" & ConvBinToHexAsc(Data))
                            Else
                                _LogRaw.prn("TX=" & ConvBinToCtrl(Data))
                            End If
                        End If
                    End If

                    Dim DataBA = System.Text.Encoding.GetEncoding(0).GetBytes(Data.ToCharArray)
                    _PortData.Write(DataBA)
                End If
            End If
        End Sub

        Private Sub ProcessDataString(ByVal Arg As xEventArg)
            Dim ArgStr As xEventArgString = CType(Arg, xEventArgString)
            _DataRxd = True
            If _RXRule.SendCommandType >= 10 Then
                If _LogRaw.Detail >= xLogDetail.Medium Then If _LogHighCmdFormat = 0 Then _LogRaw.Prn("RX=" & ConvBinToHexAsc(ArgStr.Str)) Else _LogRaw.Prn("RX=" & ConvBinToCtrl(ArgStr.Str))
            Else
                If _LogRaw.Detail = xLogDetail.High Then
                    If _LogHighCmdFormat = 0 Then _LogRaw.Prn("RX=" & ConvBinToHexAsc(ArgStr.Str)) Else _LogRaw.Prn("RX=" & ConvBinToCtrl(ArgStr.Str))
                ElseIf _LogRaw.Detail >= xLogDetail.All Then
                    If _LogAllCmdFormat = 0 Then _LogRaw.Prn("RX=" & ConvBinToHexAsc(ArgStr.Str)) Else _LogRaw.Prn("RX=" & ConvBinToCtrl(ArgStr.Str))
                End If
            End If
            _RXRule.ProcessData(ArgStr.Str)
        End Sub

        Public Property DataRxd() As Boolean
            Get
                Return _DataRxd
            End Get
            Set(ByVal value As Boolean)
                _DataRxd = value
            End Set
        End Property

        Private Sub ProcessNewCtrlString(ByVal Arg As xEventArg)
            Dim dat As String
            Dim ArgStr As xEventArgString = CType(Arg, xEventArgString)
            dat = ArgStr.Str

            Dim i As Integer
            Dim ch As Integer
            For i = 1 To Len(dat)
                ch = Asc(Mid(dat, i, 1))
                If _CtrlESCMode Then
                    If ch = 6 Or ch = 21 Or ch = 2 Or ch = 1 Then ' Positive ACK , Negative ACK, Message start, or Alt Message start
                        ' if already in receive mode then dump characters
                        If _CtrlESCRX Or Len(_CtrlRXBuffer) > 0 Then
                            If _CtrlESCRX Then
                                'If _TCPCtrl.Log.Detail >= xLogDetail.Medium Then _TCPCtrl.Log.Prn("Last Packet not terminated. Dumped Chars:" & ConvBinToHex(_CtrlRXBuffer))
                            Else
                                'If _TCPCtrl.Log.Detail >= xLogDetail.High Then _TCPCtrl.Log.Prn("Extra character received. Dumped Chars:" & ConvBinToHex(_CtrlRXBuffer))
                            End If
                        End If
                        _CtrlESCRX = True
                        _CtrlRXBuffer = Chr(ch)
                    ElseIf ch = 27 Then
                        _CtrlRXBuffer = _CtrlRXBuffer & Chr(ch)
                    ElseIf ch = 3 Then  ' end of message
                        ProcessCtrlString(_CtrlRXBuffer)
                        _CtrlRXBuffer = ""
                        _CtrlESCRX = False
                    End If
                    _CtrlESCMode = False
                Else
                    If ch = 27 Then
                        _CtrlESCMode = True
                    Else
                        _CtrlRXBuffer = _CtrlRXBuffer & Chr(ch)
                    End If
                End If
            Next

        End Sub

        Private Sub ProcessCtrlString(ByVal Dat As String)
            Dim Log As xLogItem = _LogCtrl
            _CtrlStatusReqIter = 0

            If _PortCtrlState <> xPort.State.Connected Then
                If Log.Detail >= xLogDetail.All Then Log.Prn("Port not open.  State=" & _PortCtrlMessage & " dumped bytes: " & ConvBinToHex(_CIMTXBufSizeReqAndPortStatus))
                Exit Sub
            End If

            Dim ch As Integer
            ch = Asc(Mid(Dat, 2, 1))
            If ch = 6 Then
                _CIMTXBufferCount = ConvBinToInt16(Mid$(Dat, 3, 2))
                If Log.Detail >= xLogDetail.All Then Log.Prn("TXBuffer=" & CStr(_TXQueue.Length) & " CIMTXBuf=" & CStr(_CIMTXBufferCount) & " Transit=" & CStr(_CIMTCPTransitCount) & " Buffer Usage")
                AttemptManagedSend()
            ElseIf ch = 5 Then
                Dim BytesAccepted As Integer
                Dim BytesReceived As Integer
                BytesReceived = ConvBinToInt16(Mid$(Dat, 3, 2))
                BytesAccepted = ConvBinToInt16(Mid$(Dat, 7, 2))
                _CIMTXBufferCount = ConvBinToInt16(Mid$(Dat, 5, 2))
                If _CIMTCPTransitCount = 100 Then
                    'Stop
                End If
                _CIMTCPTransitCount = _CIMTCPTransitCount - BytesReceived
                'mOutBufferCount = Len(TXQueue) + CIMTXBufferCount + CIMTCPTransitCount
                If Log.Detail >= xLogDetail.All Then Log.Prn("TXBuffer=" & CStr(_TXQueue.Length) & " CIMTXBuf=" & CStr(_CIMTXBufferCount) & " Transit=" & CStr(_CIMTCPTransitCount) & " CIMRX=" & CStr(BytesAccepted))
                If _CIMTCPTransitCount < 0 Then
                    Log.Prn("**** CIMTCPTransitCount negative (" & CStr(_CIMTCPTransitCount) & ")")
                    _CIMTCPTransitCount = 0 ' possible to have been reset and then still get the bytes received
                End If
                If BytesReceived < BytesAccepted Then
                    If Log.Detail >= xLogDetail.Low Then Log.Prn("Bytes Dropped=" & CStr(BytesReceived - BytesAccepted))
                End If
                AttemptManagedSend()

                'timTXBufCountReq.Enabled = False ' raising the event now so abort the backup timer
                'RaiseEvent OutputByteCountChanged(mOutBufferCount)
            ElseIf ch = 1 Then ' got version
                If Mid(Dat, 3) = "RS232 V1.0" Or Mid(Dat, 3) = "RS232 V1.5" Then
                    ' send request for buffer size and set baud, settings, and handshake
                    If _LogCtrl.Detail >= xLogDetail.Medium Then _LogCtrl.Prn("TX: " & ConvBinToHex(_CIMTXBufSizeReqAndPortStatus) & " Version ok. Sending Request for Buffer Size")
                    _PortCtrl.Write(_CIMTXBufSizeReqAndPortStatus)

                    SendCtrlRS232Config(_RS232PortSettings)
                Else
                    Log.Prn("Received Bad Version:" & Mid(Dat, 3))
                End If
            ElseIf ch = 2 Then ' returned the baud and settings
            ElseIf ch = 3 Then ' returned the handshake
            ElseIf ch = 4 Then ' returned the port status
                'Stop
                'If Len(Dat) >= 3 Then
                '    i = Asc(Mid(Dat, 3, 1))
                '    If Log.Detail >= logAll Then Log.Prn("Received Lines=" & Hex(i))
                '    If CIMControlInputs <> (i And 7) Then
                '        CIMControlInputs = (i And 7)
                '        RaiseEvent HardwareLineChanged()
                '    End If
                'End If
            ElseIf ch = 7 Then ' returned the TXBufferSize
                _CIMTXBufferSize = ConvBinToUInt16(Mid$(Dat, 3, 2)) - 1 ' -1 because must always be one free byte
                If Log.Detail >= xLogDetail.All Then Log.Prn("CIMTXBufferSize=" & CStr(_CIMTXBufferSize))

                ' leave the min tx size as 100 which is the same as the max packet size - so smooth download
                '        ' setup max size for transmit on RS232 (ie waits for this many bytes free before sending more data)
                '        If CIMTXBufferSize > 1000 Then
                '            MinTXSize = 500 ' this is the ideal size
                '        Else
                '            MinTXSize = CIMTXBufferSize / 2 ' have to shrink if buffer size smaller - more smaller packets will be sent
                '        End If

                ' set the threshold value to CIMTXBufferSize-MinTXSize so CIM will send notification to PC when MinTXSize bytes free in TX buffer
                Dim val = _CIMTXBufferSize - _MinTXSize * 2
                _CIMSetThreshold(3) = CByte(val >> 8)
                _CIMSetThreshold(4) = CByte(val And 255)
                If _LogCtrl.Detail >= xLogDetail.Medium Then _LogCtrl.Prn("TX: " & ConvBinToHex(_CIMSetThreshold) & " set threshold")
                _PortCtrl.Write(_CIMSetThreshold)
            ElseIf ch = 8 Then ' returned the threshold value
                _TCPCtrlSetup = True
                UpdateState(Nothing)
            End If
        End Sub

        Private Sub AttemptManagedSend()
            Dim i As Integer
            Dim s As String

            i = _CIMTXBufferSize - _CIMTXBufferCount - _CIMTCPTransitCount ' get available bytes in buffer
            '_LogRaw.Prn("AttemptManagedSend _CIMTCPTransitCount=" & CStr(_CIMTCPTransitCount))
            If i > _MinTXSize And _CIMTCPTransitCount < 200 Then ' if sufficient space to send a decent sized data packet and there is not a packet already in transit then send  (this can be called again before windows events are serviced)
                If i > 100 Then i = 100 ' ensure the largest packet size in transit is 100 bytes - so as not to overload the CPU - MOVX is very slow and there are a lot of them to get the data off the chip and move to buffers
                s = Left(_TXQueue, i)
                If Len(s) = 0 Then
                    'If _TCPCtrl.Log.Detail = xLogDetail.All Then _TCPCtrl.Log.Prn("TXBuffer=" & CStr(_TXQueue.Length) & " CIMTXBuf=" & CStr(_CIMTXBufferCount) & " Transit=" & CStr(_CIMTCPTransitCount) & " TX Done")
                Else
                    If i < _TXQueue.Length Then
                        _TXQueue = _TXQueue.Substring(i)
                    Else
                        _TXQueue = ""
                    End If
                    _CIMTCPTransitCount += Len(s)
                    'If _TCPCtrl.Log.Detail = xLogDetail.All Then _TCPCtrl.Log.Prn("TXBuffer=" & CStr(_TXQueue.Length) & " CIMTXBuf=" & CStr(_CIMTXBufferCount) & " Transit=" & CStr(_CIMTCPTransitCount) & " TX(" & CStr(Len(s)) & "): " & ConvBinToHex(s))
                    If _PortData Is Nothing Then
                        If _TXRule.SendCommandType >= 10 Then ' is not a status command
                            If _LogRaw.Detail >= xLogDetail.Medium Then
                                If _LogHighCmdFormat = 0 Then
                                    _LogRaw.Prn("* Closed TX=" & ConvBinToHexAsc(s))
                                Else
                                    _LogRaw.Prn("* Closed TX=" & ConvBinToCtrl(s))
                                End If
                            End If
                        Else
                            If _LogRaw.Detail >= xLogDetail.High Then
                                If _LogHighCmdFormat = 0 Then
                                    _LogRaw.Prn("* Closed TX=" & ConvBinToHexAsc(s))
                                Else
                                    _LogRaw.Prn("* Closed TX=" & ConvBinToCtrl(s))
                                End If
                            End If
                        End If
                    Else
                        If _TXRule.SendCommandType >= 10 Then ' is not a status command
                            If _LogRaw.Detail >= xLogDetail.Medium Then
                                If _LogHighCmdFormat = 0 Then
                                    _LogRaw.Prn("TX=" & ConvBinToHexAsc(s))
                                Else
                                    _LogRaw.Prn("TX=" & ConvBinToCtrl(s))
                                End If
                            End If
                        Else
                            If _LogRaw.Detail >= xLogDetail.High Then
                                If _LogHighCmdFormat = 0 Then
                                    _LogRaw.Prn("TX=" & ConvBinToHexAsc(s))
                                Else
                                    _LogRaw.Prn("TX=" & ConvBinToCtrl(s))
                                End If
                            End If
                        End If
                        Dim DataBA = System.Text.Encoding.GetEncoding(0).GetBytes(s.ToCharArray)
                        _PortData.Write(DataBA)
                    End If
                    'mOutBufferCount = Len(TXQueue) + CIMTXBufferCount + CIMTCPTransitCount
                    'RaiseEvent OutputByteCountChanged(mOutBufferCount)
                End If
            End If

        End Sub

        Private Sub SendCtrlRS232Config(ByVal value As String)
            Dim OutString As String = ""
            Dim CIMBaud As Byte
            Dim CIMSettings As Byte
            Dim sa() As String
            sa = (value & ",,,,,").Split(","c)
            Select Case sa(0)
                Case "300"
                    CIMBaud = 1
                Case "600"
                    CIMBaud = 2
                Case "1200"
                    CIMBaud = 3
                Case "2400"
                    CIMBaud = 4
                Case "4800"
                    CIMBaud = 5
                Case "9600"
                    CIMBaud = 6
                Case "14400"
                    CIMBaud = 7
                Case "19200"
                    CIMBaud = 8
                Case "28800"
                    CIMBaud = 9
                Case "33600"
                    CIMBaud = 10
                Case "38400"
                    CIMBaud = 11
                Case "57600"
                    CIMBaud = 12
                Case "115200"
                    CIMBaud = 13
                Case Else
                    CIMBaud = 6
                    _Message = "Bad Baud Rate. Baud=" & sa(0)
                    _Log.Prn(_Message)
            End Select

            ' setup settings
            Select Case UCase(sa(1))
                Case "N"
                    CIMSettings = 0
                Case "E"
                    CIMSettings = 8
                Case "O"
                    CIMSettings = 4
                Case Else
                    _Message = "Bad Parity. Parity=" & sa(1)
                    _Log.Prn(_Message)
                    Exit Sub
            End Select
            If sa(2) = "7" Then
                CIMSettings = CIMSettings + CByte(2)
            ElseIf sa(2) = "8" Then
            Else
                _Message = "Bad DataBits. DataBits=" & sa(2)
                _Log.Prn(_Message)
                Exit Sub
            End If
            If sa(3) = "2" Then
                CIMSettings = CIMSettings + CByte(1)
            ElseIf sa(3) = "1" Then
            Else
                _Message = "Bad StopBits. StopBits=" & sa(3)
                _Log.Prn(_Message)
                Exit Sub
            End If

            Select Case UCase(sa(4))
                Case "N", ""
                    _CIMHandshaking = 0
                Case "H"
                    _CIMHandshaking = 2
                Case "S"
                    _CIMHandshaking = 1
                Case "HS", "SH"
                    _Message = "'HS' not supported. Must be '','N','H','S'."
                    _Log.Prn(_Message)
                    Exit Sub
                Case Else
                    _Message = "Bad Handshake. Must be '','N','H','S','HS'. Handshake=" & sa(4)
                    _Log.Prn(_Message)
                    Exit Sub
            End Select

            Dim BA() As Byte = {27, 2, 2, CIMBaud, CIMSettings, _CIMControlLines, 27, 3, 27, 2, 3, _CIMHandshaking, 27, 3}
            If _LogCtrl.Detail >= xLogDetail.Medium Then _LogCtrl.Prn("TX: " & ConvBinToHex(BA) & " set RS232 settings")
            _PortCtrl.Write(BA)
        End Sub
    End Class

    Public MustInherit Class xCommPacket
        Public MustOverride Sub ProcessData(ByVal Dat As String)
        Public MustOverride Sub SendPacket(ByVal Dat As String)
        Protected _RXBuffer As String = ""
        Protected _Log As xILogItem
        Protected _DataReceivedProc As xEventProcStr
        Protected _SendDataProc As xEventProcStr
        Protected _SendCommandType As Integer
        Protected _LogHighCmdFormat As Integer
        Protected _LogAllCmdFormat As Integer

        Public Sub New(ByVal DataReceivedProc As xEventProcStr, ByVal LogHighCmdFormat As Integer, ByVal LogAllCmdFormat As Integer)
            _DataReceivedProc = DataReceivedProc
            _LogHighCmdFormat = LogHighCmdFormat
            _LogAllCmdFormat = LogAllCmdFormat
        End Sub

        Public Overridable Sub Clear()
            _RXBuffer = ""
        End Sub

        Public Sub InitByComm(ByVal Log As xILogItem, ByVal SendProc As xEventProcStr)
            _Log = Log
            _SendDataProc = SendProc
        End Sub

        Public Property SendCommandType() As Integer
            Get
                Return _SendCommandType
            End Get
            Set(ByVal value As Integer)
                _SendCommandType = value
            End Set
        End Property

        Protected Sub ProcessDataString(ByVal Data As String)
            If _Log.Detail = xLogDetail.High Then
                If _SendCommandType >= 9 Then
                    If _LogHighCmdFormat = 0 Then
                        _Log.Prn("RX=" & ConvBinToHexAsc(Data))
                    Else
                        _Log.Prn("RX=" & ConvBinToCtrl(Data))
                    End If
                End If
            ElseIf _Log.Detail >= xLogDetail.All Then
                If _LogAllCmdFormat = 0 Then
                    _Log.Prn("RX=" & ConvBinToHexAsc(Data))
                Else
                    _Log.Prn("RX=" & ConvBinToCtrl(Data))
                End If
            End If
            _DataReceivedProc(Data)
        End Sub
    End Class

    Public Class xCommPacketNone
        Inherits xCommPacket

        Public Sub New(ByVal DataReceivedProc As xEventProcStr, ByVal LogHighCmdFormat As Integer, ByVal LogAllCmdFormat As Integer)
            MyBase.New(DataReceivedProc, LogHighCmdFormat, LogAllCmdFormat)
        End Sub

        Public Overrides Sub SendPacket(ByVal Dat As String)
            _SendDataProc(Dat)
        End Sub

        Public Overrides Sub ProcessData(ByVal Dat As String)
            ProcessDataString(Dat)
        End Sub
    End Class

    Public Class xCommPacketCR
        Inherits xCommPacket

        Public Sub New(ByVal DataReceivedProc As xEventProcStr, ByVal LogHighCmdFormat As Integer, ByVal LogAllCmdFormat As Integer)
            MyBase.New(DataReceivedProc, LogHighCmdFormat, LogAllCmdFormat)
        End Sub

        Public Overrides Sub SendPacket(ByVal Dat As String)
            _SendDataProc(Dat & vbCr)
        End Sub

        Public Overrides Sub ProcessData(ByVal Dat As String)
            Dim i As Integer
            Dat = Dat.Replace(Chr(10), "")
            _RXBuffer = _RXBuffer & Dat

            Do
                ' find end of string
                i = InStr(_RXBuffer, vbCr)
                If i = 0 Then
                    Exit Sub
                End If

                Dat = Left(_RXBuffer, i - 1)

                ' remove extracted data from input buffer
                _RXBuffer = Mid(_RXBuffer, i + 1) ' i + len of term str

                ' process extracted data
                ProcessDataString(Dat)
            Loop
        End Sub
    End Class


    Public Class xCommPacketLF
        Inherits xCommPacket

        Public Sub New(ByVal DataReceivedProc As xEventProcStr, ByVal LogHighCmdFormat As Integer, ByVal LogAllCmdFormat As Integer)
            MyBase.New(DataReceivedProc, LogHighCmdFormat, LogAllCmdFormat)
        End Sub

        Public Overrides Sub SendPacket(ByVal Dat As String)
            _SendDataProc(Dat & vbLf)
        End Sub

        Public Overrides Sub ProcessData(ByVal Dat As String)
            Dim i As Integer
            _RXBuffer = _RXBuffer & Dat

            Do
                ' find end of string
                i = InStr(_RXBuffer, vbLf)
                ProcessDataString(Dat)
                If i = 0 Then
                    Exit Sub
                End If

                Dat = Left(_RXBuffer, i - 1)

                ' remove extracted data from input buffer
                _RXBuffer = Mid(_RXBuffer, i + 1) ' i + len of term str

                ' process extracted data
                ProcessDataString(Dat)
            Loop
        End Sub
    End Class

    Public Class xCommPacketCR2
        Inherits xCommPacket

        Public Sub New(ByVal DataReceivedProc As xEventProcStr, ByVal LogHighCmdFormat As Integer, ByVal LogAllCmdFormat As Integer)
            MyBase.New(DataReceivedProc, LogHighCmdFormat, LogAllCmdFormat)
        End Sub

        Public Overrides Sub SendPacket(ByVal Dat As String)
            _SendDataProc(Dat & vbCr)
        End Sub

        Public Overrides Sub ProcessData(ByVal Dat As String)
            Dim ch As Integer
            For i = 1 To Len(Dat)
                ch = Asc(Mid(Dat, i, 1))
                If ch = 2 Then
                    If _RXBuffer <> "" Then
                        If _Log.Detail >= xLogDetail.Medium Then _Log.Prn("Dumped Chars:" & ConvBinToHex(_RXBuffer))
                        _RXBuffer = ""
                    End If
                    ProcessDataString(Chr(ch))
                ElseIf ch = 13 Then
                    ProcessDataString(_RXBuffer)
                    _RXBuffer = ""
                ElseIf ch = 10 Then
                    ' do nothing
                Else
                    _RXBuffer = _RXBuffer & Chr(ch)
                End If
            Next
        End Sub
    End Class

    Public Class xCommPacketCR2HB
        Inherits xCommPacket

        Protected _HeartbeatProc As xEventProc

        Public Sub New(ByVal DataReceivedProc As xEventProcStr, ByVal LogHighCmdFormat As Integer, ByVal LogAllCmdFormat As Integer, ByVal HeartbeatProc As xEventProc)
            MyBase.New(DataReceivedProc, LogHighCmdFormat, LogAllCmdFormat)
            _HeartbeatProc = HeartbeatProc
        End Sub

        Public Overrides Sub SendPacket(ByVal Dat As String)
            _SendDataProc(Dat & vbCr)
        End Sub

        Public Overrides Sub ProcessData(ByVal Dat As String)
            Dim ch As Integer
            For i = 1 To Len(Dat)
                ch = Asc(Mid(Dat, i, 1))
                If ch = 2 Then
                    If _RXBuffer <> "" Then
                        If _Log.Detail >= xLogDetail.Medium Then _Log.Prn("Dumped Chars:" & ConvBinToHex(_RXBuffer))
                        _RXBuffer = ""
                    End If
                    ProcessDataString(Chr(ch))
                ElseIf ch = 33 Then ' '!' is heatbeat char
                    _HeartbeatProc()
                ElseIf ch = 13 Then
                    ProcessDataString(_RXBuffer)
                    _RXBuffer = ""
                ElseIf ch = 10 Then
                    ' do nothing
                Else
                    _RXBuffer = _RXBuffer & Chr(ch)
                End If
            Next
        End Sub
    End Class

    Public Class xCommPacketCRLF
        Inherits xCommPacket

        Public Sub New(ByVal DataReceivedProc As xEventProcStr, ByVal LogHighCmdFormat As Integer, ByVal LogAllCmdFormat As Integer)
            MyBase.New(DataReceivedProc, LogHighCmdFormat, LogAllCmdFormat)
        End Sub

        Public Overrides Sub SendPacket(ByVal Dat As String)
            _SendDataProc(Dat & vbCrLf)
        End Sub

        Public Overrides Sub ProcessData(ByVal Dat As String)
            Dim i As Integer
            Dat = Dat.Replace(Chr(10), "")
            _RXBuffer = _RXBuffer & Dat

            Do
                ' find end of string
                i = InStr(_RXBuffer, vbCr)
                If i = 0 Then
                    Exit Sub
                End If

                Dat = Left(_RXBuffer, i - 1)

                ' remove extracted data from input buffer
                _RXBuffer = Mid(_RXBuffer, i + 1) ' i + len of term str

                ' process extracted data
                ProcessDataString(Dat)
            Loop
        End Sub
    End Class

    Public Class xCommPacketSTXETX
        Inherits xCommPacket

        Public Sub New(ByVal DataReceivedProc As xEventProcStr, ByVal LogHighCmdFormat As Integer, ByVal LogAllCmdFormat As Integer)
            MyBase.New(DataReceivedProc, LogHighCmdFormat, LogAllCmdFormat)
        End Sub

        Public Overrides Sub SendPacket(ByVal Dat As String)
            _SendDataProc(Chr(2) & Dat & Chr(3))
        End Sub

        Public Overrides Sub ProcessData(ByVal Dat As String)
            Dim ch As Integer
            For i = 1 To Len(Dat)
                ch = Asc(Mid(Dat, i, 1))
                If ch = 2 Then
                    If _RXBuffer <> "" Then
                        If _Log.Detail >= xLogDetail.Medium Then _Log.Prn("Dumped Chars:" & ConvBinToHex(_RXBuffer))
                        _RXBuffer = ""
                    End If
                    _RXBuffer = Chr(ch)
                ElseIf ch = 3 Then
                    ProcessDataString(_RXBuffer)
                    _RXBuffer = ""
                Else
                    _RXBuffer = _RXBuffer & Chr(ch)
                End If
            Next
        End Sub
    End Class

    Public Class xCommPacketSTXETXHB
        Inherits xCommPacket

        Protected _HeartbeatProc As xEventProc

        Public Sub New(ByVal DataReceivedProc As xEventProcStr, ByVal LogHighCmdFormat As Integer, ByVal LogAllCmdFormat As Integer, ByVal HeartbeatProc As xEventProc)
            MyBase.New(DataReceivedProc, LogHighCmdFormat, LogAllCmdFormat)
            _HeartbeatProc = HeartbeatProc
        End Sub

        Public Overrides Sub SendPacket(ByVal Dat As String)
            _SendDataProc(Chr(2) & Dat & Chr(3))
        End Sub

        Public Overrides Sub ProcessData(ByVal Dat As String)
            Dim ch As Integer
            For i = 1 To Len(Dat)
                ch = Asc(Mid(Dat, i, 1))
                If ch = 2 Then
                    If _RXBuffer <> "" Then
                        If _Log.Detail >= xLogDetail.Medium Then _Log.Prn("Dumped Chars:" & ConvBinToHex(_RXBuffer))
                        _RXBuffer = ""
                    End If
                    _RXBuffer = ""
                ElseIf ch = 33 Then ' '!' is heatbeat char
                    _HeartbeatProc()
                ElseIf ch = 3 Then
                    ProcessDataString(_RXBuffer)
                    _RXBuffer = ""
                Else
                    _RXBuffer = _RXBuffer & Chr(ch)
                End If
            Next
        End Sub
    End Class

    Public Class xCommPacketESCETB
        Inherits xCommPacket
        Dim _ESCMode As Boolean
        Dim _ESCRX As Boolean
        Dim _DoChecksum As Boolean
        Dim _ChecksumBytePending As Boolean
        Protected _SendValue As Integer = 2

        Public Sub New(ByVal DataReceivedProc As xEventProcStr, ByVal LogHighCmdFormat As Integer, ByVal LogAllCmdFormat As Integer)
            MyBase.New(DataReceivedProc, LogHighCmdFormat, LogAllCmdFormat)
        End Sub

        Public Overrides Sub SendPacket(ByVal Dat As String)
            If Dat.Contains("<FS>") Then Dat = Dat.Replace("<FS>", Chr(28))
            _SendDataProc(Chr(27) & Dat & Chr(23))
        End Sub

        Public Overrides Sub ProcessData(ByVal Dat As String)
            Dim ch As Integer
            If Len(Dat) = 1 Then
                Select Case Asc(Dat)
                    Case 6
                        _RXBuffer = "ACK"
                    Case 21
                        _RXBuffer = "NAK"
                    Case Else
                        _RXBuffer = ConvBinToHexAsc(Dat)
                End Select
                ProcessDataString(_RXBuffer)
                Exit Sub
            End If
            For i = 1 To Len(Dat)
                ch = Asc(Mid(Dat, i, 1))
                    If _ESCMode Then
                        'If _ChecksumBytePending Then
                        '    _ChecksumBytePending = False
                        '    If ch <> 23 Then
                        '        If _Log.Detail >= xLogDetail.Medium Then _Log.Prn("Expecting a checksum and none detected.")
                        '    Else
                        '        ProcessChecksum(_RXBuffer, ch)
                        '        _RXBuffer = ""
                        '    End If
                        'End If
                        'If ch = 6 Or ch = 21 Or ch = 2 Or ch = 1 Then ' Positive ACK , Negative ACK, Message start, or Alt Message start
                        '    ' if already in receive mode then dump characters
                        '    If _ESCRX Or Len(_RXBuffer) > 0 Then
                        '        If _ESCRX Then
                        '            If _Log.Detail >= xLogDetail.Medium Then _Log.Prn("Last Packet not terminated. Dumped Chars:" & ConvBinToHex(_RXBuffer))
                        '        Else
                        '            If _Log.Detail >= xLogDetail.High Then _Log.Prn("Extra character received. Dumped Chars:" & ConvBinToHex(_RXBuffer))
                        '        End If
                        '    End If
                        '    _ESCRX = True
                        '    _RXBuffer = Chr(ch)
                        'Else
                        If ch = 6 Then
                            _RXBuffer = _RXBuffer & Chr(ch)
                        ElseIf ch = 23 Then  ' end of message
                            If Not _DoChecksum Then
                                ProcessDataString(_RXBuffer)
                                _RXBuffer = ""
                            Else
                                _ChecksumBytePending = True
                            End If
                            _ESCRX = False
                        ElseIf ch = 5 Then  ' default print trigger
                            ProcessDataString(Chr(ch))
                        ElseIf ch = 8 Then  ' default print delay
                            ProcessDataString(Chr(ch))
                        ElseIf ch = 15 Then  ' default print start
                            ProcessDataString(Chr(ch))
                        ElseIf ch = 25 Then  ' default print run end
                            ProcessDataString(Chr(ch))
                        End If
                        _ESCMode = False
                    Else
                        If ch = 6 Then
                            _ESCMode = True
                        ElseIf _ChecksumBytePending Then
                            _ChecksumBytePending = False
                            ProcessChecksum(_RXBuffer, ch)
                            _RXBuffer = ""
                        Else
                            _RXBuffer = _RXBuffer & Chr(ch)
                        End If
                    End If
            Next
        End Sub

        Private Sub ProcessChecksum(ByVal Data As String, ByVal RXCheckSum As Integer)
            Dim CS As Integer = 23
            For Each c In Data
                CS += Asc(c)
            Next
            CS = CS And 255
            CS = 256 - CS

            If RXCheckSum <> CS Then
                _Log.Prn("Checksum failure")
            Else
                ProcessDataString(Data)
            End If
        End Sub
    End Class

    Public Class xCommPacketESC
        Inherits xCommPacket
        Dim _ESCMode As Boolean
        Dim _ESCRX As Boolean
        Dim _DoChecksum As Boolean
        Dim _ChecksumBytePending As Boolean
        Protected _SendValue As Integer = 2

        Public Sub New(ByVal DataReceivedProc As xEventProcStr, ByVal LogHighCmdFormat As Integer, ByVal LogAllCmdFormat As Integer)
            MyBase.New(DataReceivedProc, LogHighCmdFormat, LogAllCmdFormat)
        End Sub

        Public Overrides Sub SendPacket(ByVal Dat As String)
            If _DoChecksum Then
                Dim CS As Integer = 3 + _SendValue
                For Each c In Dat
                    CS += Asc(c)
                Next
                CS = CS And 255
                CS = 256 - CS

                Dim CSV As String = Chr(CS)
                If CS = 27 Then CSV = Chr(CS) & Chr(CS)
                _SendDataProc(Chr(27) & Chr(_SendValue) & Dat.Replace(Chr(27), Chr(27) + Chr(27)) & Chr(27) & Chr(3) & CSV)
            Else
                _SendDataProc(Chr(27) & Chr(_SendValue) & Dat.Replace(Chr(27), Chr(27) + Chr(27)) & Chr(27) & Chr(3))
            End If
        End Sub

        Public Overrides Sub ProcessData(ByVal Dat As String)
            Dim ch As Integer
            For i = 1 To Len(Dat)
                ch = Asc(Mid(Dat, i, 1))
                If _ESCMode Then
                    If _ChecksumBytePending Then
                        _ChecksumBytePending = False
                        If ch <> 27 Then
                            If _Log.Detail >= xLogDetail.Medium Then _Log.Prn("Expecting a checksum and none detected.")
                        Else
                            ProcessChecksum(_RXBuffer, ch)
                            _RXBuffer = ""
                        End If
                    End If
                    If ch = 6 Or ch = 21 Or ch = 2 Or ch = 1 Then ' Positive ACK , Negative ACK, Message start, or Alt Message start
                        ' if already in receive mode then dump characters
                        If _ESCRX Or Len(_RXBuffer) > 0 Then
                            If _ESCRX Then
                                If _Log.Detail >= xLogDetail.Medium Then _Log.Prn("Last Packet not terminated. Dumped Chars:" & ConvBinToHex(_RXBuffer))
                            Else
                                If _Log.Detail >= xLogDetail.High Then _Log.Prn("Extra character received. Dumped Chars:" & ConvBinToHex(_RXBuffer))
                            End If
                        End If
                        _ESCRX = True
                        _RXBuffer = Chr(ch)
                    ElseIf ch = 27 Then
                        _RXBuffer = _RXBuffer & Chr(ch)
                    ElseIf ch = 3 Then  ' end of message
                        If Not _DoChecksum Then
                            ProcessDataString(_RXBuffer)
                            _RXBuffer = ""
                        Else
                            _ChecksumBytePending = True
                        End If
                        _ESCRX = False
                    ElseIf ch = 5 Then  ' default print trigger
                        ProcessDataString(Chr(ch))
                    ElseIf ch = 8 Then  ' default print delay
                        ProcessDataString(Chr(ch))
                    ElseIf ch = 15 Then  ' default print start
                        ProcessDataString(Chr(ch))
                    ElseIf ch = 25 Then  ' default print run end
                        ProcessDataString(Chr(ch))
                    End If
                    _ESCMode = False
                Else
                    If ch = 27 Then
                        _ESCMode = True
                    ElseIf _ChecksumBytePending Then
                        _ChecksumBytePending = False
                        ProcessChecksum(_RXBuffer, ch)
                        _RXBuffer = ""
                    Else
                        _RXBuffer = _RXBuffer & Chr(ch)
                    End If
                End If
            Next
        End Sub

        Public Property DoChecksum() As Boolean
            Get
                Return _DoChecksum
            End Get
            Set(ByVal value As Boolean)
                _DoChecksum = value
            End Set
        End Property

        Private Sub ProcessChecksum(ByVal Data As String, ByVal RXCheckSum As Integer)
            Dim CS As Integer = 3
            For Each c In Data
                CS += Asc(c)
            Next
            CS = CS And 255
            CS = 256 - CS

            If RXCheckSum <> CS Then
                _Log.Prn("Checksum failure")
            Else
                ProcessDataString(Data)
            End If
        End Sub
    End Class

    Public Class xCommPacketESC1
        Inherits xCommPacketESC

        Public Sub New(ByVal DataReceivedProc As xEventProcStr, ByVal LogHighCmdFormat As Integer, ByVal LogAllCmdFormat As Integer)
            MyBase.New(DataReceivedProc, LogHighCmdFormat, LogAllCmdFormat)
            _SendValue = 1
        End Sub
    End Class

    Public Class xCommPacketGenFoot
        Inherits xCommPacket

        Dim _Footer As String

        Public Sub New(ByVal DataReceivedProc As xEventProcStr, ByVal LogHighCmdFormat As Integer, ByVal LogAllCmdFormat As Integer, ByVal Footer As String)
            MyBase.New(DataReceivedProc, LogHighCmdFormat, LogAllCmdFormat)
            _Footer = Footer
        End Sub

        Public Overrides Sub SendPacket(ByVal Dat As String)
            _SendDataProc(Dat & _Footer)
        End Sub

        Public Overrides Sub ProcessData(ByVal Dat As String)
            Dim i As Integer
            _RXBuffer = _RXBuffer & Dat

            Do
                ' find end of string
                i = InStr(_RXBuffer, _Footer)
                If i = 0 Then
                    Exit Sub
                End If

                Dat = Left(_RXBuffer, i - _Footer.Length)

                ' remove extracted data from input buffer
                _RXBuffer = Mid(_RXBuffer, i + _Footer.Length)

                ' process extracted data
                If Dat <> "" Then
                    ProcessDataString(Dat)
                End If
            Loop
        End Sub
    End Class
End Namespace
