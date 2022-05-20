SETLOCAL ENABLEDELAYEDEXPANSION
@ECHO OFF

:: ==========================
:: Optional input parameters
:: ==========================
:: Parameter #1 values:
::  A                = do not update version and do not publish
::  U                = update version but do not publish
::  P                = do not update version only publish
::  UP               = update version and publish
:: Parameter #2 values:
::  P                = Pre-release

SET SCRIPT_PARAMETER=%1
SET USE_PRE_RELEASE=%2

:: ==========================
:: Global script variables
:: ==========================
SET "VERSION="
SET "PUBLISH_REL=FALSE"
SET "UPDATE_VER=FALSE"
IF "%SCRIPT_PARAMETER%" EQU "U" SET "UPDATE_VER=TRUE"
IF "%SCRIPT_PARAMETER%" EQU "P" SET "PUBLISH_REL=TRUE"
IF "%SCRIPT_PARAMETER%" EQU "UP" SET "UPDATE_VER=TRUE" && SET "PUBLISH_REL=TRUE"
SET "SCRIPT_DIR=%~dp0"
SET "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"
SET "RELEASE_MANAGER=%SCRIPT_DIR%\ReleaseManager.bat"
SET "PUBLISH_FOLDER=%SCRIPT_DIR%\..\bin\Release\net6.0-windows\win-x86\publish\win-x86"

IF "%SCRIPT_PARAMETER%" EQU "" (
	START "Release Manager" "%RELEASE_MANAGER%"
	EXIT
)

:: ==========================
:: GitHub release API variables
:: ==========================
:: Get secret GitHub access token from external file into variable 'GITHUB_ACCESS_TOKEN'
CALL GITHUB_ACCESS_TOKEN.bat
SET "REPO_OWNER=voltura"
SET "REPO_NAME=WeekNumberLite2"
:: v%VERSION%
SET "TAG_NAME=" 
:: BRANCH (master)
SET "TARGET_COMMITISH=master"
:: WeekNumber Lite 2 %VERSION%
SET "NAME="
::"Release of version %VERSION%"
SET "BODY="
SET "DRAFT=false"
SET "PRERELEASE=false"
IF "%USE_PRE_RELEASE%" EQU "P" SET "PRERELEASE=true"
SET "CURL_RESULT="
SET "UPLOAD_URL="

:: ==========================
:: Tools
:: ==========================
SET "SEVEN_ZIP_FULLPATH=C:\Program Files\7-Zip\7z.exe"
SET "MSBUILD_FULLPATH=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
SET "FART=%SCRIPT_DIR%\..\Tools\fart.exe"
SET "CURL=C:\Program Files\curl-7.81.0-win64-mingw\bin\curl.exe"

:: ==========================
:: Script logic
:: ==========================
TITLE Creating WeekNumber Lite 2 release...
COLOR 1F
CLS
CALL :UPDATE_VERSION
CALL :COMPILE_RELEASE
CALL :CREATE_FILES_FOR_RELEASE
CALL :PUBLISH_RELEASE
CALL :DISP_MSG "All tasks completed successfully, launching the Release Manager..." 2
START "Release Manager" "%RELEASE_MANAGER%" 0
EXIT

:: ==========================
:: Functions
:: ==========================
:CREATE_FILES_FOR_RELEASE
CD /D "%PUBLISH_FOLDER%"
	CALL :DISP_MSG "Creating files for release." 2
	CALL :GENERATE_MD5 WeekNumberLite2.exe
	CALL :COMPRESS_WeekNumber_Lite_2_ZIP
	CALL :GENERATE_MD5 WeekNumberLite2.zip
	CALL :GENERATE_VERSION_INFO %VERSION% WeekNumberLite2.exe
	CALL :COPY_RELEASE
	CALL :DISP_MSG "Generated all release files successfully." 0
CD /D "%SCRIPT_DIR%"
GOTO :EOF

:GENERATE_MD5
CALL :DISP_MSG "Generating MD5 for '%1'..." 0
SET "MD5="
FOR /F "skip=1" %%G IN ('CertUtil -hashfile "%PUBLISH_FOLDER%\%1" MD5') DO (
	SET "MD5=%%G"
	GOTO :CREATE_MD5 %1
)
CALL :ERROR_MESSAGE_EXIT "Failed to generate MD5 for '%1'." 10
:CREATE_MD5
SET FILE_NAME=%1
ECHO %MD5%  %FILE_NAME%> "%PUBLISH_FOLDER%\%FILE_NAME%.MD5"
ECHO.>> "%PUBLISH_FOLDER%\%FILE_NAME%.MD5"
CALL :DISP_MSG "Generated MD5 checksum file '%FILE_NAME%.MD5'." 0
GOTO :EOF

:COMPRESS_INSTALLER
IF NOT EXIST "%SEVEN_ZIP_FULLPATH%" CALL :ERROR_MESSAGE_EXIT "Compress tool not found, cannot compress installer." 20
CD /D %PUBLISH_FOLDER%
"%SEVEN_ZIP_FULLPATH%" a -t7z -y WeekNumber_Lite_%VERSION%_Installer.7z WeekNumber_Lite_%VERSION%_Installer.exe WeekNumber_Lite_%VERSION%_Installer.exe.MD5 >NUL
SET SEVEN_ZIP_RESULT=%ERRORLEVEL%
CD /D %SCRIPT_DIR%
IF "%SEVEN_ZIP_RESULT%" NEQ "0" CALL :ERROR_MESSAGE_EXIT "Failed to compress installer" %SEVEN_ZIP_RESULT%
GOTO :EOF

:COMPRESS_WeekNumber_Lite_2_ZIP
CALL :DISP_MSG "Archiving WeekNumber Lite 2..." 0
IF NOT EXIST "%SEVEN_ZIP_FULLPATH%" CALL :ERROR_MESSAGE_EXIT "7-zip not found '%SEVEN_ZIP_FULLPATH%', cannot compress installer." 30
CD /D "%PUBLISH_FOLDER%"
"%SEVEN_ZIP_FULLPATH%" a -tzip -y WeekNumberLite2.zip WeekNumberLite2.exe WeekNumberLite2.exe.MD5 >NUL
SET SEVEN_ZIP_RESULT=%ERRORLEVEL%
CD /D "%SCRIPT_DIR%"
IF "%SEVEN_ZIP_RESULT%" NEQ "0" CALL :ERROR_MESSAGE_EXIT "7-zip failed to generate 'WeekNumberLite_.zip'." %SEVEN_ZIP_RESULT%
CALL :DISP_MSG "Archiving WeekNumber Lite 2 completed." 0
GOTO :EOF

:GENERATE_VERSION_INFO
CALL :DISP_MSG "Generating version info..." 0
ECHO %1 %2> "%PUBLISH_FOLDER%\VERSION.TXT"
CALL :DISP_MSG "%PUBLISH_FOLDER%\VERSION.TXT created" 0
GOTO :EOF

:COPY_RELEASE
CALL :DISP_MSG "Copying release files to release folder..." 0
MD "%SCRIPT_DIR%\..\Releases\%VERSION%" >NUL 2>&1
IF NOT EXIST "%PUBLISH_FOLDER%\WeekNumberLite2.zip" CALL :ERROR_MESSAGE_EXIT "WeekNumberLite2.zip could not be copied" 40
MOVE /Y "%PUBLISH_FOLDER%\WeekNumberLite2.zip" "%SCRIPT_DIR%\..\Releases\%VERSION%\" >NUL 2>&1
IF "%ERRORLEVEL%" NEQ "0" CALL :ERROR_MESSAGE_EXIT "Failed to move WeekNumberLite2.zip" 50
IF NOT EXIST "%PUBLISH_FOLDER%\WeekNumberLite2.zip.MD5" CALL :ERROR_MESSAGE_EXIT "WeekNumberLite2.zip.MD5 not found" 60
MOVE /Y "%PUBLISH_FOLDER%\WeekNumberLite2.zip.MD5" "%SCRIPT_DIR%\..\Releases\%VERSION%\" >NUL 2>&1
IF "%ERRORLEVEL%" NEQ "0" CALL :ERROR_MESSAGE_EXIT "Failed to move WeekNumberLite2.zip.MD5" 70
IF NOT EXIST "%PUBLISH_FOLDER%\WeekNumberLite2.exe" GOTO :FAILED_COPY_RELEASE
MOVE /Y "%PUBLISH_FOLDER%\WeekNumberLite2.exe" "%SCRIPT_DIR%\..\Releases\%VERSION%\" >NUL 2>&1
IF "%ERRORLEVEL%" NEQ "0" CALL :ERROR_MESSAGE_EXIT "Copy failed" 120
IF NOT EXIST "%PUBLISH_FOLDER%\WeekNumberLite2.exe.MD5" GOTO :FAILED_COPY_RELEASE
MOVE /Y "%PUBLISH_FOLDER%\WeekNumberLite2.exe.MD5" "%SCRIPT_DIR%\..\Releases\%VERSION%\" >NUL 2>&1
IF "%ERRORLEVEL%" NEQ "0" CALL :ERROR_MESSAGE_EXIT "Copy failed" 140
IF NOT EXIST "%PUBLISH_FOLDER%\VERSION.TXT" CALL :ERROR_MESSAGE_EXIT "Failed, missing file" 150
MOVE /Y "%PUBLISH_FOLDER%\VERSION.TXT" "%SCRIPT_DIR%\..\Releases\%VERSION%\" >NUL 2>&1
IF "%ERRORLEVEL%" NEQ "0" CALL :ERROR_MESSAGE_EXIT "Failed to copy release files." 160
GOTO :EOF

:UPDATE_VERSION
CALL :DISP_MSG "Getting current version from project..." 0
TYPE "%SCRIPT_DIR%\..\Properties\AssemblyInfo.cs"|FINDSTR AssemblyFileVersion >"%SCRIPT_DIR%\VERSION_REPLACE.TXT"
SET /P AssemblyFileVersion=<"%SCRIPT_DIR%\VERSION_REPLACE.TXT"
DEL /F /Q "%SCRIPT_DIR%\VERSION_REPLACE.TXT"
SET "CurrentAssemblyFileVersion=%AssemblyFileVersion:~32,-3%"
SET "MAJOR="
SET "MINOR="
SET "BUILD="
SET "REVISION="
FOR /F "tokens=1,2,3,4 delims=." %%G IN ("%CurrentAssemblyFileVersion%") DO (
	SET /A MAJOR=%%G
	SET /A MINOR=%%H
	SET /A BUILD=%%I
	SET /A REVISION=%%J
)
SET VERSION=%MAJOR%.%MINOR%.%BUILD%.%REVISION%
IF "%UPDATE_VER%" EQU "TRUE" (
	CALL :UPDATE_REVISION
) ELSE (
	CALL :SKIP_VERSION_UPDATE
)
GOTO :EOF

:SKIP_VERSION_UPDATE
CALL :DISP_MSG "Current version = %VERSION%" 2
GOTO :EOF

:UPDATE_REVISION
IF %REVISION% GEQ 9 GOTO :UPDATE_BUILD
ECHO   - Updating revision number
SET /A REVISION=%REVISION%+1
SET NewAssemblyFileVersion=%MAJOR%.%MINOR%.%BUILD%.%REVISION%
GOTO :DO_VER_UPDATE

:UPDATE_BUILD
IF %BUILD% GEQ 9 GOTO :UPDATE_MINOR
ECHO   - Updating build number
SET /A BUILD=%BUILD%+1
SET NewAssemblyFileVersion=%MAJOR%.%MINOR%.%BUILD%.0
GOTO :DO_VER_UPDATE

:UPDATE_MINOR
IF %MINOR% GEQ 9 GOTO :UPDATE_MAJOR
ECHO   - Updating minor number
SET /A MINOR=%MINOR%+1
SET NewAssemblyFileVersion=%MAJOR%.%MINOR%.0.0
GOTO :DO_VER_UPDATE

:UPDATE_MAJOR
SET /A MAJOR=%MAJOR%+1
ECHO   - Updating major number
SET NewAssemblyFileVersion=%MAJOR%.0.0.0
GOTO :DO_VER_UPDATE

:DO_VER_UPDATE
CALL :DISP_MSG "Updating version from '%CurrentAssemblyFileVersion%' to '%NewAssemblyFileVersion%'..." 0
"%FART%" -q "%SCRIPT_DIR%\..\Properties\AssemblyInfo.cs" %CurrentAssemblyFileVersion% %NewAssemblyFileVersion% >NUL
SET FART_RESULT=%ERRORLEVEL%
IF "%FART_RESULT%" NEQ "2" CALL :ERROR_MESSAGE_EXIT "Failed to update version." 170
SET VERSION=%NewAssemblyFileVersion%
CALL :DISP_MSG "Version updated from '%CurrentAssemblyFileVersion%' to '%NewAssemblyFileVersion%'." 2
CALL :SYNC_SOURCE
GOTO :EOF

:SYNC_SOURCE
CALL :DISP_MSG "Syncing sources, please wait..." 0
GIT pull -q >NUL 2>&1
ECHO  - Get changes from git result = %ERRORLEVEL%
GIT add --all >NUL 2>&1
ECHO  - Add all changes result = %ERRORLEVEL%
GIT commit -a -m "Updated version to %VERSION%" >NUL 2>&1
ECHO  - Commit all changes result = %ERRORLEVEL%
GIT push --all >NUL 2>&1
ECHO  - Push all changes result = %ERRORLEVEL%
CALL :DISP_MSG "Sync complete." 0
GOTO :EOF

:COMPILE_RELEASE
CALL :DISP_MSG "Compiling solution release build..." 0
PUSHD "%SCRIPT_DIR%\.."
	CALL build.bat >NUL
	SET BUILD_RESULT=%ERRORLEVEL%
POPD
IF "%BUILD_RESULT%" NEQ "0" CALL :ERROR_MESSAGE_EXIT "Build failed. Cannot create release." 180
IF "%BUILD_RESULT%" EQU "0" CALL :DISP_MSG "Build was successfully executed." 0
GOTO :EOF

:PUBLISH_RELEASE
IF "%PUBLISH_REL%" NEQ "TRUE" GOTO :EOF
CALL :DISP_MSG "Publishing release to Github..." 1
SET "TAG_NAME=v%VERSION%"
SET "NAME=WeekNumber Lite 2 %VERSION%"
SET "BODY=Release of version %VERSION%"
"%CURL%" -s -H "Accept: application/vnd.github.v3+json" -H "Authorization: token %GITHUB_ACCESS_TOKEN%" -H "Content-Type:application/json" "https://api.github.com/repos/%REPO_OWNER%/%REPO_NAME%/releases" -d "{ \"tag_name\": \"%TAG_NAME%\", \"target_commitish\": \"%TARGET_COMMITISH%\",\"name\": \"%NAME%\",\"body\": \"%BODY%\",\"draft\": %DRAFT%, \"prerelease\": %PRERELEASE%}" >"%SCRIPT_DIR%\release_info.txt"
SET CURL_RESULT=%ERRORLEVEL%
IF "%CURL_RESULT%" NEQ "0" CALL :ERROR_MESSAGE_EXIT "Failed to publish release" 190
CALL :DISP_MSG "Successfully published release." 0
CALL :PARSE_RELEASE_INFO
CALL :UPLOAD_RELEASE_ASSETS
GOTO :EOF

:PARSE_RELEASE_INFO
CALL :DISP_MSG "Parsing release info..." 0
TYPE "%SCRIPT_DIR%\release_info.txt"|FINDSTR upload_url >"%SCRIPT_DIR%\UPLOAD_URL.TXT"
DEL /F /Q "%SCRIPT_DIR%\release_info.txt" >NUL
SET /P UPLOAD_URL=<"%SCRIPT_DIR%\UPLOAD_URL.TXT"
DEL /F /Q "%SCRIPT_DIR%\UPLOAD_URL.TXT" >NUL
SET UPLOAD_URL=%UPLOAD_URL:~17,-15%
CALL :DISP_MSG "Successfully parsed received release info." 1
GOTO :EOF

:UPLOAD_RELEASE_ASSETS
CALL :DISP_MSG "Uploading release '%NAME%' assets to Github..." 1
PUSHD "%SCRIPT_DIR%\..\Releases\%VERSION%"
	CALL :UPLOAD_FILE WeekNumberLite2.zip
	CALL :UPLOAD_FILE WeekNumberLite2.zip.MD5
	CALL :UPLOAD_FILE WeekNumberLite2.exe
	CALL :UPLOAD_FILE WeekNumberLite2.exe.MD5
	CALL :UPLOAD_FILE VERSION.TXT
POPD
CALL :DISP_MSG "Upload completed." 0
GOTO :EOF

:UPLOAD_FILE
SET FILE_TO_UPLOAD=%1
CALL :CHECK_IF_MISSING_FILE %FILE_TO_UPLOAD%
CALL :DISP_MSG "Uploading '%FILE_TO_UPLOAD%' to release '%NAME%' on Github..." 0
"%CURL%" -s -H "Accept: application/vnd.github.v3+json" -H "Authorization: token %GITHUB_ACCESS_TOKEN%" -H "Content-Type: application/octet-stream" --data-binary @%FILE_TO_UPLOAD% "%UPLOAD_URL%?name=%FILE_TO_UPLOAD%&label=%FILE_TO_UPLOAD%" >NUL
SET CURL_RESULT=%ERRORLEVEL%
:: Note: curl result can be 0 but file not uploaded, need to parse received json to validate success
IF "%CURL_RESULT%" NEQ "0" CALL :ERROR_MESSAGE_EXIT "Failed to upload '%FILE_TO_UPLOAD%'" 200
CALL :DISP_MSG "Successfully uploaded '%FILE_TO_UPLOAD%'." 1
GOTO :EOF

:CHECK_IF_MISSING_FILE
SET FILE_TO_CHECK=%1
IF NOT EXIST "%FILE_TO_CHECK%" CALL :ERROR_MESSAGE_EXIT "Missing '%FILE_TO_CHECK%', cannot publish file." 210
GOTO :EOF

:ERROR_MESSAGE_EXIT
SET MSG=%1
SET MSG=%MSG:~1,-1%
SET CODE=%2
COLOR 4F
ECHO.
ECHO   ==================================================
ECHO   CODE:  %CODE%
ECHO   ERROR: %MSG%
ECHO   ==================================================
ECHO   Press any key to restart the Release Manager...
PAUSE >NUL
START "Release Manager" "%RELEASE_MANAGER%" %CODE%
EXIT

:DISP_MSG
SET MSG=%1
SET MSG=%MSG:~1,-1%
SET /A DELAY_SEC=%2+0
ECHO.
ECHO   ==================================================
ECHO   %MSG%
ECHO   ==================================================
TIMEOUT /T %DELAY_SEC% /NOBREAK >NUL
GOTO :EOF
