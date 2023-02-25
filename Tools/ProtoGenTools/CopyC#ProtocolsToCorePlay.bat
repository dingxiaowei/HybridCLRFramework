@echo off

echo "开始拷贝到客户端"
for %%i in (client\*) do (
    echo begin copy... %%i
    copy /y client\%%~nxi ..\..\Assets\HotUpdate\UnityWebSocket\Scripts\ProtoMsg\%%~nxi
    echo copy complate ... %%i
)
echo "拷贝完成"


echo "开始拷贝到服务器"
for %%i in (client\*) do (
    echo begin copy... %%i
    copy /y client\%%~nxi ..\..\Server\src\Samples\ConsoleApp\Message\%%~nxi
    echo copy complate ... %%i
)
echo "拷贝完成"

pause