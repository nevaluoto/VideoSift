
--- VideoSift -----------------------------------------------------------------

  VideoSift is a fork of PhotoSift that adds video support.
  VideoSift modifications (c) 2026 VideoSift contributors.

  Based on PhotoSift, developed by RL Vision (c) 2013-2014
  (https://www.rlvision.com/photosift/about.php)
  Special thanks (from PhotoSift) to Michael for all support, suggestions,
  testing and the logo.

  Video playback uses LibVLC (https://www.videolan.org/vlc/libvlc.html),
  licensed under LGPL 2.1 or later.

  Free, open source. Licensed under GPL

  Portable:
    • Just unzip and run
    • Settings are saved in the application folder as 'Settings.xml'.
      This file can be kept when upgrading. Removing it restores the
      default settings.


--- About ---------------------------------------------------------------------

  VideoSift is a utility helping you to quickly organize unsorted image
  and video libraries. The basic idea is to load the program with images
  and videos, then show and inspect each file and press a key on the
  keyboard. The file will then be moved or copied to a folder of your
  choice corresponding to that key, and the next file is displayed.

  Videos play in a loop where the image would be shown, so you can look at
  them for as long as you need before deciding where they go.

  Features:
	• Quickly organize your unsorted images and videos
    • Looping video playback with simple controls (play/pause, seek,
      loop, mute, volume)
    • Fast keyboard-based workflow
    • Multithreaded cache-ahead loading
    • Various inspection tools (zoom, flip, rotate)
    • Rename or delete files when needed
	• Unlimited undo
    • Fullscreen mode for less distractions
    • Highly configurable
    • Portable
    • Free, open source


--- Getting Started -----------------------------------------------------------

  This section outlines how to use VideoSift for the first time. Images and
  videos are handled the same way: everything you add goes into one list
  (the "image pool"), and you sort each file with a single key press.

  1. Set the base target folder

     First decide where files should end up when you press a key. On the
     "File" menu, select "Set Target Base Folder". (If you don't change
     this folder, it defaults to the folder VideoSift.exe is in.)

  2. Set file mode

     VideoSift can either move or copy your files. Default is move. To
     change this, open "Edit" > "Settings" and look under "File
     Operations". Check the other file operation settings too.

  3. Add images and videos

     Open the "Image Pool" menu and select "Add Files" or "Add from
     Folder". The latter lets you choose a folder, and then finds all
     images and videos in that folder and its subfolders. You can also
     drag and drop files or folders onto the window.

  4. Browse the pool

     Press Right and Left to step forwards and backwards. Images are shown
     to fit the window. Videos start playing automatically and loop, with a
     control bar below them:

       [Play/Pause] [------ seek ------] [time] [Loop] [Mute] [Volume]

     You can also use the keyboard: Enter plays/pauses, comma and period
     seek backwards/forwards, Ctrl+L toggles looping and Ctrl+M mutes.
     Clicking the video pauses it, double-clicking toggles fullscreen.
     See the hotkey section below for everything else.

  5. Press a key

     When a file is displayed, press a letter or number key. Let's say you
     press "A". VideoSift will move (or copy) the file into a subfolder
     named "A" inside the folder you chose in step 1, and show the next
     file. A playing video is stopped first, so it can be moved.

     For example:
       • Currently viewing: "c:\temp\clip.mp4"
       • Base target folder is: "d:\sorted\"
       • You press "A"
       • The video is moved to: "d:\sorted\A\clip.mp4"

     Made a mistake? Ctrl+Z undoes the last move, copy, rename or delete.

     The next section describes how to change the destination folder
     associated with each key.


--- Destination Folders -------------------------------------------------------

  In Settings you can choose the destination folder for each key. There
  are three ways to set it:

  1: Leave the field empty.
     The key name is used as the folder name. If you press "A", the file
     goes into a subfolder named "A" in the base target folder.

  2: Enter a custom name.
     This is used as the folder name, as a subfolder of the base target
     folder. Example: "Holiday Videos"

  3: Enter a complete path.
     This is used as the destination folder as is.
     Example: "c:\videos\keepers\"

  You can change the base target folder from the "File" menu. If you don't
  change it, it defaults to the folder VideoSift.exe is in.

  If the destination folder does not exist, it is created automatically.


--- Hotkeys -------------------------------------------------------------------

  VideoSift is built around using shortcut keys. This is a complete list of all
  available keys:

  Navigate:
   • Right/Down/PgDown/Space: Show next file
   • Left/Up/PgUp: Show previous file
   • Ctrl+Arrows: Skip 10 files forward/backwards
   • Shift+Arrows: Skip 25 files forward/backwards
   • Home/End: Go to first or last file

  Mouse (images):
   • Left Mouse Button: Toggle scale vs actual size (1:1) mode
   • Right Mouse Button: Toggle zoom mode
   • Middle Mouse Button: Flip image vertically (*)
   • MouseWheel: Next/previous file

  File operations & image pool:
   • A-Z and 0-9: Move or copy current file
   • F9: Select base target folder
   • Shift+Del: Deletes the current file
   • Alt+Del: Deletes the current file to recycle bin
   • Ctrl+Del: Remove the current file from the image pool
   • Del: Delete/Recycle/Remove depending on settings
   • Insert: Opens a file dialog to add images and videos
   • Ctrl+Insert: Add all images and videos in a folder and its subfolders
   • F2: Rename the current file
   • Ctrl+Z: Undo last file operation (**)
   • Ctrl+R: Randomize the order of the files in the pool

  Display
   • Ctrl+W: Opens input box to scale to specific width (in pixels)
   • Ctrl+H: Opens input box to scale to specific width (in pixels)
   • +: Zoom in
   • -: Zoom out
   • Ctrl+Shift+Up: Flip image vertically (*)
   • Ctrl+Shift+Down: Flip image horizontally (*)
   • Ctrl+Shift+Left: Rotate image CCW (*)
   • Ctrl+Shift+Right: Rotate image CW (*)
   • F4: Toggle enlarge small images
   • F8: Toggles if the scale mode should be kept when changing image

  System:
   • F1: Open this help file
   • F11 or Alt+Enter: Toggle fullscreen
   • F12: Open settings
   • Ctrl+C: Copy the current image to clipboard
   • Pause/Break: Toggle auto advance on or off
   • Tab: Toggle showing the menubar
   • Ctrl+1-9: In a multi monitor setup, if VideoSift is in fullscreen
               mode, Ctrl+# moves the application to screen number #.

  Video (when a video is shown):
   • Enter: Play / pause (also: click the video)
   • . (period): Seek forward 5 seconds (Shift: 30 seconds)
   • , (comma): Seek back 5 seconds (Shift: 30 seconds)
   • Ctrl+L: Toggle looping
   • Ctrl+M: Toggle mute
   • Double-click video: Toggle fullscreen
   • Ctrl+C: Copy the video file to clipboard (paste it in Explorer)

   The seek step, volume, looping and whether to show the control bar can
   be changed in Settings under "Video". In fullscreen the control bar
   appears when you move the mouse to the bottom of the screen.

   With auto advance (Pause/Break) enabled, videos play to the end once
   before moving on (can be changed in Settings).

   Supported video files: mp4, m4v, mov, mkv, webm, avi, wmv, asf, flv,
   f4v, mpg, mpeg, m2ts, mts, ts, 3gp, ogv, vob, divx

   (*)  Flipping and rotating is only visual and does not affect the
        actual image file.

   (**) Undo works on move, copy, rename and delete to recycle bin.


--- Command Line --------------------------------------------------------------

  When starting VideoSift you can supply a list of files or folders. VideoSift
  will attempt to load these files and the content of folders (including
  subfolders).

  Example:

    VideoSift.exe "c:\temp\someimage.jpg" "c:\allmyvideos"


--- Requirements --------------------------------------------------------------

  VideoSift needs 64-bit Windows 10 or 11 (".NET Framework 4.8" is already
  part of Windows). Keep the "libvlc" folder next to VideoSift.exe; it
  contains the video player.

--- Version History -----------------------------------------------------------

  VideoSift 1.0 (2026-10-16)
       Forked from PhotoSift 1.11
       Added looping video playback with play/pause, seek, loop, mute and
       volume controls (LibVLC)
       Videos can be sorted, renamed, deleted and undone just like images
       Auto advance can wait for a video to finish
       Now targets .NET Framework 4.8 (64-bit)

  PhotoSift history:

  1.11 (2014-06-15)
       Fixed a visual problem with the black scheme on Windows 8

  1.1  (2014-03-29)
       Added undo for all file operations
       Added customizable menu colors (defaults to black scheme)
       Added zoom in/out and to specific width/height
       Added command line support
       Added option to keep scale mode when changing picture
       Added rudimentary stats to about dialog
       Added menu item to shuffle loaded images
       Lots of minor additionas, tweaks & fixes

  1.0  (2014-01-22)
       First public release

