title users
dotnet run --project gameofwords.users.csproj -- ../../config/config.yaml
@if %errorlevel% neq 0 goto :err
exit /b

:err
@pause
