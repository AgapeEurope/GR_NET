Public Class MeasurementType
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

    Private _description As String
    Public Property Description() As String
        Get
            Return _description
        End Get
        Set(ByVal value As String)
            _description = value
        End Set
    End Property

    Private _frequency As String
    Public Property Frequency() As String
        Get
            Return _frequency
        End Get
        Set(ByVal value As String)
            _frequency = value
        End Set
    End Property

    Private _category As String
    Public Property Category() As String
        Get
            Return _category
        End Get
        Set(ByVal value As String)
            _category = value
        End Set
    End Property

    Private _relatedEntityTypeId As String
    Public Property RelatedEntityTypeId() As String
        Get
            Return _relatedEntityTypeId
        End Get
        Set(ByVal value As String)
            _relatedEntityTypeId = value
        End Set
    End Property

    Public measurements As New List(Of Measurement)


End Class
