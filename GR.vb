

Imports System.Net
Imports System.Text

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

        _grUrl = gr_url.TrimEnd("/") & "/"  'ensure url ends in '/'

        GetEntityTypeDefFromGR()

    End Sub
#End Region

#Region "Public Methods - Measurements"
    Public Function GetMeasurements(ByVal RelatedEntityId As String, ByVal PeriodFrom As String, ByVal PeriodTo As String, Optional ByVal MeasurementTypeId As String = "", Optional Category As String = "", Optional DefinitionOnly As Boolean = False) As List(Of MeasurementType)

        Dim web As New WebClient()
        web.Encoding = Encoding.UTF8
        Dim extras As String = "&filters[period_from]=" & PeriodFrom & "&filters[period_to]=" & PeriodTo & "&filters[related_entity_id]=" & RelatedEntityId
        If Category <> "" Then
            extras &= "&filters[category]=" & Category
        End If
        If DefinitionOnly Then
            extras = "&filters[related_entity_type_id]=" & RelatedEntityId

        End If
        Dim typeString = ""
        If MeasurementTypeId = "" Then
            typeString = "measurement_types/" & MeasurementTypeId
        End If
        Dim json = web.DownloadString(_grUrl & typeString & "?access_token=" & _apikey.ToString & extras)

        Dim jss = New Web.Script.Serialization.JavaScriptSerializer()
        Dim ent = jss.Deserialize(Of Dictionary(Of String, Object))(json)



        Dim rtn As New List(Of MeasurementType)


        If ent.ContainsKey("measurement_type") Then
            Dim insert As New MeasurementType
            insert.ID = ent("measurement_type")("id")
            insert.Name = ent("measurement_type")("name")
            insert.Description = ent("measurement_type")("description")
            insert.Category = ent("measurement_type")("category")
            insert.Frequency = ent("measurement_type")("frequency")
            insert.RelatedEntityTypeId = ent("measurement_type")("related_entity_type_id")
            For Each row In ent("measurement_type")("measurements")
                Dim insertm As New Measurement
                insertm.Period = row("period")
                insertm.Value = row("value")
                insertm.RelatedEntityId = RelatedEntityId
                insert.measurements.Add(insertm)
            Next
            rtn.Add(insert)
        Else
            For Each row In ent("measurement_types")
                Dim insert As New MeasurementType
                insert.ID = row("id")
                insert.Name = row("name")
                insert.Description = row("description")
                insert.Category = row("category")
                insert.Frequency = row("frequency")
                insert.RelatedEntityTypeId = row("related_entity_type_id")
                For Each row2 In row("measurements")
                    Dim insertm As New Measurement
                    insertm.Period = row2("period")
                    insertm.Value = row2("value")
                    insertm.RelatedEntityId = RelatedEntityId
                    insert.measurements.Add(insertm)
                Next
                rtn.Add(insert)
            Next


        End If


        Return rtn
    End Function


    Private Sub AddMeasurementBatch(ByVal mt As MeasurementType, Optional ByVal Page As Integer = 0)

        Dim postData = mt.MeasurementsToJson(Page)

        Dim rest = _grUrl & "measurements?access_token=" & _apikey.ToString


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
                Using requestStream = request.GetRequestStream()


                    requestStream.Write(bytes, 0, bytes.Length)

                    response = DirectCast(request.GetResponse(), HttpWebResponse)
                End Using
                success = True
                request.Abort()
                request = Nothing
            Catch ex As WebException
                count += 1

                Select Case CType(ex.Response, HttpWebResponse).StatusCode.ToString
                    Case "500"
                        count += 1
                        Trace.TraceWarning("500 error from create entity: attempt " & count)
                        If count > 5 Then
                            Throw ex
                            success = True

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
                        Trace.TraceWarning("Entity has been merged with duplicate, please use this new ID ID")
                    Case "201"
                        'Create

                    Case "200"
                        'OK



                End Select

            End Try
        End While
    End Sub

    Public Sub AddUpdateMeasurement(ByVal mt As MeasurementType)
        If mt.measurements.Count <= 250 Then
            AddMeasurementBatch(mt)
        Else
            For i As Integer = 0 To CInt(Math.Truncate(mt.measurements.Count / 250))
                Console.WriteLine("Batch " & i & " of " & CInt(Math.Truncate(mt.measurements.Count / 250)))
                AddMeasurementBatch(mt, i)
            Next
        End If


    End Sub

#End Region

#Region "Public Methods - MeasurementTypes"





    Public Sub AddMeasurementType(ByVal mt As MeasurementType)
        Dim postData = mt.ToJson()

        Dim rest = _grUrl & "measurement_types?access_token=" & _apikey.ToString


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
                Using requestStream = request.GetRequestStream()


                    requestStream.Write(bytes, 0, bytes.Length)

                    response = DirectCast(request.GetResponse(), HttpWebResponse)

                    success = True
                End Using
            Catch ex As WebException
                count += 1
                Select Case CType(ex.Response, HttpWebResponse).StatusCode.ToString
                    Case "500"
                        count += 1
                        Trace.TraceWarning("500 error from create entity: attempt " & count)
                        If count > 5 Then
                            Throw ex
                            success = True

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
                        Trace.TraceWarning("Entity has been merged with duplicate, please use this new ID ID")
                    Case "201"
                        'Create

                    Case "200"
                        'OK



                End Select

            End Try
        End While

        'Dim reader As New IO.StreamReader(response.GetResponseStream())
        'Dim json = reader.ReadToEnd()
        '  Dim newEntity = CreateEntityFromJsonResp(json)
        ' Return newEntity.ID

    End Sub


    Public Sub UpdateMeasurementType(ByVal mt As MeasurementType)
        Dim postData = mt.ToJson()

        Dim rest = _grUrl & "measurement_types/" & mt.ID & "?access_token=" & _apikey.ToString


        Dim success = False
        Dim count = 0
        Dim response As HttpWebResponse
        While Not success


            Try
                Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)

                request.Method = "PUT"

                Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
                request.ContentLength = bytes.Length
                request.ContentType = "application/json"
                Using requestStream = request.GetRequestStream()


                    requestStream.Write(bytes, 0, bytes.Length)

                    response = DirectCast(request.GetResponse(), HttpWebResponse)

                    success = True
                End Using
            Catch ex As WebException
                count += 1
                Select Case CType(ex.Response, HttpWebResponse).StatusCode.ToString
                    Case "500"
                        count += 1
                        Trace.TraceWarning("500 error from create entity: attempt " & count)
                        If count > 5 Then
                            Throw ex
                            success = True

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
                        Trace.TraceWarning("Entity has been merged with duplicate, please use this new ID ID")
                    Case "201"
                        'Create

                    Case "200"
                        'OK



                End Select

            End Try
        End While

        'Dim reader As New IO.StreamReader(response.GetResponseStream())
        'Dim json = reader.ReadToEnd()
        '  Dim newEntity = CreateEntityFromJsonResp(json)
        ' Return newEntity.ID

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

    Public Function CreateEntity(ByRef p As Entity, ByVal EntityName As String) As String
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
                Using requestStream = request.GetRequestStream()


                    requestStream.Write(bytes, 0, bytes.Length)

                    response = DirectCast(request.GetResponse(), HttpWebResponse)

                    success = True
                End Using
            Catch ex As WebException
                Select Case CType(ex.Response, HttpWebResponse).StatusCode.ToString
                    Case "500"

                        Trace.TraceWarning("500 error from create entity: attempt " & count)
                        count += 1
                        If count > 5 Then
                            success = True
                            Throw ex

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
                        Trace.TraceWarning("Entity has been merged with duplicate, please use this new ID ")


                        success = True

                    Case "201"
                        'Create
                        success = True
                    Case "200"
                        'OK
                    Case Else

                        count += 1
                        If count > 5 Then
                            success = True
                            'Throw ex
                            Console.Write("ERROR - gave up after 5 attempts!!!" & vbNewLine)
                            Return Nothing
                        End If

                End Select

            End Try
        End While

        Using reader As New IO.StreamReader(response.GetResponseStream())
            Dim json = reader.ReadToEnd()
            Dim newEntity = CreateEntityFromJsonResp(json)
            Return newEntity.ID
        End Using
      


    End Function

    Public Function UpdateEntity(ByRef p As Entity, ByVal EntityName As String) As String
        If Not p.HasValues Then
            Return p.ID
        End If
        Dim postData = "{""entity"": {""" & EntityName & """:" & p.ToJson & "}}"
        Console.Write(postData & vbNewLine)
        Trace.WriteLine("to_update:" & postData)
        Dim rest = _grUrl & "entities/" & p.ID & "/?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)

        request.Method = "PUT"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        Using requestStream = request.GetRequestStream()
            requestStream.Write(bytes, 0, bytes.Length)
            Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)
            Using reader As New IO.StreamReader(response.GetResponseStream())
                Dim json = reader.ReadToEnd()
                Trace.WriteLine("from-update:" & json)
                Dim newEntity = CreateEntityFromJsonResp(json)
                Return newEntity.ID
            End Using
        End Using
       




      


        'ID's need to be writted back to entity structure
    End Function

    Public Sub DeleteEntity(ByVal ID As String)
        Dim rest = _grUrl & "entities/" & ID & "?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)

        request.Method = "DELETE"
        Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)

        Using reader As New IO.StreamReader(response.GetResponseStream())
            Dim json = reader.ReadToEnd()
            Console.Write(json & vbNewLine & vbNewLine)
        End Using
       
    End Sub

    Public Function GetEntity(ByVal ID As String, Optional ByVal AllSystems As Boolean = False) As Entity
        Dim web As New WebClient()
        web.Encoding = Encoding.UTF8
        Dim extras As String = ""
        If AllSystems Then
            extras = "&created_by=all"
        End If
        Dim json = web.DownloadString(_grUrl & "entities/" & ID & "?access_token=" & _apikey.ToString & extras)


        ' Return New Entity(json)
        Return CreateEntityFromJsonResp(json)
    End Function
    Public Function GetEntities(ByVal EntityType As String, ByVal Filters As String, Optional ByVal Page As Integer = 0, Optional ByVal PerPage As Integer = 0, Optional ByRef TotalPage As Integer = 0) As List(Of Entity)
        Dim web As New WebClient()
        web.Encoding = Encoding.UTF8

        Dim json = web.DownloadString(_grUrl & "entities?access_token=" & _apikey.ToString & "&entity_type=" & EntityType & Filters & CreatePageString(Page, PerPage))
        TotalPage = GetTotalPagesFromJson(json)
        Dim rtn As New List(Of Entity)

        Return CreateEntitiesFromJsonResp(json)


    End Function
    Public Sub addNewRelationshipType(ByVal entity_type1 As String, ByVal entity_type2 As String, ByVal relationship1 As String, ByVal relationship2 As String)
        Dim postData = "{""relationship_type"": {""entity_type1_id"":""" & entity_type1 & """, ""entity_type2_id"":""" & entity_type2 & """,""relationship1"":""" & relationship1 & """,""relationship2"":""" & relationship2 & """ }}"

        Dim rest = _grUrl & "relationship_types?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        ' request.CookieContainer = myCookieContainer
        request.Method = "POST"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        Using requestStream = request.GetRequestStream()


            requestStream.Write(bytes, 0, bytes.Length)



            Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)

            ' Dim reader As New IO.StreamReader(response.GetResponseStream())
            'Dim json = reader.ReadToEnd()

            'refresh the local entityType model
            'GetEntityTypeDefFromGR()
        End Using

    End Sub


    Public Sub editRelationshipType(ByVal relationship_id As String, ByVal entity_type1 As String, ByVal entity_type2 As String, ByVal relationship1 As String, ByVal relationship2 As String)
        Dim postData = "{""relationship_type"": {""entity_type1_id"":""" & entity_type1 & """, ""entity_type2_id"":""" & entity_type2 & """,""relationship1"":""" & relationship1 & """,""relationship2"":""" & relationship2 & """ }}"

        Dim rest = _grUrl & "relationship_types/" & relationship_id & "?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        ' request.CookieContainer = myCookieContainer
        request.Method = "PUT"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        Using requestStream = request.GetRequestStream()


            requestStream.Write(bytes, 0, bytes.Length)



            Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)

            ' Dim reader As New IO.StreamReader(response.GetResponseStream())
            'Dim json = reader.ReadToEnd()

            'refresh the local entityType model
            'GetEntityTypeDefFromGR()
        End Using

    End Sub

    Public Function RelateEntity(ByVal EntityType1 As String, ByVal Id1 As String, ByVal EntityType2 As String, ByVal Id2 As String, ByVal RelationshipType As String, Optional Role As String = "") As Entity
        Dim r As String = ""
        If Not Role = "" Then
            r = ",""role"": """ & Role & """"
        End If

        Dim postData = "{""entity"":{""" & EntityType1 & """: {""" & RelationshipType & ":relationship"":{""" & EntityType2 & """: """ & Id2 & """" & r & "}}}}"
        Console.Write(postData & vbNewLine)
        Trace.WriteLine("to_update:" & postData)
        Dim rest = _grUrl & "entities/" & Id1 & "/?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)

        request.Method = "PUT"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        Using requestStream = request.GetRequestStream()


            requestStream.Write(bytes, 0, bytes.Length)



            Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)

            Using reader As New IO.StreamReader(response.GetResponseStream())


                Dim json = reader.ReadToEnd()
                Trace.WriteLine("from-update:" & json)
                Dim newEntity = CreateEntityFromJsonResp(json)
            End Using
        End Using
        Return New Entity
    End Function
    Public Function GetRelationshipsForEntityType(ByVal entity_type_id As String) As List(Of RelationshipType)
        Dim web As New WebClient()
        web.Encoding = Encoding.UTF8

        Dim json = web.DownloadString(_grUrl & "relationship_types?access_token=" & _apikey.ToString & "&filters[involving]=" & entity_type_id)
        Dim jss = New Web.Script.Serialization.JavaScriptSerializer()
        Dim rts = jss.Deserialize(Of Dictionary(Of String, List(Of Dictionary(Of String, Object))))(json)
        Dim rtn As New List(Of RelationshipType)
        If rts.ContainsKey("relationship_types") Then
            For Each row In rts("relationship_types")
                Dim insert As New RelationshipType
                insert.ID = row("id")
                insert.Relationship1 = row("relationship1")("relationship_name")
                insert.Relationship2 = row("relationship2")("relationship_name")
                insert.EntityType1 = entity_types_def.Where(Function(c) c.Name = row("relationship1")("entity_type")).FirstOrDefault
                insert.EntityType2 = entity_types_def.Where(Function(c) c.Name = row("relationship2")("entity_type")).FirstOrDefault


                rtn.Add(insert)



            Next
        End If



        Return rtn
    End Function


    Private Function GetTotalPagesFromJson(ByVal json As String) As Integer
        Try
            Dim jss = New Web.Script.Serialization.JavaScriptSerializer()
            Dim ent = jss.Deserialize(Of Dictionary(Of String, Object))(json)
            Return CInt(CType(ent("meta"), Dictionary(Of String, Object))("total_pages"))
        Catch ex As Exception
            Trace.TraceWarning("Could not process the ""meta"" response from get_entity to find total_pages")
            Return 1
        End Try


    End Function

    Private Function CreatePageString(ByVal Page As Integer, ByVal PerPage As Integer)
        Return IIf(Page = Nothing, IIf(PerPage = 0, "", "&per_page=" & PerPage), "&page=" & Page & IIf(PerPage = 0, "", "&per_page=" & PerPage))

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
        Dim FlatList As New List(Of EntityType)
        If String.IsNullOrEmpty(rootEntity) Then
            For Each row In _entity_types_def
                row.GetDecendents(FlatList, type)
            Next
        Else
            Dim root = _entity_types_def.Where(Function(c) c.Name = rootEntity).First

            root.GetDecendents(FlatList, type)
        End If

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
        web.Encoding = Encoding.UTF8
        web.Credentials = mycache
        Dim rest = _grUrl & method & "?access_token=" & _apikey.ToString & "&" & method & "&" & filter
        Return web.DownloadString(rest)

    End Function

    Public Sub CreateEntityType(ByVal Name As String, ByVal ParentId As String, ByVal type As String, Optional ByVal Description As String = "")

        Dim postData = "{""entity_type"": {""name"":""" & Name & """, ""field_type"":""" & type & """" & IIf(String.IsNullOrEmpty(ParentId) Or ParentId = "null", "", ",""parent_id"":""" & ParentId & """") & IIf(String.IsNullOrEmpty(Description), "", ",""description"":""" & Description & """") & "}}"

        Dim rest = _grUrl & "entity_types?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        ' request.CookieContainer = myCookieContainer
        request.Method = "POST"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        Using requestStream = request.GetRequestStream()


            requestStream.Write(bytes, 0, bytes.Length)



            Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)

            Using reader As New IO.StreamReader(response.GetResponseStream())
                Dim json = reader.ReadToEnd()

                'refresh the local entityType model

            End Using
        End Using
        GetEntityTypeDefFromGR()
    End Sub
    Public Sub UpdateEntityType(ByVal EntityTypeId As String, ByVal Name As String, ByVal ParentId As String, ByVal type As String, Optional ByVal Description As String = "")

        Dim postData = "{""entity_type"": {""name"":""" & Name & """, ""field_type"":""" & type & """" & IIf(String.IsNullOrEmpty(ParentId) Or ParentId = "null", "", ",""parent_id"":""" & ParentId & """") & IIf(String.IsNullOrEmpty(Description), "", ",""description"":""" & Description & """") & "}}"


        Dim rest = _grUrl & "entity_types/" & EntityTypeId & "?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        ' request.CookieContainer = myCookieContainer
        request.Method = "PUT"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        Using requestStream = request.GetRequestStream()


            requestStream.Write(bytes, 0, bytes.Length)



            Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)

            Using reader As New IO.StreamReader(response.GetResponseStream())
                Dim json = reader.ReadToEnd()
            End Using

        End Using
        'refresh the local entityType model
        GetEntityTypeDefFromGR()
    End Sub

    Private Sub GetEntityTypeDefFromGR()
        'Make REST CAll
        ServicePointManager.ServerCertificateValidationCallback = AddressOf TrustAllCertificateCallback
        Dim mycache As CredentialCache = New CredentialCache()

        Dim web As New WebClient()
        web.Encoding = Encoding.UTF8
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
        If input.ContainsKey("field_type") Then
            insert.Field_Type = input("field_type")
        End If
        If input.ContainsKey("description") Then
            insert.Description = input("description")
        End If
        If input.ContainsKey("enum_values") Then

            insert.EnumValues = CType(input("enum_values"), ArrayList).ToArray(GetType(String))


        End If

        If input.ContainsKey("fields") Then
            For Each row As Dictionary(Of String, Object) In input("fields")

                addSubEntityTypes(row, insert)
            Next

        End If
        If Parent Is Nothing Then
            _entity_types_def.Add(insert)

        End If
    End Sub

    Public Function GetEntityType(ByVal ID As String) As EntityType
        Dim rtn As EntityType
        For Each row In entity_types_def

            searchForEntityType(ID, row, rtn)
            If Not rtn Is Nothing Then
                Return rtn
            End If
        Next
        Return rtn
    End Function

    Private Sub searchForEntityType(ByVal Id As String, ByVal et As EntityType, ByRef rtn As EntityType)
        If et.ID = Id Then
            rtn = et
            Return
        Else
            For Each child In et.Children
                searchForEntityType(Id, child, rtn)
            Next


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
            For Each row2 In CType(input, ArrayList)
                Dim t2 As String = row2.GetType().Name
                If (t2 = "String") Then
                    person_dict.Add(dot.TrimEnd(".") & "[" & i & "]", row2)
                ElseIf t2.Contains("Dictionary") Then
                    For Each row3 In CType(row2, Dictionary(Of String, Object))
                        ProcessJsonEntity(row3.Value, dot.TrimEnd(".") & "[" & i & "]." & row3.Key, person_dict)
                    Next
                Else


                    ProcessJsonEntity(row2.Value, dot.TrimEnd(".") & "[" & i & "]", person_dict)
                End If

                i += 1
            Next





        ElseIf Not t.Contains("Collection") Then
            person_dict.Add(dot.TrimEnd("."), input)
        End If



    End Sub


#End Region



#Region "System Methods"
    Public Function GetSystems() As List(Of grSystem)
        Dim web As New WebClient()
        web.Encoding = Encoding.UTF8



        Dim json = web.DownloadString(_grUrl & "systems?access_token=" & _apikey.ToString)

        Dim jss = New Web.Script.Serialization.JavaScriptSerializer()
        Dim rts = jss.Deserialize(Of Dictionary(Of String, List(Of Dictionary(Of String, Object))))(json)
        Dim rtn As New List(Of grSystem)
        If rts.ContainsKey("systems") Then
            For Each row In rts("systems")
                Dim insert As New grSystem
                insert.ID = row("id")
                insert.Name = row("name")
                If row.ContainsKey("root") Then
                    insert.IsRoot = row("root")
                End If
                If row.ContainsKey("access_token") Then
                    insert.AccessToken = row("access_token")
                End If



                rtn.Add(insert)



            Next
        End If



        Return rtn
    End Function

    Public Function ResetAccessToken(Optional ByVal id As String = "") As String

        Dim rest = _grUrl & "systems/reset_access_token?access_token=" & _apikey.ToString
        If Not String.IsNullOrEmpty(id) Then
            rest &= "&id=" & id
        End If

        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        ' request.CookieContainer = myCookieContainer
        request.Method = "Post"
        Dim rtn As String = ""

        request.ContentType = "application/json"
        Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)
        Using reader As New IO.StreamReader(response.GetResponseStream())
            Dim json = reader.ReadToEnd()
            Dim jss = New Web.Script.Serialization.JavaScriptSerializer()
            Dim rts = jss.Deserialize(Of Dictionary(Of String, Dictionary(Of String, Object)))(json)

            If rts.ContainsKey("system") Then


                If rts("system").ContainsKey("access_token") Then
                    rtn = rts("system")("access_token")
                End If





            End If
            Return rtn
        End Using
    End Function

    Public Shared Function GetSystems(ByVal root_key As String, ByVal grUrl As String) As List(Of grSystem)
        Dim web As New WebClient()
        web.Encoding = Encoding.UTF8

        Dim rtn As New List(Of grSystem)
        Try


            Dim json = web.DownloadString(grUrl & "systems?access_token=" & root_key)

            Dim jss = New Web.Script.Serialization.JavaScriptSerializer()
            Dim rts = jss.Deserialize(Of Dictionary(Of String, List(Of Dictionary(Of String, Object))))(json)

            If rts.ContainsKey("systems") Then
                For Each row In rts("systems")
                    Dim insert As New grSystem
                    insert.ID = row("id")
                    insert.Name = row("name")
                    If row.ContainsKey("root") Then
                        insert.IsRoot = row("root")
                    End If
                    If row.ContainsKey("access_token") Then
                        insert.AccessToken = row("access_token")
                    End If



                    rtn.Add(insert)



                Next
            End If

        Catch ex As Exception

        End Try

        Return rtn
    End Function
    Public Shared Function GetSystem(ByVal grUrl As String, ByVal target_api_key As String, root_api_key As String) As grSystem

        Return GetSystems(root_api_key, grUrl).Where(Function(c) c.AccessToken = target_api_key).FirstOrDefault

    End Function

    Public Sub EditSystemRoot(ByVal id As String, ByVal makeRoot As Boolean)
        Dim postData = "{""system"": {""root"":" & makeRoot.ToString.ToLower & "}}"



        Dim rest = _grUrl & "systems/" & id & "?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        ' request.CookieContainer = myCookieContainer
        request.Method = "PUT"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        Using requestStream = request.GetRequestStream()


            requestStream.Write(bytes, 0, bytes.Length)



            Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)

        End Using
        'refresh the local entityType model

    End Sub

    Public Sub CreateSystem(ByVal name As String)
        Dim postData = "{""system"": {""name"":""" & name & """}}"


        Dim rest = _grUrl & "systems?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        ' request.CookieContainer = myCookieContainer
        request.Method = "POST"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        Using requestStream = request.GetRequestStream()
            requestStream.Write(bytes, 0, bytes.Length)



            Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)
        End Using
       


        'refresh the local entityType model

    End Sub

#End Region






























End Class

