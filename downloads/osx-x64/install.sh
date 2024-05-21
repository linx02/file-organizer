#!/bin/bash

# Define the URL of your tar.gz file
TOOL_URL="https://raw.githubusercontent.com/linx02/file-organizer/master/downloads/osx-x64/FileOrganizer-OSX.tar.gz"
TOOL_NAME="FileOrganizer"
LINK_NAME="organize"

# Define the installation directory
INSTALL_DIR="$HOME/.local/bin"

# Create the installation directory if it doesn't exist
mkdir -p $INSTALL_DIR

# Create a temporary directory for downloading and extracting the tool
TEMP_DIR=$(mktemp -d)
cd $TEMP_DIR

echo "Downloading $TOOL_NAME..."
wget -q $TOOL_URL -O $TOOL_NAME.tar.gz

echo "Extracting $TOOL_NAME..."
tar -xzf $TOOL_NAME.tar.gz

# Move the extracted files to the tool name directory
if [ -d "osx-x64" ]; then
    mv osx-x64 $TOOL_NAME
else
    echo "Extraction failed. Directory 'osx-x64' not found."
    exit 1
fi

# Move the tool to the installation directory and ensure it’s executable
echo "Installing $TOOL_NAME to $INSTALL_DIR..."
mv $TOOL_NAME/* $INSTALL_DIR
chmod +x $INSTALL_DIR/$TOOL_NAME

# Create a symbolic link
echo "Creating a symbolic link '$LINK_NAME' for '$TOOL_NAME'..."
ln -sf $INSTALL_DIR/$TOOL_NAME $INSTALL_DIR/$LINK_NAME

# Clean up temporary files
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
