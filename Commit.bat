@echo off

call update.bat

echo "================�ύ��־=========================="
set BatDir=%~dp0
set/p log=�������ύ��־:

git add -A .
if "%log%"=="" (git commit -m "�Զ��ύ") else (git commit -m %log%)
git push

echo "================�ύ�ɹ�==========================="
pause