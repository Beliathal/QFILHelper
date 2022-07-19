<b>QFIL Helper</b> is a console app created out of the necessity to automate the process of partition backup with QFIL utility.  As one of the “lucky” owners of the sprint branded LG v50, I was faced with the grim prospect of making manual backup of every available partition, every time I’d want to upgrade, downgrade, tamper with the system files or mess with Magisk modules. 

While researching this subject, I've stumbled upon [this guide](https://forum.xda-developers.com/t/tutorial-full-flash-backup-and-restore.4362809/) explaining in great detail how to use fh_loader.exe supplied within QFIL (QPST) utility. Then I've decided to take it one step further and automate the entire process…

Features:

◆ Backup Partitions
<ul>
<li>	Performs an automated backup of all aviable partitions. </li>
<li>	COM?_PartitionsList.xml must be up-to-date. </li>
<li>	Userdata will be skipped due to its size. </li>
</ul>

◆ Backup LUNs
<ul>
<li>	Performs an automated backup of LUNs 0-1-2-4-5.</li>
<li>	LUN0 backup will exclude Userdata due to its size.</li>
</ul>

◆ Backup hidden partitions
<ul>
<li>	Some partitions, like DDR, CDT  (LUN3), DevInfo, Limits (LUN4) are hidden.</li>
<li>	QFIL Partition manager won't display them nor exported to COM?_PartitionsList.xml.</li>
<li>	QFIL Helper will attempt to locate and save those partitions.</li>
</ul>

◆ Backup hidden LUNs
<ul>
<li>	LUNs 3 and 6 are hidden and won't show-up in QFIL Partiton Manager.</li>
<li>	There's no possible way for me to calculate the size of these LUNs properly for every model of v50/G8.</li>
<li>  QFIL Helper will ask the user to enter LUN number (3,6) and its size in sectors.</li>
<li>  Use this mode only if you're absolutely sure you know what you doing.</li>
</ul>

Things to remember:

⚑ It's recommended, but not mandatory, to place QFIL Helper in the same directory where QFIL was installed.

	Usualy: C:\Program Files (x86)\Qualcomm\QPST\bin\

⚑ Every time you make a backup with QFIL Helper, it'll be saved in: Backup-Year-Month-Day-Hour-Minute-Seconds

	For Example, if today's date is 2022-07-20 and current time is 18:14:44,
	then the backup directory would be named like this:
	
	Backup-2022-07-20-18-14-44
	
⚑ COM?_PartitionsList.xml contains the layout of all partitions in your device and is needed for QFIL Helper to function properly. 
By default QFIL Helper will attempt to look for COM?_PartitionsList.xml file in this folder: 

	C:\Users\Your User Name\AppData\Roaming\Qualcomm\QFIL\

<i>v1.0.0.61 was tested sucefully with:</i>

	Android 9, LG V450PM, Qualcomm USB Driver v1.00.37, QPST v2.7.496. 

<i>Update: 19-07-2022, v1.0.0.373: Tested sucefully with:</i>

	Android 10, LG V450PM, Qualcomm USB Driver v1.00.37, QPST v2.7.496. 

How to use:
<ul>
<li>Connect your phone to the PC in EDL mode</li>
<li>Launch QFIL and open Partition Manager</li>
<li>Click on "Save Partition File"</li>
<li>Keep running QFIL and Partition Manager open</li>
<li>Run QFIL helper</li>
</ul>
