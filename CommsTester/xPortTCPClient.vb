Option Explicit On
Option Strict On

Imports System.Net.Sockets
Imports System.Runtime.InteropServices

Namespace xCore
    Public Class xPortTCPClient
        Inherits xPort

        '' socket vars and functions
        Private _Socket As Net.Sockets.TcpClient = Nothing
        Private _NetStream As Net.Sockets.NetworkStream = Nothing
        Private _WriteInProgress As Boolean = False

        Private _IPAddress As String = "127.0.0.1"
        Private _PortAddress As Integer = 1280

        ''' <summary>
        ''' Initializes a new instance of the <see cref="xPortTCPClient" /> class.
        ''' </summary>
        Public Sub New(ByVal Log As xILogItem)
            MyBase.New(Log)
            AddParameterDecoder("IP", AddressOf ParameterIP)
            AddParameterDecoder("Port", AddressOf ParameterPort)
            If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("New Client (closed)")
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="xPortTCPClient" /> class.
        ''' </summary>
        ''' <param name="client">Is a TCPClient connection typicaly created from TCPServer.</param>
        Public Sub New(ByVal Client As System.Net.Sockets.TcpClient, ByVal Log As xILogItem)
            MyBase.New(Log)
            _AutoReconnect = False
            _Socket = Client
            _Open = True
            If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("New Client (open)")
            If _Socket Is Nothing Then _Open = False
            If Not _Socket.Connected Then _Open = False
            If _Open Then
                Try
                    _NetStream = _Socket.GetStream
                    _NetStream.BeginRead(_ReadBuffer, 0, _ReadBuffer.Length, New AsyncCallback(AddressOf ReadCallBack), _NetStream)
                Catch ex As Exception
                    UpdateStatus(State.Error, ex.Message)
                    Exit Sub
                End Try
                ' Raise connected event
                UpdateStatus(State.Connected, "Connected from Listener")
            End If
        End Sub

        ''' <summary>
        ''' Opens a new Client TCP connection based on a previously provided set of parameters.
        ''' </summary>
        Public Overrides Sub Open()
            '' if the params are valid then allow the auto reconnect function to work.
            If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("Open")
            _Open = True
            Try
                UpdateStatus(State.Connecting, String.Format("Connecing to IP:  {0}, Port {1}", _IPAddress, _PortAddress))
                _Socket = New System.Net.Sockets.TcpClient
                _Socket.BeginConnect(_IPAddress, _PortAddress, AddressOf ConnectCallBack, _Socket)
            Catch ex As Exception
                UpdateStatus(State.Error, "Open: " & ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' This handles the connection callback for a successfull / failed Client TCP connection.
        ''' </summary>
        ''' <param name="selectedSocket">The selectedSocket is the socket that initiated the connection attempt.</param>
        Protected Sub ConnectCallBack(ByVal SelectedSocket As IAsyncResult)
            If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("Open")
            If _Open Then
                Try
                    Dim socket As System.Net.Sockets.TcpClient = CType(SelectedSocket.AsyncState, System.Net.Sockets.TcpClient)
                    socket.EndConnect(SelectedSocket)
                    _NetStream = socket.GetStream
                    _NetStream.BeginRead(_ReadBuffer, 0, _ReadBuffer.Length, New AsyncCallback(AddressOf ReadCallBack), _NetStream)
                Catch ex As Exception
                    CloseReconnect(State.Error, ex.Message)
                    Exit Sub
                End Try
                UpdateStatus(State.Connected, "Connected")
            End If
        End Sub

        ''' <summary>
        ''' This closed the Client TCP connection.
        ''' </summary>
        Protected Overrides Sub CloseWorker()
            If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("CloseWorker")
            If Not _NetStream Is Nothing Then
                _NetStream.Close()
                _NetStream = Nothing
            End If
            If Not _Socket Is Nothing Then
                _Socket.Close()
                _Socket = Nothing
            End If
        End Sub

        ''' <summary>
        ''' Asynchronous read events on the current Client TCP connection.
        ''' </summary>
        ''' <param name="selectedStream">The network stream that initiated the read callback.</param>
        Protected Sub ReadCallBack(ByVal SelectedStream As IAsyncResult)
            If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("ReadCallBack")
            Dim netstream As Net.Sockets.NetworkStream = CType(SelectedStream.AsyncState, Net.Sockets.NetworkStream)
            If _Open And netstream.CanRead Then
                Dim numberOfBytesRead As Integer
                Try
                    numberOfBytesRead = netstream.EndRead(SelectedStream)
                Catch ex As Exception
                    CloseReconnect(State.Error, ex.Message)
                    Exit Sub
                End Try
                If numberOfBytesRead > 0 Then
                    Dim data(numberOfBytesRead) As Byte
                    Buffer.BlockCopy(_ReadBuffer, 0, data, 0, numberOfBytesRead)
                    If Not _Read Is Nothing Then _Read.Invoke(data, numberOfBytesRead)
                    netstream.BeginRead(_ReadBuffer, 0, _ReadBuffer.Length, New AsyncCallback(AddressOf ReadCallBack), netstream)
                Else
                    CloseReconnect(State.Closed, "Port Closed by Client")
                End If
            Else
                UpdateStatus(State.Closed, "Port Closed by Host during read")
            End If
        End Sub

        ''' <summary>
        ''' Asynchronous write of TCP Data. If a write is currently in progress then 
        ''' the data is stored and writen when then current write is completed.
        ''' </summary>
        ''' <param name="data">The data to write.</param>
        Public Overrides Sub Write(ByVal Data() As Byte)
            If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("Write " & CStr(Data.Length) & " bytes")
            If _NetStream Is Nothing Then
                If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("Write aborted. no stream.")
                Exit Sub
            End If
            If Not _NetStream.CanWrite Then
                If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("Write aborted. can not write.")
                Exit Sub
            End If

            Try
                If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("Write writing " & CStr(Data.Length) & " bytes")
                _NetStream.BeginWrite(Data, 0, Data.Count, Nothing, _NetStream)
                If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("Write Begun")
            Catch ex As Exception ' got exception that the other device forcibly closed the connection
                If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("Write Error: " & ex.Message)
                CloseReconnect(State.Error, ex.Message)
                Exit Sub
            End Try
        End Sub

        ''' <summary>
        ''' This is called to decode the ip address.
        ''' </summary>
        ''' <param name="parameterData">The data string containing the ip address information.</param>
        Private Sub ParameterIP(ByVal ParameterData As String)
            _IPAddress = ParameterData
        End Sub

        ''' <summary>
        ''' This is called to decode the port address.
        ''' </summary>
        ''' <param name="parameterData">The data sting containing the port address information.</param>
        Private Sub ParameterPort(ByVal ParameterData As String)
            Integer.TryParse(ParameterData, _PortAddress)
        End Sub
    End Class

End Namespace
