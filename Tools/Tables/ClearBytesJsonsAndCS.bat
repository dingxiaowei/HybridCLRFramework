@echo off
cd config

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

pause