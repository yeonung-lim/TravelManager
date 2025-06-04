@echo off
echo ====================================
echo Travel Manager 배포 빌드 스크립트
echo ====================================
echo.

REM 빌드 환경 확인
echo [1/4] 빌드 환경 확인 중...
dotnet --version
if %ERRORLEVEL% neq 0 (
    echo ERROR: .NET SDK가 설치되지 않았습니다.
    pause
    exit /b 1
)

REM 이전 빌드 정리
echo [2/4] 이전 빌드 파일 정리 중...
if exist "publish\win-x64" rmdir /s /q "publish\win-x64"
if exist "TravelManagerWPF\bin\Release" rmdir /s /q "TravelManagerWPF\bin\Release"
if exist "TravelManagerWPF\obj\Release" rmdir /s /q "TravelManagerWPF\obj\Release"

REM Release 빌드
echo [3/4] Release 빌드 중...
dotnet build TravelManagerWPF\TravelManagerWPF.csproj -c Release
if %ERRORLEVEL% neq 0 (
    echo ERROR: 빌드에 실패했습니다.
    pause
    exit /b 1
)

REM 배포 파일 생성
echo [4/4] 배포 파일 생성 중...
dotnet publish TravelManagerWPF\TravelManagerWPF.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o publish\win-x64
if %ERRORLEVEL% neq 0 (
    echo ERROR: 배포 파일 생성에 실패했습니다.
    pause
    exit /b 1
)

echo.
echo ====================================
echo 빌드 완료!
echo ====================================
echo 배포 파일 위치: publish\win-x64\TravelManagerWPF.exe
echo 폰트 폴더: publish\win-x64\fonts\
echo.
echo 파일 크기:
dir publish\win-x64\TravelManagerWPF.exe | find "TravelManagerWPF.exe"
echo.
echo 포함된 파일들:
dir publish\win-x64 /B
echo.
echo 배포 준비가 완료되었습니다.
echo ====================================
pause 