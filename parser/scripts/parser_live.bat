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

echo.
echo (You can leave any field blank.)
echo.

set "CH_URL="
set "CH_DB="
set "CH_USER="
set "CH_PASSWORD="
set "CH_HEAD_TABLE="
set "CH_RAW_TABLE="
set "IFACE="

set /p CH_URL=ClickHouse URL (e.g., http://localhost:8123):
set /p CH_DB=ClickHouse Database:
set /p CH_USER=ClickHouse User:
set /p CH_PASSWORD=ClickHouse Password (press Enter to skip):
set /p CH_HEAD_TABLE=ClickHouse Head table:
set /p CH_RAW_TABLE=ClickHouse Raw table:
set /p IFACE=Live capture interface (e.g., eth0):

echo.
echo ==== Summary =======================
echo   CH URL:     %CH_URL%
echo   CH DB:      %CH_DB%
echo   CH User:    %CH_USER%
if defined CH_PASSWORD (
    echo   CH Password: [provided]
) else (
    echo   CH Password: [none]
)
echo   CH Head Table: %CH_HEAD_TABLE%
echo   CH Raw  Table: %CH_RAW_TABLE%
echo   Live IFACE:    %IFACE%
echo ====================================
echo.

echo Command to run:
echo   RUST_LOG=%RUST_LOG_LEVEL% "%BIN_PATH%" [CH args] live [--iface=IFACE]
echo.

choice /M "Proceed"
if errorlevel 2 goto :eof

REM --- Build args ---

set "CH_ARGS="
if not "%CH_URL%"==""        set "CH_ARGS=%CH_ARGS% --ch-url=%CH_URL%"
if not "%CH_DB%"==""         set "CH_ARGS=%CH_ARGS% --ch-db=%CH_DB%"
if not "%CH_USER%"==""       set "CH_ARGS=%CH_ARGS% --ch-user=%CH_USER%"
if not "%CH_PASSWORD%"==""   set "CH_ARGS=%CH_ARGS% --ch-password=%CH_PASSWORD%"
if not "%CH_HEAD_TABLE%"=="" set "CH_ARGS=%CH_ARGS% --ch-head-table=%CH_HEAD_TABLE%"
if not "%CH_RAW_TABLE%"==""  set "CH_ARGS=%CH_ARGS% --ch-raw-table=%CH_RAW_TABLE%"

set "LIVE_ARGS="
if not "%IFACE%"==""         set "LIVE_ARGS=%LIVE_ARGS% --iface=%IFACE%"

set "RUST_LOG=%RUST_LOG_LEVEL%"

echo Running:
echo   "%BIN_PATH%" %CH_ARGS% live %LIVE_ARGS%
echo.

"%BIN_PATH%" %CH_ARGS% live %LIVE_ARGS%

endlocal
