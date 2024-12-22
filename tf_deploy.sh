##!/bin/bash

# Step 1: Install az cli and terraform
#> choco install terraform
#> winget install -e --id Microsoft.AzureCLI
# Step 2: Restart your terminal
# Step 3a: Login to Azure in az cli
# Step 3b: Probably restart your terminal again
# Step 4: Run this script

#Bash
dotnet publish -c Release -r win-x64

#Powershell
powershell Compress-Archive -Path ./LocalVenue/bin/Release/net8.0/publish/* -DestinationPath ./LocalVenue/bin/Release/publish.zip -Force

#Bash
terraform init --upgrade
terraform apply --parallelism=25 -auto-approve

#az webapp deploy --resource-group <name> --name <name> --src-path "path"