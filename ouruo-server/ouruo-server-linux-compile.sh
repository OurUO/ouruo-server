#!/bin/bash
gmcs -optimize -unsafe -d:MONO -out:ouruo-server.exe -recurse:src/*.cs
mv ouruo-server.exe ../ouruo-server-bin/
