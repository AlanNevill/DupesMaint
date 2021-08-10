# DupesMaint Solution


## DupesMaintConsole - Command line project
Find duplicate files in a folder tree.

 - Calculate SHA256 values for all files in a folder tree.

 - Save the details into a database table POPS.Checksum.

 - Find duplicate SHA256 in the table POPS.Checksum and insert details of the duplicates in a second table POPS.ChecksumDups.

### --folder string

The root folder of the tree to scan which must exist.

Example:  --folder "`C:\\Users\\User\\OneDrive\\Photos`"

### --replace <u>true</u>/false

Replace default (true) or append (false) to the database tables POPS.CheckSum & POPS.CheckSumDupes.

Example: `--replace false`

### Usage

Using PowerShell from Bin folder or Developer PowerShell in Visual Studio.

`./DupesMaint --folder "C:\\Users\\User\\OneDrive\\Photos"`

`./DupesMaint --folder "C:\\Users\\User\\OneDrive\\Photos" --replace false`







## DupesMaintWinForms - WinForms project

Review and discard duplicate JPG files.

