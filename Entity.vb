Public Class Entity
   

    Private profileProperties As New Dictionary(Of String, String)
    Private collections As New Dictionary(Of String, Entity)


    Public Sub AddPropertyValue(ByVal Key As String, ByVal Value As String)
        'Key uses DotNotation. (eg. "Address.Line1")
        If (Not String.IsNullOrEmpty(Value)) Then


            Dim keys As String() = Key.Split(".")
            If keys.Count = 1 Then
                If Not profileProperties.ContainsKey(Key) Then
                    profileProperties.Add(Key, Value)
                Else
                    profileProperties(Key) = Value
                End If

            Else
                Key = Key.Replace(keys(0) & ".", "")
                If Not collections.ContainsKey(keys(0)) Then
                    Dim ent As New Entity()
                    collections.Add(keys(0), ent)

                End If


                collections(keys(0)).AddPropertyValue(Key, Value)
            End If
        End If
    End Sub

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

End Class
