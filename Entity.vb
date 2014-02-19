
''' <summary>
''' Local object model to represent GR entity types. 
''' </summary>
''' <remarks>Entities relect actual people and their profile properties. (eg. "Jon Vellacott" is an entity of entity_type 'person'. He has a child Entity (Name=FirstName, value =Jon). )</remarks>
Public Class Entity

#Region "Private Members"

    'profileproperties is a Dictionary (key/value pair) of attributes for this entity
    Private profileProperties As New Dictionary(Of String, String)

    'collections is a Dictionary (key/value pair) of child entities. The key is the name, and the value is the child Entity
    Private collections As New Dictionary(Of String, Entity)
#End Region
#Region "Public Methods"

    ''' <summary>
    ''' Recursive method to add a property (and its ancestor entities)
    ''' </summary>
    ''' <param name="Key">The name of the Property in dot notation (eg person.address.city)</param>
    ''' <param name="Value">The value of this property (eg London)</param>
    ''' <remarks></remarks>
    Public Sub AddPropertyValue(ByVal Key As String, ByVal Value As String)
        'Key uses DotNotation. (eg. "Address.Line1")
        If (Not String.IsNullOrEmpty(Value)) Then


            Dim keys As String() = Key.Split(".")
            If keys.Count = 1 Then
                'This is the direct parent of the entity that contains the property
                If Not profileProperties.ContainsKey(Key) Then
                    profileProperties.Add(Key, Value)
                Else
                    profileProperties(Key) = Value
                End If

            Else
                'The property belonds to an entity that is a decendant of this one. 
                Key = Key.Replace(keys(0) & ".", "")
                If Not collections.ContainsKey(keys(0)) Then  ' Check if the next entity exists
                    'The next entity supplied in dot notation does not exist. Create this entity and carry on
                    Dim ent As New Entity()
                    collections.Add(keys(0), ent)

                End If

                'Carry on down the family tree!
                collections(keys(0)).AddPropertyValue(Key, Value)
            End If
        End If
    End Sub


    ''' <summary>
    ''' Returns this object in JSON format
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>The JSON response can be supplied to GR API to add/update this entity (and its decendants) to the GR</remarks>
    Public Function ToJson() As String
        Dim json As String = "{"
        For Each row In profileProperties.Where(Function(c) Not String.IsNullOrEmpty(c.Value))
            json &= """" & row.Key & """: """ & row.Value & ""","
        Next
        For Each row In collections
            json &= """" & row.Key & """: "

            json &= row.Value.ToJson()
            json &= ","

        Next
        json = json.TrimEnd(",")
        json &= "}"
        Return json
    End Function
#End Region
End Class
