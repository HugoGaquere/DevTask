#!/bin/bash 

# Build the application
dotnet publish --configuration Release --runtime linux-x64 --self-contained true DevTask/DevTask.csproj -o ./publish

# Copy the install.sh file to the publish directory
cp install.sh ./publish

# Create the installer
makeself ./publish devtask.sh "DevTask installer" ./install.sh
