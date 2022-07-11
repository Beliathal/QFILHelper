Imports System.IO
Imports Microsoft.VisualBasic

Public Class clsMsg

    Private Enum Language As Byte
        EN_Lang = 0
        RU_Lang = 1
    End Enum

    Private sUIMsg() As String
    Private eCurLang As Language

    Public Function ValidateLanguage(ByRef sCurLang() As String)

        If sCurLang.Length = 0 Then _
            ReDim sCurLang(0 To 0)

        Select Case sCurLang(0)

            Case "-en", "-EN", ""

                If Not File.Exists("english.msg") Then
                    Console.WriteLine("Critical error: english.msg file is missing!")
                    Console.ReadKey(True)
                    Return False
                End If

                eCurLang = Language.EN_Lang

            Case "-ru", "-RU"

                If Not File.Exists("russian.msg") Then
                    Console.WriteLine("Критическая ошибка: Не найден файл russian.msg!")
                    Console.ReadKey(True)
                    Return False
                End If

                eCurLang = Language.RU_Lang

            Case Else

                Console.WriteLine("Ivalid arguments")
                Console.ReadKey(True)
                Return False

        End Select

        Return LoadMessages()

    End Function

    Private Function LoadMessages() As Boolean

        eCurLang = Language.RU_Lang

        Select Case eCurLang
            Case Language.EN_Lang : sUIMsg = File.ReadAllLines("english.msg")
            Case Language.RU_Lang : sUIMsg = File.ReadAllLines("russian.msg")
        End Select

        If sUIMsg.Length > 23 Then Return True

        Select Case eCurLang

            Case Language.EN_Lang
                Console.WriteLine("Critical error: File english.msg is corrupt!")
                Console.ReadKey(True)
                Return False

            Case Language.RU_Lang
                Console.WriteLine("Критическая ошибка: Файл russian.msg повреджен!")
                Console.ReadKey(True)
                Return False

        End Select

    End Function

    Public ReadOnly Property ID2Msg(ByVal iCurID As Byte) As String

        Get
            Return sUIMsg(iCurID - 1)
        End Get

    End Property

End Class
