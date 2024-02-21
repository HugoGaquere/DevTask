#!/bin/bash 

# Build the application
dotnet publish --configuration Release --runtime linux-x64 --self-contained true DevTask/DevTask.csproj -o ./publish

# Copy the install.sh file to the publish directory
cp ./scripts/install.sh ./publish

# Create the installer
makeself ./publish devtask_linux_x64.sh "DevTask installer" ./install.sh

# Set the necessary permissions
chmod 755 ./devtask_linux_x64.sh
