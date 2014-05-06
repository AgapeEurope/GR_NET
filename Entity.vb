
''' <summary>
''' Local object model to represent GR entity types. 
''' </summary>
''' <remarks>Entities relect actual people and their profile properties. (eg. "Jon Vellacott" is an entity of entity_type 'person'. He has a child Entity (Name=FirstName, value =Jon). )</remarks>
Public Class Entity

#Region "Private Members"
    Private _id As String
    Public Property ID() As String
        Get
            Return _id
        End Get
        Set(ByVal value As String)
            _id = value
        End Set
    End Property




    'profileproperties is a Dictionary (key/value pair) of attributes for this entity
    Public profileProperties As New Dictionary(Of String, List(Of String))

    'collections is a Dictionary (key/value pair) of child entities. The key is the name, and the value is the child Entity
    Public collections As New Dictionary(Of String, List(Of Entity))
#End Region
#Region "Public Methods"

    Public Function GetPropertyValue(ByVal Key As String) As String
        If Key = "id" Then
            Return ID
        ElseIf profileProperties.ContainsKey(Key) Then
            Return profileProperties(Key).GroupBy(Function(n) n).OrderByDescending(Function(g) g.Count).Select(Function(g) g.Key).FirstOrDefault

        ElseIf Key.Contains(".") Then
            Dim left_key = Key.Substring(0, Key.IndexOf("."))
            Dim right_key = Key.Substring(Key.IndexOf(".") + 1)
            If collections.ContainsKey(left_key) Then
                Return collections(left_key).GroupBy(Function(n) n.GetPropertyValue(right_key)).OrderByDescending(Function(g) g.Count).Select(Function(g) g.Key).FirstOrDefault
                '  Return collections(left_key).First.GetPropertyValue(right_key)


            End If

        End If
        Return ""
    End Function

    Public Function HasValues() As Boolean
        Return profileProperties.Count + collections.Count > 0
    End Function

    ''' <summary>
    ''' Recursive method to add a property (and its ancestor entities)
    ''' </summary>
    ''' <param name="Key">The name of the Property in dot notation (eg person.address.city)</param>
    ''' <param name="Value">The value of this property (eg London)</param>
    ''' <remarks></remarks>
    Public Sub AddPropertyValue(ByVal Key As String, ByVal Value As String)
        If Value Is Nothing Then
            Return
        End If
        If Key = "id" Then
            Me._id = Value
            Return
        End If

        'Key uses DotNotation. (eg. "Address.Line1")
        ' If (Not String.IsNullOrEmpty(Value)) Then


        Dim keys As String() = Key.Split(".")

        Dim thisKey = keys(0)
        Dim index As Integer = 0
        If thisKey.Contains("[") Then
            index = thisKey.Substring(thisKey.IndexOf("[") + 1, (thisKey.IndexOf("]") - thisKey.IndexOf("[")) - 1)
            thisKey = thisKey.Substring(0, thisKey.IndexOf("["))

        End If
        If keys.Count = 1 Then

            'This is the direct parent of the entity that contains the property
            'check for replace option, and change/add property as appropriate
            If Not profileProperties.ContainsKey(thisKey) Then

                profileProperties.Add(thisKey, {Value}.ToList)
            Else
                While profileProperties(thisKey).Count < index + 1 ' make sure the indexes exist
                    profileProperties(thisKey).Add("")
                End While

                profileProperties(thisKey)(index) = Value



            End If

        Else
            'The property belonds to an entity that is a decendant of this one. 
            Key = Key.Replace(keys(0) & ".", "")
            If Not collections.ContainsKey(thisKey) Then  ' Check if the next entity key exists
                'The next entity supplied in dot notation does not exist. Create this entity and carry on
                Dim ent As New Entity()
                collections.Add(thisKey, {ent}.ToList)

            End If
            While collections(thisKey).Count < index + 1  ' check if this index exists in the collection lit.
                Dim ent As New Entity()
                collections(thisKey).Add(ent)
            End While


            'Carry on down the family tree!

            collections(thisKey)(index).AddPropertyValue(Key, Value)


        End If


        '  End If
    End Sub


    ''' <summary>
    ''' Returns this object in JSON format
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>The JSON response can be supplied to GR API to add/update this entity (and its decendants) to the GR</remarks>
    Public Function ToJson(Optional ByVal sbc As String = "") As String
        Dim json As String = "{"
        For Each row In profileProperties.Where(Function(c) c.Value.Count > 0)
            If row.Value.Count > 1 Then
                json &= """" & row.Key & """: ["
                For Each row2 In row.Value
                    json &= "{""" & row2 & """},"
                Next
                json = json.TrimEnd(",")
                json &= "],"
            Else
                json &= """" & row.Key & """: """ & row.Value.First & ""","
            End If


        Next
        For Each row In collections
            json &= """" & row.Key & """: "
            If row.Value.Count > 1 Then
                json &= "["
                For Each row2 In row.Value
                    json &= row.Value.First.ToJson() & ","
                Next
                json = json.TrimEnd(",")
                json &= "]"
            Else
                json &= row.Value.First.ToJson()
            End If

            json &= ","

        Next
        json = json.TrimEnd(",")
        json &= "}"
        Return json
    End Function
#End Region

    
End Class
