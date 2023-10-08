Imports System.IO
Imports System.Environment
Imports Microsoft.VisualBasic

Public Class clsInit : Inherits clsBase

    Public Sub QueryCOMPorts()

        Dim tCOMPort = New Timers.Timer

        tCOMPort.AutoReset = True
        tCOMPort.Interval = 500
        AddHandler tCOMPort.Elapsed, AddressOf WaitForCOMPort
        tCOMPort.Start()

        Console.WriteLine(goSpeaker.ID2Msg(30))
        Console.ReadKey(True)

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

        If Not FindCOMPort(sPortName) Then

            If iBlinker Mod 2 = 0 Then

                eColor = Console.ForegroundColor
                Console.ForegroundColor = ConsoleColor.Yellow
                Console.Write(goSpeaker.ID2Msg(31))
                Console.ForegroundColor = eColor
                Console.CursorTop -= 1

                iBlinker += IIf(iBlinker = UInt16.MaxValue, 0, 1)

            Else

                eColor = Console.ForegroundColor
                Console.ForegroundColor = Console.BackgroundColor
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

    Private Function FindCOMPort(ByRef sPortName As String) As Boolean

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

    ' This function will extract COM port number from: "Qualcomm HS-USB QDLoader 9008 (COM?)"

    Private Function ParseCOMPortNumber(ByRef sPortName As String) As Boolean

        If sPortName.IndexOf("COM") < 0 Then Return False

        gsCOMPort = sPortName.Replace("(", "")
        gsCOMPort = sPortName.Replace(")", "")
        gsCOMPort = sPortName.Substring(sPortName.IndexOf("COM"))

        Return True

    End Function

    ' Test to see if COMPort detected, if QFIL is running and FHLoader is present: ValidateCQF

    Protected Function ValidateCQF() As Boolean

        ' Is there a Qualcomm device connected?
        If Not FindCOMPort(gsCOMPort) Then

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

        ' Has the user lunched QFIL.exe?
        If Process.GetProcessesByName("QFIL").Count = 0 Then

            Console.WriteLine(goSpeaker.ID2Msg(41))
            Console.WriteLine(goSpeaker.ID2Msg(42))
            Console.WriteLine(goSpeaker.ID2Msg(21))
            Console.ReadKey(True)
            Return False

        End If

        LocateFHLoader()
        Return True

    End Function

    ' Creates new backup folder according to this pattern: 
    ' Backup-year-month-days-houes-minutes-seconds

    Protected Sub CreateBackupFolder()

        gsBackupDir = "Backup-" & _
            DateTime.Now.ToString("yyyy-MM-dd-HHmmss")

        Directory.CreateDirectory(gsBackupDir)
        FileSystem.FileCopy(gsPXMLList, gsBackupDir & "\" & gsPXMLList)

    End Sub

    ' If for some reason backup has failed, and there arern't any *.bin files within the backup folder, 
    ' then it should be deleted, to avoid creating garbage backup folders...

    Protected Sub CleanUpBackupFolder()

        If Directory.GetFiles(gsBackupDir, "*.bin").Count > 0 Then Exit Sub

        ' True to force delete non empty Dir
        Directory.Delete(gsBackupDir, True)

    End Sub

    ' Displays: Press any key to return to the main menu

    Protected Sub ProcessCompletedMsg()

        If gbFailed <= 1 Then

            If gbFailed = 1 Then _
                 Console.WriteLine(goSpeaker.ID2Msg(47)) _
            Else Console.WriteLine(goSpeaker.ID2Msg(48))

            Console.WriteLine(goSpeaker.ID2Msg(17))
            Console.ReadKey(True)

        End If

        gbFailed = 0

    End Sub

    Protected Sub LocateFHLoader()

        gsFHLoader = getProgramFiles()

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

    Protected Function ExecuteCommand( _
                          ByRef sCMDLine As String) As Boolean

        Dim oProcess As New Process
        Dim sError, sBuffer As String

        oProcess.StartInfo.Arguments = sCMDLine
        oProcess.StartInfo.CreateNoWindow = False
        'oProcess.StartInfo.FileName = "fh_loader.exe"
        oProcess.StartInfo.FileName = gsFHLoader
        oProcess.StartInfo.UseShellExecute = False
        oProcess.StartInfo.RedirectStandardOutput = True
        oProcess.StartInfo.RedirectStandardError = True
        oProcess.Start()

        sBuffer = oProcess.StandardOutput.ReadToEnd()
        sError = oProcess.StandardError.ReadToEnd()

        oProcess.WaitForExit()
        oProcess.Close()
        oProcess.Dispose()
        oProcess = Nothing

        Console.WriteLine(sBuffer)
        Console.WriteLine(sError)

        If sBuffer.Contains("Failed to open com port") OrElse _
            sBuffer.Contains("ERROR: Could not write to") OrElse _
            sBuffer.Contains("SAHARA mode!!") Then
            Console.Clear()
            Console.WriteLine(goSpeaker.ID2Msg(11) & vbCrLf)
            Console.WriteLine(goSpeaker.ID2Msg(12))
            Console.WriteLine(goSpeaker.ID2Msg(13))
            Console.WriteLine(goSpeaker.ID2Msg(14))
            Console.WriteLine(goSpeaker.ID2Msg(15))
            Console.WriteLine(goSpeaker.ID2Msg(16) & vbCrLf)
            'Console.WriteLine(vbCrLf & goSpeaker.ID2Msg(17))
            'Console.ReadKey(True)
            gbFailed = 1
        Else : Return True
        End If

    End Function

End Class
