@echo off

call Pull.bat

echo "================提交日志=========================="
set BatDir=%~dp0
set/p tag=请输入tag版号:
set/p log=请输入tag日志:

git add -A .
if "%log%"=="" (git commit -m "autotag") else (git tag -a %tag% -m %log%)
git push

echo "================提交成功==========================="
pause