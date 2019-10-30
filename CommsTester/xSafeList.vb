Namespace xCore
    Public Class xSafeList(Of T)
        Protected _List As List(Of T)

        Public Sub New()
            _List = New List(Of T)
        End Sub

        ''' <summary>
        ''' Safely adds the Item
        ''' </summary>
        ''' <param name="Item"></param>
        ''' <remarks></remarks>
        Public Function Add(ByVal Item As T) As List(Of T)
            Dim initialValue As List(Of T)
            Dim NewAll As List(Of T)
            Do
                NewAll = New List(Of T)
                initialValue = _List
                NewAll.AddRange(_List)
                NewAll.Add(Item)
            Loop While Not ReferenceEquals(initialValue, Threading.Interlocked.CompareExchange(_List, NewAll, initialValue))
            Return initialValue
        End Function

        ''' <summary>
        ''' Safely removes the Item
        ''' </summary>
        ''' <param name="Item"></param>
        ''' <remarks></remarks>
        Public Function Remove(ByVal Item As T) As List(Of T)
            Dim initialValue As List(Of T)
            Dim NewAll As List(Of T)
            Do
                NewAll = New List(Of T)
                initialValue = _List
                NewAll.AddRange(_List)
                NewAll.Remove(Item)
            Loop While Not ReferenceEquals(initialValue, Threading.Interlocked.CompareExchange(_List, NewAll, initialValue))
            Return initialValue
        End Function

        ''' <summary>
        ''' Clears the list and returns the list before it was cleared
        ''' </summary>
        ''' <returns>The list before it was cleared</returns>
        ''' <remarks></remarks>
        Public Function Clear() As List(Of T)
            Dim initialValue As List(Of T)
            Dim NewAll As New List(Of T)
            Do
                initialValue = _List
            Loop While Not ReferenceEquals(initialValue, Threading.Interlocked.CompareExchange(_List, NewAll, initialValue))
            Return initialValue
        End Function

        ''' <summary>
        ''' Returns the current list.  Note: do not store this reference because every Add, Remove, and Clear will give a new reference.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property List As List(Of T)
            Get
                Return _List
            End Get
        End Property
    End Class

    Public Class xSafeDictionary(Of TKey, TValue)
        Dim _Dictionary As Dictionary(Of TKey, TValue)

        Public Sub New()
            _Dictionary = New Dictionary(Of TKey, TValue)
        End Sub

        ''' <summary>
        ''' Safely adds the Item
        ''' </summary>
        ''' <param name="Key"></param>
        ''' <param name="Value"></param>
        ''' <returns>existing value in dictionary or the added value if was not already in dictionary</returns>
        ''' <remarks></remarks>
        Public Function Add(ByVal Key As TKey, ByVal Value As TValue) As TValue
            Dim initialValue As Dictionary(Of TKey, TValue)
            Dim NewAll As Dictionary(Of TKey, TValue)
            Do
                NewAll = New Dictionary(Of TKey, TValue)
                initialValue = _Dictionary
                If initialValue.ContainsKey(Key) Then Return initialValue(Key)
                For Each k In initialValue.Keys
                    NewAll.Add(k, initialValue(k))
                Next
                NewAll.Add(Key, Value)
            Loop While Not ReferenceEquals(initialValue, Threading.Interlocked.CompareExchange(_Dictionary, NewAll, initialValue))
            Return Value
        End Function

        ''' <summary>
        ''' Safely removes the Item
        ''' </summary>
        ''' <param name="Key"></param>
        ''' <param name="Value"></param>
        ''' <returns>true if removed and was in dictionary</returns>
        ''' <remarks></remarks>
        Public Function Remove(ByVal Key As TKey, ByVal Value As TValue) As Boolean
            Dim initialValue As Dictionary(Of TKey, TValue)
            Dim NewAll As Dictionary(Of TKey, TValue)
            Do
                NewAll = New Dictionary(Of TKey, TValue)
                initialValue = _Dictionary
                For Each k In initialValue.Keys
                    NewAll.Add(k, initialValue(k))
                Next
                NewAll.Remove(Key)
            Loop While Not ReferenceEquals(initialValue, Threading.Interlocked.CompareExchange(_Dictionary, NewAll, initialValue))
        End Function

        ''' <summary>
        ''' Clears the list and returns the list before it was cleared
        ''' </summary>
        ''' <returns>The list before it was cleared</returns>
        ''' <remarks></remarks>
        Public Function Clear() As Dictionary(Of TKey, TValue)
            Dim initialValue As Dictionary(Of TKey, TValue)
            Dim NewAll As New Dictionary(Of TKey, TValue)
            Do
                initialValue = _Dictionary
            Loop While Not ReferenceEquals(initialValue, Threading.Interlocked.CompareExchange(_Dictionary, NewAll, initialValue))
            Return initialValue
        End Function

        ''' <summary>
        ''' Returns the current list.  Note: do not store this reference because every Add, Remove, and Clear will give a new reference.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Dictionary As Dictionary(Of TKey, TValue)
            Get
                Return _Dictionary
            End Get
        End Property
    End Class

End Namespace
