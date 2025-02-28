#!/bin/bash 

# TODO: Install dotnet framework onto wsl
set -e 

echo "Building..."
dotnet build

echo "Running successfully."
dotnet run