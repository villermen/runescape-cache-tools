#!/usr/bin/env bash

set -e
rm -rf dist/*
mkdir -p dist
./packages/ILMerge.*/tools/net452/ILMerge.exe -ndebug -out:dist/rsct.exe RuneScapeCacheToolsCLI/bin/Release/rsct.exe RuneScapeCacheToolsCLI/bin/Release/*.dll
cp -r RuneScapeCacheToolsCLI/bin/Release/runtimes dist/
