#!/usr/bin/env bash

set -e
rm -rf dist/*
mkdir -p dist
mv RuneScapeCacheToolsCLI/bin/Release/sqlite3.dll RuneScapeCacheToolsCLI/bin/Release/sqlite3.leavemealone
./packages/ILMerge.*/tools/net452/ILMerge.exe -ndebug -out:dist/rsct.exe RuneScapeCacheToolsCLI/bin/Release/rsct.exe RuneScapeCacheToolsCLI/bin/Release/*.dll
mv RuneScapeCacheToolsCLI/bin/Release/sqlite3.leavemealone RuneScapeCacheToolsCLI/bin/Release/sqlite3.dll
cp RuneScapeCacheToolsCLI/bin/Release/sqlite3.dll RuneScapeCacheToolsCLI/bin/Release/libsqlite3.so dist/
