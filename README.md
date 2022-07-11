<b>QFIL helper</b> is a console app created out of the necessity to automate the process of partition backup with QFIL utility.  As one of the “lucky” owners of the sprint branded LG v50, I was faced with the grim prospect of making manual backup of every available partition, every time I’d want to upgrade, downgrade, temper with the system files or mess with Magisk modules. 

While researching this subject, I’ve stumbled upon this guide [this guide](https://forum.xda-developers.com/t/tutorial-full-flash-backup-and-restore.4362809/) explaining in great detail how to use fh_loader.exe supplied within QFIL (QPST) utility. Then I’ve decided to take it one step further and automate the entire process…

In its current state QFIL Helper is capable of:

■ Making a full backup of every partition defined in COMx_PartitionsList.xml
<ul>
<li>	QFIL Helper will parse PartitionsList.xml file, extract partition name, start sector and total size and then feed this info to fh_loader.exe. </li>
<li>	Userdata will be skipped automatically. </li>
</ul>

■ Searching and saving hidden partitions
<ul>
<li>	Some partitions, like DDR, CDT  (LUN3) , DevInfo, Limits (LUN4),  are hidden and neither being displayed nor exported to partitionsList.xml</li>
<li>		QFIL Helper will attempt to locate those partitions by searching for gaps between the end sector and the start sector of two neighboring partitions</li>
</ul>

■ Making an automated backup of LUNs 0-1-2-4-5
<ul>
<li>QFIL Helper will parse PartitionsList.xml file and calculate the sizes of each LUN.</li>
<li>LUN0 backup will exclude Userdata partition.</li>
</ul>

■ Making a manual backup of the hidden LUNs 3,6
<ul>
<li>LUNs 3 & 6 aren’t shown in the QFIL Partition Manager and there’s no possible way for me to calculate their sizes properly for every model of v50/G8.</li>
<li>In this mode QFIL Helper will query the user for LUN index and the number of sectors to be saved</li>
<li>Use this mode only if you’re absolutely sure you know what you doing.</li>
</ul>

Every LUN (at least in my phone) starts with sector 6 instead of sector 0. I’ve assumed that this initial 6 sectors contain GTP layout info, but I’m not sure, hence there’s no option to save them separately, though it’s possible to add such an option, if necessary.

To prevent clutter and avoid overwriting files, QFIL Helper will create a new subfolder for every backup you’d make. The subfolder will be named according to this pattern: 
Backup-year-month-day-hours-minutes-seconds

The program was tested with: Android 9, LG V450PM, Qualcomm USB Driver v1.00.37, QPST v2.7.496. I cannot guarantee that it will work in any other configuration, id est: higher android version, different phone model, newer drivers .

How to use:
<ul>
<li>Connect your phone to the PC in EDL mode</li>
<li>Launch QFIL, connect to the phone and open Partition Manager</li>
<li>Export Partition List File from QFIL and put it in QFIL Helper folder</li>
<li>Run QFIL helper and select the backup method</li>
</ul>
