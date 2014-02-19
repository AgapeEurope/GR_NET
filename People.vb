''' <summary>
''' People is a very tiny class that defnies a group of persons( entites)
''' </summary>
''' <remarks>GR has a local People member. (gr.people). Create an intstance of gr and use its people member rather than creating your own.</remarks>
Public Class People






#Region "Public Members"
    Public people_list As New List(Of Entity)
#End Region



#Region "Public Functions"

    ''' <summary>
    ''' Crreate a new person and add them to the people_list
    ''' </summary>
    ''' <param name="LocalUserId">Enter the UserId of this person (ie. the id of this person in your system)</param>
    ''' <returns>The newly created person. </returns>
    ''' <remarks>The new person is an entity with one property (client_integration_id). Use the "AddProperty" method on the returned entity to add other attributes (like firstname, lastname, email, etc)</remarks>
    Public Function createPerson(ByVal LocalUserId As String) As Entity
        Dim person As New Entity()
        person.AddPropertyValue("client_integration_id", LocalUserId)

        people_list.Add(person)
        Return person
    End Function
#End Region













End Class

