@echo off
cd config

echo "��ʼ����config�µ�bytes�ļ�"
for %%i in (*.bytes) do (
    echo begin copy... %%i
    copy /y %%~nxi ..\..\..\Assets\Demo\Configs\%%~nxi
    echo copy complate ... %%i
)
echo "bytes�ļ��������"

echo "��ʼ����config�µ�cs�ļ�"
for %%i in (*.cs) do (
    echo begin copy... %%i
    copy /y %%~nxi ..\..\..\Assets\HotUpdate\Table\Configs\%%~nxi
    echo copy complate ... %%i
)
echo "cs�ļ��������"

echo "��ʼ������ɵ��ļ�"
echo "ɾ�����ɵ��ļ�"
for /r %%i in (*.bytes) do (
    del %%i
    echo delete complate ... %%i
)
for /r %%i in (*.cs) do (
    del %%i
    echo delete complate ... %%i
)

for /r %%i in (*.json) do (
    del %%i
    echo delete complate ... %%i
)
echo "ɾ�����"
echo "������"

pause