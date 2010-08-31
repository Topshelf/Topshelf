build.bat

if %errorlevel% = 1 then
 exit
end


git tag %VERSIONNUM%
git push topshelf-org master --tags 

gem build .\path\to\spec
gem push topshelf-%VERSION%.gem

REM Tweet

