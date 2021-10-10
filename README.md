# Build
```
dotnet publish MkvDefaultSwitcher2.Gtk/MkvDefaultSwitcher2.Gtk.csproj --configuration Release  -r linux-x64 -p:PublishSingleFile=true --self-contained true -p:PublishTrimmed=True -p:TrimMode=CopyUsed
dotnet publish MkvDefaultSwitcher2.Mac/MkvDefaultSwitcher2.Mac.csproj --configuration Release  -r osx-x64 -p:PublishSingleFile=true --self-contained true

```
