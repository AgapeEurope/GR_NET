

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
    Private _api_system As String
    Public Property ApiSystem() As String
        Get
            Return _api_system
        End Get
        Set(ByVal value As String)
            _api_system = value
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
    Private _x_forwarded_for As String
    Public Property XForwardedFor() As String
        Get
            Return _x_forwarded_for
        End Get
        Set(ByVal value As String)
            _x_forwarded_for = value
        End Set
    End Property
    Private _measApi As String
    Public Property MeasApi() As String
        Get
            Return _measApi
        End Get
        Set(ByVal value As String)
            _measApi = value
        End Set
    End Property



#End Region


#Region "Constructors"
    ''' <summary>
    ''' The constructor initialises the GR object. 
    ''' </summary>
    ''' <param name="apiKey">Your GR Auth Key</param>
    ''' <param name="gr_url">The URL of the GR server</param>
    ''' <remarks>The test GR server is used by default.</remarks>
    Public Sub New(ByVal apiKey As String, Optional ByVal gr_url As String = "https://gr.stage.uscm.org/", Optional ByVal getTypes As Boolean = True, Optional ByVal xff As String = "")
        If Not apiKey = Nothing Then
            _apikey = apiKey
        End If
        If Not String.IsNullOrEmpty(xff) Then
            XForwardedFor = xff
        End If
        _grUrl = gr_url.TrimEnd("/") & "/"  'ensure url ends in '/'
        If getTypes Then
            GetEntityTypeDefFromGR()
        End If

    End Sub
#End Region



#Region "Subscriptions"
    Public Function GetSubscriptions() As List(Of Subscription)
        Dim rtn As New List(Of Subscription)
        Dim web As New WebClient()
        web.Encoding = Encoding.UTF8
        If Not _x_forwarded_for Is Nothing Then
            web.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        Dim json = web.DownloadString(_grUrl & "subscriptions?access_token=" & _apikey.ToString)

        Dim jss = New Web.Script.Serialization.JavaScriptSerializer()
        Dim subs = jss.Deserialize(Of Dictionary(Of String, List(Of Dictionary(Of String, Object))))(json)
        For Each row In subs("subscriptions")
            Dim insert As New Subscription()
            insert.ID = row("id")
            insert.EndPoint = row("endpoint")
            insert.EntityTypeId = row("entity_type_id")
            insert.SystemId = row("system_id")
            insert.Format = row("format")
            insert.Confirmed = row("confirmed")
            insert.EntityTypeName = entity_types_def.Find(Function(c) c.ID = insert.EntityTypeId).Name
            rtn.Add(insert)
        Next
        Return rtn
    End Function

    Public Sub DeleteSubscriptions(ByVal SubscriptionId As String)
        Dim rest = _grUrl & "subscriptions/" & SubscriptionId & "?access_token=" & _apikey.ToString
        Dim response As HttpWebResponse
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        request.Method = "DELETE"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        response = DirectCast(request.GetResponse(), HttpWebResponse)


        request.Abort()
        request = Nothing
    End Sub

    Public Sub CreateSubscription(ByVal entity_type_id As String, ByVal endpoint As String, Optional ByVal format As String = "json")

        Dim postData = "{""subscription"": {""entity_type_id"":""" & entity_type_id & """, ""endpoint"": """ & endpoint & """, ""format"": """ & format & """}}"
        'Console.Write(postData & vbNewLine)
        Dim rest = _grUrl & "subscriptions?access_token=" & _apikey.ToString


        Dim response As HttpWebResponse

        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        request.Method = "POST"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        Using requestStream = request.GetRequestStream()


            requestStream.Write(bytes, 0, bytes.Length)

            response = DirectCast(request.GetResponse(), HttpWebResponse)


        End Using
    End Sub


#End Region
#Region "Public Methods - Measurements"



    Public Sub DeleteMeasuerment(ByVal MeasurementId As String)
        Dim rest = _grUrl & "measurements/" & MeasurementId & "?access_token=" & _apikey.ToString
        Dim response As HttpWebResponse
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        request.Method = "DELETE"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        response = DirectCast(request.GetResponse(), HttpWebResponse)


        request.Abort()
        request = Nothing
    End Sub


    Public Async Function GetMeasurementsAsync(ByVal RelatedEntityId As String, ByVal PeriodFrom As String, ByVal PeriodTo As String, Optional ByVal MeasurementTypeId As String = "", Optional Category As String = "", Optional DefinitionOnly As Boolean = False, Optional filters As String = "") As Task(Of List(Of MeasurementType))

        ' Dim web As New WebClient()
        'web.Encoding = Encoding.UTF8
        Dim extras As String = "&filters[period_from]=" & PeriodFrom & "&filters[period_to]=" & PeriodTo & IIf(RelatedEntityId = "", "", "&filters[related_entity_id]=" & RelatedEntityId) & filters

        If DefinitionOnly Then
            If Not String.IsNullOrEmpty(MeasurementTypeId) Then
                extras = "&per_page=250&filters[related_entity_type_id]=" & RelatedEntityId
            Else
                extras = "&per_page=250" & filters
            End If

        End If
        If Category <> "" Then
            extras &= "&filters[category]=" & Category
        End If
        Dim typeString = "measurement_types"
        If MeasurementTypeId <> "" Then
            typeString = "measurement_types/" & MeasurementTypeId
        End If

        Dim Json As String = ""


        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(_grUrl & typeString & "?access_token=" & _apikey.ToString & extras), HttpWebRequest)
        request.Proxy = Nothing
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        Using response As WebResponse = Await request.GetResponseAsync()
            Using reader As New IO.StreamReader(response.GetResponseStream())
                json = reader.ReadToEnd()

            End Using
        End Using





        '   Dim json = Await web.DownloadStringAsync(_grUrl & typeString & "?access_token=" & _apikey.ToString & extras)

        Dim jss = New Web.Script.Serialization.JavaScriptSerializer()
        Dim ent = jss.Deserialize(Of Dictionary(Of String, Object))(json)



        Dim rtn As New List(Of MeasurementType)


        If ent.ContainsKey("measurement_type") Then
            Dim insert As New MeasurementType
            insert.ID = ent("measurement_type")("id")
            insert.Name = ent("measurement_type")("name")
            insert.Description = ent("measurement_type")("description")
            If ent("measurement_type").ContainsKey("category") Then
                insert.Category = ent("measurement_type")("category")
            End If
            If ent("measurement_type").ContainsKey("perm_link") Then
                insert.PermLink = ent("measurement_type")("perm_link")
            End If
            insert.Frequency = ent("measurement_type")("frequency")
            insert.Unit = ent("measurement_type")("unit")
            insert.RelatedEntityTypeId = ent("measurement_type")("related_entity_type_id")
            For Each row In ent("measurement_type")("measurements")
                Dim insertm As New Measurement
                insertm.Period = row("period")
                insertm.Value = row("value")
                insertm.RelatedEntityId = row("related_entity_id")
                If row.ContainsKey("dimension") Then
                    insertm.Dimension = row("dimension")
                End If
                insertm.MeasurementTypeId = insert.ID
                insertm.ID = row("id")
                insert.measurements.Add(insertm)
            Next
            rtn.Add(insert)
        Else
            For Each row In ent("measurement_types")
                Dim insert As New MeasurementType
                insert.ID = row("id")
                insert.Name = row("name")
                insert.Description = row("description")
                If row.ContainsKey("category") Then
                    insert.Category = row("category")
                End If
                If row.ContainsKey("perm_link") Then
                    insert.PermLink = row("perm_link")
                End If
                insert.Frequency = row("frequency")
                insert.Unit = row("unit")

                insert.RelatedEntityTypeId = row("related_entity_type_id")
                For Each row2 In row("measurements")
                    Dim insertm As New Measurement
                    insertm.Period = row2("period")
                    insertm.Value = row2("value")
                    insertm.RelatedEntityId = row2("related_entity_id")
                    If row2.ContainsKey("dimension") Then
                        insertm.Dimension = row2("dimension")
                    End If

                    insertm.MeasurementTypeId = insert.ID
                    insertm.ID = row2("id")
                    insert.measurements.Add(insertm)
                Next
                rtn.Add(insert)
            Next


        End If


        Return rtn
    End Function
    Public Function GetMeasurementsAsOfDate(ByVal RelatedEntityId As String, ByVal Period As String, Optional ByVal MeasurementTypeId As String = "", Optional filters As String = "") As List(Of MeasurementType)

        Dim web As New WebClient()
        web.Encoding = Encoding.UTF8
        Dim extras As String = "&filters[as_of_date]=" & Period & IIf(RelatedEntityId = "", "", "&filters[related_entity_id]=" & RelatedEntityId) & filters


        Dim typeString = "measurement_types"
        If MeasurementTypeId <> "" Then
            typeString = "measurement_types/" & MeasurementTypeId
        End If
        If Not _x_forwarded_for Is Nothing Then
            web.Headers.Add("X-Forwarded-For", _x_forwarded_for)
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
            If ent("measurement_type").ContainsKey("category") Then
                insert.Category = ent("measurement_type")("category")
            End If
            If ent("measurement_type").ContainsKey("perm_link") Then
                insert.PermLink = ent("measurement_type")("perm_link")
            End If
            'insert.Category = ent("measurement_type")("category")
            insert.Frequency = ent("measurement_type")("frequency")
            insert.Unit = ent("measurement_type")("unit")
            insert.RelatedEntityTypeId = ent("measurement_type")("related_entity_type_id")
            For Each row In ent("measurement_type")("measurements")
                Dim insertm As New Measurement
                insertm.Period = row("period")
                insertm.Value = row("value")
                insertm.RelatedEntityId = row("related_entity_id")
                If row.ContainsKey("dimension") Then
                    insertm.Dimension = row("dimension")
                End If
                insertm.MeasurementTypeId = insert.ID
                If row.ContainsKey("created_by") Then
                    insertm.CreatedBy = row("created_by")
                End If

                insertm.ID = row("id")
                insert.measurements.Add(insertm)
            Next
            rtn.Add(insert)
        Else
            For Each row In ent("measurement_types")
                Dim insert As New MeasurementType
                insert.ID = row("id")
                insert.Name = row("name")
                insert.Description = row("description")
                If row.ContainsKey("category") Then
                    insert.Category = row("category")
                End If
                If row.ContainsKey("perm_link") Then
                    insert.PermLink = row("perm_link")
                End If

                insert.Frequency = row("frequency")
                insert.Unit = row("unit")

                insert.RelatedEntityTypeId = row("related_entity_type_id")
                For Each row2 In row("measurements")
                    Dim insertm As New Measurement
                    insertm.Period = row2("period")
                    insertm.Value = row2("value")
                    insertm.RelatedEntityId = row2("related_entity_id")
                    If row2.ContainsKey("dimension") Then
                        insertm.Dimension = row2("dimension")
                    End If
                    If row2.ContainsKey("created_by") Then
                        insertm.CreatedBy = row2("created_by")
                    End If
                    insertm.MeasurementTypeId = insert.ID
                    insertm.ID = row2("id")
                    insert.measurements.Add(insertm)
                Next
                rtn.Add(insert)
            Next


        End If


        Return rtn
    End Function


    Public Function GetMeasurements(ByVal RelatedEntityId As String, ByVal PeriodFrom As String, ByVal PeriodTo As String, Optional ByVal MeasurementTypeId As String = "", Optional Category As String = "", Optional DefinitionOnly As Boolean = False, Optional filters As String = "") As List(Of MeasurementType)

        Dim web As New WebClient()
        web.Encoding = Encoding.UTF8
        Dim extras As String = "&filters[period_from]=" & PeriodFrom & "&filters[period_to]=" & PeriodTo & IIf(RelatedEntityId = "", "", "&filters[related_entity_id]=" & RelatedEntityId) & filters

        If DefinitionOnly Then
            extras = "&per_page=250&filters[related_entity_type_id]=" & RelatedEntityId & filters

        End If
        If Category <> "" Then
            extras &= "&filters[category]=" & Category
        End If
        Dim typeString = "measurement_types"
        If MeasurementTypeId <> "" Then
            typeString = "measurement_types/" & MeasurementTypeId
        End If
        If Not _x_forwarded_for Is Nothing Then
            web.Headers.Add("X-Forwarded-For", _x_forwarded_for)
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
            If ent("measurement_type").ContainsKey("category") Then
                insert.Category = ent("measurement_type")("category")
            End If
            If ent("measurement_type").ContainsKey("perm_link") Then
                insert.PermLink = ent("measurement_type")("perm_link")
            End If
            'insert.Category = ent("measurement_type")("category")
            insert.Frequency = ent("measurement_type")("frequency")
            insert.Unit = ent("measurement_type")("unit")
            insert.RelatedEntityTypeId = ent("measurement_type")("related_entity_type_id")
            For Each row In ent("measurement_type")("measurements")
                Dim insertm As New Measurement
                insertm.Period = row("period")
                insertm.Value = row("value")
                insertm.RelatedEntityId = row("related_entity_id")
                If row.ContainsKey("dimension") Then
                    insertm.Dimension = row("dimension")
                End If
                insertm.MeasurementTypeId = insert.ID
                If row.ContainsKey("created_by") Then
                    insertm.CreatedBy = row("created_by")
                End If

                insertm.ID = row("id")
                insert.measurements.Add(insertm)
            Next
            rtn.Add(insert)
        Else
            For Each row In ent("measurement_types")
                Dim insert As New MeasurementType
                insert.ID = row("id")
                insert.Name = row("name")
                insert.Description = row("description")
                If row.ContainsKey("category") Then
                    insert.Category = row("category")
                End If
                If row.ContainsKey("perm_link") Then
                    insert.PermLink = row("perm_link")
                End If

                insert.Frequency = row("frequency")
                insert.Unit = row("unit")

                insert.RelatedEntityTypeId = row("related_entity_type_id")
                For Each row2 In row("measurements")
                    Dim insertm As New Measurement
                    insertm.Period = row2("period")
                    insertm.Value = row2("value")
                    insertm.RelatedEntityId = row2("related_entity_id")
                    If row2.ContainsKey("dimension") Then
                        insertm.Dimension = row2("dimension")
                    End If
                    If row2.ContainsKey("created_by") Then
                        insertm.CreatedBy = row2("created_by")
                    End If
                    insertm.MeasurementTypeId = insert.ID
                    insertm.ID = row2("id")
                    insert.measurements.Add(insertm)
                Next
                rtn.Add(insert)
            Next


        End If


        Return rtn
    End Function


    Private Sub AddMeasurementBatch(ByVal mt As MeasurementType, Optional ByVal Page As Integer = 0, Optional BatchSize As Integer = 250, Optional ByVal UseLMI As Boolean = False)
        If mt.measurements.Count > 0 Then
            Dim postData As String
            Dim rest As String
            If UseLMI Then
                postData = mt.LmiMeasurementsToJson(Page, BatchSize)
                rest = _measApi & "sys_measurements?access_token=" & _apikey.ToString

            Else
                postData = mt.MeasurementsToJson(Page, BatchSize)

                rest = _grUrl & "measurements?access_token=" & _apikey.ToString
            End If



            If Not String.IsNullOrEmpty(postData) Then



                Dim response As HttpWebResponse


                'System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls
                Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
                request.Proxy = Nothing
                request.Method = "POST"
                If Not _x_forwarded_for Is Nothing Then
                    request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
                End If
                Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
                request.ContentLength = bytes.Length
                request.ContentType = "application/json"
                Using requestStream = request.GetRequestStream()


                    requestStream.Write(bytes, 0, bytes.Length)
                    Try
                        response = DirectCast(request.GetResponse(), HttpWebResponse)
                        Console.Write(".")
                    Catch ex As Exception
                        Console.Write("e")
                    End Try


                End Using

                request.Abort()
                request = Nothing

            End If

        End If
    End Sub

    Private Async Function AddMeasurementBatchAsync(ByVal mt As MeasurementType, Optional ByVal Page As Integer = 0, Optional BatchSize As Integer = 250, Optional ByVal UseLMI As Boolean = False) As Task
        If mt.measurements.Count > 0 Then
            Dim postData As String
            Dim rest As String
            If UseLMI Then
                postData = mt.LmiMeasurementsToJson(Page)
                rest = _grUrl & "measurements?access_token=" & _apikey.ToString
            Else
                postData = mt.MeasurementsToJson(Page)
                rest = _grUrl & "measurements?access_token=" & _apikey.ToString
            End If

            If Not String.IsNullOrEmpty(postData) Then




                Dim response As HttpWebResponse


                'System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls
                Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
                request.Proxy = Nothing
                request.Method = "POST"

                Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
                request.ContentLength = bytes.Length
                request.ContentType = "application/json"
                If Not _x_forwarded_for Is Nothing Then
                    request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
                End If
                Using requestStream = request.GetRequestStream()


                    requestStream.Write(bytes, 0, bytes.Length)

                    response = DirectCast(request.GetResponse(), HttpWebResponse)
                End Using

                request.Abort()
                request = Nothing

            End If

        End If
    End Function

    Public Sub AddUpdateMeasurement(ByVal mt As MeasurementType, Optional ByVal UseLMI As Boolean = False, Optional BatchSize As Integer = 250)
        If mt.measurements.Count = 0 Then
            'nothing to do
            Return
        End If
        If mt.measurements.Count <= BatchSize Then
            AddMeasurementBatch(mt, Page:=-1, BatchSize:=BatchSize, UseLMI:=UseLMI)
        Else
            For i As Integer = 0 To CInt(Math.Truncate(mt.measurements.Count / BatchSize))
                If Not UseLMI Then
                    Console.WriteLine("Batch " & i & " of " & CInt(Math.Truncate(mt.measurements.Count / BatchSize)))
                End If

                AddMeasurementBatch(mt, i, BatchSize, UseLMI)
            Next
        End If


    End Sub




    Public Async Function AddUpdateMeasurementAsync(ByVal mt As MeasurementType) As Task
        If mt.measurements.Count = 0 Then
            'nothing to do
            Return
        End If

        If mt.measurements.Count <= 100 Then
            Await AddMeasurementBatchAsync(mt)
        Else
            Dim tasks As List(Of Task) = New List(Of Task)


            For i As Integer = 0 To CInt(Math.Truncate(mt.measurements.Count / 100))
                'Console.WriteLine("Batch " & i & " of " & CInt(Math.Truncate(mt.measurements.Count / 10)))
                tasks.Add(AddMeasurementBatchAsync(mt, i))

            Next
            Task.WaitAll(tasks.ToArray)

        End If


    End Function



#End Region

#Region "Public Methods - MeasurementTypes"





    Public Sub AddMeasurementType(ByVal mt As MeasurementType)
        Dim postData = mt.ToJson()

        Dim rest = _grUrl & "measurement_types?access_token=" & _apikey.ToString



        Dim response As HttpWebResponse



        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        request.Method = "POST"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"

        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        Using requestStream = request.GetRequestStream()


            requestStream.Write(bytes, 0, bytes.Length)

            response = DirectCast(request.GetResponse(), HttpWebResponse)


        End Using

        request.Abort()
        request = Nothing

        'Dim reader As New IO.StreamReader(response.GetResponseStream())
        'Dim json = reader.ReadToEnd()
        '  Dim newEntity = CreateEntityFromJsonResp(json)
        ' Return newEntity.ID

    End Sub


    Public Sub UpdateMeasurementType(ByVal mt As MeasurementType)
        Dim postData = mt.ToJson()

        Dim rest = _grUrl & "measurement_types/" & mt.ID & "?access_token=" & _apikey.ToString



        Dim count = 0
        Dim response As HttpWebResponse

        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        request.Method = "PUT"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        Using requestStream = request.GetRequestStream()


            requestStream.Write(bytes, 0, bytes.Length)

            response = DirectCast(request.GetResponse(), HttpWebResponse)


        End Using
        request.Abort()
        request = Nothing


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
        'Console.Write(postData & vbNewLine)
        Dim rest = _grUrl & "entities?access_token=" & _apikey.ToString




        Dim response As HttpWebResponse

        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        request.Method = "POST"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        Using requestStream = request.GetRequestStream()


            requestStream.Write(bytes, 0, bytes.Length)

            response = DirectCast(request.GetResponse(), HttpWebResponse)


        End Using


        Using reader As New IO.StreamReader(response.GetResponseStream())
            Dim json = reader.ReadToEnd()
            Dim newEntity = CreateEntityFromJsonResp(json)
            Return newEntity.ID
        End Using



    End Function

    Public Function CreateEntityWithResponse(ByRef p As Entity, ByVal EntityName As String, Optional filters As String = "") As Entity
        Dim postData = "{""entity"": {""" & EntityName & """:" & p.ToJson & "}}"
        'Console.Write(postData & vbNewLine)
        Dim rest = _grUrl & "entities?access_token=" & _apikey.ToString & filters




        Dim response As HttpWebResponse

        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        request.Method = "POST"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        Using requestStream = request.GetRequestStream()


            requestStream.Write(bytes, 0, bytes.Length)

            response = DirectCast(request.GetResponse(), HttpWebResponse)


        End Using


        Using reader As New IO.StreamReader(response.GetResponseStream())
            Dim json = reader.ReadToEnd()
            Dim newEntity = CreateEntityFromJsonResp(json)
            Return newEntity
        End Using



    End Function

    Public Function UpdateEntity(ByRef p As Entity, ByVal EntityName As String, Optional ByVal SkipEmptyArray As Boolean = True, Optional filters As String = "") As String
        If Not p.HasValues Then
            Return p.ID
        End If
        Dim postData = "{""entity"": {""" & EntityName & """:" & p.ToJson(skip_empty_array:=SkipEmptyArray) & "}}"
        'Console.Write(postData & vbNewLine)

        Dim rest = _grUrl & "entities/" & p.ID & "/?access_token=" & _apikey.ToString & filters
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        request.Method = "PUT"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
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
    Public Function UpdateEntityWithResponse(ByRef p As Entity, ByVal EntityName As String, Optional ByVal SkipEmptyArray As Boolean = True, Optional filters As String = "") As Entity
        If Not p.HasValues Then
            Return Nothing
        End If
        Dim postData = "{""entity"": {""" & EntityName & """:" & p.ToJson(skip_empty_array:=SkipEmptyArray) & "}}"
        'Console.Write(postData & vbNewLine)

        Dim rest = _grUrl & "entities/" & p.ID & "/?access_token=" & _apikey.ToString & filters
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        request.Method = "PUT"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
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


                Return newEntity
            End Using
        End Using




        Return Nothing



        'ID's need to be writted back to entity structure
    End Function



    Public Sub DeleteEntity(ByVal ID As String)
        Dim rest = _grUrl & "entities/" & ID & "?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        request.Method = "DELETE"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)

        Using reader As New IO.StreamReader(response.GetResponseStream())
            Dim json = reader.ReadToEnd()
            'Console.Write(json & vbNewLine & vbNewLine)
        End Using

    End Sub

    Public Function GetEntity(ByVal ID As String, Optional ByVal AllSystems As Boolean = False, Optional ByVal extras As String = "") As Entity
        Dim web As New WebClient()
        web.Encoding = Encoding.UTF8
        'Dim extras As String = ""
        If AllSystems Then
            extras &= "&filters[owned_by]=all"
        End If
        If Not _x_forwarded_for Is Nothing Then
            web.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        Dim json = web.DownloadString(_grUrl & "entities/" & ID & "?access_token=" & _apikey.ToString & extras)


        ' Return New Entity(json)
        Return CreateEntityFromJsonResp(json)
    End Function
    Public Async Function GetEntityAsync(ByVal ID As String, Optional ByVal AllSystems As Boolean = False, Optional ByVal extras As String = "") As Task(Of Entity)

        ' Dim extras As String = ""
        If AllSystems Then
            extras &= "&filters[owned_by]=all"
        End If

        Dim Json As String = ""


        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(_grUrl & "entities/" & ID & "?access_token=" & _apikey.ToString & extras), HttpWebRequest)
        request.Proxy = Nothing
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        Using response As WebResponse = Await request.GetResponseAsync()
            Using reader As New IO.StreamReader(response.GetResponseStream())
                Json = reader.ReadToEnd()

            End Using
        End Using


        ' Return New Entity(json)
        Return CreateEntityFromJsonResp(Json)
    End Function

    Private Function hasNextPage(ByVal json As String) As Boolean

        Try
            Dim jss = New Web.Script.Serialization.JavaScriptSerializer()
            Dim ent = jss.Deserialize(Of Dictionary(Of String, Object))(json)
            Return CType(ent("meta"), Dictionary(Of String, Object))("next_page") = "true"
        Catch ex As Exception

        End Try

        Return False
    End Function

    Public Function GetEntities(ByVal EntityType As String, ByVal Filters As String, Optional ByVal Page As Integer = 1, Optional ByVal PerPage As Integer = 100, Optional ByRef TotalPage As Integer = 1, Optional GetAllPages As Boolean = True) As List(Of Entity)
        Dim web As New WebClient()
        web.Encoding = Encoding.UTF8

        Dim rtn As New List(Of Entity)
        Dim has_next = True
        TotalPage = 1
        While has_next

            Dim url = _grUrl & "entities?access_token=" & _apikey.ToString & "&entity_type=" & EntityType & Filters & "&page=" & Page & "&per_page=" & PerPage
            If Not _x_forwarded_for Is Nothing Then
                web.Headers.Add("X-Forwarded-For", _x_forwarded_for)
            End If
            Dim json = web.DownloadString(url)


            ' TotalPage = GetTotalPagesFromJson(json)

            has_next = hasNextPage(json) And GetAllPages
            If has_next = False And Not GetAllPages Then
                TotalPage = GetTotalPagesFromJson(json)
            End If
            rtn.AddRange(CreateEntitiesFromJsonResp(json))
            Page += 1
        End While



        Return rtn

    End Function


    Public Async Function GetEntitiesAsync(ByVal EntityType As String, ByVal Filters As String) As Task(Of List(Of Entity))
        Dim web As New WebClient()
        web.Encoding = Encoding.UTF8

        Dim rtn As New List(Of Entity)
        Dim has_next = True
        Dim Page As Integer = 1
        Dim PerPage As Integer = 100
        While has_next

            Dim url = _grUrl & "entities?access_token=" & _apikey.ToString & "&entity_type=" & EntityType & Filters & "&page=" & Page & "&per_page=" & PerPage

            Dim Json As String = ""


            Dim request As HttpWebRequest = DirectCast(WebRequest.Create(url), HttpWebRequest)
            request.Proxy = Nothing
            If Not _x_forwarded_for Is Nothing Then
                request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
            End If
            Using response As WebResponse = Await request.GetResponseAsync()
                Using reader As New IO.StreamReader(response.GetResponseStream())
                    Json = reader.ReadToEnd()

                End Using
            End Using


            ' TotalPage = GetTotalPagesFromJson(json)

            has_next = hasNextPage(Json)

            rtn.AddRange(CreateEntitiesFromJsonResp(Json))
            Page += 1
        End While



        Return rtn





    End Function
    Public Sub addNewRelationshipType(ByVal entity_type1 As String, ByVal entity_type2 As String, ByVal relationship1 As String, ByVal relationship2 As String)
        Dim postData = "{""relationship_type"": {""entity_type1_id"":""" & entity_type1 & """, ""entity_type2_id"":""" & entity_type2 & """,""relationship1"":""" & relationship1 & """,""relationship2"":""" & relationship2 & """ }}"

        Dim rest = _grUrl & "relationship_types?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        ' request.CookieContainer = myCookieContainer
        request.Method = "POST"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
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
        request.Proxy = Nothing
        ' request.CookieContainer = myCookieContainer
        request.Method = "PUT"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        Using requestStream = request.GetRequestStream()


            requestStream.Write(bytes, 0, bytes.Length)



            Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)

            ' Dim reader As New IO.StreamReader(response.GetResponseStream())
            'Dim json = reader.ReadToEnd()

            'refresh the local entityType model
            'GetEntityTypeDefFromGR()
        End Using

    End Sub

    Public Function addRelationshipToEntity(ByVal Entity As Entity, ByVal EntityType1 As String, ByVal Id1 As String, ByVal EntityType2 As String, ByVal Id2 As String, ByVal RelationshipType As String, Optional Role As String = "", Optional ClientIntegrationId1 As String = "", Optional ClientIntegrationId2 As String = "", Optional rel_ent_props As Dictionary(Of String, String) = Nothing) As Entity
        Dim r As String = ""
        If Not Role = "" Then
            r = ",""role"": """ & Role & """"
        End If

        Dim cid As String = ""
        If Not ClientIntegrationId1 = "" Then
            cid = """client_integration_id"": """ & ClientIntegrationId1 & """, "
        End If
        '  Dim rel = Entity.collections.Where(Function(c) c.Key = RelationshipType & ":relationship").ToArray
        Entity.profileProperties.Clear()
        Entity.collections.Clear()
        Entity.collections.Add(RelationshipType & ":relationship", New List(Of Entity))
        'If rel.Count > 0 Then
        '    For Each row In rel.First.Value

        '        Dim ins As New Entity()


        '        ins.AddPropertyValue(EntityType2, row.GetPropertyValue(EntityType2))

        '        ins.AddPropertyValue("client_integration_id", )
        '        Entity.collections(RelationshipType & ":relationship").Add(ins)

        '    Next
        'End If







        Dim insert As New Entity()
        insert.AddPropertyValue(EntityType2, Id2)
        insert.AddPropertyValue("client_integration_id", ClientIntegrationId1 & "_" & ClientIntegrationId2)
        If Not rel_ent_props Is Nothing Then
            For Each row In rel_ent_props
                insert.AddPropertyValue(row.Key, row.Value)
            Next
        End If
        Entity.AddPropertyValue("client_integration_id", ClientIntegrationId1)
        Entity.collections(RelationshipType & ":relationship").Add(insert)


        Dim postData = "{""entity"":{""" & EntityType1 & """: " & Entity.ToJson() & "}}"
        'Console.Write(postData & vbNewLine)
        'Trace.WriteLine("to_update:" & postData)
        Dim rest = _grUrl & "entities/" & Id1 & "/?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        request.Method = "PUT"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
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
                Return newEntity
            End Using

        End Using
        Return New Entity
    End Function

    Public Function RelateEntity(ByVal EntityType1 As String, ByVal Id1 As String, ByVal EntityType2 As String, ByVal Id2 As String, ByVal RelationshipType As String, Optional Role As String = "", Optional ClientIntegrationId1 As String = "", Optional Client_Integration_Id2 As String = "", Optional rel_ent_props As Dictionary(Of String, String) = Nothing) As Entity
        Dim r As String = ""
        If Not Role = "" Then
            r = ",""role"": """ & Role & """"
        End If

        If Not rel_ent_props Is Nothing Then
            For Each row In rel_ent_props
                r = ",""" & row.Key & """: """ & row.Value & """"
            Next
        End If


        Dim cr As String = ""
        ' If Not Client_Integration_Id2 = "" Then
        cr = ",""client_integration_id"": """ & ClientIntegrationId1 & "_" & Client_Integration_Id2 & """"
        ' End If
        Dim cid As String = ""
        If Not ClientIntegrationId1 = "" Then
            cid = """client_integration_id"": """ & ClientIntegrationId1 & """, "
        End If
        Dim postData = "{""entity"":{""" & EntityType1 & """: {" & cid & """" & RelationshipType & ":relationship"":{""" & EntityType2 & """: """ & Id2 & """" & r & cr & " }}}}"
        ' Console.Write(postData & vbNewLine)
        Trace.WriteLine("to_update:" & postData)
        Dim rest = _grUrl & "entities/" & Id1 & "/?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        request.Method = "PUT"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
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
        If Not _x_forwarded_for Is Nothing Then
            web.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
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
        If Not _x_forwarded_for Is Nothing Then
            web.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        Return web.DownloadString(rest)

    End Function

    Public Sub CreateEntityType(ByVal Name As String, ByVal ParentId As String, ByVal type As String, Optional ByVal Description As String = "")

        Dim postData = "{""entity_type"": {""name"":""" & Name & """, ""field_type"":""" & type & """" & IIf(String.IsNullOrEmpty(ParentId) Or ParentId = "null", "", ",""parent_id"":""" & ParentId & """") & IIf(String.IsNullOrEmpty(Description), "", ",""description"":""" & Description & """") & "}}"

        Dim rest = _grUrl & "entity_types?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        ' request.CookieContainer = myCookieContainer
        request.Method = "POST"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
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
        request.Proxy = Nothing
        ' request.CookieContainer = myCookieContainer
        request.Method = "PUT"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
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
        If (Not String.IsNullOrEmpty(_apikey)) And (Not String.IsNullOrEmpty(_grUrl)) Then


            ServicePointManager.ServerCertificateValidationCallback = AddressOf TrustAllCertificateCallback
            Dim mycache As CredentialCache = New CredentialCache()

            Dim web As New WebClient()
            web.Encoding = Encoding.UTF8
            web.Credentials = mycache
            If Not _x_forwarded_for Is Nothing Then
                web.Headers.Add("X-Forwarded-For", _x_forwarded_for)
            End If
            Dim json = web.DownloadString(_grUrl & "entity_types?access_token=" & _apikey.ToString & "&per_page=500")

            Dim jss = New Web.Script.Serialization.JavaScriptSerializer()
            Dim allEntityTypes = jss.Deserialize(Of Dictionary(Of String, List(Of Dictionary(Of String, Object))))(json)
            _entity_types_def = New List(Of EntityType)

            For Each row In allEntityTypes("entity_types")
                addSubEntityTypes(row, Nothing)
            Next

        End If
    End Sub


    Public Function GetEnums() As List(Of EntityType)
        Dim rtn As New List(Of EntityType)
        If (Not String.IsNullOrEmpty(_apikey)) And (Not String.IsNullOrEmpty(_grUrl)) Then


            ServicePointManager.ServerCertificateValidationCallback = AddressOf TrustAllCertificateCallback
            Dim mycache As CredentialCache = New CredentialCache()

            Dim web As New WebClient()
            web.Encoding = Encoding.UTF8
            web.Credentials = mycache
            If Not _x_forwarded_for Is Nothing Then
                web.Headers.Add("X-Forwarded-For", _x_forwarded_for)
            End If
            Dim json = web.DownloadString(_grUrl & "entity_types?access_token=" & _apikey.ToString & "&filters[name]=_enum_values")

            Dim jss = New Web.Script.Serialization.JavaScriptSerializer()
            Dim allEntityTypes = jss.Deserialize(Of Dictionary(Of String, List(Of Dictionary(Of String, Object))))(json)
            _entity_types_def = New List(Of EntityType)

            For Each row As Dictionary(Of String, Object) In (allEntityTypes("entity_types").First()("fields"))
                Dim insert As New EntityType("_enum_values", "")
                insert.Name = row("name")
                insert.EnumValues = CType(row("enum_values"), ArrayList).Cast(Of String)().ToArray

                rtn.Add(insert)
            Next

        End If
        Return rtn
    End Function
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

        Else
            insert.EnumValues = {}
        End If

        If input.ContainsKey("fields") Then
            For Each row As Dictionary(Of String, Object) In input("fields")

                addSubEntityTypes(row, insert)
            Next
            Dim cid = New EntityType("client_integration_id", "", insert)




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
            rtn.EntityType = ent_resp.Values.First.Keys.First
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
                ent.EntityType = row.Keys.First()
                rtn.Add(ent)
            Next



        End If
        Return rtn
    End Function


    Private Sub ProcessJsonEntity(ByVal input As Object, ByRef dot As String, ByRef person_dict As Dictionary(Of String, String))
        If dot <> "" Then
            dot &= "."
        End If
        If Not input Is Nothing Then


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

        End If

    End Sub


#End Region



#Region "System Methods"
    Public Function GetSystems() As List(Of grSystem)
        If String.IsNullOrEmpty(_apikey) Or String.IsNullOrEmpty(_grUrl) Then
            Return Nothing

        End If
        Dim web As New WebClient()
        web.Encoding = Encoding.UTF8


        If Not _x_forwarded_for Is Nothing Then
            web.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
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
                If row.ContainsKey("trusted_ips") Then

                    Dim ips As New List(Of String)
                    For Each ip In row("trusted_ips")
                        ips.Add(ip)

                    Next
                    insert.TrustedIps = ips
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
        request.Proxy = Nothing
        ' request.CookieContainer = myCookieContainer
        request.Method = "Post"
        Dim rtn As String = ""

        request.ContentType = "application/json"
        request.ContentLength = 0
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
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

                    If row.ContainsKey("trusted_ips") Then

                        Dim ips As New List(Of String)
                        For Each ip In row("trusted_ips")
                            ips.Add(ip)

                        Next
                        insert.TrustedIps = ips
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
    Public Sub EditSystem(ByVal sys As grSystem)
        Dim postData = "{""system"": {"

        If Not sys.TrustedIps Is Nothing Then
            postData &= """trusted_ips"": ["
            For Each row In sys.TrustedIps
                postData &= """" & row & ""","
            Next
            postData = postData.TrimEnd(",")
            postData &= "],"
        End If

        postData = postData.TrimEnd(",")
        postData &= "}}"




        Dim rest = _grUrl & "systems/" & sys.ID & "?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        ' request.CookieContainer = myCookieContainer
        request.Method = "PUT"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        Using requestStream = request.GetRequestStream()


            requestStream.Write(bytes, 0, bytes.Length)



            Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)

        End Using
    End Sub
    Public Sub EditSystemRoot(ByVal id As String, ByVal makeRoot As Boolean)
        Dim postData = "{""system"": {""root"":" & makeRoot.ToString.ToLower & "}}"



        Dim rest = _grUrl & "systems/" & id & "?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        ' request.CookieContainer = myCookieContainer
        request.Method = "PUT"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        Using requestStream = request.GetRequestStream()


            requestStream.Write(bytes, 0, bytes.Length)



            Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)

        End Using
        'refresh the local entityType model

    End Sub

    Public Sub CreateSystem(ByVal name As String, Optional trusted_ips As List(Of String) = Nothing)
        Dim tips = ""
        If Not trusted_ips Is Nothing Then
            tips = ", ""trusted_ips"": ["
            For Each row In trusted_ips
                tips &= """" & row & ""","
            Next
            tips = tips.TrimEnd(",")
            tips &= "]"
        End If
        Dim postData = "{""system"": {""name"":""" & name & """" & tips & "}}"


        Dim rest = _grUrl & "systems?access_token=" & _apikey.ToString
        Dim request As HttpWebRequest = DirectCast(WebRequest.Create(rest), HttpWebRequest)
        request.Proxy = Nothing
        ' request.CookieContainer = myCookieContainer
        request.Method = "POST"

        Dim bytes As Byte() = Text.Encoding.UTF8.GetBytes(postData)
        request.ContentLength = bytes.Length
        request.ContentType = "application/json"
        If Not _x_forwarded_for Is Nothing Then
            request.Headers.Add("X-Forwarded-For", _x_forwarded_for)
        End If
        Using requestStream = request.GetRequestStream()
            requestStream.Write(bytes, 0, bytes.Length)



            Dim response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)
        End Using



        'refresh the local entityType model

    End Sub

    Public Shared Function ValidateApiKey(ByVal gr_url As String, ByVal api_key As String, Optional ByVal xff As String = "") As Boolean
        Dim rtn = False
        Try
            Dim web As New WebClient()
            web.Encoding = Encoding.UTF8
            If Not xff Is Nothing Then
                web.Headers.Add("X-Forwarded-For", xff)
            End If
            Dim json = web.DownloadString(gr_url & "systems?access_token=" & api_key.ToString)
            If Not String.IsNullOrEmpty(json) Then
                rtn = True
            End If
        Catch

        End Try
        Return rtn
    End Function

#End Region






























End Class

