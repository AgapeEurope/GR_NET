Public Class Subscription
    Private _id As String
    Public Property ID() As String
        Get
            Return _id
        End Get
        Set(ByVal value As String)
            _id = value
        End Set
    End Property

    Private _systemId As String
    Public Property SystemId() As String
        Get
            Return _systemId
        End Get
        Set(ByVal value As String)
            _systemId = value
        End Set
    End Property

    Private _enitity_type_id As String
    Public Property EntityTypeId() As String
        Get
            Return _enitity_type_id
        End Get
        Set(ByVal value As String)
            _enitity_type_id = value
        End Set
    End Property

    Private _endpoint As String
    Public Property EndPoint() As String
        Get
            Return _endpoint
        End Get
        Set(ByVal value As String)
            _endpoint = value
        End Set
    End Property
    Private _entity_type_name As String
    Public Property EntityTypeName() As String
        Get
            Return _entity_type_name
        End Get
        Set(ByVal value As String)
            _entity_type_name = value
        End Set
    End Property
    Private _confirmed As Boolean
    Public Property Confirmed() As Boolean
        Get
            Return _confirmed
        End Get
        Set(ByVal value As Boolean)
            _confirmed = value
        End Set
    End Property
    Private _format As String
    Public Property Format() As String
        Get
            Return _format
        End Get
        Set(ByVal value As String)
            _format = value
        End Set
    End Property



    


End Class
