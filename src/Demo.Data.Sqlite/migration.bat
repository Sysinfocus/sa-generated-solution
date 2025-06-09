@echo off
dotnet ef migrations add %1 --startup-project ..\Demo.API
