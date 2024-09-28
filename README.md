<b>QFIL Helper</b> was created out of the necessity to automate the process of partition backup with QFIL utility.  As one of the “lucky” owners of the sprint branded LG v50, I was faced with the grim prospect of making manual backup of every available partition, every time I’d want to upgrade, downgrade, tamper with the system files or mess with Magisk modules. 

While researching this subject, I've stumbled upon [this guide](https://forum.xda-developers.com/t/tutorial-full-flash-backup-and-restore.4362809/) explaining in great detail how to use fh_loader.exe supplied within QFIL (QPST) utility. Then I've decided to take it one step further and automate the entire process…

<a href='https://ko-fi.com/F1F4W9DQ6' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://storage.ko-fi.com/cdn/kofi3.png?v=3' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>

<hr>

<b>Features:</b>

◆ Backup LUNs
<ul>
<li>	Performs an automated backup of LUNs 0-1-2-4-5.</li>
<li>	LUN0 backup will exclude Userdata due to its size.</li>
</ul>

◆ Backup Partitions
<ul>
<li>	Performs an automated backup of all available partitions. </li>
<li>	Userdata will be skipped due to its size. </li>
</ul>

◆ Detect connected COM ports
<ul>
<li>	QFIL Helper will continuously query Windows for all connected devices.</li>
<li>	If Qualcomm HS-USB QDLoader 9008 is detected, then your phone is connected in EDL mode.</li>
</ul>

◆ Flash firmware
<ul>
<li>	Restore your partitions backup.</li>
<li>	Restore your LUN backup. </li>
<li>	Flash 3rd party images like Engineering ABL.</li>
</ul>

◆ Command line
<ul>
<li>	Enables advanced options: -advanced </li>
<li>	Enables NTFS compression: -NTFS </li>
</ul>

◆ GPT (GGUID Partition Table) Edition Update
<ul>
<li>	PartitionsList.xml no longer needed </li>
<li>	LUN and partition info is pulled directly from the GPT headers </li>
<li>	The app now has access to all the info about hidden partitions and LUNs </li>
<li>	Backward compatibility with back-ups created with previous veriouns (except for hidden partitions/LUNs) </li>
<li>	Added optional NTFS compression: - NTFS </li>
<li>	Added userdata backup (with compression enabled, takes at-least 40 mins on a "good" hardware) </li>
<li>	Added option to erase partitions (not fully tested) </li>
<li>	Added option to manually specify what partitions/LUNs to backup </li>
</ul>

<hr>

<b>How to use:</b>
<ul>
<li>Connect your phone to the PC in EDL mode</li>
<li>Launch QFIL and open Partition Manager</li>
<li>Leave QFIL running with Partition Manager open</li>
<li>Run QFIL helper</li>
</ul>

<b>Things to remember:</b>

⚑ Every time you make a backup with QFIL Helper, the files will be saved in: Backup-Year-Month-Day-Hour-Minute-Seconds

	For Example, if today's date is 2022-07-20 and current time is 18:14:44,
	then the backup directory would be named like this:
	
	Backup-2022-07-20-18-14-44
	
⚑ When flashing firmware, the files must be placed in the "Flash" subfolder and named in compliance with any of the following formats:</li>
			
	lun#_partition_$.bin | parition_$.bin | partition.bin | lun#.bin | lun#_complete.bin
		
	For example:
		
	lun4_abl_a.bin - will be flashed into LUN4, Slot A
	abl_a.bin - will also be flashed into LUN4, Slot A.
	lun4.bin - will flash entire LUN4
	
<hr>

<i>Tested  with:</i>

	LG V45, V50, V50S, V60, G8, G8S, G8X, Samsung Galaxy A52s

<hr>

18/06/2024
<ul>
<li>	Japanese translation kindly provided by reindex-ot </li>
<li>	To run the app with Japanese support, please use this command line: QFILHelper.exe -ja -utf8</li>
</ul>

12/10/2023
<ul>
<li>	Chinese translation kindly added by wellqrg </li>
<li>	To run the app with Chinese support, please use this command line: QFILHelper.exe -zh -utf8</li>
</ul>

09/01/2024
<ul>
<li>	Motorola Edge 30 support added </li>
<li>	Many thanks to s4704 for testing the app and providing GPT headers for device emulation </li>
</ul>

<hr>
<i>Big thanks to <b>FreeMaan</b>, <b>marahuan</b>(v500N), from 4pda.io, <b>leavve</b>(v50s) from the TG group, for testing this app with their phones.</i>
