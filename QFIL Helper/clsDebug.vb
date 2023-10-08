Imports System.IO
Imports System.Management
Imports Microsoft.VisualBasic
Imports System.Text.RegularExpressions

Public Class clsDebug : Inherits clsErase

    Public Sub Test(ByVal eCode As OPCode)
        Stop
    End Sub

    Private Sub LINQSortAscii(ByRef lsPInfo As List(Of stPInfo))

        ' Dim lsSorted = _
        ' From obTInfo As stPInfo In lsPInfo _
        ' Order By obTInfo.iLabel Ascending Select obTInfo()

        ' lsPInfo = lsSorted.ToList()
        ' lsSorted = Nothing

    End Sub

    Protected Function Ascii2Int(ByRef sPName As String) As UInt64

        Ascii2Int = 1

        For iCnt As UInt16 = 0 To sPName.Length - 1
            Ascii2Int = Ascii2Int + Asc(sPName(iCnt))
        Next

    End Function

    Protected Function Ascii2IntRec(ByRef sPName As String, _
                                        Optional ByVal iCnt As Integer = 0) As UInt64

        If iCnt = sPName.Length Then Return 0

        Return Asc(sPName(iCnt)) + Ascii2IntRec(sPName, iCnt + 1)

    End Function

    Protected Function BinarySearch(ByRef obPInfo As stPInfo) As Boolean

        Dim iLBound, iUBound As Byte
        Dim iLow, iMid, iHigh As Int16
        Dim iLookUp As UInt16

        Dim iLabel As UInt16 = _
            Ascii2Int(obPInfo.sLabel.ToLower)

        If obPInfo.iLUN.HasValue Then

            iLBound = obPInfo.iLUN
            iUBound = obPInfo.iLUN

        Else

            iLBound = 0
            iUBound = galoLookUp.Count - 1

        End If

        For iCnt As Byte = iLBound To iUBound

            iLow = 0
            iMid = 0
            iHigh = galoLookUp(iCnt).Count - 1

            Do Until iLow > iHigh

                iMid = iLow + (iHigh - iLow) / 2

                'iLookUp = galoLookUp(iCnt)(iMid).iLabel

                If iLabel = iLookUp Then

                    obPInfo = galoLookUp(iCnt)(iMid) : Return True

                ElseIf iLabel > iLookUp Then : iLow = iMid + 1
                ElseIf iLabel < iLookUp Then : iHigh = iMid - 1

                End If

            Loop

        Next

        Return False

    End Function


    ' Fails to find cmnlib64_a, cmnlib64_b regardless of the fact that 
    ' cmnlib_a and cmnlib_b come before them

    Protected Function BinarySearchOld(ByRef obPInfo As stPInfo) As Boolean

        Dim iLBound, iUBound As Byte
        Dim iLow, iMid, iHigh As Int16

        If obPInfo.iLUN.HasValue Then

            iLBound = obPInfo.iLUN
            iUBound = obPInfo.iLUN

        Else

            iLBound = 0
            iUBound = galoLookUp.Count - 1

        End If

        For iCnt As Byte = iLBound To iUBound

            iLow = 0
            iMid = 0
            iHigh = galoLookUp(iCnt).Count - 1

            Do Until iLow > iHigh

                iMid = iLow + (iHigh - iLow) / 2

                If obPInfo.sLabel.ToLower = _
                    galoLookUp(iCnt)(iMid).sLabel.ToLower Then

                    obPInfo = galoLookUp(iCnt)(iMid) : Return True

                ElseIf obPInfo.sLabel.ToLower > _
                        galoLookUp(iCnt)(iMid).sLabel.ToLower Then : iLow = iMid + 1

                ElseIf obPInfo.sLabel.ToLower < _
                        galoLookUp(iCnt)(iMid).sLabel.ToLower Then : iHigh = iMid - 1

                End If

            Loop

        Next

        Return False

    End Function



    Private Function BinarySearch2( _
                                       ByRef saFileList() As String, _
                                       ByRef sFind As String) As Boolean

        Dim iLow, iMid, iHigh As Int16

        iLow = 0
        iMid = 0
        iHigh = saFileList.Count - 1

        Array.Sort(saFileList)

        Do Until iLow > iHigh

            iMid = iLow + (iHigh - iLow) / 2

            Select Case True
                Case sFind = saFileList(iMid) : Return True
                Case sFind > saFileList(iMid) : iLow = iMid + 1
                Case sFind < saFileList(iMid) : iHigh = iMid - 1
            End Select

        Loop

    End Function

    Protected Sub DebugFindPartiton(ByRef sLabel As String)

        Dim obPInfo As stPInfo

        ResetLookUp()

        obPInfo.sLabel = sLabel
        LookUpNames(obPInfo)

        obPInfo = Nothing

    End Sub

    Private Sub NTFSCompression2()

        If Directory.Exists("0") Then Directory.Delete("0", True)

        Directory.CreateDirectory("0")

        'Dim Path As String = gsBackupDir
        'Dim strComputer As String = "."
        'Dim objWMIService = GetObject("winmgmts:{impersonationLevel=impersonate}!\\.\root\cimv2")
        'Dim colFolders = objWMIService.ExecQuery("Select * from Win32_Directory where name = '" & Replace(Path, "\", "\\") & "'")
        'For Each objFolder In colFolders
        'objFolder.Compress()
        'Next

        ' The userdata partiton is not dynamicly allocated, and as such, there's no way to get 
        ' the actual used space from the GPT headers. They'll always return the maximum
        ' allocated size. 

        ' While in reality the actual used space might take only a few GBs, QFIL will read entire
        ' userdata partition with alot of empty space, which can be effiently compressed, 
        ' if NTFS compression is apllied to the backup folder before the backup begins.

        ' It's not ellegant and rather ugly solution, but it's beter than nothing...

        ' NOTE: In every example the directory name is passed with single \ 
        ' while in reality it should be \\

        ' Also: Full directory path should be provided like shown below

        Dim sFolder As String = Directory.GetCurrentDirectory.Replace("\", "\\") & "\\0"
        Dim sQuery As String = String.Format("SELECT * FROM Win32_Directory WHERE Name='{0}'", sFolder)
        Dim oSearcher As New ManagementObjectSearcher(sQuery)

        For Each oItem As ManagementObject In oSearcher.Get()
            oItem.InvokeMethod("Compress", Nothing)
        Next

        ' An intertaning conv with ChatGPT:

        ' The For Each loop in the code example loops through a collection of ManagementObject instances 
        ' returned by the ManagementObjectSearcher.Get method.

        ' In this specific case, the ManagementObjectSearcher is searching for a specific directory using 
        ' the query "SELECT * FROM Win32_Directory WHERE Name='C:\test'" and the Get method returns a 
        ' collection of all ManagementObject instances that match the query. 

        ' The For Each loop then iterates through each ManagementObject in the collection, allowing 
        ' you to perform actions on each object.

        ' ME: The code assumes there are multiple "C:\Test" directories, which is not possible!

        ' ChatGPT: No, the code does not assume there are multiple directories named "C:\Test". 

        ' The ManagementObjectSearcher.Get method returns a collection of all ManagementObject instances 
        ' that match the query. In this case, the query is "SELECT * FROM Win32_Directory WHERE Name='C:\test'", 
        ' which is searching for a directory named "C:\Test".

        ' If there is only one directory named "C:\Test" on the system, then the collection returned by 
        ' Get will contain only one ManagementObject instance representing that directory. 

        ' The For Each loop will still iterate through that one instance, allowing you to perform actions on it. 

        ' If there are multiple directories with the same name, the collection returned by Get will contain a 
        ' ManagementObject for each of those directories, and the For Each loop will iterate through each instance, 
        ' allowing you to perform actions on each one.

        ' ME: Palm Face


        Console.WriteLine("NTFS compression applied to directory: " & sFolder)
        Console.ReadLine()

    End Sub

    Public Sub DebugerTester()

        'SelectiveLUNBackup({"lun8", "lun6_op", "system_a", "lun4", "lunnnn"})
        'BackupSelPartitions({"ftm", "modemst1", "modemst2", "fsg", "fsc", "poop"})


        'Dim sTemp As String
        'sTemp = getProgramFiles()
        'sTemp = getAppRoaming()
        'sTemp = getTempFolder()
        'DebugFindPartiton("")
        Stop

        'sTemp = String.Concat(Enumerable.Repeat("ab", 2))
        'sTemp = "ab" * 2

        'Dim oKey As New System.ConsoleKey
        'Dim okey2 As ConsoleKeyInfo '= Console.ReadKey(False)

        'oKey = Console.Read
        'key = Console.ReadKey(False)
        'oKey = Console.ReadKey(True).Key
        'oKey = key.Key
        'Stop

        ' If Console.KeyAvailable Then
        'okey2 = Console.ReadKey(intercept:=False)
        ' Console.Read()
        ' oKey = okey2.Key
        ' process the key press here
        'End If

        '        Select Case True

        'Case oKey = ConsoleKey.Escape : Exit Do
        'Case oKey = ConsoleKey.Enter : Exit Do

        '    Case oKey = ConsoleKey.OemMinus : sInput &= "_"
        '   Case oKey = ConsoleKey.OemComma : sInput &= ","

        '    Case oKey >= ConsoleKey.A And oKey <= ConsoleKey.Z : sInput &= Chr(oKey)
        '   Case oKey >= ConsoleKey.D0 And oKey <= ConsoleKey.D9 : sInput &= Chr(oKey)
        '  Case oKey >= ConsoleKey.NumPad0 And oKey <= ConsoleKey.NumPad9 : sInput &= Chr(oKey)
        '
        '   Case oKey = ConsoleKey.Backspace : sInput.Remove(sInput.Length - 2, 1)

        'Case Else : Stop

        'End Select

    End Sub

End Class
