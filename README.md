# Build
```
dotnet publish MkvDefaultSwitcher.Gtk/MkvDefaultSwitcher.Gtk.csproj --configuration Release  -r linux-x64 -p:PublishSingleFile=true --self-contained true -p:PublishTrimmed=True -p:TrimMode=CopyUsed
dotnet publish MkvDefaultSwitcher.Mac/MkvDefaultSwitcher.Mac.csproj --configuration Release  -r osx-x64 -p:PublishSingleFile=true --self-contained true

```
