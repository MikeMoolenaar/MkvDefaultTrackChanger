# Building 

## Windows
```
dotnet publish MkvDefaultTrackChanger/MkvDefaultTrackChanger.WinForms/MkvDefaultTrackChanger.WinForms.csproj --configuration Release  -r win-x64 -p:PublishSingleFile=true --self-contained true -p:PublishTrimmed=True -p:IncludeNativeLibrariesForSelfExtract=true -p:Version=1.0
```

## Linux
```
dotnet publish MkvDefaultTrackChanger.Gtk/MkvDefaultTrackChanger.Gtk.csproj --configuration Release -r linux-x64 -p:PublishSingleFile=true --self-contained true -p:PublishTrimmed=True -p:TrimMode=CopyUsed -p:Version=1.0
```

## Mac
```
dotnet publish MkvDefaultTrackChanger.Mac/MkvDefaultTrackChanger.Mac.csproj --configuration Release -r osx-x64 -p:PublishSingleFile=true --self-contained true -p:Version=1.0
```
