﻿Public Class MeasurementType
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
    Private _unit As String
    Public Property Unit() As String
        Get
            Return _unit
        End Get
        Set(ByVal value As String)
            _unit = value
        End Set
    End Property

    Public measurements As New List(Of Measurement)
    Public Sub addMeasurement(ByVal RelatedEntityId As Integer, ByVal Period As String, ByVal Value As Double)
        Dim existing = measurements.Where(Function(c As Measurement) c.Period = Period And c.RelatedEntityId = RelatedEntityId)
        If existing.Count = 0 Then
            Dim m As New Measurement()
            m.Period = Period
            m.Value = Value
            m.RelatedEntityId = RelatedEntityId
            measurements.Add(m)

        End If

    End Sub


    Public Function MeasurementsToJson() As String
        Dim rtn = "{""measurements"": ["

        For Each row In measurements
            rtn &= "{""measurement_type_id"":" & ID & "," _
                & """related_entity_id"":" & row.RelatedEntityId & "," _
         & """period"": """ & row.Period & """," _
         & """value"": """ & row.Value & """},"
        Next
        rtn = rtn.TrimEnd(",")
        rtn &= "]}"
        Return rtn
    End Function
    Public Function ToJson() As String
        Dim rtn = "{""measurement_type"": {"
        AddTag(rtn, "name", Name)
        AddTag(rtn, "description", Description)
        AddTag(rtn, "frequency", Frequency)
        AddTag(rtn, "category", Category)
        AddTag(rtn, "relationhip_type_id", RelatedEntityTypeId, False)

        rtn = rtn.TrimEnd(",")


        rtn &= "}}"
        Return rtn
    End Function

    Private Sub AddTag(ByRef rtn As String, ByVal name As String, ByVal value As String, Optional ByVal isString As Boolean = True)
        If Not String.IsNullOrEmpty(value) Then
            rtn &= """" & name & """ : """ & value & ""","

        End If

    End Sub
End Class