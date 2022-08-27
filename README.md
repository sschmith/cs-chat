# cs-chat

Author: Stephen Schmith


INSTALLING THE SERVER

NOTE: I've provided a pre-compiled version of IMServer in the IMServer\bin directory called IMServer.exe. It's not necessary to compile the source code unless you really want to. However, I'll give the instructions for compiling it here, anyway.

IMServer is written in C#. To compile it, you need to have the Microfsoft .NET Framework v4.0 or higher installed on your system. The compilation process has only been tested on Windows 7, but it should work on any version of Windows with .NET 4.0 or higher installed.

To compile the source code, you first need to add the C# compiler to your PATH directory in Windows. To do that, run a command prompt as an administrator and enter this line:


path=%path%;%SystemRoot%\Microsoft.NET\Framework\v4.0.30319


You may need to change the last directory of this command to one that's actually on your system. In this case, you would change "v4.0.30319" to something in the format "v4.x.xxxx" and then run it.

Once you've done that, cd to the source code directory and run this command:


csc Program.cs UserList.cs User.cs


This will produce an output file called Program.exe. This program is identical to the one I provided (IMServer.exe); it just has a different name. You can run it by typing "Program.exe" into the command prompt and pressing enter, or by double-clicking the file itself.

You can change the port number the server listens to by editing port.txt.


RUNNING THE SERVER ON LINUX


The instructions above are written for a Windows machine. However, it's possible to run the server on a Linux machine with the Mono Framework installed.

I won't cover the compilation process, and there's no guarantee that it would work anyway. However, the server should run just fine on a Linux system with Mono installed.

To do this, just install the mono and libgdiplus packages on your system. In Ubuntu, for example, you can do this by running the package explorer and searching for "mono" and "libgdiplus".

To run the server, cd to the directory containing IMServer.exe and enter this command:


mono IMServer.exe


This functionality has only been tested on Linux Mint 13, but again, there's no reason it shouldn't work on any version of Linux as long as Mono is installed.


INSTALLING THE CLIENT


NOTE: As before, I've included pre-compiled .class files in the IMClient/bin directory. These files contain Java bytecode and should run on any Java Virtual Machine implementing Java 1.6 Update 43 or later. To try them out, cd to the IMClient/bin directory and run


java IMClient


Compilation Instructions:

IMClient is written in Java, and was compiled with JDK 1.6 Update 43. It will not compile with JDK 1.7, although it should run on JRE 1.7 (but this hasn't been tested).

To compile IMClient, cd to the IMClient/src directory and run this command (assuming you've added the JDK bin directory to your system path):


javac IMClient.java MessageThread.java


You can then run the client by entering


java IMClient


You can change the IP Address and port number for the server in config.txt. Be aware that unless the server is configured otherwise, it always listens to port 2410.# cs-chat
