Public Class EntityType
    Private _name As String
    Public Property Name() As String
        Get
            Return _name
        End Get
        Set(ByVal value As String)
            _name = value
        End Set
    End Property

    Private _id As Integer
    Public Property ID() As Integer
        Get
            Return _id
        End Get
        Set(ByVal value As Integer)
            _id = value
        End Set
    End Property


    Private _children As New List(Of EntityType)
    Public Property Children() As List(Of EntityType)
        Get
            Return _children
        End Get
        Set(ByVal value As List(Of EntityType))
            _children = value
        End Set
    End Property

    Private _parent As EntityType
    Public Property Parent() As EntityType
        Get
            Return _parent
        End Get
        Set(ByVal value As EntityType)
            _parent = value
        End Set
    End Property


    Public Function IsCollection() As Boolean
        Return _children.Count > 0

    End Function

    Public Function IsRoot() As Boolean
        Return _parent Is Nothing
    End Function

    Public Function GetDotNotation() As String
        Dim rtn As String = _name

        If IsRoot() Then
            Return rtn
        Else
            rtn = _parent.GetDotNotation & "." & rtn
        End If



        Return rtn

    End Function





    Public Sub New(ByVal Type_Name As String, ByVal Type_Id As Integer, Optional Type_Parent As EntityType = Nothing)
        _name = Type_Name
        _id = Type_Id
        If Not Type_Parent Is Nothing Then
            _parent = Type_Parent
            _parent.Children.Add(Me)
        End If
        '  Console.Write(GetDotNotation() & vbNewLine)
    End Sub


    Public Sub GetDecendents(ByRef FlatList As List(Of EntityType))
        If IsCollection() Then
            For Each child In Children
                child.GetDecendents(FlatList)
            Next
        Else
            FlatList.Add(Me)
        End If

    End Sub

    Public Sub Print()
        If (Not IsRoot()) Then
            Console.Write(GetDotNotation() & vbNewLine)
        End If

        For Each child In _children
            child.Print()
        Next
    End Sub

End Class
