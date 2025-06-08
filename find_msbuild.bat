@echo off
setlocal EnableDelayedExpansion

:: --- START: New robust vswhere logic ---
set VSWHERE_PATH="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"

if exist %VSWHERE_PATH% (
    echo Using vswhere.exe to locate Visual Studio...
    
    :: vswhere.exe를 임시 파일로 출력하여 경로의 공백 문제를 확실히 해결
    %VSWHERE_PATH% -latest -version "[17.0,18.0)" -property installationPath -requires Microsoft.Component.MSBuild > "%TEMP%\vspath.tmp"
    
    set /p VSPATH=<"%TEMP%\vspath.tmp"
    del "%TEMP%\vspath.tmp"

    if defined VSPATH (
        echo Visual Studio Path Found: !VSPATH!
        if exist "!VSPATH!\MSBuild\Current\Bin\MSBuild.exe" (
            echo Found MSBuild in Visual Studio installation.
            set MSBUILD="!VSPATH!\MSBuild\Current\Bin\MSBuild.exe"
            set FOUND_MSBUILD=1
            goto :found
        )
    )
)
:: --- END: New vswhere logic ---

echo vswhere.exe not found or failed. Falling back to legacy path checks...

:: Check Visual Studio 2022 paths
if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD="%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    set FOUND_MSBUILD=1
    goto :found
)
if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD="%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
    set FOUND_MSBUILD=1
    goto :found
)
if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD="%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    set FOUND_MSBUILD=1
    goto :found
)

:: Check Visual Studio 2022 Build Tools
if exist "%ProgramFiles%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD="%ProgramFiles%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    set FOUND_MSBUILD=1
    goto :found
)

:: Check Visual Studio 2019 paths
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD="%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
    set FOUND_MSBUILD=1
    goto :found
)
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD="%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
    set FOUND_MSBUILD=1
    goto :found
)
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD="%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    set FOUND_MSBUILD=1
    goto :found
)

:: Check Visual Studio 2019 Build Tools
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set MSBUILD="%ProgramFiles(x86)%\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    set FOUND_MSBUILD=1
    goto :found
)

:: Check Visual Studio 2017 paths
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" (
    set MSBUILD="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
    set FOUND_MSBUILD=1
    goto :found
)
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe" (
    set MSBUILD="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"
    set FOUND_MSBUILD=1
    goto :found
)
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe" (
    set MSBUILD="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
    set FOUND_MSBUILD=1
    goto :found
)

:: Check Visual Studio 2017 Build Tools
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe" (
    set MSBUILD="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe"
    set FOUND_MSBUILD=1
    goto :found
)

:: Check older Visual Studio paths
if exist "%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" (
    set MSBUILD="%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"
    set FOUND_MSBUILD=1
    goto :found
)
if exist "%ProgramFiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe" (
    set MSBUILD="%ProgramFiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe"
    set FOUND_MSBUILD=1
    goto :found
)

:: Check .NET SDK paths as a last resort
where dotnet >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    set MSBUILD=dotnet
    set FOUND_MSBUILD=2
    goto :found
)

echo MSBuild could not be found.
goto :error


:found
echo MSBuild found: !MSBUILD!
goto :export

:error
set MSBUILD=
set FOUND_MSBUILD=0

:export
:: Export the variables to calling script
endlocal & (
    set "MSBUILD=%MSBUILD%"
    set "FOUND_MSBUILD=%FOUND_MSBUILD%"
)
if %FOUND_MSBUILD% EQU 0 (
    exit /b 1
) else (
    exit /b 0
) 