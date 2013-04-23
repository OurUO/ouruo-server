#!/bin/bash
gmcs -optimize -unsafe -d:MONO -t:library -out:ouruo-scripts.dll -r:../ouruo-server-bin/ouruo-server.exe,System.Drawing,System.Web,System.Windows.Forms,System.Xml,System.Data -recurse:src/*.cs
mv ouruo-scripts.dll ../ouruo-server-bin/
