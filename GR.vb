﻿

Imports System.Net


''' <summary>
''' The main object in the GR_NET Libarary
''' </summary>
''' <remarks>
''' To get started with the GR_NET library create an instance of this class - using the constructor. This class allows you to 
''' </remarks>
Public Class GR


#Region "Properties"


    Private _apikey As String
    Public Property ApiKey() As String
        Get
            Return _apikey
        End Get
        Set(ByVal value As String)
            _apikey = value
        End Set
    End Property

    Private _grUrl = ""

    Private _entity_types_def As New List(Of EntityType)
    Public Property entity_types_def() As List(Of EntityType)
        Get
            Return _entity_types_def
        End Get
        Set(ByVal value As List(Of EntityType))
            _entity_types_def = value
        End Set
    End Property

    Public People As New People()



#End Region


#Region "Constructors"
    ''' <summary>
    ''' The constructor initialises the GR object. 
    ''' </summary>
    ''' <param name="apiKey">Your GR Auth Key</param>
    ''' <param name="gr_url">The URL of the GR server</param>
    ''' <remarks>The test GR server is used by default.</remarks>
    Public Sub New(ByVal apiKey As String, Optional gr_url As String = "https://gr.stage.uscm.org/")
        If Not apiKey = Nothing Then
            _apikey = apiKey
        End If
        _grUrl = gr_url

        GetEntityTypeDefFromGR()

    End Sub
#End Region



#Region "Public Methods - Entities"

    ''' <summary>
    ''' Update all People stored on this object
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SyncPeople()
        For Each person In People.people_list
            CreateEntity(person, "person")
        Next

    End Sub

    Public Function CreateEntity(ByRef p As Entity, ByVal EntityName As String) As Integer
        Dim postData = "{""entity"": {""" & EntityName & """:" & p.ToJson & "}}"
        Console.Write(postData & vbNewLine)
        Dim rest = _grUrl & "entities?access_token=" & _apikey.ToString



        Dim success = False
        Dim count = 0
        Dim response As HttpWebResponse
        While Not success


            Try
                Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)

                request.Method = "POST"

                Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
                request.ContentLength = bytes.Length
                request.ContentType = "application/json"
                Dim requestStream = request.GetRequestStream()
                requestStream.Write(bytes, 0, bytes.Length)

                response = DirectCast(request.GetResponse(), HttpWebResponse)

                success = True
            Catch ex As WebException
                Select Case CType(ex.Response, HttpWebResponse).StatusCode.ToString
                    Case "500"
                        count += 1
                        If count > 5 Then
                            Throw New Exception()
                            success = True
                            Threading.Thread.Sleep(10000)
                        End If

                    Case "400"
                        'Bad request
                        Throw ex
                    Case "401"
                        'Bad API key
                        Throw ex
                    Case "404"
                        'not found
                        Throw ex
                    Case "304"
                        'not Modified
                        Throw ex
                    Case "301"
                        'duplicate... try here

                    Case "201"
                        'Create

                    Case "200"
                        'OK
                    Case 304
                    Case 301
                    Case 201
                    Case 200


                End Select

            End Try
        End While

        Dim reader As New IO.StreamReader(response.GetResponseStream())
        Dim json = reader.ReadToEnd()
        Dim newEntity = CreateEntityFromJsonResp(json)
        Return newEntity.ID

        ' Console.Write(json & vbNewLine & vbNewLine)

        'ID's need to be written back to Entity structure

    End Function

    Public Function UpdateEntity(ByRef p As Entity, ByVal EntityName As String) As Integer
        Dim postData = "{""entity"": {""" & EntityName & """:" & p.ToJson & "}}"
        Console.Write(postData & vbNewLine)
        Dim rest = _grUrl & "entities/" & p.ID & "/?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)

        request.Method = "PUT"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        Dim requestStream = request.GetRequestStream()
        requestStream.Write(bytes, 0, bytes.Length)



        Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)

        Dim reader As New IO.StreamReader(response.GetResponseStream())
        Dim json = reader.ReadToEnd()
        Dim newEntity = CreateEntityFromJsonResp(json)
        Return newEntity.ID

        'ID's need to be writted back to entity structure
    End Function

    Public Sub DeleteEntity(ByVal ID As Integer)
        Dim rest = _grUrl & "entities/" & ID & "?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)

        request.Method = "DELETE"
        Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)

        Dim reader As New IO.StreamReader(response.GetResponseStream())
        Dim json = reader.ReadToEnd()
        Console.Write(json & vbNewLine & vbNewLine)
    End Sub

    Public Function GetEntity(ByVal ID As Integer) As Entity
        Dim web As New WebClient()
        Dim json = web.DownloadString(_grUrl & "entities/" & ID & "?access_token=" & _apikey.ToString)


        ' Return New Entity(json)
        Return CreateEntityFromJsonResp(json)
    End Function
    Public Function GetEntities(ByVal EntityType As String, ByVal Filters As String) As List(Of Entity)
        Dim web As New WebClient()
       
        Dim json = web.DownloadString(_grUrl & "entities?access_token=" & _apikey.ToString & "&entity_type=" & EntityType & Filters)
        Dim rtn As New List(Of Entity)

        Return CreateEntitiesFromJsonResp(json)


    End Function

    ''' <summary>
    ''' Update an enitity (or entity tree) on the on GR server
    ''' </summary>
    ''' <param name="p">The entity to update (or entity tree).</param>
    ''' <remarks>Only one root entity permitted. You must have a supplied a client_integration_id</remarks>
    'Public Sub SyncPerson(ByVal p As Entity)
    '    Dim postData = "{""entity"": {""person"":" & p.ToJson & "}}"
    '    Console.Write(postData & vbNewLine)
    '    Dim rest = _grUrl & "entities?access_token=" & _apikey.ToString
    '    Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)

    '    request.Method = "POST"

    '    Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
    '    request.ContentLength = bytes.Length
    '    request.ContentType = "application/json"
    '    Dim requestStream = request.GetRequestStream()
    '    requestStream.Write(bytes, 0, bytes.Length)



    '    Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)

    '    Dim reader As New IO.StreamReader(response.GetResponseStream())
    '    Dim json = reader.ReadToEnd()
    '    Console.Write(json & vbNewLine & vbNewLine)
    '    Dim test = CreateEntityFromJsonResp(json)
    'End Sub


#End Region
#Region "Public Methods - EntityTypes"
    ''' <summary>
    ''' Returns a flat list of all entity types
    ''' </summary>
    ''' <param name="rootEntity">The root entity in dot notation (usually person but could be somethine like person.address)</param>
    ''' <param name="type">Leave blank to return leaves only. To return everything enter "All". Otherswise only entities of the specified Type are retured (as listed in the FieldTypes class)</param>
    ''' <returns>A Flat list of all EntityTypes</returns>
    ''' <remarks>Note Each entity type has a GetDotNotation function - which allows you to populate a list of enitities in DotNotation(eg person.address.city) </remarks>
    Public Function GetFlatEntityLeafList(ByVal rootEntity As String, Optional type As String = Nothing) As List(Of EntityType)
        Dim root = _entity_types_def.Where(Function(c) c.Name = rootEntity).First
        Dim FlatList As New List(Of EntityType)
        root.GetDecendents(FlatList, type)
        Return FlatList

    End Function


    ''' <summary>
    ''' Creates a new entity type in GR
    ''' </summary>
    ''' <param name="entityName">The name of the new entity</param>
    ''' <param name="type">The field type (as listed in FieldType class)</param>
    ''' <param name="parentEntityType">Parent Entity in DotNotation (eg person.address)</param>
    ''' <remarks>The ParentEntity must exist. CAUTION - there is currently no way to delete a created entity.</remarks>
    Public Sub addNewEntityType(ByVal entityName As String, ByVal type As String, ByVal parentEntityType As String)
        'entity name is dot-notated
        If String.IsNullOrEmpty(parentEntityType) And type = FieldType._entity Then
            CreateEntityType(entityName, Nothing, type)
        ElseIf Not String.IsNullOrEmpty(parentEntityType) Then

            Dim rootName As String = parentEntityType
            If parentEntityType.Contains(".") Then
                rootName = parentEntityType.Substring(0, parentEntityType.IndexOf("."))
            End If
            Dim FlatList = GetFlatEntityLeafList(rootName, "All")
            Dim parent = FlatList.Where(Function(c) c.GetDotNotation() = parentEntityType)

            If parent.Count > 0 Then
                If parent.First.Children.Where(Function(c) c.Name = entityName).Count = 0 Then
                    CreateEntityType(entityName, parent.First.ID, type)
                End If

            End If
        End If



    End Sub
#End Region


#Region "Private Methods - API calls"
    Private Shared Function TrustAllCertificateCallback(ByVal sender As Object, ByVal certificate As System.Security.Cryptography.X509Certificates.X509Certificate, ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, ByVal sslPolicyErrors As Security.SslPolicyErrors) As Boolean
        Return True
    End Function
    Private Function ApiCall(ByVal method As String, ByVal filter As String) As String
        ServicePointManager.ServerCertificateValidationCallback = AddressOf TrustAllCertificateCallback
        Dim mycache As CredentialCache = New CredentialCache()

        Dim web As New WebClient()
        web.Credentials = mycache
        Dim rest = _grUrl & method & "?access_token=" & _apikey.ToString & "&" & method & "&" & filter
        Return web.DownloadString(rest)

    End Function

    Private Sub CreateEntityType(ByVal Name As String, ByVal ParentId As Integer, ByVal type As String)

        Dim postData = "{""entity_type"": {""name"":""" & Name & """, ""field_type"":""" & type & """,""parent_id"":""" & IIf(ParentId = Nothing, "null", ParentId) & """}}"

        Dim rest = _grUrl & "entity_types?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        ' request.CookieContainer = myCookieContainer
        request.Method = "POST"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        Dim requestStream = request.GetRequestStream()
        requestStream.Write(bytes, 0, bytes.Length)



        Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)

        Dim reader As New IO.StreamReader(response.GetResponseStream())
        Dim json = reader.ReadToEnd()

        'refresh the local entityType model
        GetEntityTypeDefFromGR()
    End Sub


    Private Sub GetEntityTypeDefFromGR()
        'Make REST CAll
        ServicePointManager.ServerCertificateValidationCallback = AddressOf TrustAllCertificateCallback
        Dim mycache As CredentialCache = New CredentialCache()

        Dim web As New WebClient()
        web.Credentials = mycache

        Dim json = web.DownloadString(_grUrl & "entity_types?access_token=" & _apikey.ToString)

        Dim jss = New Web.Script.Serialization.JavaScriptSerializer()
        Dim allEntityTypes = jss.Deserialize(Of Dictionary(Of String, List(Of Dictionary(Of String, Object))))(json)
        _entity_types_def = New List(Of EntityType)

        For Each row In allEntityTypes("entity_types")
            addSubEntityTypes(row, Nothing)
        Next


    End Sub

    ''' <summary>
    ''' Recursive subroutine to parse the JSON response for entity_type definition
    ''' </summary>
    ''' <param name="input"></param>
    ''' <param name="Parent"></param>
    ''' <remarks></remarks>
    Private Sub addSubEntityTypes(ByVal input As Dictionary(Of String, Object), ByRef Parent As EntityType)
        Dim insert As New EntityType(input("name"), input("id"), Parent)
        If input.ContainsKey("fields") Then
            For Each row As Dictionary(Of String, Object) In input("fields")

                addSubEntityTypes(row, insert)
            Next

        End If
        If Parent Is Nothing Then
            _entity_types_def.Add(insert)

        End If
    End Sub


    Public Function CreateEntityFromJsonResp(Optional ByVal json As String = Nothing) As Entity
        Dim rtn As New Entity()
        If Not String.IsNullOrEmpty(json) Then
            'prepopulate from json...
            Dim jss = New Web.Script.Serialization.JavaScriptSerializer()
            '  Dim ent_resp = jss.Deserialize(Of Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Object))))(json)
            Dim ent_resp = jss.Deserialize(Of Dictionary(Of String, Dictionary(Of String, Object)))(json)


            Dim person_dict As New Dictionary(Of String, String)

            ProcessJsonEntity(ent_resp.Values.First.Values.First, "", person_dict)
            For Each row In person_dict
                rtn.AddPropertyValue(row.Key, row.Value)
            Next

        End If
        Return rtn
    End Function

    Public Function CreateEntitiesFromJsonResp(Optional ByVal json As String = Nothing) As List(Of Entity)
        Dim rtn As New List(Of Entity)
        If Not String.IsNullOrEmpty(json) Then
            'prepopulate from json...
            Dim jss = New Web.Script.Serialization.JavaScriptSerializer()
            '  Dim ent_resp = jss.Deserialize(Of Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Object))))(json)
            Dim ent_resp = jss.Deserialize(Of Dictionary(Of String, List(Of Dictionary(Of String, Object))))(json)



            For Each row In ent_resp.Values.First
                Dim person_dict As New Dictionary(Of String, String)
                Dim ent As New Entity

                ProcessJsonEntity(row.Values.First, "", person_dict)
                For Each row2 In person_dict
                    ent.AddPropertyValue(row2.Key, row2.Value)
                Next
                rtn.Add(ent)
            Next



        End If
        Return rtn
    End Function


    Private Sub ProcessJsonEntity(ByVal input As Object, ByRef dot As String, ByRef person_dict As Dictionary(Of String, String))
        If dot <> "" Then
            dot &= "."
        End If

        Dim t As String = input.GetType.Name
        If t.Contains("Dictionary") Then
            For Each row2 In CType(input, Dictionary(Of String, Object))
                ProcessJsonEntity(row2.Value, dot & row2.Key, person_dict)
            Next


        ElseIf t.Contains("List") Then
            Dim i As Integer = 0
            For Each row2 In CType(input.Value, List(Of Object))

                ProcessJsonEntity(row2.Values, dot & "[" & i & "]", person_dict)
                i += 1
            Next





        Else
            person_dict.Add(dot.TrimEnd("."), input)
        End If



    End Sub


#End Region

































End Class

