@echo off

echo "��ʼ�������ͻ���"
for %%i in (client\*) do (
    echo begin copy... %%i
    copy /y client\%%~nxi ..\..\Assets\HotUpdate\UnityWebSocket\Scripts\ProtoMsg\%%~nxi
    echo copy complate ... %%i
)
echo "�������"


echo "��ʼ������������"
for %%i in (client\*) do (
    echo begin copy... %%i
    copy /y client\%%~nxi ..\..\Server\src\Samples\ConsoleApp\Message\%%~nxi
    echo copy complate ... %%i
)
echo "�������"

pause