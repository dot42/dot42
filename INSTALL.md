### Compiling

If you want to compile dot42 yourself be prepared to dig into `nant` and maybe `msbuild` basics. I found the build file to be not as portable as one would wish, so some errors might creep up when compiling. These should be solvable by simple adjustments in the build files.

Before building, make sure you have
- `nant` installed and in you path
- `git` in your path, e.g. msysgit. This dependency is not crucial, and could be easily removed from the buildfile.
- have `msbuild.exe` in your path, i.e. have called `VsDevCmd.bat` (Developer command prompt).
- VS 2013, 2012 or 2010. While you can perfectly use VS 2015 to compile your Dot42 projects, when compiling the Dot42 Framework itself the VS 2015 roslyn compiler crashes at the moment for unknown reasons. Recommended is VS 2013. 
- The build script tries to autodetect the correct VS installation. If it fails, try to adjust the `Common\Build\SelectVs.build` script. 
- if you want to compile the Visual Studio extension you will need an VS 201x SDK installed (and VS 201x as well). See also below. You might have to adjust the build files if you don't want the extension to be compiled, I did not test this.
- to build the setup, you will need to have InnoSetup installed.
- In the original dot42 GitHub-repository, some "Buildtools" where missing, making it impossible to compile the project with `nant`. I wrote a replacement that handles the most important usages and is a noop for others. You can compile it yourself by invoking `nant` in `.\Common\Build\NAnt.BuildTools.Tasks`.
  It would of course be better if the original Buildtools where available.



You can now go through the `.build` files in `Common\Build` and the  `NAnt.build` file in the root directory and adjust any obviously wrong paths. I successfully compiled with VS 2013 installed. Alternatively you can also come back to this step if you run into errors. 

To build, invoke `nant build-setup` from the root folder. It'll take a while to finish. You will find the installer in the `Build\Setup` folder.

###### Notes about the Visual Studio Integration

The VS plugin supports VS 2010, 2012, 2013 and 2015. Due to limitations of the VS XML Editor component, you can compile only for those versions that you have installed. I believe it is neccessary to install at least one VS 201x SDK, matching your installed VS version. 
For VS 2015, you should select the "Visual Studio Extensibility Tools" in the installer, or install it later by opening the New Project dialog and selecting the Install Visual Studio Extensibility Tools item under Visual C# / Extensibility (https://msdn.microsoft.com/en-us/library/bb166441.aspx). 

### Installing

After installing the setup, you might want to replace the included `adb.exe` with the one coming from you Android SDK, so the different versions don't interfere with each other. **Do not update aapt.exe**, since the dot42 compiler relies on a specific version.

### Hacking on Dot42

###### Recreating the Android framework bindings

If you indent to regenerate the Android API Framework bindings (everything under `Sources\Framework\Generated`), e.g. because you wish to include a new version or you improved the FrameworkBuilder, you should generate the Android API doxygen XML documentation. If you omit this step, proper IntelliSense for the Android API will not be available.

The Android platform binaries itself are located under `Binaries\Platform`. 

The doxygen xml documentation can not be included in the repository, for its sheer size and number of files. You can generate it from the Android sources with the help of the `AndroidFramework.doxygen.cfg` file found at `Sources\Android`. You can download the Android sources through the SDK manager. Pick the newest you can find (see also http://stackoverflow.com/questions/8832122/android-sdk-source-code). You will also need to download the doxygen binaries [from TODO]. [TODO: more details on how to prepare the doxygen xml files].
Once everything is in place, a simple call to `doxygen AndroidFramework.doxygen.cfg` will build the documentation. This will take a quite a while.

You then regenerate the framework bindings with a call to `nant generate-and-build-frameworks` from the Dot42 root folder.

###### Building the Visual Studio Extensions from within Visual Studio 

If you want to hack on Dot42 Visual Studio Extensions, there are some quirks to note. The VStudio.Xml.sln should be build first. Only those projects will succeed for which you have VS installed, but you can ignore these errors. Open VStudio.Android.sln only after you have build VStudio.Xml.sln at least once, so that VS can pick up the previously build XML Editor assemblies.
With VS2013 I found that the VSIX only gets property build in debug mode when I do a 'rebuild'. With an incremental build some dlls are missing from the VSIX for unknown reasons. This should not be a problem for debugging, as you will just copy the output file to the appropriate VS Extension directory. In release I did not encounter such a quirk.