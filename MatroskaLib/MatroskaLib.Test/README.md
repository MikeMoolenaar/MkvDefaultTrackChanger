# Unit tests

## Setup
The unit tests depend on ffmpeg and mkvalidator to make sure the program outputs valid MKV files. If you want to run the tests locally, please follow the steps below:

**Windows**  
- `winget install ffmpeg`
- *mkvalidator.exe is already included for windows, no need to install*

**Linux**
- Install ffmpeg using your package manager or see the [ffmpeg website](https://ffmpeg.org/download.html) for instructions to install.
- If you are on Arch Linux: [install mkvalidator from the AUR](https://aur.archlinux.org/packages/mkvalidator)  
  If you are on a Ubuntu based distro: [use this PPA](https://launchpad.net/~hizo/+archive/ubuntu/mkv-extractor-gui) to install mkvalidator  
  Otherwise: [Download the source code](https://sourceforge.net/projects/matroska/files/mkvalidator/) and compile using make.

**MacOS**
Use homebrew:
```
brew install mkvalidator ffmpeg
```
