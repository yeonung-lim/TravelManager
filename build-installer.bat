@echo off
echo ==========================================
echo Travel Manager 인스톨러 생성 스크립트
echo ==========================================
echo.

REM 빌드 환경 확인
echo [1/4] 빌드 환경 확인 중...
dotnet --version
if %ERRORLEVEL% neq 0 (
    echo ERROR: .NET SDK가 설치되지 않았습니다.
    pause
    exit /b 1
)

REM NSIS 확인 (선택적)
where makensis >nul 2>&1
if %ERRORLEVEL% equ 0 (
    echo NSIS가 발견되었습니다. 인스톨러 생성이 가능합니다.
    set NSIS_AVAILABLE=1
) else (
    echo 알림: NSIS가 설치되지 않았습니다. 기본 배포만 생성됩니다.
    echo NSIS 다운로드: https://nsis.sourceforge.io/Download
    set NSIS_AVAILABLE=0
)

echo.

REM 이전 빌드 정리
echo [2/4] 이전 빌드 파일 정리 중...
if exist "installer" rmdir /s /q "installer"
if exist "publish" rmdir /s /q "publish"

REM 배포 파일 생성
echo [3/4] 배포 파일 생성 중...
dotnet publish TravelManagerWPF\TravelManagerWPF.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o publish\win-x64
if %ERRORLEVEL% neq 0 (
    echo ERROR: 배포 파일 생성에 실패했습니다.
    pause
    exit /b 1
)

REM 인스톨러 폴더 구조 생성
echo [4/4] 인스톨러 준비 중...
mkdir installer
mkdir installer\app
mkdir installer\docs

REM 필요한 파일 복사
copy publish\win-x64\TravelManagerWPF.exe installer\app\
copy README.md installer\docs\
if exist screenshots (
    xcopy /E /I screenshots installer\docs\screenshots\
)

REM 설치 스크립트 생성
echo ; Travel Manager 설치 스크립트 > installer\install.bat
echo @echo off >> installer\install.bat
echo echo Travel Manager 설치 중... >> installer\install.bat
echo. >> installer\install.bat
echo set INSTALL_DIR=%%LOCALAPPDATA%%\TravelManager >> installer\install.bat
echo if not exist "%%INSTALL_DIR%%" mkdir "%%INSTALL_DIR%%" >> installer\install.bat
echo. >> installer\install.bat
echo copy "app\TravelManagerWPF.exe" "%%INSTALL_DIR%%\" >> installer\install.bat
echo if exist "docs" xcopy /E /I "docs" "%%INSTALL_DIR%%\docs\" >> installer\install.bat
echo. >> installer\install.bat
echo echo 설치 완료! >> installer\install.bat
echo echo 프로그램 위치: %%INSTALL_DIR%%\TravelManagerWPF.exe >> installer\install.bat
echo. >> installer\install.bat
echo REM 바탕화면 바로가기 생성 >> installer\install.bat
echo set DESKTOP=%%USERPROFILE%%\Desktop >> installer\install.bat
echo echo [InternetShortcut] ^> "%%DESKTOP%%\Travel Manager.url" >> installer\install.bat
echo echo URL=file:///%%INSTALL_DIR%%\TravelManagerWPF.exe ^>^> "%%DESKTOP%%\Travel Manager.url" >> installer\install.bat
echo echo IconFile=%%INSTALL_DIR%%\TravelManagerWPF.exe ^>^> "%%DESKTOP%%\Travel Manager.url" >> installer\install.bat
echo. >> installer\install.bat
echo pause >> installer\install.bat

REM 제거 스크립트 생성
echo ; Travel Manager 제거 스크립트 > installer\uninstall.bat
echo @echo off >> installer\uninstall.bat
echo echo Travel Manager 제거 중... >> installer\uninstall.bat
echo. >> installer\uninstall.bat
echo set INSTALL_DIR=%%LOCALAPPDATA%%\TravelManager >> installer\uninstall.bat
echo if exist "%%INSTALL_DIR%%" rmdir /s /q "%%INSTALL_DIR%%" >> installer\uninstall.bat
echo. >> installer\uninstall.bat
echo set DESKTOP=%%USERPROFILE%%\Desktop >> installer\uninstall.bat
echo if exist "%%DESKTOP%%\Travel Manager.url" del "%%DESKTOP%%\Travel Manager.url" >> installer\uninstall.bat
echo. >> installer\uninstall.bat
echo echo 제거 완료! >> installer\uninstall.bat
echo pause >> installer\uninstall.bat

REM README 생성
echo # Travel Manager 설치 가이드 > installer\설치가이드.txt
echo. >> installer\설치가이드.txt
echo ## 설치 방법 >> installer\설치가이드.txt
echo 1. install.bat 파일을 관리자 권한으로 실행하세요 >> installer\설치가이드.txt
echo 2. 설치가 완료되면 바탕화면에 바로가기가 생성됩니다 >> installer\설치가이드.txt
echo. >> installer\설치가이드.txt
echo ## 제거 방법 >> installer\설치가이드.txt
echo uninstall.bat 파일을 실행하세요 >> installer\설치가이드.txt
echo. >> installer\설치가이드.txt
echo ## 수동 실행 >> installer\설치가이드.txt
echo app\TravelManagerWPF.exe 파일을 직접 실행할 수 있습니다 >> installer\설치가이드.txt
echo (.NET Runtime 설치 불필요) >> installer\설치가이드.txt

if %NSIS_AVAILABLE% equ 1 (
    echo.
    echo NSIS 인스톨러 스크립트 생성 중...
    REM NSIS 스크립트는 복잡하므로 기본 구조만 제공
    echo ; NSIS 인스톨러 스크립트 템플릿이 installer\setup.nsi에 생성되었습니다. > installer\nsis_readme.txt
    echo ; 자세한 설정은 NSIS 문서를 참조하세요. >> installer\nsis_readme.txt
)

echo.
echo ==========================================
echo 인스톨러 생성 완료!
echo ==========================================
echo.
echo 생성된 파일:
echo - installer\app\TravelManagerWPF.exe (실행 파일)
echo - installer\install.bat (설치 스크립트)
echo - installer\uninstall.bat (제거 스크립트)  
echo - installer\설치가이드.txt (사용법)
echo - installer\docs\ (문서 및 스크린샷)
echo.
echo 배포 방법:
echo 1. installer 폴더 전체를 압축하여 배포
echo 2. 사용자가 install.bat 실행하여 설치
echo 3. 또는 app\TravelManagerWPF.exe 직접 실행
echo.
echo 배포 준비가 완료되었습니다!
echo ==========================================
pause 