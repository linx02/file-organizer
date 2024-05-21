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

# Rename the extracted directory to TOOL_NAME
if [ -d "osx-x64" ]; then
    mv osx-x64 $TOOL_NAME
else
    echo "Extraction failed. Directory 'osx-x64' not found."
    exit 1
fi

# Move the entire TOOL_NAME directory to the installation directory
echo "Installing $TOOL_NAME to $INSTALL_DIR..."
mv $TOOL_NAME $INSTALL_DIR

# Ensure the main executable is executable
chmod +x $INSTALL_DIR/$TOOL_NAME/FileOrganizer

# Create a symbolic link
echo "Creating a symbolic link '$LINK_NAME' for '$TOOL_NAME'..."
ln -sf $INSTALL_DIR/$TOOL_NAME/FileOrganizer $HOME/.local/bin/$LINK_NAME

# Clean up temporary files
cd ~
rm -rf $TEMP_DIR

# Check if the .local/bin is in the PATH, add if it's not
if ! echo "$PATH" | grep -q "$HOME/.local/bin"; then
    echo "Adding $HOME/.local/bin to PATH..."
    echo "export PATH=\"$HOME/.local/bin:\$PATH\"" >> ~/.bash_profile
    source ~/.bash_profile
fi

# Verify installation
if command -v $LINK_NAME >/dev/null 2>&1; then
    echo "$TOOL_NAME installed successfully. Use '$LINK_NAME --help' to get started!"
else
    echo "Installation failed. Please check the installation script."
fi
