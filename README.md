# Build
```
dotnet publish EtoTest.Gtk/EtoTest.Gtk.csproj --configuration Release  -r linux-x64 -p:PublishSingleFile=true --self-contained true -p:PublishTrimmed=True -p:TrimMode=CopyUsed
dotnet publish EtoTest.Mac./EtoTest.Mac.csproj --configuration Release  -r osx-x64 -p:PublishSingleFile=true --self-contained true -p:PublishTrimmed=True -p:TrimMode=CopyUsed

```