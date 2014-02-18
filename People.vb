Namespace GR.NET
    Public Class People





      

        Private people_list As New List(Of Entity)



        Public Function createPerson(ByVal LocalId As String, ByVal TheKeyGuid As String, ByVal FirstName As String, ByVal LastName As String) As Entity
            Dim person As New Entity()
            person.AddPropertyValue("client_integration_id", LocalId)
            ' person.AddProperty("keyguid", TheKeyGuid)
            person.AddPropertyValue("first_name", FirstName)
            person.AddPropertyValue("last_name", LastName)
            people_list.Add(person)
            Return person
        End Function










      

     

    End Class
End Namespace

