Imports System.IO
Imports Microsoft.VisualBasic

Public Class clsInit : Inherits clsInfo

    Public Overloads Function ValidateFiles() As Boolean

        Dim saFileList() As String = _
            Directory.GetFiles(Directory.GetCurrentDirectory)

        Dim isPortNumber As Boolean
        Dim isPartitionList As Boolean

        For iCnt As UInt16 = 0 To saFileList.Length - 1

            If saFileList(iCnt).IndexOf("_PartitionsList.xml") > -1 And _
               saFileList(iCnt).IndexOf("COM") > -1 Then

                isPartitionList = True
                sFileName = Path.GetFileName(saFileList(iCnt))
                isPortNumber = ParseCOMPortNumber()
                Exit For

            End If

        Next

        If Not isPartitionList Then
            Console.WriteLine(ID2Msg(19))
            Console.WriteLine(ID2Msg(21))
            Console.ReadKey()
            Return False

        ElseIf Not isPortNumber Then
            Console.WriteLine(ID2Msg(20))
            Console.WriteLine(ID2Msg(21))
            Console.ReadKey()
            Return False

        End If

        ' IF fh_loader is missing in QFIL instalation folder

        If Not File.Exists("fh_loader.exe") Then
            File.WriteAllBytes("fh_loader.exe", My.Resources.fh_loader)
        End If

        ' The reason why I preffer not to use the attached fh_loader is because 
        ' the urser's build of the QFIL might differ from the one I'm using and 
        ' the attached fh_loader might not function correctly.

        Return True

    End Function

    Private Overloads Function ValidateFiles(ByVal isOutdatedCode As Boolean) As Boolean

        ' Get all files in current folder

        Dim saFileList() As String = _
            Directory.GetFiles(Directory.GetCurrentDirectory)

        Dim isPortNumber As Boolean
        Dim isPartitionList As Boolean
        Dim isFHLoader As Boolean

        For iCnt As UInt16 = 0 To saFileList.Length - 1

            If saFileList(iCnt).IndexOf("_PartitionsList.xml") > -1 And _
               saFileList(iCnt).IndexOf("COM") > -1 Then

                isPartitionList = True
                sFileName = Path.GetFileName(saFileList(iCnt))
                isPortNumber = ParseCOMPortNumber()

            ElseIf saFileList(iCnt).IndexOf("fh_loader.exe") > -1 Then
                isFHLoader = True

            ElseIf saFileList(iCnt).IndexOf("fh_loader.exe") < 0 Then
                File.WriteAllBytes("fh_loader.exe", My.Resources.fh_loader)
            End If

            ' Found both the fh_loader.exe and the PartitionsList.xml
            If isFHLoader And isPortNumber Then Exit For

        Next

        If Not isFHLoader Then
            Console.WriteLine(ID2Msg(18))
            Console.WriteLine(ID2Msg(21))
            Console.ReadKey()
            Return False

        ElseIf Not isPartitionList Then
            Console.WriteLine(ID2Msg(19))
            Console.WriteLine(ID2Msg(21))
            Console.ReadKey()
            Return False

        ElseIf Not isPortNumber Then
            Console.WriteLine(ID2Msg(20))
            Console.WriteLine(ID2Msg(21))
            Console.ReadKey()
            Return False

        End If

        'Directory.CreateDirectory(sDirName)
        'FileSystem.FileCopy(sFileName, DirName & sFileName)
        Return True

    End Function

    Private Function ParseCOMPortNumber() As Boolean

        ' File Name Example: COM13_PartitionsList.xml
        ' Start immidiately after COM and continue until _ sign
        ' Everything in-between should be numbers

        Dim iPortNumber As UInt16

        For iCnt As UInt16 = _
            sFileName.IndexOf("COM") + 3 To sFileName.Length - 1

            If UInt16.TryParse(sFileName(iCnt), iPortNumber) Then
                sCOMPort &= sFileName(iCnt)
                ParseCOMPortNumber = True

            ElseIf sFileName(iCnt) = "_" Then
                Exit For
            End If

        Next

    End Function

    Protected Sub CreateBackupFolder()

        ResetBackupDate()
        Directory.CreateDirectory(sDirName)
        FileSystem.FileCopy(sFileName, DirName & sFileName)

    End Sub

    ' Checks if Backup folder doesn't contain any *.bin files, 
    ' if so, then backup has failed and folder should be removed

    Protected Sub CleanUpBackupFolder()

        If Directory.GetFiles(sDirName, "*.bin").Length > 0 Then Exit Sub

        ' True to force delete non empty Dir
        Directory.Delete(sDirName, True)

    End Sub

End Class
