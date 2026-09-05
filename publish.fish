#!/usr/bin/env fish

set projects \
    BloodSugarWatchdog.DailyReport \
    BloodSugarWatchdog.Monitor \
    BloodSugarWatchdog.Nightscout

for project in $projects
    dotnet publish \
        Source/$project \
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
        nightscout:~/.local/bin/$project/
    or exit

    rm -r ./publish
end
