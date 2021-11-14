[![GPLv3 License](https://img.shields.io/badge/License-GPL%20v3-yellow.svg)](https://opensource.org/licenses/)
# MkvDefaultTrackChanger
Small application to change the default subtitle and audio tracks in
MKV video files. It can handle multiple files and runs on Windows, Linux and Mac OS.

## What is MKV and what problem does this program solve?
MKV is a multimedia container format and can store multiple
video, audio and subtitle files with a variety of different formats. This is mostly
used by having multiple subtitles and audio tracks in different languages. When an  MKV
file is played in a video player, like VLC, it chooses the subtitle and audio track to show
by default. This depends on which tracks are set as default in the MKV file itself
and the settings of the video player (VLC, for example, can show the subtitle in a certain language
by default).

This program aims to make editing the default audio and/or subtitle track
in (multiple) MKV files easy. It can edit multiple files provided they
have the same audio and subtitle tracks, through the contents of these files can naturally
be different.

## Demo
![Demo](https://github.com/MikeMoolenaar/MkvDefaultTrackChanger/blob/main/Assets/Animation.gif?raw=true)  
Aditional screenshots for each platform: [Linux](https://github.com/MikeMoolenaar/MkvDefaultTrackChanger/blob/main/Assets/Screenshot%20linux.jpg?raw=true), [MacOS](https://github.com/MikeMoolenaar/MkvDefaultTrackChanger/blob/main/Assets/Screenshot%20mac.png?raw=true) and [Windows](https://github.com/MikeMoolenaar/MkvDefaultTrackChanger/blob/main/Assets/Screenshot%20windows.jpg?raw=true)

## Download
Go to the [releases page](https://github.com/MikeMoolenaar/MkvDefaultTrackChanger/releases) to download the latest version for your platform. Download
and extract the ZIP archive to start using the application.

### Additional instructions for linux
The ZIP file for Linux contains the GTK application which you must run from the command
line, for example:
```sh
unzip MkvDefaultTrackChanger-linux-V1.zip
./MkvDefaultTrackChanger-V1.0.Gtk
```

## Credits
The following packages were used to make creating this application possible. I would
like to sincirely thank their authors, contributors and other involved developers.
- [Eto.Forms](https://github.com/picoe/Eto) ([licence](https://github.com/picoe/Eto/blob/develop/LICENSE.txt))  
  GUI framework which makes it possible to create applications for multiple platforms.
- [NEbml](https://github.com/OlegZee/NEbml) ([licence](https://github.com/OlegZee/NEbml/blob/master/LICENSE))  
  Reader and writer for the EBML markup language that MKV uses.

## Support
Feel free to create an [issue](https://github.com/MikeMoolenaar/MkvDefaultTrackChanger/issues/new) if you run into any problems.


## License
Copright (C) 2020 Mike Moolenaar  
Licenced under the [GNU General Public Licence v3](https://www.gnu.org/licenses/gpl-3.0.html).
