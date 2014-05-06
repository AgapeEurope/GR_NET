Public Class RelationshipType
    Private _id As String
    Public Property ID() As String
        Get
            Return _id
        End Get
        Set(ByVal value As String)
            _id = value
        End Set
    End Property
    Private _relationship1 As String
    Public Property Relationship1() As String
        Get
            Return _relationship1
        End Get
        Set(ByVal value As String)
            _relationship1 = value
        End Set
    End Property
    Private _relationship2 As String
    Public Property Relationship2() As String
        Get
            Return _relationship2
        End Get
        Set(ByVal value As String)
            _relationship2 = value
        End Set
    End Property

    Private _entity_type1 As EntityType
    Public Property EntityType1() As EntityType
        Get
            Return _entity_type1
        End Get
        Set(ByVal value As EntityType)
            _entity_type1 = value
        End Set
    End Property

    Private _entity_type2 As EntityType
    Public Property EntityType2() As EntityType
        Get
            Return _entity_type2
        End Get
        Set(ByVal value As EntityType)
            _entity_type2 = value
        End Set
    End Property

    


End Class
