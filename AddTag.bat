@echo off

call Pull.bat

echo "================�ύ��־=========================="
set BatDir=%~dp0
set/p tag=������tag���:
set/p log=������tag��־:

git add -A .
if "%log%"=="" (git commit -m "autotag") else (git tag -a %tag% -m %log%)
git push

echo "================�ύ�ɹ�==========================="
pause