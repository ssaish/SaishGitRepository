Imports System.Threading
Imports System.Runtime.InteropServices

Namespace xCore
    Public MustInherit Class xPort

        Public Enum State
            <System.Reflection.ObfuscationAttribute(Exclude:=True)> Closed
            <System.Reflection.ObfuscationAttribute(Exclude:=True)> Connecting
            <System.Reflection.ObfuscationAttribute(Exclude:=True)> Connected
            <System.Reflection.ObfuscationAttribute(Exclude:=True)> Listening
            <System.Reflection.ObfuscationAttribute(Exclude:=True)> [Error]
            <System.Reflection.ObfuscationAttribute(Exclude:=True)> ErrorAlreadyOpenOtherApp
            <System.Reflection.ObfuscationAttribute(Exclude:=True)> ErrorAccessDenied
            <System.Reflection.ObfuscationAttribute(Exclude:=True)> ErrorInvalidPort
        End Enum

        '' reading vars and functions
        Delegate Sub read(ByVal data() As Byte, ByVal length As Integer)
        Protected _Read As read = Nothing
        Protected _ReadBuffer(1024) As Byte

        '' writing vars and functions
        Protected _writeBuffer() As Byte
        Public MustOverride Sub Write(ByVal data() As Byte)
        Private _WriteQueue(&H1FFF) As GCHandle
        Private _WriteQueuePtrIn As Integer
        Private _WriteQueuePtrOut As Integer
        Private _WriteQueueCount As Integer

        '' auto reconnect vars and functions
        Protected _TimoutDelegate As TimerCallback = AddressOf AutoReconnectTimer
        Protected _AutoReconnectTimer As Timer
        Protected _AutoReconnect As Boolean = False

        '' connection state vars and functions
        Protected _ConnectionState As State = State.Closed
        Protected _ConnectionMessage As String = "Not Connected..."
        Delegate Sub Status(ByVal connectionState As State, ByVal ConnectionMessage As String)
        Protected _Status As Status = Nothing

        '' open vars and functions
        Public MustOverride Sub Open()
        Protected _Open As Boolean

        '' close vars and functions
        Protected MustOverride Sub CloseWorker()

        '' parameters
        Protected Delegate Sub ParameterDecoder(ByVal ParameterData As String)
        Private _ParameterDecoder As New Generic.Dictionary(Of String, ParameterDecoder)

        Protected _Log As xILogItem

        Public Sub New(ByVal Log As xILogItem)
            _Log = Log
        End Sub

        ''' <summary>
        ''' Opens a new connection.
        ''' </summary>
        ''' <param name="ConnectionParameters">Connect using this parameter list</param>
        ''' <param name="autoReconnect">if set to <c>true</c> the connection will automaticaly reconnect if disconnected.</param>
        Public Sub Open(ByVal ConnectionParameters As xParameterList, ByVal autoReconnect As Boolean)
            If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("Open Recon=" & autoReconnect)
            Close()
            _AutoReconnect = autoReconnect
            Dim paramsOk As Boolean = True
            For Each kp As KeyValuePair(Of String, ParameterDecoder) In _ParameterDecoder
                If ConnectionParameters.Contains(kp.Key) Then
                    If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("Param " & kp.Key & " Value=" & ConnectionParameters.GetValue(kp.Key))
                    kp.Value.Invoke(ConnectionParameters.GetValue(kp.Key))
                Else
                    paramsOk = False
                End If
            Next
            Open()
        End Sub

        Public Sub Close()
            If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("Close")
            _Open = False
            CloseWorker()
            UpdateStatus(State.Closed, "Closed")
        End Sub

        ''' <summary>
        ''' This closes the current connection and reopens it if required.
        ''' </summary>
        ''' <param name="state">This is the port state on the closed connection.</param>
        ''' <param name="message">This is a message explaning why the port was closed.</param>
        Protected Sub CloseReconnect(ByVal State As State, ByVal Message As String)
            If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("CloseReconnect State=" & State.ToString & " msg=" & Message)
            CloseWorker()
            If _AutoReconnectTimer Is Nothing And _Open Then _AutoReconnectTimer = New Timer(_TimoutDelegate, Nothing, 2000, System.Threading.Timeout.Infinite)
            UpdateStatus(State, Message)
        End Sub

        ''' <summary>
        ''' attached a function pointer for the read event.
        ''' </summary>
        ''' <param name="readDelegate">The function to call on the read event.</param>
        Public Sub AttachReadDelegate(ByVal ReadDelegate As read)
            _Read = ReadDelegate
        End Sub

        ''' <summary>
        ''' Auto the reconnect timer.
        ''' </summary>
        ''' <param name="state">The state of the timer.</param>
        Protected Sub AutoReconnectTimer(ByVal State As Object)
            _AutoReconnectTimer = Nothing
            If _AutoReconnect And _Open Then Open()
        End Sub

        ''' <summary>
        ''' This adds a byte() to the write queue as a GCHandle.
        ''' </summary>
        ''' <param name="data">The byte() to be added.</param>
        Protected Sub AddWrite(ByVal Data() As Byte)
            If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("Add Write " & CStr(Data.Length) & " bytes")
            _WriteQueueCount += 1
            Dim ptrOut = _WriteQueuePtrOut
            Dim ptrIn As Integer
            Dim initialValue As Integer
            Do
                initialValue = _WriteQueuePtrIn
                ptrIn = (initialValue + 1) And &H1FFF
                ' CompareExchange compares _EventQueuePtrIn to initialValue. If 
                ' they are not equal, then another thread has updated the 
                ' running total since this loop started. CompareExchange 
                ' does not update _EventQueuePtrIn. CompareExchange returns the 
                ' contents of _EventQueuePtrIn, which do not equal initialValue, 
                ' so the loop executes again. 
                If ptrIn = ptrOut Then ' queue full 
                    If _Log IsNot Nothing Then _Log.Prn("** Write Data Queue Full. Data Lost. " & CStr(Data.Length) & " bytes.")
                    Exit Sub
                End If
            Loop While initialValue <> Interlocked.CompareExchange(_WriteQueuePtrIn, ptrIn, initialValue)

            'Dim gch As GCHandle = GCHandle.Alloc(data)
            _WriteQueue(ptrIn) = GCHandle.Alloc(Data) 'GCHandle.ToIntPtr(gch)
            If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("Add Write ptrIn=" & CStr(ptrIn))
        End Sub

        ''' <summary>
        ''' Determines whether the write queue is empty.
        ''' </summary>
        ''' <returns>
        ''' <c>true</c> if the write queue is empty; otherwise, <c>false</c>.
        ''' </returns>
        Protected Function IsWriteQueueEmpty() As Boolean
            If _WriteQueuePtrIn = _WriteQueuePtrOut Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Gets the byte() from the write queue.
        ''' </summary>
        ''' <returns>GCHandle to the next byte()</returns>
        Protected Function GetNextWrite() As GCHandle
            If _WriteQueuePtrIn = _WriteQueuePtrOut Then Return Nothing

            Dim ptrIn As Integer = _WriteQueuePtrIn
            Dim ptrOut As Integer = (_WriteQueuePtrOut + 1) And &H1FFF
            Dim retVal As GCHandle = _WriteQueue(ptrOut)
            Dim iter As Integer
            Do While Not retVal.IsAllocated
                iter += 1
                If (iter And &H1F) = 0 Then
                    If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("** GetNextWrite iter=" & CStr(iter))
                End If
                Thread.Sleep(1)
                retVal = _WriteQueue(ptrOut)
            Loop
            _WriteQueuePtrOut = (_WriteQueuePtrOut + 1) And &H1FFF ' increment the pointer after the event is safe to be removed from the queue. this is the only thread which modifies the Out pointer 
            _WriteQueue(ptrOut) = Nothing

            If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("GetNextWrite ptrOut=" & CStr(ptrOut))

            Return retVal
        End Function

        ''' <summary>
        ''' This will add a function poiner to decode required parameters.
        ''' </summary>
        ''' <param name="name">The name of the parameter to decode.</param>
        ''' <param name="paramDecoder">The function to call to decode the parameter.</param>
        Protected Sub AddParameterDecoder(ByVal Name As String, ByVal ParamDecoder As ParameterDecoder)
            If _ParameterDecoder.ContainsKey(UCase(Name)) Then
                _ParameterDecoder(Name) = ParamDecoder
            Else
                _ParameterDecoder.Add(Name, ParamDecoder)
            End If
        End Sub

        ''' <summary>
        ''' attached a function pointer for the status event.
        ''' </summary>
        ''' <param name="statusDelegate">The function to call on the status event.</param>
        Public Sub AttachStatusDelegate(ByVal StatusDelegate As Status)
            _Status = StatusDelegate
            If Not _Status Is Nothing Then _Status.Invoke(_ConnectionState, _ConnectionMessage)
        End Sub

        ''' <summary>
        ''' Updates the status of the parent object.
        ''' </summary>
        ''' <param name="connectionState">State of the connection.</param>
        ''' <param name="connectionMessage">The connection message.</param>
        Protected Sub UpdateStatus(ByVal ConnectionState As State, ByVal ConnectionMessage As String)
            If _Log IsNot Nothing AndAlso _Log.Detail >= xLogDetail.All Then _Log.Prn("UpdateStatus State=" & ConnectionState.ToString & " msg=" & ConnectionMessage)
            If _ConnectionState <> ConnectionState Or _ConnectionMessage <> ConnectionMessage Then
                _ConnectionState = ConnectionState
                _ConnectionMessage = ConnectionMessage
                If Not _Status Is Nothing Then _Status.Invoke(_ConnectionState, _ConnectionMessage)
            End If
        End Sub

    End Class
End Namespace
