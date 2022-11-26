#!/usr/bin/env bash

[[ "$TRACE" ]] && set -x
pushd `dirname "$0"` > /dev/null
trap __EXIT EXIT

colorful=false
tput setaf 7 > /dev/null 2>&1
if [[ $? -eq 0 ]]; then
    colorful=true
fi

function __EXIT() {
    popd > /dev/null
}

function printError() {
    $colorful && tput setaf 1
    >&2 echo "Error: $@"
    $colorful && tput setaf 7
}

function printImportantMessage() {
    $colorful && tput setaf 3
    >&2 echo "$@"
    $colorful && tput setaf 7
}

function printUsage() {
    $colorful && tput setaf 3
    >&2 echo "$@"
    $colorful && tput setaf 7
}

source ./misc/var.sh

function processProtoFile() {
    local OLD_DIR=`pwd`
    cd "$1"
    echo "Processing $1/${@:2}..."
    protoc -I=. "${@:2}" --gogo_out=plugins=grpc:./server
    protoc -I=. "${@:2}" --csharp_out=./client
    [[ $? -ne 0 ]] && exit 1
    cd "$OLD_DIR"
}

for file in *.proto
do
    processProtoFile . $file
done


[[ $? -ne 0 ]] && exit 1
