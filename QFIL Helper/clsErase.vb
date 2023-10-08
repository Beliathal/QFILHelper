Imports System.IO
Imports Microsoft.VisualBasic
Imports System.Text.RegularExpressions

Public Class clsErase : Inherits clsFlash

    Public Sub ErasePartitions(ByRef saLookUp() As String)

        Dim isDebug As Boolean = False
        Dim sCMDLine As String

        If Not ValidateCQF(isDebug) Then Exit Sub

        ResetLookUp()

        Dim obPInfo As stPInfo
        Dim isExec As Boolean ' Did we execute even 1 command?

        If Not DisplayWarning(saLookUp, WarningType.WR_ERS) Then GoTo EXT
        If Not ReadGPTHeaders(True, True, isDebug) Then GoTo EXT

        For Each sLabel As String In saLookUp

            If Not Regex.IsMatch(sLabel.ToLower, "\w+") Then Continue For

            obPInfo = Nothing
            obPInfo.sLabel = sLabel.ToLower

            ' If partition info not found: 
            ' Continue proccessing other files or exit?

            If Not ValidatePartLabel(obPInfo) Then _
                If geFailed = ErrorType.ER_ABORT Then _
                GoTo EXT _
                Else Continue For

            BuildXML(obPInfo)
            sCMDLine = BuildCommand(obPInfo, True)

            If isDebug Then Continue For
            If Not ExecuteCommand(sCMDLine) Then Exit For

            isExec = True

        Next

EXT:
        ProcessCompletedMsg(isExec Or isDebug)
        obPInfo = Nothing

    End Sub

    Public Sub SPErase(ByVal eCode As OPCode)

        Dim saLookUp() As String = LoadQuickList(eCode)

        ErasePartitions(saLookUp)
        Erase saLookUp

    End Sub

    Public Sub ManualErase()

        Console.Clear()
        Console.WriteLine(goSpeaker.ID2Msg(49))
        Console.WriteLine(goSpeaker.ID2Msg(54))
        Console.WriteLine(goSpeaker.ID2Msg(55) & vbCr)
        Console.WriteLine(goSpeaker.ID2Msg(52) & vbCrLf)

        Dim sInput As String
        Dim saParams() As String

        Console.CursorVisible = True
        sInput = Console.ReadLine()
        Console.CursorVisible = False

        If sInput = Nothing Then Exit Sub

        saParams = sInput.ToLower.Split(";")

        ErasePartitions(saParams)
        Erase saParams

    End Sub

    Private Sub BuildXML(ByRef obPInfo As stPInfo)

        Dim saXMLErase() As String

        saXMLErase = My.Resources.erase_xml.Split( _
            vbCrLf.ToArray, StringSplitOptions.RemoveEmptyEntries)

        saXMLErase(2) = saXMLErase(2).Replace("#", obPInfo.iLUN)
        saXMLErase(2) = saXMLErase(2).Replace("@", obPInfo.iStart)
        saXMLErase(2) = saXMLErase(2).Replace("$", obPInfo.iSectors)

        File.WriteAllLines(getTempFolder() & "FHLoaderErase.xml", saXMLErase)

    End Sub

    ' fh_loader.exe --port=\\.\COM5 --sendxml=FHLoaderErase.xml --search_path=%TEMP%\QFILHelper 
    ' --noprompt --showpercentagecomplete --zlpawarehost=1 --verify_programming --memoryname=ufs 

    Private Overloads Function BuildCommand( _
                                           ByRef obPInfo As stPInfo, _
                                           ByRef sXMLPath As String) As String

        Dim sMerged As String
        Dim saMerge() As String = { _
                    " --port=\\.\$", _
                    " --sendxml=FHLoaderErase.xml", _
                    " --search_path=#", _
                    " --noprompt --showpercentagecomplete --zlpawarehost=1" & _
                    " --verify_programming --memoryname=ufs"}

        ' Remove "\"
        sXMLPath = sXMLPath.Substring(0, sXMLPath.Length - 1)

        saMerge(0) = saMerge(0).Replace("$", gsCOMPort)
        saMerge(2) = saMerge(2).Replace("#", sXMLPath)

        For iCnt As Byte = 0 To saMerge.Count - 1
            sMerged &= saMerge(iCnt)
        Next

        ShowProgress(obPInfo, ProgressType.PR_ERS)

        Erase saMerge
        Return sMerged

    End Function

End Class
