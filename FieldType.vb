''' <summary>
''' Basic class to define the possible values for Field Type in the GR system. 
''' </summary>
''' <remarks>Several of the methods in GR.NET ask for a Type (or field Type) of type string. Rather than enter "integer" is would be better to enter FieldType._integer. This insures that you don't enter an invalid FieldType</remarks>
Public Class FieldType

    Public Shared _entity As String = "entity"
    Public Shared _string As String = "string"
    Public Shared _integer As String = "integer"
    Public Shared _boolean As String = "boolean"
    Public Shared _date As String = "date"
    Public Shared _enum As String = "enum"
    Public Shared _text As String = "text"
    Public Shared _datetime As String = "datatime"
    Public Shared _float As String = "float"
    Public Shared type_list = {"entity", "string", "integer", "boolean", "date", "enum", "text", "datetime", "float"} ' the list of valid FieldTypes

    ''' <summary>
    ''' Checks if a FieldType is valid
    ''' </summary>
    ''' <param name="name"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function isValid(ByVal name As String) As Boolean

        Return type_list.Contains(name)

    End Function





End Class
