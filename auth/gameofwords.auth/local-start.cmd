title auth
dotnet run --project gameofwords.auth.csproj -- ../../config/config.yaml
@if %errorlevel% neq 0 goto :err
exit /b

:err
@pause
