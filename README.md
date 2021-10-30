# Samsung Flow Key Dumper
Use this app to dump the AES keys used by SamsungFlow (Desktop application) when communicating with a mobile device.

# Compilation
1. Clone
2. Open `SamFlowKeyDumper.sln` in VisualStudio 2019
3. Contile with `CTRL+B`

# Usage 
1. Make sure Samsung Flow is running and there's an active "Notifications Session" with your mobile device
2. Run SamFlowKeyDumper.exe
3. Key & IV will be written to stdout. Note that other messages are printed to strerr
