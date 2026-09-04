#!/usr/bin/env fish

dotnet publish \
    BloodSugarWatchdog.Nightscout \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=false \
    -o ./publish
or exit

cp -a appsettings.Production.json publish
or exit

rsync -avz --delete \
    ./publish/ \
    nightscout:~/.local/bin/BloodSugarWatchdog.Nightscout/
or exit

rm -r ./publish
