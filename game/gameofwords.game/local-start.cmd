title game
dotnet run --project gameofwords.game.csproj -- ../../config/config.yaml
@if %errorlevel% neq 0 goto :err
exit /b

:err
@pause
