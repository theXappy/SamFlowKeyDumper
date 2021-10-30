# Samsung Flow Key Dumper
Use this app to dump the AES keys used by SamsungFlow (Desktop application) when communicating with a mobile device.

# Compilation
1. Clone
2. Open `SamFlowKeyDumper.sln` in VisualStudio 2019
3. Contile with `CTRL+B`

# Usage 
NOTE: Binaries are available in the "Release" section. You don't have to compile yourself.
Make sure you have .NET framework 4.8 installed

1. Open Samsung Flow
2. Authenticate a mobile device (android phone) and get to the "Flow history" windows.
3. Run SamFlowKeyDumper.exe
4. Key & IV will be written to stdout. Note that other messages are printed to strerr.

# Inner Working
If you want to know more about *how* this tools works, here's a short description.  
Samsung Flow runs a .NET (framework) process called "SamsungFlowDesktop.exe" (not the GUI process).  
This process is responsible of communicating with the mobile device. It has several internal classes to do so  
but most importantly it has a singleton object of type "SessionKeyManager".  
This object holds both Key and IV for the current ongoing session.  
By utilizing my other project (RemoteNET)[https://github.com/theXappy/RemoteNET] (Go check it out :)) we can easily 
get hold of this object remotly (from SamFlowKeyDumper.exe) and then dump both of his fields containing the key and IV.
