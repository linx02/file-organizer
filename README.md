# FileOrganizer

FileOrganizer is a Command Line Interface (CLI) tool designed to help you automatically organize files in a specified folder based on custom rules.

## Features

- **Customizable Rules**: Define how files should be organized using a `.organizer` file.
- **Pattern Matching**: Use wildcards to specify file types and patterns for sorting.
- **Negative Rules**: Exclude specific files or folders from being organized.
- **Directory Specification**: Set the target directory to organize.
- **Empty Folder Removal**: Optionally remove empty folders after organizing.
- **Verbose Output**: Get detailed output of the organization process.
- **Syntax Checking**: Validate the syntax of the `.organizer` file before organizing.

## Installation
Copy and paste the following command to the terminal:
```bash
sh -c "$(wget -qO- https://raw.githubusercontent.com/linx02/file-organizer/master/downloads/osx-x64/install.sh)"

```

## Getting Started

### Creating the `.organizer` File

Create a `.organizer` file in the directory you want to organize. Define folders and rules within this file. Here are some examples:

#### Sorting by File Extension

```
Images/ *.jpg, *.png
```
This rule sorts all `.jpg` and `.png` files into the `Images` folder.

#### Sorting Specific Files

```
Code/ my_code.py, my_data.json
```
This rule sorts `my_code.py` and `my_data.json` into the `Code` folder.

#### Using Wildcards

```
Tests/ test_*.py, *_test.py
```
This rule sorts all `.py` files that begin with `test_` or end with `_test` into the `Tests` folder.

### Negative Rules

Exclude certain files or folders from being organized using negative rules.

#### Ignoring a Folder

```
!Important/
```
This rule will ignore the entire `Important` folder.

#### Ignoring a File

```
!super_important.txt
```
This rule will ignore the file `super_important.txt`.

## Usage

Run the `organize` command with the appropriate options:

```
organize [OPTIONS]
```

### Options

- `-d, --directory <path>`: Set the directory to organize (default: current directory).
- `-re, --remove-empty`: Remove empty folders after organizing.
- `-v, --verbose`: Print verbose output.
- `-h, --help`: Show help message.
- `-s, --syntax`: Check the syntax of the `.organizer` file.

### Examples

#### Organize the Current Directory

```
organize
```

#### Organize a Specific Directory

```
organize -d /path/to/directory
```

#### Remove Empty Folders After Organizing

```
organize -re
```

#### Print Verbose Output

```
organize -v
```

#### Check Syntax of `.organizer` File

```
organize -s
```

#### Show Help Message

```
organize -h
```

## Contributing

Contributions are welcome! Please open an issue or submit a pull request on GitHub.

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.

---