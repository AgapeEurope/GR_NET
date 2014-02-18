Imports System.Net

Namespace GR.NET

    Public Class GR
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
        Private _rootEntityTypes As New List(Of EntityType)

        Public Sub New(Optional ByVal apiKey As String = Nothing, Optional gr_url As String = "https://gr.stage.uscm.org/")
            If Not apiKey = Nothing Then
                _apikey = apiKey
            End If
            _grUrl = gr_url
            GetRootEntities()

        End Sub

        Private rootEntities As String
        Public Property NewProperty() As String
            Get
                Return rootEntities
            End Get
            Set(ByVal value As String)
                rootEntities = value
            End Set
        End Property

        Public Sub GetRootEntities()
            'Make REST CAll
            ServicePointManager.ServerCertificateValidationCallback = AddressOf TrustAllCertificateCallback
            Dim mycache As CredentialCache = New CredentialCache()

            Dim web As New WebClient()
            web.Credentials = mycache

            Dim json = web.DownloadString(_grUrl & "entity_types?access_token=" & _apikey.ToString & "&filters[field_type]=entity")

            Dim jss = New Web.Script.Serialization.JavaScriptSerializer()
            Dim allEntityTypes = jss.Deserialize(Of Dictionary(Of String, List(Of Dictionary(Of String, Object))))(json)


            For Each row In allEntityTypes("entity_types")
                addSubEntities(row, Nothing)
            Next


        End Sub


        Private Sub addSubEntities(ByVal input As Dictionary(Of String, Object), ByRef Parent As EntityType)
            Dim insert As New EntityType(input("name"), input("id"), Parent)
            If input.ContainsKey("fields") Then
                For Each row As Dictionary(Of String, Object) In input("fields")

                    addSubEntities(row, insert)
                Next

            End If
            If Parent Is Nothing Then
                _rootEntityTypes.Add(insert)

            End If
        End Sub


        Public Shared Function TrustAllCertificateCallback(ByVal sender As Object, ByVal certificate As System.Security.Cryptography.X509Certificates.X509Certificate, ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, ByVal sslPolicyErrors As Security.SslPolicyErrors) As Boolean
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
        Public Function GetStaffForLocation(ByVal location As String)
            Return ApiCall("entities", "entity_type=person&filters[address][country]=" & location)
        End Function

        Public Sub CreatePerson(ByVal firstName As String, ByVal lastName As String, ByVal country As String, ByVal localId As String, Optional ByVal middleName As String = "")
            Dim postData = "{""entity"": {""person"":{""first_name"":""" & firstName & """," _
                         & """last_name"":""" & lastName & """," _
                          & """client_integration_id"":""" & localId & """," _
                          & """middle_name"":""" & middleName & """," _
                          & """address"":{""country"":""" & country & """}}}"

            Dim rest = _grUrl & "entities?access_token=LwLE5ay3N6jMNZ0pfSJJ3jq6EZZG0NQtP6PSWebaDI0"
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

            Console.Write(json)
        End Sub
        Public Sub SyncPerson(ByVal p As Entity)
            Dim postData = "{""entity"": {""person"":" & p.ToJson & "}}"

            Dim rest = _grUrl & "entities?access_token=" & _apikey.ToString
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

            Console.Write(json)
        End Sub


        Public Function GetFlatEntityLeafList(ByVal rootEntity As String) As List(Of EntityType)
            Dim root = _rootEntityTypes.Where(Function(c) c.Name = rootEntity).First
            Dim FlatList As New List(Of EntityType)
            root.GetDecendents(FlatList)
            Return FlatList

        End Function



    End Class
End Namespace
