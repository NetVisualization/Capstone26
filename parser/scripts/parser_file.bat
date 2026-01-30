@echo off
setlocal

REM --- Config ---
set "BIN_PATH=%~dp0..\pcap2ch\target\release\pcap2ch.exe"
set "RUST_LOG_LEVEL=info"

REM --- Check binary ---
if not exist "%BIN_PATH%" (
    echo Error: binary not found or not executable at:
    echo   %BIN_PATH%
    echo Update BIN_PATH in this script or build the project first:
    echo   cargo build --release
    goto :eof
)

echo Configure NetVis file -> ClickHouse ingest
echo.
echo You can leave any ClickHouse field blank except the password.
echo The pcap file path is required.
echo.

REM --- ClickHouse connection (mostly optional) ---

set "CH_URL="
set /p CH_URL="ClickHouse URL (e.g., http://localhost:8123): "

set "CH_DB="
set /p CH_DB="ClickHouse Database: "

set "CH_USER="
set /p CH_USER="ClickHouse User: "

REM --- Password (required) ---
:ask_password
set "CH_PASSWORD="
set /p CH_PASSWORD="ClickHouse Password (required): "
if "%CH_PASSWORD%"=="" (
    echo   Password cannot be empty.
    goto ask_password
)

set "CH_HEAD_TABLE="
set /p CH_HEAD_TABLE="ClickHouse Head table (optional): "

set "CH_RAW_TABLE="
set /p CH_RAW_TABLE="ClickHouse Raw table (optional): "

REM --- File path (required, must exist) ---
:ask_file
set "FILE_PATH="
set /p FILE_PATH="Path to pcap file (required): "
if "%FILE_PATH%"=="" (
    echo   File path cannot be empty.
    goto ask_file
)
if not exist "%FILE_PATH%" (
    echo   File not found: "%FILE_PATH%"
    echo   Please enter a valid path.
    goto ask_file
)

echo.
echo ==== Summary ==================================
echo   CH URL:           %CH_URL%
echo   CH DB:            %CH_DB%
echo   CH User:          %CH_USER%
echo   CH Password:      [provided]
echo   CH Head Table:    %CH_HEAD_TABLE%
echo   CH Raw  Table:    %CH_RAW_TABLE%
echo   File Path:        %FILE_PATH%
echo.
echo Command to run:
echo   RUST_LOG=%RUST_LOG_LEVEL% "%BIN_PATH%" [CH args] file --path="FILE_PATH" --insert --batch-size=5000
echo ==============================================
echo.

set "CONFIRM="
set /p CONFIRM="Proceed? [y/N]: "
if /I not "%CONFIRM%"=="Y" if /I not "%CONFIRM%"=="y" (
    echo Aborted.
    goto :eof
)

REM --- Build argument list ---
set "ARGS="

if not "%CH_URL%"==""        set "ARGS=%ARGS% --ch-url=%CH_URL%"
if not "%CH_DB%"==""         set "ARGS=%ARGS% --ch-db=%CH_DB%"
if not "%CH_USER%"==""       set "ARGS=%ARGS% --ch-user=%CH_USER%"
REM password is required, so always include it
set "ARGS=%ARGS% --ch-password=%CH_PASSWORD%"
if not "%CH_HEAD_TABLE%"=="" set "ARGS=%ARGS% --ch-head-table=%CH_HEAD_TABLE%"
if not "%CH_RAW_TABLE%"==""  set "ARGS=%ARGS% --ch-raw-table=%CH_RAW_TABLE%"

set "RUST_LOG=%RUST_LOG_LEVEL%"

echo Running:
echo   "%BIN_PATH%" %ARGS% file --path="%FILE_PATH%" --insert --batch-size=5000
echo.

"%BIN_PATH%" %ARGS% file --path="%FILE_PATH%" --insert --batch-size=5000

endlocal
