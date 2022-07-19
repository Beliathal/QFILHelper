Imports System.Reflection

Public Class clsInfo

    Protected gsLabel As String
    Protected gsLUN As String
    Protected gsStart As String
    Protected gsSectors As String

    Protected giLUN As Byte
    Protected giStart As UInt32
    Protected giSectors As UInt32
    Protected gbFailed As Boolean

    Private Const scHiddenLUN3 As String = "2048"
    Private Const scHiddenLUN6 As String = "1024"

    Protected ReadOnly Property HiddenPart() As String
        Get
            Return "_hidden_partition_b-" & gsStart & "_s-" & gsSectors
        End Get
    End Property

    Protected ReadOnly Property HiddenLUN() As String
        Get
            Return "_hidden" & "_s-" & gsSectors
        End Get
    End Property

    ' Used for converting Hidden Partition Sectors to String

    Protected WriteOnly Property Sectors() As UInt32

        Set(value As UInt32)
            gsSectors = value.ToString
        End Set

    End Property

    Protected WriteOnly Property Start() As UInt32

        Set(value As UInt32)
            gsStart = value.ToString
        End Set

    End Property

    Protected WriteOnly Property LUN() As Byte

        Set(value As Byte)
            gsLUN = value.ToString
        End Set

    End Property

    Protected ReadOnly Property Size() As UInt32

        Get
            Return giStart + giSectors
        End Get

    End Property

    ' Loop thru declared variables and reset them. 

    ' I know that using reflection without a good reason is lame... 
    ' I there are other methods of doings this.... 
    ' But I'm too lazy to implement them :)

    Protected Sub ResetInfo()

        Dim oaPtr() As FieldInfo = _
            GetType(clsInfo).GetFields(BindingFlags.NonPublic Or BindingFlags.Instance)

        ' Use Dim sName As string = oPtr.Name for getting the name of the variable
        ' Use TypeOf oPtr.GetValue(Me) Is Nothing for empty strings

        For Each oPtr As FieldInfo In oaPtr

            If TypeOf oPtr.GetValue(Me) Is String Then oPtr.SetValue(Me, String.Empty)
            If TypeOf oPtr.GetValue(Me) Is UInt32 Then oPtr.SetValue(Me, UInt32.MinValue)
            If TypeOf oPtr.GetValue(Me) Is Boolean Then oPtr.SetValue(Me, False)

        Next

    End Sub

    ' There's no way to know the exact number of sectors contained within the hidden LUNs, 
    ' as their sizes can change depending on the phone model, OS version & build number.

    ' This is why I've decided to let the user enter this info manualy, but I still can
    ' help by suggesting a possible number based on this guide:
    ' https://forum.xda-developers.com/t/tutorial-full-flash-backup-and-restore.4362809/

    Protected Function SuggestSectors(ByVal iCurLUN As Byte) As String

        Select Case iCurLUN
            Case 3 : Return scHiddenLUN3
            Case 6 : Return scHiddenLUN6
        End Select

        Return "4096"

    End Function

End Class
