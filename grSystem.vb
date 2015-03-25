Public Class grSystem
    Private _id As String
    Public Property ID() As String
        Get
            Return _id
        End Get
        Set(ByVal value As String)
            _id = value
        End Set
    End Property

    Private _name As String
    Public Property Name() As String
        Get
            Return _name
        End Get
        Set(ByVal value As String)
            _name = value
        End Set
    End Property

    Private _accessToken As String
    Public Property AccessToken() As String
        Get
            Return _accessToken
        End Get
        Set(ByVal value As String)
            _accessToken = value
        End Set
    End Property

    Private _isRoot As Boolean
    Public Property IsRoot() As Boolean
        Get
            Return _isRoot
        End Get
        Set(ByVal value As Boolean)
            _isRoot = value
        End Set
    End Property

    Private _trustedSystems As String
    Public Property TrustedSystems() As String
        Get
            Return _trustedSystems
        End Get
        Set(ByVal value As String)
            _trustedSystems = value
        End Set
    End Property
    Private _trustedIps As List(Of String)
    Public Property TrustedIps() As List(Of String)
        Get
            Return _trustedIps
        End Get
        Set(ByVal value As List(Of String))
            _trustedIps = value
        End Set
    End Property
    Private _description As String
    Public Property Description() As String
        Get
            Return _description
        End Get
        Set(ByVal value As String)
            _description = value
        End Set
    End Property

End Class
