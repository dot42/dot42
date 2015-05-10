### Compiling

If you want to compile dot42 yourself be prepared to dig into `nant` and maybe `msbuild` basics. I found the build file to be not as portable as one would wish, so some errors might creep up when compiling. These should be solvable by simple adjustments in the build files.

Before building, make sure you have
- `nant` installed and in you path
- `git` in your path, e.g. msysgit. This dependency is not crucial, and could be easily removed from the buildfile.
- have `msbuild.exe` in your path, i.e. have called `VsDevCmd.bat` (Developer command prompt).
- if you want to compile the Visual Studio extension you will need the VS 2013 SDK installed (and probably VS 2013 as well). If you want the extension to work with VS2010 or VS 2012 see below. You might have to adjust the build files if you don't want the extension to be compiled, I did not test this.
- to build the setup, you will need to have InnoSetup installed.
- In the original dot42 GitHub-repository, some "Buildtools" where missing, making it impossible to compile the project with `nant`. I wrote a replacement that handles the most important usages and is a noop for others. To enable it, call `set BUILDTOOLS={path-to-dot42-root-folder}\Common\Build\NAnt.BuildTools.Tasks.dll`. You can compile it yourself by invoking `nant` in `.\Common\Build\NAnt.BuildTools.Tasks`.
  It would of course be better if the original Buildtools where available.


You can now go through the `.build` files in `Common\Build` and the  `NAnt.build` file in the root directory and adjust any obviously wrong paths. I successfully compiled with VS 2013 installed. Alternatively you can also come back to this step if you run into errors.

To build, invoke `nant build-setup` from the root folder. It'll take a while to finish. You will find the installer in the `Build\Setup` folder.

###### Notes about the Visual Studio Integration

I disabled the VS 2010 and VS 2012 compatibility, mainly because my SDD wouldn't fit the two additional installations, and I did not find out how to compile for those versions without installing them. If you want to compile for VS 2010 or VS 2012 versions, adjustments to `VStudio.Android.sln` and `VStudio\VStudio.Extension.Android.csproj` have to be made. The crucial part is the "XmlEditor". Dig through the Git-logs to see what needs to be changed. It'll be great if a way was found to compile for all three versions (and maybe VS 2015) without having them all installed. You will probably also need to install the appropriate VS 20xx SDKs.

### Installing

After installing the setup, you might want to replace the included `adb.exe` with the one coming from you Android SDK, so the different versions don't interfere with each other. **Do not update aapt.exe**, since the dot42 compiler relies on a specific version.

### Hacking on Dot42

###### Recreating the Android framework bindings

If you indent to regenerate the Android API Framework bindings (everything under `Sources\Framework\Generated`), e.g. because you wish to include a new version or you improved the FrameworkBuilder, you should generate the Android API doxygen XML documentation. If you omit this step, proper IntelliSense for the Android API will not be available.

The Android platform binaries itself are located under `Binaries\Platform`. 

The doxygen xml documentation can not be included in the repository, for its sheer size and number of files. You can generate it from the Android sources with the help of the `AndroidFramework.doxygen.cfg` file found at `Sources\Android`. You can download the Android sources through the SDK manager. Pick the newest you can find (see also http://stackoverflow.com/questions/8832122/android-sdk-source-code). You will also need to download the doxygen binaries [from TODO]. [TODO: more details on how to prepare the doxygen xml files].
Once everything is in place, a simple call to `doxygen AndroidFramework.doxygen.cfg` will build the documentation. This will take a quite a while.

You then regenerate the framework bindings with a call to `nant generate-and-build-frameworks` from the Dot42 root folder.