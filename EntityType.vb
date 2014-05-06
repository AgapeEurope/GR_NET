''' <summary>
''' Local object model to represent GR entity types. 
''' </summary>
''' <remarks>
''' EntityTypes refect the structure of profile properties and collections in the Global Registry. (eg 'person' is an enitity_type. It has a child entity_types of 'first_name', 'last_name' etc)
''' Entity types can be nested (ie they have parents/children). These objects are created by the GR constructor. The main method you are likely to call on this object is GetDotNotation. 
''' GR.GetFlatEntityTypeList returs a list of entity types. Calling GetDotNotation on each of these entity types allows you create a list of valid inputs to the Entity.AddPropertyValue method. </remarks>
Public Class EntityType
#Region "Properties"


    Private _name As String
    Public Property Name() As String
        Get
            Return _name
        End Get
        Set(ByVal value As String)
            _name = value
        End Set
    End Property

    Private _id As String
    Public Property ID() As String
        Get
            Return _id
        End Get
        Set(ByVal value As String)
            _id = value
        End Set
    End Property

    Private _field_type As String
    Public Property Field_Type() As String
        Get
            Return _field_type
        End Get
        Set(ByVal value As String)
            _field_type = value
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

    Private _relationship_types As List(Of RelationshipType)
    Public Property RelationshipTypes() As List(Of RelationshipType)
        Get
            Return _relationship_types
        End Get
        Set(ByVal value As List(Of RelationshipType))
            _relationship_types = value
        End Set
    End Property

    Private _measurement_types As List(Of MeasurementType)
    Public Property MeasurementTypes() As List(Of MeasurementType)
        Get
            Return _measurement_types
        End Get
        Set(ByVal value As List(Of MeasurementType))
            _measurement_types = value
        End Set
    End Property


    Public Function IsCollection() As Boolean
        Return _children.Count > 0

    End Function

#End Region

#Region "Constructor"


    ''' <summary>
    ''' Creates a new EntityType
    ''' </summary>
    ''' <param name="Type_Name"></param>
    ''' <param name="Type_Id"></param>
    ''' <param name="Type_Parent"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal Type_Name As String, ByVal Type_Id As String, Optional Type_Parent As EntityType = Nothing)
        _name = Type_Name
        _id = Type_Id
        If Not Type_Parent Is Nothing Then
            _parent = Type_Parent
            _parent.Children.Add(Me)
        End If
        '  Console.Write(GetDotNotation() & vbNewLine)
    End Sub
#End Region


#Region "Public Methods"



    ''' <summary>
    ''' Recursive function to gather a flat list of all enitities
    ''' </summary>
    ''' <param name="FlatList">The list to which all entity_types will be added and returned</param>
    ''' <param name="type">Enter Nothing/Null for leaves only. "All" for everything. Or filter by FieldType (see field type class)</param>
    ''' <remarks></remarks>
    Public Sub GetDecendents(ByRef FlatList As List(Of EntityType), ByVal type As String)
        If IsCollection() Then
            If type = FieldType._entity Or type = "All" Then
                FlatList.Add(Me)
            End If
            For Each child In Children
                child.GetDecendents(FlatList, type)
            Next
        Else
            If type Is Nothing Then
                FlatList.Add(Me)
            ElseIf type = _field_type Or type = "All" Then
                FlatList.Add(Me)
            End If

        End If

    End Sub

    ''' <summary>
    ''' Checks if this is a root entity
    ''' </summary>
    ''' <returns>True/False</returns>
    ''' <remarks>Returns true if this EntityType is a RootEntity type (ie has no parent)</remarks>
    Public Function IsRoot() As Boolean
        Return _parent Is Nothing
    End Function

    ''' <summary>
    ''' Returns the name of this entity type in Dot Notation
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>eg person.address.city</remarks>
    Public Function GetDotNotation() As String
        Dim rtn As String = _name

        If IsRoot() Then
            Return rtn
        Else
            rtn = _parent.GetDotNotation & "." & rtn
        End If



        Return rtn

    End Function

#End Region
End Class
