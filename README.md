AccXtract
=========

Recover access to all logged in accounts and passwords for Google Chrome in just a few clicks!


This program comes in two parts: Grabber.exe and AccXtract.exe


Grabber.exe –
Throw this guy on a USB and take to your target computer.  Then, just run it.

Running as current user:
-Google Chrome cookies (i.e. every account the current user is logged in to in Chrome)
-Google Chrome passwords
-Firefox cookies (see Chrome cookies)
-Firefox passwords

Running as Admin:
-All of the above
-SAM Hashes (i.e. the easily cracked hashes of all of the Windows passwords for the computer)

When Grabber.exe runs, all of the data for that privilege level is copied to the USB drive.  It is organized by computer name, user name, then program.  Any required decryption is handled as Grabber.exe runs.  Generally the entire process from running to completion takes less than 5 seconds.



AccXtract.exe –
Once you have the data, AccXtract.exe decrypts and encrypts the data as needed and puts it all in place to be immediately useful.

Simply run AccXtract.exe, load the AccXtract folder created on the USB, and select which data you want to view/use.
