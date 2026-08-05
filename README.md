[![NuGet](https://img.shields.io/nuget/v/SA3D.Texturing)](https://www.nuget.org/packages/SA3D.Texturing) 
[![downloads](https://img.shields.io/nuget/dt/SA3D.Texturing)](https://www.nuget.org/packages/SA3D.Texturing)

# SA3D.Texturing
SA3D Texture handler library

## Releasing
!! Requires authorization via the X-Hax organisation

1. Edit the version number in src/SA3D.Texturing/SA3D.Texturing.csproj; Example: `<Version>1.0.0</Version>` -> `<Version>2.0.0</Version>`
2. Commit the change but dont yet push.
3. Tag the commit: `git tag -a [version number] HEAD -m "Release version [version number]"`
4. Push with tags: `git push --follow-tags`

This will automatically start the Github `Build and Publish` workflow