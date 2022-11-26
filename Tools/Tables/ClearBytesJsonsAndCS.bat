@echo off
cd config

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

pause