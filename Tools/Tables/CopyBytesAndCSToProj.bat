@echo off
cd config

echo "开始拷贝config下的bytes文件"
for %%i in (*.bytes) do (
    echo begin copy... %%i
    copy /y %%~nxi ..\..\..\Assets\Demo\Configs\%%~nxi
    echo copy complate ... %%i
)
echo "bytes文件拷贝完成"

echo "开始拷贝config下的cs文件"
for %%i in (*.cs) do (
    echo begin copy... %%i
    copy /y %%~nxi ..\..\..\Assets\HotUpdate\Table\Configs\%%~nxi
    echo copy complate ... %%i
)
echo "cs文件拷贝完成"

echo "开始清除生成的文件"
echo "删除生成的文件"
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
echo "删除完成"
echo "清除完成"

pause