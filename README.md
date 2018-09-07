KPSAPButton is a simple KeePass plugin used for automatic login into SAP systems via SAPgui.

The plugin utilizes the sapshcut.exe utility to make the call to the SAPgui.

The entries in the KeePass should be stored in the format:
<System ID>[./]<Client #>

Examples:
D02.400
E44/100

The plugin of course requires installed SAPgui.

A credit for the idea goes to Martin Kostov, an ex-coleague of mine who created a separate GUI as a replacement for SAPgui in C#. His GUI stored the password in a separate storage.

KeePass website: https://keepass.info/

Usage:
=====================================

1. Download KeePass
2. Install KeePass
3. Download KPSAPButton project
4. Compile the project or use the ready DLL file from the BIN folder
5. Copy the DLL into the KeePass main program folder (for newer version it is the Plugins subfolder).
6. Start the program and press F12 for login into the desired system/client
