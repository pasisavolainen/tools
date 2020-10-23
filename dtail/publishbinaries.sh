#!/bin/sh

dotnet publish /p:RuntimeIdentifier=win10-x64 /p:PublishSingleFile=true -o /out -v m
dotnet publish /p:RuntimeIdentifier=linux-x64 /p:PublishSingleFile=true -o /out -v m
