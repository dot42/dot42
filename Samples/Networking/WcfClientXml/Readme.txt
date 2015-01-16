This sample consists of two parts: a server part and a client part.

SERVER
------
The server part can be found in the Server folder and should be started with admin rights. 
It is written in C# and uses Wcf. Note that it uses port 9222, so please make sure your firewall allows connections to it.


CLIENT
------
The client runs on your android device and uses the server above as end point.
Client\MainActivity.cs has the IP address of the machine where the service runs hard-coded. 
Change it to your IP address before running the sample.

The client has two implementation, one without async (compatible with Visual Studio 2010 / .NET 4.0 / C# 4.0) and one with async
(compatible with Visual Studio 2012 and Sharp Develop / .NET 4.5 / C# 5.0) 
