Imports System.IO
Imports System.Environment
Imports Microsoft.VisualBasic

Public Class clsInit : Inherits clsInfo

    Protected gsCOMPort As String
    Protected gsFileName As String
    Protected gsDirName As String
    Protected gsFHLoader As String

    Protected ResetBackupDate As Action = _
        Sub() gsDirName = "Backup-" & System.DateTime.Now.ToString("yyyy-MM-dd-HHmmss")

    Private CalcDaysDif As Func(Of UInt16) = _
        Function() Now.DayOfYear - File.GetLastWriteTime(gsFileName).DayOfYear

    Private ReturnProgramFiles As Func(Of String) = _
        Function() GetFolderPath(SpecialFolder.ProgramFilesX86) _
                    & "\Qualcomm\QPST\bin\fh_loader.exe"

    Private ReturnRoaming As Func(Of String) = _
        Function() GetFolderPath(SpecialFolder.ApplicationData) _
                    & "\Qualcomm\QFIL\" & gsCOMPort & "_PartitionsList.xml"

    Private InitFileName As Action = _
        Sub() gsFileName = gsCOMPort & "_PartitionsList.xml"

    Public Sub QueryCOMPorts()

        'Dim cCurKey As Char
        Dim tCOMPort = New Timers.Timer

        tCOMPort.AutoReset = True
        tCOMPort.Interval = 500
        AddHandler tCOMPort.Elapsed, AddressOf WaitForCOMPort
        tCOMPort.Start()

        'Do

        Console.WriteLine(goSpeaker.ID2Msg(30))
        Console.ReadKey(True)

        'cCurKey = Console.ReadKey(True).KeyChar
        'Loop Until cCurKey = "Q" OrElse cCurKey = "q"

        tCOMPort.Enabled = False
        tCOMPort.Stop()

    End Sub

    Private Sub WaitForCOMPort( _
                              ByVal sender As Object, _
                              ByVal e As System.Timers.ElapsedEventArgs)

        Static iBlinker As UInt16
        Dim eColor As System.ConsoleColor
        Dim sPortName As String

        sender.Enabled = False

        If Not isCOMPort(sPortName) Then

            If iBlinker Mod 2 = 0 Then

                eColor = Console.ForegroundColor
                Console.ForegroundColor = ConsoleColor.Yellow
                'Console.SetCursorPosition(0, Console.CursorTop - 1)
                Console.Write(goSpeaker.ID2Msg(31))
                Console.ForegroundColor = eColor
                Console.CursorTop -= 1

                iBlinker += IIf(iBlinker = UInt16.MaxValue, 0, 1)

            Else

                eColor = Console.ForegroundColor
                Console.ForegroundColor = Console.BackgroundColor
                'Console.SetCursorPosition(0, Console.CursorTop - 1)
                Console.Write(goSpeaker.ID2Msg(31))
                Console.ForegroundColor = eColor
                Console.CursorTop -= 1

                iBlinker += IIf(iBlinker = UInt16.MaxValue, 0, 1)

            End If

            sender.Enabled = True

        Else

            sender.Enabled = False
            sender.Stop()

            eColor = Console.ForegroundColor
            Console.ForegroundColor = Console.BackgroundColor
            'Console.SetCursorPosition(0, Console.CursorTop - 1)
            Console.Write(goSpeaker.ID2Msg(31))
            Console.ForegroundColor = eColor
            Console.CursorTop -= 1

            Console.ForegroundColor = ConsoleColor.Green
            Console.WriteLine(goSpeaker.ID2Msg(32) & sPortName)
            Console.ForegroundColor = eColor

        End If

    End Sub

    ' This function requires referrence to System.Management assembly. It would iterate thru all
    ' aviable devices in Windows, looking for "Qualcomm". There's yet another way to do this,
    ' using QPST assembly supplied with Qualcomm's USB drivers which can be refereced in the VS.

    Private Function isCOMPort(ByRef sPortName As String) As Boolean

        If Ports.SerialPort.GetPortNames.Count = 0 Then Return False

        Dim oSearcher As Management.ManagementObjectSearcher
        Dim oCollection As Management.ManagementObjectCollection

        oSearcher = New Management.ManagementObjectSearcher("Select * from Win32_PnPEntity")
        oCollection = oSearcher.Get

        For Each oEntity As Management.ManagementObject In oCollection

            sPortName = oEntity.Properties.Item("Name").Value

            If sPortName = "" Then Continue For
            If sPortName.IndexOf("Qualcomm HS-USB QDLoader 9008") > -1 Then Return True

        Next

    End Function

    Public Function ValidateFiles() As Boolean

        ' Is there a Qualcomm device connected?
        If Not isCOMPort(gsCOMPort) Then

            Console.WriteLine(goSpeaker.ID2Msg(33))
            Console.WriteLine(goSpeaker.ID2Msg(34))
            Console.WriteLine(goSpeaker.ID2Msg(21))
            Console.ReadKey(True)
            Return False

        End If

        ' Attempt to extract COM port number
        If Not ParseCOMPortNumber(gsCOMPort) Then

            Console.WriteLine(goSpeaker.ID2Msg(27))
            Console.WriteLine(goSpeaker.ID2Msg(21))
            Console.ReadKey(True)
            Return False

        End If

        ' Is there a PartitionsList file with mathing COM port number?
        If Not LocatePartitionsList() Then

            Console.WriteLine(goSpeaker.ID2Msg(19))
            Console.WriteLine(goSpeaker.ID2Msg(21))
            Console.ReadKey(True)
            Return False

        End If

        ' Has the user lunched QFIL.exe?
        If Process.GetProcessesByName("QFIL").Count = 0 Then

            Console.WriteLine(goSpeaker.ID2Msg(41))
            Console.WriteLine(goSpeaker.ID2Msg(42))
            Console.WriteLine(goSpeaker.ID2Msg(21))
            Console.ReadKey(True)
            Return False

        End If

        ' Is PartitionsList.xml file older than 1 day?
        If Not OutdatedWarning() Then Return False

        LocateFHLoader()
        Return True

    End Function

    ' This is old function that would look for the most recent version of the PartitionsList.xml file
    ' by checking the modification date parameter. The user can have more than one such file if they 
    ' connect their phone to multiple different USB ports which in turn would produce different COM numbers... 

    ' I've abanded this algorithm in favor of the one used above, where the COM number is determent 
    ' by iteration tru Windows devices list.

    Public Function ValidateFiles_Debug() As Boolean

        Dim saFileList() As String = Directory.GetFiles( _
            Directory.GetCurrentDirectory, "*PartitionsList.xml")

        Dim isPartitionList As Boolean
        Dim dLastDate As Date = Date.MinValue
        Dim dCurDate As Date = Date.MinValue
        Dim sCurFile As String

        For iCnt As UInt16 = 0 To saFileList.Length - 1

            If saFileList(iCnt).IndexOf("_PartitionsList.xml") > -1 And _
               saFileList(iCnt).IndexOf("COM") > -1 Then

                sCurFile = Path.GetFileName(saFileList(iCnt))
                dCurDate = File.GetLastWriteTime(sCurFile)

                If dCurDate > dLastDate Then _
                    gsFileName = sCurFile : dLastDate = dCurDate : isPartitionList = True

            End If

        Next

        If Not isPartitionList Then
            Console.WriteLine(goSpeaker.ID2Msg(19))
            Console.WriteLine(goSpeaker.ID2Msg(21))
            Console.ReadKey()
            Return False

        ElseIf Not ParseCOMPortNumber() Then
            Console.WriteLine(goSpeaker.ID2Msg(27))
            Console.WriteLine(goSpeaker.ID2Msg(21))
            Console.ReadKey()
            Return False

        End If

        LocateFHLoader()
        Return True

    End Function

    Private Overloads Function ParseCOMPortNumber( _
                                                 ByRef sPortName As String) As Boolean

        If sPortName.IndexOf("COM") < 0 Then Return False

        gsCOMPort = sPortName.Replace("(", "")
        gsCOMPort = sPortName.Replace(")", "")
        gsCOMPort = sPortName.Substring(sPortName.IndexOf("COM"))

        Return True

    End Function

    ' This is old function that was ment to extract the port number from PartitionsList.xml name
    ' It's been replaced with the function above and is needed for debugin purpouses when the phone is
    ' not connected to the PC

    Private Overloads Function ParseCOMPortNumber() As Boolean

        ' File Name Example: COM13_PartitionsList.xml
        ' Start immidiately after COM and continue until _ sign
        ' Everything in-between should be numbers

        Dim iPortNumber As UInt16

        For iCnt As UInt16 = _
            gsFileName.IndexOf("COM") + 3 To gsFileName.Length - 1

            If UInt16.TryParse(gsFileName(iCnt), iPortNumber) Then
                gsCOMPort &= gsFileName(iCnt)
                ParseCOMPortNumber = True

            ElseIf gsFileName(iCnt) = "_" Then
                Exit For
            End If

        Next

    End Function

    Protected Sub CreateBackupFolder()

        ResetBackupDate()
        Directory.CreateDirectory(gsDirName)
        FileSystem.FileCopy(gsFileName, getDirWSlash & gsFileName)

    End Sub

    ' Checks if Backup folder doesn't contain any *.bin files, 
    ' if so, then backup has failed and folder should be removed

    Protected Sub CleanUpBackupFolder()

        If Directory.GetFiles(gsDirName, "*.bin").Length > 0 Then Exit Sub

        ' True to force delete non empty Dir
        Directory.Delete(gsDirName, True)

    End Sub

    Protected ReadOnly Property getDirWSlash() As String

        Get
            Return gsDirName & "\"
        End Get

    End Property

    Private Function OutdatedWarning() As Boolean

        If CalcDaysDif() = 0 Then Return True

        Dim sLine1 As String = goSpeaker.ID2Msg(43).Replace("$", gsFileName)
        Dim sLine2 As String = goSpeaker.ID2Msg(44).Replace("@", Now.ToShortDateString)

        sLine1 = sLine1.Replace("@", File.GetLastWriteTime(gsFileName).ToShortDateString)

        Console.WriteLine(sLine1)
        Console.WriteLine(sLine2)
        Console.WriteLine(goSpeaker.ID2Msg(45))
        Console.WriteLine(goSpeaker.ID2Msg(46))

        If Console.ReadKey(True).Key <> _
            ConsoleKey.Y Then Return False

        Console.Clear()
        Return True

    End Function

    ' Displays: Press any key to return to the main menu

    Protected Sub ProcessCompletedMsg()

        If gbFailed Then _
             Console.WriteLine(goSpeaker.ID2Msg(47)) _
        Else Console.WriteLine(goSpeaker.ID2Msg(48))

        Console.WriteLine(goSpeaker.ID2Msg(17))
        Console.ReadKey(True)

    End Sub

    '1. Is there a PartitionsList.xml at System Drive:\Users\Username\AppData\Roaming\Qualcomm\QFIL\ ?
    '2. Is there a PartitionsList.xml at System Drive:\Program Files (x86)\Qualcomm\QPST\bin\ ?
    '3. Is there a PartitionsList.xml at Current Directory?

    Private Function LocatePartitionsList() As Boolean

        ' Environment.SpecialFolder.ApplicationData - Roaming
        ' Environment.SpecialFolder.LocalApplicationData - Local
        ' Environment.SpecialFolder.ProgramFilesX86
        ' Environment.GetEnvironmentVariable("ProgramFiles(x86)")

        InitFileName()

        Dim sSrcFile As String = ReturnRoaming()
        Dim sDstFile As String = Directory.GetCurrentDirectory() & "\" & gsFileName

        If File.Exists(sSrcFile) Then

            File.Copy(sSrcFile, sDstFile, True)
            Return True

        End If

        ' sSrcFile = ReturnProgramFiles()

        ' If File.Exists(sSrcFile) Then

        '   File.Copy(sSrcFile, sDstFile, True)
        '   Return True

        ' End If

        ' If a copy exists withing QFIL Helper folder

        If File.Exists(gsFileName) Then Return True

    End Function

    Private Sub LocateFHLoader()

        gsFHLoader = ReturnProgramFiles()

        If File.Exists(gsFHLoader) Then Exit Sub

        ' IF fh_loader is missing in QFIL instalation folder

        If Not File.Exists("fh_loader.exe") Then
            File.WriteAllBytes("fh_loader.exe", My.Resources.fh_loader)
            gsFHLoader = "fh_loader.exe"
        End If

        ' The reason why I preffer not to use the attached fh_loader is because 
        ' the urser's build of the QFIL might differ from the one I'm using and 
        ' the attached fh_loader might not function correctly.

    End Sub

End Class
