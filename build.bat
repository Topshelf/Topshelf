@echo off

echo Building for .NET 3.5
call rake BUILD_CONFIG_KEY=NET35

echo Building for .NET 4.0
call rake unclean

