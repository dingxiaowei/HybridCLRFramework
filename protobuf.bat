@echo off
set protogen=%~dp0\3Party\protobuf-net r668\ProtoGen\protogen.exe

rem ShenCommon����
del /Q /S "%~dp0\Assets\Scripts\Common\*.cs"
pushd "%~dp0\Common"

del /Q /S *.cs

call:buildDir "%~dp0\Common" "%~dp0\Assets\Scripts\Common"
del /Q /S *.cs
popd

rem PlatCommon����
pushd "%~dp0\PlatCommon"
del /Q /S *.cs
call:buildDir "%~dp0\PlatCommon" "%~dp0\Assets\Scripts\Common"
del /Q /S *.cs
call:clearMeta "%~dp0\Assets\Scripts\Common"
popd

rem �ͻ�������
"%protogen%" -i:"Assets\Scripts\Config\UserData.proto" -o:"Assets\Scripts\Config\UserData.proto.cs" -p:observable -q

pause
GOTO:EOF

rem ===================================================================
rem ��ָ��Ŀ¼��proto�ļ�����C#��������
rem ����1: proto·��
rem ����2: cs�ļ�Ŀ��·��
:buildDir
echo build: %~1 -> %~2
for /f "tokens=* delims=" %%i in ('dir /b "%~1\*.proto"') do (
	echo %~1\%%i
	"%protogen%" -i:"%~1\%%i" -o:"%~1\%%i.cs" -q
)
xcopy "%~1\*.cs" "%~2" /Y /F
GOTO:EOF

rem ===================================================================
rem �ݹ�ɾ������Ŀ¼�����й�����*.meta�ļ�
rem ����: ·��
:clearMeta
echo clear meta: %~1
for /f "tokens=* delims=" %%i in ('dir /b /s "%~1\*.meta"') do (
	if not exist "%%~dpni" (
		echo %%i
		del /Q "%%i"
	)
)
GOTO:EOF