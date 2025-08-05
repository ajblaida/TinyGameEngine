#!/bin/bash

# start-azurite.sh
# Installs Azurite if not present, then starts Azurite for local Azure Blob Storage emulation.

set -e

# Check if azurite is installed
if ! command -v azurite &> /dev/null; then
    echo "Azurite not found. Installing globally via npm..."
    if ! command -v npm &> /dev/null; then
        echo "npm is required but not installed. Please install Node.js and npm first."
        exit 1
    fi
    npm install -g azurite
fi

# Create a data directory for Azurite if it doesn't exist
DATA_DIR="./.azurite"
mkdir -p "$DATA_DIR"

# Start Azurite Blob service
echo "Starting Azurite Blob service..."
azurite-blob --location "$DATA_DIR" --debug "./.azurite/debug.log" &

echo "Azurite started. Blob service running at http://127.0.0.1:10000/"
echo "To stop Azurite, kill the process or run: pkill -f azurite-blob"