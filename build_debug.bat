@echo off
setlocal

echo ===== FileBrowser Solution Debug Build Starting =====

:: Kill running process to prevent file lock error
echo Killing previous TotalCommander.exe process if it exists...
taskkill /f /im TotalCommander.exe /t 2>nul || ver > nul

:: Find MSBuild using our helper script
call find_msbuild.bat
if %ERRORLEVEL% NEQ 0 (
    echo Failed to find MSBuild. Build cannot continue.
    goto :error
)

echo MSBuild path: %MSBUILD%

:: Clean existing Debug folder content (optional)
if exist "TotalCommander\bin\Debug" (
    echo Cleaning existing Debug folder...
    rmdir /S /Q "TotalCommander\bin\Debug"
)

:: Build solution
echo Building FileBrowser.sln (Debug configuration)...

:: Check if we're using dotnet
if %FOUND_MSBUILD% EQU 2 (
    echo Using dotnet CLI for build...
    echo WARNING: Using dotnet CLI may cause architecture issues with this project.
    echo If build fails, please install Visual Studio Build Tools and try again.
    echo.
    %MSBUILD% msbuild FileBrowser.sln /t:Clean,Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /m
) else (
    echo Using MSBuild directly...
    %MSBUILD% FileBrowser.sln /t:Clean,Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /m
)

IF %ERRORLEVEL% NEQ 0 (
    echo.
    echo Build failed! Check errors.
    echo.
    echo NOTE: This project might require Visual Studio or Visual Studio Build Tools.
    echo You can download Visual Studio Build Tools (free) from:
    echo https://visualstudio.microsoft.com/downloads/ (look for "Build Tools for Visual Studio")
    goto :error
)

echo.
echo ===== Build Successful! =====
echo Debug build files located at: %~dp0TotalCommander\bin\Debug\
echo.
goto :end

:error
echo.
echo ===== Build Failed! =====
exit /b 1

:end
echo Press any key to exit...
pause > nul
exit /b 0 