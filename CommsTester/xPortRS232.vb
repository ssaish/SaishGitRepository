Option Explicit On
Option Strict On

Imports System.Runtime.InteropServices

Namespace xCore

    Public Class xPortRS232
        Inherits xPort
        '' rs232 vars 
        Private _RS232 As IO.Ports.SerialPort
        Private _RS232Stream As IO.Stream = Nothing
        Private _WriteInProgress As Boolean = False

        Private _Port As String = "COM1"
        Private _BaudRate As Integer = 9600
        Private _Parity As IO.Ports.Parity = IO.Ports.Parity.None
        Private _DataBits As Integer = 8
        Private _StopBits As IO.Ports.StopBits = IO.Ports.StopBits.One
        Private _FlowControl As IO.Ports.Handshake = IO.Ports.Handshake.None

        ''' <summary>
        ''' Initializes a new instance of the <see cref="xPortRS232" /> class and
        ''' setups the parameter decoding
        ''' </summary>
        Public Sub New(ByVal Log As xILogItem)
            MyBase.New(Log)
            AddParameterDecoder("Port", AddressOf ParameterPort)
            AddParameterDecoder("BaudRate", AddressOf ParameterBaudRate)
            AddParameterDecoder("Parity", AddressOf ParameterParity)
            AddParameterDecoder("DataBits", AddressOf ParameterDataBits)
            AddParameterDecoder("StopBits", AddressOf ParameterStopBits)
            AddParameterDecoder("FlowControl", AddressOf ParameterFlowControl)
        End Sub
        ''' <summary>
        ''' Opens the RS232 connection.
        ''' </summary>
        Public Overrides Sub Open()
            _Open = True
            Try
                _RS232 = New IO.Ports.SerialPort(_Port, _BaudRate, _Parity, _DataBits, _StopBits)
                _RS232.DtrEnable = True
                _RS232.RtsEnable = True
                _RS232.Handshake = _FlowControl
                _RS232.Open()
                _RS232Stream = _RS232.BaseStream
                _RS232Stream.BeginRead(_ReadBuffer, 0, _ReadBuffer.Length, New AsyncCallback(AddressOf ReadCallBack), _RS232Stream)
                UpdateStatus(State.Connected, String.Format("Connected to Com: {0}, Baud: {1}, Data Bits: {2}, Parity: {2}, Stop Bits: {4}, Flow: {5}", _Port, _BaudRate, _Parity, _DataBits, _StopBits, _FlowControl))
            Catch ex As System.ArgumentNullException
                CloseReconnect(State.Error, "Port Not Specified")
            Catch ex As InvalidOperationException
                CloseReconnect(State.ErrorAlreadyOpenOtherApp, "Port '" & CStr(_Port) & "' Already Open")
            Catch ex As UnauthorizedAccessException
                CloseReconnect(State.ErrorAlreadyOpenOtherApp, "Port '" & CStr(_Port) & "' Already opened by another application or Access Denied")
            Catch ex As System.ArgumentException
                If ex.ParamName = "portName" Then
                    CloseReconnect(State.ErrorInvalidPort, "Port '" & CStr(_Port) & "' not available. Perhaps used by windows printer.")
                Else
                    CloseReconnect(State.ErrorInvalidPort, "Port Argument Error: " & ex.Message)
                End If
            Catch ex As Exception
                CloseReconnect(State.Error, "Port Error: " & ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' Closes the RS232 connection.
        ''' </summary>
        Protected Overrides Sub CloseWorker()
            If _RS232Stream IsNot Nothing Then
                Try
                    _RS232Stream.Close() ' could throw exception if safe handle closed
                Catch ex As Exception

                End Try
                _RS232Stream = Nothing
            End If
            If _RS232 IsNot Nothing Then
                _RS232.Close()
                _RS232 = Nothing
            End If
            UpdateStatus(State.Closed, "Closed by close")
        End Sub

        ''' <summary>
        ''' Asynchronous read events on the current RS232 connection.
        ''' </summary>
        ''' <param name="selectedStream">The network stream that initiated the read callback.</param>
        Protected Sub ReadCallBack(ByVal SelectedStream As IAsyncResult)
            Dim RS232Stream As IO.Stream = CType(SelectedStream.AsyncState, IO.Stream)
            If RS232Stream.CanRead Then
                Dim NumberOfBytesRead As Integer
                Try
                    NumberOfBytesRead = RS232Stream.EndRead(SelectedStream)
                Catch ex As Exception
                    CloseReconnect(State.Error, ex.Message)
                    Exit Sub
                End Try
                If NumberOfBytesRead > 0 Then
                    Dim data(NumberOfBytesRead) As Byte
                    Buffer.BlockCopy(_ReadBuffer, 0, data, 0, NumberOfBytesRead)
                    If Not _Read Is Nothing Then _Read.Invoke(data, NumberOfBytesRead)
                    ' block on the next read
                    RS232Stream.BeginRead(_ReadBuffer, 0, _ReadBuffer.Length, New AsyncCallback(AddressOf ReadCallBack), RS232Stream)
                Else
                    CloseReconnect(State.Closed, "Port Closed by Client")
                End If
            Else
                Close()
            End If
        End Sub

        ''' <summary>
        ''' Asynchronous write of RS232 Data. If a write is currently in progress then 
        ''' the data is stored and writen when then current write is completed.
        ''' </summary>
        ''' <param name="data">The data to write.</param>
        Public Overrides Sub Write(ByVal Data() As Byte)
            '' do we have a connection
            If _RS232Stream Is Nothing Then Exit Sub
            If Not _RS232Stream.CanWrite Then Exit Sub
            If Not _RS232.IsOpen Then Exit Sub

            If _WriteInProgress Then
                AddWrite(Data)
            Else
                _WriteInProgress = True
                If Not _RS232Stream Is Nothing AndAlso _RS232Stream.CanWrite Then
                    _RS232Stream.BeginWrite(Data, 0, Data.Count, AddressOf WriteCallBack, _RS232Stream)
                Else
                    _WriteInProgress = False
                End If
            End If
        End Sub

        ''' <summary>
        ''' This is the callback to the write operation.
        ''' </summary>
        ''' <param name="selectedStream">This is the RS232 stream that started the write operation.</param>
        Protected Sub WriteCallBack(ByVal SelectedStream As IAsyncResult)
            Dim RS232Stream As IO.Stream = CType(SelectedStream.AsyncState, IO.Stream)
            If RS232Stream.CanWrite Then
                Try
                    RS232Stream.EndWrite(SelectedStream)
                Catch ex As Exception
                    CloseReconnect(State.Error, ex.Message)
                    Exit Sub
                End Try
                If Not IsWriteQueueEmpty() Then
                    If Not RS232Stream Is Nothing AndAlso RS232Stream.CanWrite Then
                        Dim handle As GCHandle = GetNextWrite()
                        Dim ba() As Byte = CType(handle.Target, Byte())
                        RS232Stream.BeginWrite(ba, 0, ba.Count, AddressOf WriteCallBack, RS232Stream)
                        handle.Free()
                    End If
                Else
                    _WriteInProgress = False
                End If
            Else
                _WriteInProgress = False
                UpdateStatus(State.Closed, "Port Closed by Host")
            End If
        End Sub

        ''' <summary>
        ''' This is called to decode the com port number.
        ''' </summary>
        ''' <param name="parameterData">The data string containing the com port number information.</param>
        Private Sub ParameterPort(ByVal ParameterData As String)
            If InStr(ParameterData.ToUpper, "COM") = 0 Then
                _Port = "COM" & ParameterData
            Else
                _Port = ParameterData
            End If
        End Sub

        ''' <summary>
        ''' This is called to decode the baud rate.
        ''' </summary>
        ''' <param name="parameterData">The data sting containing the baud rate information.</param>
        Private Sub ParameterBaudRate(ByVal ParameterData As String)
            _BaudRate = CInt(ParameterData)
        End Sub

        ''' <summary>
        ''' This is called to decode the parity.
        ''' </summary>
        ''' <param name="parameterData">The data sting containing the parity information.</param>
        Private Sub ParameterParity(ByVal ParameterData As String)
            ParameterData = ParameterData.ToUpper
            If ParameterData = "E" Or ParameterData = "EVEN" Then
                _Parity = IO.Ports.Parity.Even
            ElseIf ParameterData = "O" Or ParameterData = "ODD" Then
                _Parity = IO.Ports.Parity.Odd
            Else
                _Parity = IO.Ports.Parity.None
            End If
        End Sub

        ''' <summary>
        ''' This is called to decode the number of data bits.
        ''' </summary>
        ''' <param name="parameterData">The data sting containing the data bits information.</param>
        Private Sub ParameterDataBits(ByVal ParameterData As String)
            _DataBits = CInt(ParameterData)
        End Sub

        ''' <summary>
        ''' This is called to decode the number of stop bits.
        ''' </summary>
        ''' <param name="parameterData">The data sting containing the stop bits information.</param>
        Private Sub ParameterStopBits(ByVal ParameterData As String)
            If ParameterData = "2" Or ParameterData = "TWO" Then
                _StopBits = IO.Ports.StopBits.Two
            ElseIf ParameterData = "1.5" Or ParameterData = "ONEPOINTFIVE" Then
                _StopBits = IO.Ports.StopBits.OnePointFive
            Else
                _StopBits = IO.Ports.StopBits.One
            End If
        End Sub

        ''' <summary>
        ''' This is called to decode the flow contol.
        ''' </summary>
        ''' <param name="parameterData">The data sting containing the flow control information.</param>
        Private Sub ParameterFlowControl(ByVal ParameterData As String)
            If ParameterData = "HW" Or ParameterData = "HARDWARE" Then
                _FlowControl = IO.Ports.Handshake.RequestToSend
            ElseIf ParameterData = "HW/SW" Or ParameterData = "HARDWARESOFTWARE" Then
                _FlowControl = IO.Ports.Handshake.RequestToSendXOnXOff
            ElseIf ParameterData = "SW" Or ParameterData = "SOFTWARE" Then
                _FlowControl = IO.Ports.Handshake.XOnXOff
            Else
                _FlowControl = IO.Ports.Handshake.None
            End If
        End Sub
    End Class
End Namespace
