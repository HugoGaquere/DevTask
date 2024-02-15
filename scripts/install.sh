#!/bin/bash 

# Define the application directory
APP_DIR="/opt/devtask"

echo "Installing DevTask"

# Create the application directory
mkdir -p $APP_DIR

# Copy the application files
cp -r "$PWD"/* $APP_DIR

# Set the necessary permissions
chmod -R 755 $APP_DIR

# Optionally, create a symbolic link to the application executable
ln -s $APP_DIR/DevTask /usr/local/bin/DevTask
