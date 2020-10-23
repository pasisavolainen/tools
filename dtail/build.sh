#!/bin/sh

mkdir -p out

docker run \
    --rm -it --name dtail-build \
    -w /src \
    -v `pwd`:/src -v `pwd`/out:/out:rw \
    mcr.microsoft.com/dotnet/sdk:5.0 \
    /bin/sh publishbinaries.sh
