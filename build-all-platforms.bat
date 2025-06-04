@echo off
echo ==========================================
echo Travel Manager 다중 플랫폼 배포 빌드
echo ==========================================
echo.

REM 빌드 환경 확인
echo [1/3] 빌드 환경 확인 중...
dotnet --version
if %ERRORLEVEL% neq 0 (
    echo ERROR: .NET SDK가 설치되지 않았습니다.
    pause
    exit /b 1
)

REM 이전 빌드 정리
echo [2/3] 이전 빌드 파일 정리 중...
if exist "publish" rmdir /s /q "publish"
if exist "TravelManagerWPF\bin\Release" rmdir /s /q "TravelManagerWPF\bin\Release"
if exist "TravelManagerWPF\obj\Release" rmdir /s /q "TravelManagerWPF\obj\Release"

echo [3/3] 다중 플랫폼 배포 파일 생성 중...

REM Windows x64 배포
echo   - Windows x64 빌드 중...
dotnet publish TravelManagerWPF\TravelManagerWPF.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o publish\win-x64

REM Windows x86 배포  
echo   - Windows x86 빌드 중...
dotnet publish TravelManagerWPF\TravelManagerWPF.csproj -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o publish\win-x86

REM Windows ARM64 배포
echo   - Windows ARM64 빌드 중...
dotnet publish TravelManagerWPF\TravelManagerWPF.csproj -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o publish\win-arm64

REM Framework-dependent 배포 (더 작은 크기)
echo   - Framework-dependent 빌드 중...
dotnet publish TravelManagerWPF\TravelManagerWPF.csproj -c Release --self-contained false -p:PublishSingleFile=true -o publish\framework-dependent

echo.
echo ==========================================
echo 모든 플랫폼 빌드 완료!
echo ==========================================
echo.
echo 배포 파일 목록:
echo - Windows x64: publish\win-x64\TravelManagerWPF.exe
echo - Windows x86: publish\win-x86\TravelManagerWPF.exe  
echo - Windows ARM64: publish\win-arm64\TravelManagerWPF.exe
echo - Framework-dependent: publish\framework-dependent\TravelManagerWPF.exe
echo.
echo 파일 크기:
if exist "publish\win-x64\TravelManagerWPF.exe" (
    echo Windows x64:
    dir publish\win-x64\TravelManagerWPF.exe | find "TravelManagerWPF.exe"
)
if exist "publish\win-x86\TravelManagerWPF.exe" (
    echo Windows x86:
    dir publish\win-x86\TravelManagerWPF.exe | find "TravelManagerWPF.exe"
)
if exist "publish\win-arm64\TravelManagerWPF.exe" (
    echo Windows ARM64:
    dir publish\win-arm64\TravelManagerWPF.exe | find "TravelManagerWPF.exe"
)
if exist "publish\framework-dependent\TravelManagerWPF.exe" (
    echo Framework-dependent:
    dir publish\framework-dependent\TravelManagerWPF.exe | find "TravelManagerWPF.exe"
)
echo.
echo 배포 준비가 완료되었습니다.
echo ==========================================
pause 