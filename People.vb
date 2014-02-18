Namespace GR.NET
    Public Class People





      

        Public people_list As New List(Of Entity)



        Public Function createPerson(ByVal LocalUserId As String) As Entity
            Dim person As New Entity()
            person.AddPropertyValue("client_integration_id", LocalUserId)

            people_list.Add(person)
            Return person
        End Function

       








      

     

    End Class
End Namespace

