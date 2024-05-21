#!/bin/bash

TOOL_URL="https://raw.githubusercontent.com/linx02/file-organizer/master/downloads/osx-x64/FileOrganizer-OSX.tar.gz"
TOOL_NAME="FileOrganizer"
LINK_NAME="organize"

INSTALL_DIR="$HOME/.local/bin"

mkdir -p $INSTALL_DIR

TEMP_DIR=$(mktemp -d)
cd $TEMP_DIR

echo "Downloading $TOOL_NAME..."
wget $TOOL_URL -O $TOOL_NAME.tar.gz

echo "Extracting $TOOL_NAME..."
tar -xzvf $TOOL_NAME.tar.gz

echo "Installing $TOOL_NAME to $INSTALL_DIR..."
mv $TOOL_NAME/* $INSTALL_DIR

echo "Creating a symbolic link '$LINK_NAME' for '$TOOL_NAME'..."
ln -s $INSTALL_DIR/$TOOL_NAME $INSTALL_DIR/$LINK_NAME

cd ~
rm -rf $TEMP_DIR

if ! echo "$PATH" | grep -q "$INSTALL_DIR"; then
    echo "Adding $INSTALL_DIR to PATH..."
    echo "export PATH=\"$INSTALL_DIR:\$PATH\"" >> ~/.bash_profile
    source ~/.bash_profile
fi

if command -v $LINK_NAME >/dev/null 2>&1; then
    echo "$TOOL_NAME installed successfully. Use '$LINK_NAME --help' to get started!"
else
    echo "Installation failed. Please check the installation script."
fi
