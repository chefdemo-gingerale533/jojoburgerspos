@echo off
REM Script to automate the setup, cloning, and execution of the Restaurant POS system in Release mode

REM Set the base directory for the project
set BASE_DIR=%cd%\RestaurantSystem
set GIT_REPO=https://github.com/chefdemo-gingerale533/jojoburgerspos.git
set REPO_DIR=%BASE_DIR%\jojoburgerspos
set DOTNET_CMD=dotnet

REM Check if .NET SDK is installed
%DOTNET_CMD% --version >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo .NET SDK is not installed. Please install it from https://dotnet.microsoft.com/download
    exit /b 1
)

REM Check if Git is installed
git --version >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo Git is not installed. Please install Git from https://git-scm.com/
    exit /b 1
)

REM Create the base directory
echo Creating base project directory...
mkdir "%BASE_DIR%"
cd "%BASE_DIR%"

REM Clone the GitHub repository
echo Cloning the repository: %GIT_REPO% ...
git clone %GIT_REPO%
IF %ERRORLEVEL% NEQ 0 (
    echo Failed to clone the repository. Please check the repository URL and your internet connection.
    exit /b 1
)

REM Create WPF projects for each component
echo Creating WPF projects...
%DOTNET_CMD% new wpf -n MainServer
%DOTNET_CMD% new wpf -n POS
%DOTNET_CMD% new wpf -n KDS
%DOTNET_CMD% new wpf -n Kiosk

REM Add Newtonsoft.Json dependency to each project
echo Adding Newtonsoft.Json to projects...
cd MainServer
%DOTNET_CMD% add package Newtonsoft.Json
cd ..

cd POS
%DOTNET_CMD% add package Newtonsoft.Json
cd ..

cd KDS
%DOTNET_CMD% add package Newtonsoft.Json
cd ..

cd Kiosk
%DOTNET_CMD% add package Newtonsoft.Json
cd ..

REM Move .cs files from the cloned repository to their respective project directories
echo Moving .cs files to respective project directories...
move "%REPO_DIR%\MainServer\*.cs" "%BASE_DIR%\MainServer"
move "%REPO_DIR%\POS\*.cs" "%BASE_DIR%\POS"
move "%REPO_DIR%\KDS\*.cs" "%BASE_DIR%\KDS"
move "%REPO_DIR%\Kiosk\*.cs" "%BASE_DIR%\Kiosk"

REM Build each project in Release mode
echo Building projects in Release mode...
cd MainServer
%DOTNET_CMD% build -c Release
cd ..

cd POS
%DOTNET_CMD% build -c Release
cd ..

cd KDS
%DOTNET_CMD% build -c Release
cd ..

cd Kiosk
%DOTNET_CMD% build -c Release
cd ..

REM Launch applications from the Release folder
echo Launching applications from Release folder...
start "" "%BASE_DIR%\MainServer\bin\Release\net7.0-windows\MainServer.exe"
start "" "%BASE_DIR%\POS\bin\Release\net7.0-windows\POS.exe"
start "" "%BASE_DIR%\KDS\bin\Release\net7.0-windows\KDS.exe"
start "" "%BASE_DIR%\Kiosk\bin\Release\net7.0-windows\Kiosk.exe"

echo Setup and execution complete. All applications are running.
pause
