title api
dotnet run --project gameofwords.api.csproj -- ../../config/config.yaml
@if %errorlevel% neq 0 goto :err
exit /b

:err
@pause
