Option Explicit On
Option Strict On

Imports System.Net
Imports System.Net.Sockets

Namespace xCore
    Public Class xPortTCPServer
        Inherits xPort

        Private _Socket As Net.Sockets.TcpListener = Nothing

        Delegate Sub Client(ByVal Client As xPortTCPClient)
        Protected _Client As Client = Nothing

        Private _IPAddress As String = "127.0.0.1"
        Private _PortAddress As Integer = 1280

        ''' <summary>
        ''' Initializes a new instance of the <see cref="xPortTCPServer" /> class.
        ''' </summary>
        Public Sub New(ByVal Log As xILogItem)
            MyBase.New(Log)
            AddParameterDecoder("IP", AddressOf ParameterIP)
            AddParameterDecoder("Port", AddressOf ParameterPort)
        End Sub

        ''' <summary>
        ''' Opens a new Client TCP connection based on a previously provided set of parameters.
        ''' </summary>
        Public Overrides Sub Open()
            _Open = True
            Try
                Dim ip As IPAddress = IPAddress.Parse(_IPAddress)
                UpdateStatus(State.Listening, String.Format("Listening on IP: {0}, Port {1}", _IPAddress, _PortAddress))
                _Socket = New System.Net.Sockets.TcpListener(ip, _PortAddress)
                _Socket.Start()
                _Socket.BeginAcceptTcpClient(AddressOf AcceptCallBack, _Socket)
            Catch ex As Exception
                UpdateStatus(State.Error, ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' This closed the Client TCP connection.
        ''' </summary>
        Protected Overrides Sub CloseWorker()
            _Open = False
            If Not _Socket Is Nothing Then
                _Socket.Stop()
                _Socket = Nothing
            End If
            UpdateStatus(State.Closed, "Closed.")
        End Sub

        ''' <summary>
        ''' Not implemented for TCp Servers
        ''' </summary>
        ''' <param name="data">The data to write.</param>
        Public Overrides Sub Write(ByVal Data() As Byte)
            ' nothing to be done here
        End Sub

        ''' <summary>
        ''' attached a function pointer for the new client event.
        ''' </summary>
        ''' <param name="clientDelegate">The function to call on the new client event.</param>
        Public Sub AttachNewClientDelegate(ByVal ClientDelegate As Client)
            _Client = clientDelegate
        End Sub

        ''' <summary>
        ''' This is the callback to the client accept operation.
        ''' </summary>
        ''' <param name="selectedListener">This is the network stream that started the listen operation.</param>
        Protected Sub AcceptCallBack(ByVal SelectedListener As IAsyncResult)
            If _Open Then
                Try
                    Dim socket As System.Net.Sockets.TcpListener = CType(SelectedListener.AsyncState, System.Net.Sockets.TcpListener)
                    Dim newClient As System.Net.Sockets.TcpClient = socket.EndAcceptTcpClient(SelectedListener)
                    If Not newClient Is Nothing Then
                        Dim newTcpClient As xPortTCPClient = New xPortTCPClient(newClient, _Log)
                        If Not _Client Is Nothing Then _Client.Invoke(newTcpClient)
                    End If
                    _Socket.BeginAcceptTcpClient(AddressOf AcceptCallBack, _Socket)
                Catch ex As SocketException
                    UpdateStatus(State.Error, ex.Message)
                Catch ex As Exception
                    UpdateStatus(State.Error, ex.Message)
                End Try
            Else
                UpdateStatus(State.Closed, "Closed By Host.")
            End If
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
            _PortAddress = CInt(ParameterData)
        End Sub
    End Class
End Namespace
