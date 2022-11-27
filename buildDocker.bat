@echo off

if [%1]==[] goto missing

echo Building docker image for version %1
docker build -t krsogaard/audible-series-downloader:%1 .
echo Pushing version %1 to docker hub
docker push krsogaard/audible-series-downloader:%1
echo Tagging version %1 as latest
docker tag krsogaard/audible-series-downloader:%1 krsogaard/audible-series-downloader:latest
echo Pushing version latest to docker hub
docker push krsogaard/audible-series-downloader:latest
goto :eof


:missing
@echo Version argument is missing
