
--- VideoSift Source Code Comments --------------------------------------------

  VideoSift is a fork of PhotoSift 1.11 by RL Vision. The original source
  comments are kept below.

  Language: C#
  Target: .NET Framework 4.8, Windows x64
  License: GPL v3 (see LICENSE.txt)

  Building:

    Needs the .NET SDK (6 or newer) or Visual Studio 2022. NuGet restores
    LibVLCSharp and the LibVLC binaries (nuget.config points at nuget.org).

      dotnet build -c Release

    Output goes to bin\Release
et48\. Ship the whole folder, including
    the "libvlc" subfolder.

  Video support:

    frmMain_Video.cs  LibVLC player, control bar and Video menu
    SeekBar.cs        Non-focusable slider for seek/volume (keeps keys
                      going to the main form, where they sort files)

    Videos are never put in the image cache. A playing video keeps its file
    open, so ReleaseVideoFile() is called before moving, renaming or
    deleting it.

    The LibVLC plugins that a local viewer doesn't need are excluded in
    VideoSift.csproj (VlcWindowsX64ExcludeFiles) to keep the size down.


--- Original PhotoSift Source Code Comments --------------------------------------------

  Language: C#
  IDE: Visual Studio 2010 Professional
  Target: DotNet Framework 2.0 on Windows
  License: GPL

  Notes:

    I need to maintain a custom version of PhotoSift with some special 
	requirements that are not suitable for the public version. This is 
	managed using "#if RLVISION ... #endif" where needed. This symbol 
	is defined in two special configurations called "RLV Debug" and 
	"RLV Release".
	
	Is Mono supported? I don't know. There are a few P/Invoke that probably
	need to be removed, but these are generally not essential. I have collected
	all Windows specific API calls in the WinAPI.cs file. Let me know if you 
	know more about Mono!

	I have tried to make the code well documented and structured. I think I
	have done a fairly good job, with one exception: The code and workflow
	managing the various zoom and view modes is not always pretty. Apologies 
	for	this. I will work on improving it in the future.

	If you modify PhotoSift and release it publicly, I would be happy to hear
	about it!

