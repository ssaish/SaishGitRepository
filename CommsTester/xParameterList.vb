Namespace xCore
    ''' <summary>
    ''' This holds a collectiom of parameters of selected types
    ''' </summary>
    Public Class xParameterList
        Dim _Params As New Generic.Dictionary(Of String, String)
        Dim _Delimiter As Char

        ''' <summary>
        ''' Initializes a new instance of the <see cref="xParameterList" /> class.
        ''' </summary>
        ''' <param name="data">Holds a string repesentation of the parameters.  This strring is then broken up based on the delemiters and stored as strings in the parameter array.   eg 'P1=val1,P2=Val2'</param>
        ''' <param name="delimiter">This states the delimiter used to separate the parameters.  Typically is a comma.</param>
        Public Sub New(ByVal data As String, ByVal delimiter As Char)
            _Delimiter = delimiter
            If data Is Nothing OrElse data = "" Then Exit Sub
            For Each p In data.Split(_Delimiter)
                Dim pa = p.Split("="c)
                If pa.Length = 1 Then
                    _Params.Add(pa(0), "")
                ElseIf pa.Length >= 2 Then
                    _Params.Add(pa(0), pa(1).Trim)
                End If
            Next
        End Sub

        ''' <summary>
        ''' Updates the parameters specified.
        ''' </summary>
        ''' <param name="data">Parameters are in the same format as the constructor.  eg 'P1=val1,P2=Val2'</param>
        ''' <remarks></remarks>
        Public Sub Update(ByVal data As String)
            If data Is Nothing OrElse data = "" Then Exit Sub
            For Each p In data.Split(_Delimiter)
                Dim pa = p.Split("="c)
                If pa.Length = 1 Then
                    If _Params.ContainsKey(pa(0)) Then
                        _Params(pa(0)) = ""
                    Else
                        _Params.Add(pa(0), "")
                    End If
                ElseIf pa.Length >= 2 Then
                    If _Params.ContainsKey(pa(0)) Then
                        _Params(pa(0)) = pa(1).Trim
                    Else
                        _Params.Add(pa(0), pa(1).Trim)
                    End If
                End If
            Next
        End Sub

        ''' <summary>
        ''' Determines whether the parameter array contains the given key.
        ''' </summary>
        ''' <param name="parameterKey">The key to find.</param>
        ''' <returns>
        ''' <c>true</c> if the parameterKey is found; otherwise, <c>false</c>.
        ''' </returns>
        Public Function Contains(ByVal parameterKey As String) As Boolean
            Return _Params.ContainsKey(parameterKey)
        End Function

        ''' <summary>
        ''' Determines whether the parameter array contains the given keys.
        ''' </summary>
        ''' <param name="parameterKey">The key to find.</param>
        ''' <returns>
        ''' <c>true</c> if the parameterKey is found; otherwise, <c>false</c>.
        ''' </returns>
        Public Function Contains(ByVal parameterKey() As String) As Boolean
            Dim found As Boolean = True
            For Each key As String In parameterKey
                If _Params.ContainsKey(key) = False Then
                    found = False
                    Exit For
                End If
            Next
            Return found
        End Function

        ''' <summary>
        ''' Gets the specified paramater based on the parameterKey given.
        ''' </summary>
        ''' <param name="paramaterKey">Name of the paramater to return.</param>
        ''' <returns>Prameter information for the given key</returns>
        Public Function GetValue(ByVal paramaterKey As String) As String
            Return _Params(paramaterKey)
        End Function

    End Class
End Namespace
