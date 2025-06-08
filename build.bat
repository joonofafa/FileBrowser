@echo off
setlocal

echo ===== FileBrowser Build System =====
echo.
echo Select build option:
echo 1. Debug Build
echo 2. Release Build
echo 3. Exit
echo.

:choose
set /p choice=Select option (1-3): 

if "%choice%"=="1" goto debug
if "%choice%"=="2" goto release
if "%choice%"=="3" goto end

echo Invalid choice. Please enter 1, 2, or 3.
goto choose

:debug
call build_debug.bat
goto end

:release
call build_release.bat
goto end

:end
exit /b 0 