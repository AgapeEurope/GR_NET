﻿Public Class Measurement
    Private _period As String
    Public Property Period() As String
        Get
            Return _period
        End Get
        Set(ByVal value As String)
            _period = value
        End Set
    End Property

    Private _value As Double
    Public Property Value() As Double
        Get
            Return _value
        End Get
        Set(ByVal value As Double)
            _value = value
        End Set
    End Property

    Private _relatedEnityId As String
    Public Property RelatedEntityId() As String
        Get
            Return _relatedEnityId
        End Get
        Set(ByVal value As String)
            _relatedEnityId = value
        End Set
    End Property


End Class
