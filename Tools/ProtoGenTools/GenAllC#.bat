@echo off
for %%i in (*.proto) do (
   echo gen %%~nxi...
   protoc.exe --csharp_out=client  %%~nxi)

echo finish... 
pause