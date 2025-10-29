@echo off
echo Building Define project...

dotnet clean
dotnet restore
dotnet build

echo.
echo Copying generated files...

set SOURCE=Generated\MessagePack.SourceGenerator\MessagePack.SourceGenerator.MessagePackGenerator
set DEST=Generated

xcopy /Y /Q "%SOURCE%\*.cs" "%DEST%\"
rmdir /S /Q "%DEST%\MessagePack.SourceGenerator"

echo.
echo Done! Files in Generated\:
dir /B Generated\*.cs

pause
