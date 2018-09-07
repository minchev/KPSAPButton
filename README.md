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
