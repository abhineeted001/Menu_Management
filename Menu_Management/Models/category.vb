'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated from a template.
'
'     Manual changes to this file may cause unexpected behavior in your application.
'     Manual changes to this file will be overwritten if the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Imports System
Imports System.Collections.Generic

Partial Public Class category
    Public Property CATEGORY_ID As Integer
    Public Property CATEGORY_NAME As String

    Public Overridable Property menus As ICollection(Of menu) = New HashSet(Of menu)

End Class
