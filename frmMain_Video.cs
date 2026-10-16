/*
 *  VideoSift (based on PhotoSift by RL Vision)
 *  Copyright (C) 2026  VideoSift contributors
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * */

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;

namespace VideoSift
{
	/// <summary>
	/// This "partial" collects all video related code for the main form:
	/// The LibVLC player, the playback control bar and the Video menu
	/// </summary>
	public partial class frmMain : Form
	{
		// LibVLC is created lazily, the first time a video is shown
		private LibVLC libVLC = null;
		private MediaPlayer mediaPlayer = null;
		private Media currentMedia = null;

		private bool bVideoActive = false;			// is the current file a video?
		private string sCurrentVideo = null;
		private bool bCurrentMediaRepeats = false;	// was the current media opened with VLC's own (seamless) repeat?
		private long lPendingSeekMs = 0;			// seek to apply once playback has started
		private bool bPendingPause = false;			// pause to apply once playback has started
		private bool bEnterDown = false;			// suppress key repeat for play/pause
		private Point LastVideoMousePos = new Point( -1, -1 );

		// GUI
		private VideoSurface videoView;
		private Panel panelVideoControls;
		private Label lblVideoPlayPause;
		private Label lblVideoTime;
		private Label lblVideoLoop;
		private Label lblVideoMute;
		private SeekBar seekVideo;
		private SeekBar seekVolume;
		private Timer timerVideo;
		private ToolTip toolTipVideo;
		private ToolStripMenuItem mnuVideo;
		private ToolStripMenuItem mnuVideoPlayPause;
		private ToolStripMenuItem mnuVideoLoop;
		private ToolStripMenuItem mnuVideoMute;
		private ToolStripMenuItem mnuVideoSeekForward;
		private ToolStripMenuItem mnuVideoSeekBack;

		private const string GlyphPlay = "▶";
		private const string GlyphPause = "⏸";
		private const string GlyphLoop = "\U0001F501";
		private const string GlyphSound = "\U0001F50A";
		private const string GlyphMuted = "\U0001F507";

		/// <summary>
		/// VideoView that never takes keyboard focus, so all keys keep going to the main form
		/// </summary>
		private class VideoSurface : VideoView
		{
			public VideoSurface()
			{
				SetStyle( ControlStyles.Selectable, false );
				TabStop = false;
			}
		}


		// -- Setup ----------------------------------------------------------------------------------------------------

		/// <summary>
		/// Creates the video surface, control bar and Video menu (the player itself is created on demand)
		/// </summary>
		private void InitVideoGui()
		{
			// video surface, behind the labels
			videoView = new VideoSurface();
			videoView.BackColor = Color.Black;
			videoView.Dock = DockStyle.Fill;
			videoView.Visible = false;
			videoView.MouseUp += videoView_MouseUp;
			videoView.MouseDoubleClick += videoView_MouseDoubleClick;
			panelMain.Controls.Add( videoView );
			videoView.SendToBack();

			// control bar: [play] [seek..........] [time] [loop] [mute] [volume]
			// (docked controls are laid out in reverse add order, so the fill control is added first)
			toolTipVideo = new ToolTip();
			panelVideoControls = new Panel();
			panelVideoControls.Height = 30;
			panelVideoControls.Dock = DockStyle.Bottom;
			panelVideoControls.Padding = new Padding( 2, 0, 6, 0 );
			panelVideoControls.Visible = false;

			seekVideo = new SeekBar();
			seekVideo.Dock = DockStyle.Fill;
			seekVideo.UserChanged += seekVideo_UserChanged;
			panelVideoControls.Controls.Add( seekVideo );

			lblVideoTime = new Label();
			lblVideoTime.AutoSize = false;
			lblVideoTime.Width = 120;
			lblVideoTime.Dock = DockStyle.Right;
			lblVideoTime.TextAlign = ContentAlignment.MiddleCenter;
			lblVideoTime.Font = new Font( "Segoe UI", 9f );
			panelVideoControls.Controls.Add( lblVideoTime );

			lblVideoLoop = MakeVideoButton( GlyphLoop, DockStyle.Right, "Loop (Ctrl+L)", delegate { ToggleVideoLoop(); } );
			lblVideoMute = MakeVideoButton( GlyphSound, DockStyle.Right, "Mute (Ctrl+M)", delegate { ToggleVideoMute(); } );

			seekVolume = new SeekBar();
			seekVolume.Width = 80;
			seekVolume.Dock = DockStyle.Right;
			seekVolume.UserChanged += seekVolume_UserChanged;
			toolTipVideo.SetToolTip( seekVolume, "Volume" );
			panelVideoControls.Controls.Add( seekVolume );

			lblVideoPlayPause = MakeVideoButton( GlyphPause, DockStyle.Left, "Play / Pause (Enter)", delegate { ToggleVideoPlayPause(); } );

			panelMain.Controls.Add( panelVideoControls );
			panelVideoControls.SendToBack();	// docked first, so the video fills the remaining space

			// Video menu, placed before Help
			mnuVideo = new ToolStripMenuItem( "&Video" );
			mnuVideoPlayPause = new ToolStripMenuItem( "Play / Pause", null, delegate { ToggleVideoPlayPause(); } );
			mnuVideoPlayPause.ShortcutKeyDisplayString = "Enter";
			mnuVideoLoop = new ToolStripMenuItem( "Loop", null, delegate { ToggleVideoLoop(); } );
			mnuVideoLoop.ShortcutKeys = Keys.Control | Keys.L;
			mnuVideoMute = new ToolStripMenuItem( "Mute", null, delegate { ToggleVideoMute(); } );
			mnuVideoMute.ShortcutKeys = Keys.Control | Keys.M;
			mnuVideoSeekForward = new ToolStripMenuItem( "Seek Forward", null, delegate { SeekVideo( settings.VideoSeekStep ); } );
			mnuVideoSeekForward.ShortcutKeyDisplayString = ".";
			mnuVideoSeekBack = new ToolStripMenuItem( "Seek Back", null, delegate { SeekVideo( -settings.VideoSeekStep ); } );
			mnuVideoSeekBack.ShortcutKeyDisplayString = ",";
			mnuVideo.DropDownItems.AddRange( new ToolStripItem[] {
				mnuVideoPlayPause, mnuVideoLoop, mnuVideoMute, new ToolStripSeparator(), mnuVideoSeekForward, mnuVideoSeekBack } );
			menuStripMain.Items.Insert( menuStripMain.Items.IndexOf( helpToolStripMenuItem ), mnuVideo );

			// polls playback position (and the mouse, see timerVideo_Tick)
			timerVideo = new Timer();
			timerVideo.Interval = 200;
			timerVideo.Tick += timerVideo_Tick;
			timerVideo.Start();
		}

		private Label MakeVideoButton( string glyph, DockStyle dock, string tooltip, EventHandler click )
		{
			Label lbl = new Label();
			lbl.AutoSize = false;
			lbl.Width = 34;
			lbl.Dock = dock;
			lbl.Text = glyph;
			lbl.TextAlign = ContentAlignment.MiddleCenter;
			lbl.Font = new Font( "Segoe UI Symbol", 12f );
			lbl.Cursor = Cursors.Hand;
			lbl.Click += click;
			toolTipVideo.SetToolTip( lbl, tooltip );
			panelVideoControls.Controls.Add( lbl );
			return lbl;
		}

		/// <summary>
		/// Called from ApplySettings
		/// </summary>
		private void ApplyVideoSettings()
		{
			Color back = settings.CustomMenuColors ? settings.CustomMenuColorBackground : Color.FromArgb( 30, 30, 30 );
			Color fore = settings.CustomMenuColors ? settings.CustomMenuColorText : Color.White;
			panelVideoControls.BackColor = back;
			panelVideoControls.ForeColor = fore;
			foreach( Control c in panelVideoControls.Controls ) c.BackColor = back;
			seekVideo.FillColor = seekVolume.FillColor = fore;

			ApplyVideoAudio();
			LayoutVideoControls();
			UpdateVideoControls();
		}

		private void EnsureVideoPlayer()
		{
			if( mediaPlayer != null ) return;

			Core.Initialize();	// loads libvlc from the libvlc\win-x64 folder
			libVLC = new LibVLC( "--no-video-title-show", "--no-osd", "--quiet" );
			mediaPlayer = new MediaPlayer( libVLC );
			mediaPlayer.EnableKeyInput = false;		// let the main form handle all input
			mediaPlayer.EnableMouseInput = false;

			// LibVLC events arrive on LibVLC's threads, and the player must not be called from them
			mediaPlayer.Playing += delegate { PostToGui( OnVideoPlaying ); };
			mediaPlayer.EndReached += delegate { PostToGui( OnVideoEnded ); };
			mediaPlayer.EncounteredError += delegate { PostToGui( OnVideoError ); };
			mediaPlayer.Vout += delegate { PostToGui( UpdateInfoLabel ); };

			videoView.MediaPlayer = mediaPlayer;
		}

		private void PostToGui( MethodInvoker m )
		{
			try
			{
				if( IsHandleCreated && !IsDisposed ) BeginInvoke( m );
			}
			catch( Exception ex )
			{
				Console.WriteLine( "PostToGui: " + ex.Message );	// form is closing
			}
		}

		private void DisposeVideoPlayer()
		{
			ReleaseVideoFile();
			if( mediaPlayer != null )
			{
				videoView.MediaPlayer = null;
				mediaPlayer.Dispose();
				mediaPlayer = null;
			}
			if( libVLC != null )
			{
				libVLC.Dispose();
				libVLC = null;
			}
		}


		// -- Showing and hiding videos --------------------------------------------------------------------------------

		/// <summary>
		/// Shows and starts playing a video in place of the picture box
		/// </summary>
		private void ShowVideo( string filename )
		{
			EnsureVideoPlayer();

			bVideoActive = true;
			sCurrentVideo = filename;
			picCurrent.Visible = false;
			videoView.Visible = true;
			LastVideoMousePos = new Point( -1, -1 );

			OpenVideo( 0, false );
			LayoutVideoControls();
			UpdateVideoControls();
		}

		/// <summary>
		/// Stops any playing video and hides the video surface
		/// </summary>
		private void HideVideo()
		{
			if( !bVideoActive ) return;

			ReleaseVideoFile();
			bVideoActive = false;
			sCurrentVideo = null;
			videoView.Visible = false;
			LayoutVideoControls();
		}

		/// <summary>
		/// Stops playback so that the file is no longer locked (needed before moving, renaming or deleting it)
		/// </summary>
		private void ReleaseVideoFile()
		{
			if( mediaPlayer != null ) mediaPlayer.Stop();
			if( currentMedia != null )
			{
				currentMedia.Dispose();
				currentMedia = null;
			}
		}

		/// <summary>
		/// (Re)opens the current video, optionally starting at a position
		/// </summary>
		private void OpenVideo( long startMs, bool startPaused )
		{
			Media oldMedia = currentMedia;

			// Use VLC's built-in repeat when looping, as it loops seamlessly
			bCurrentMediaRepeats = ShouldVideoRepeat();
			currentMedia = new Media( libVLC, sCurrentVideo, FromType.FromPath );
			if( bCurrentMediaRepeats ) currentMedia.AddOption( ":input-repeat=65535" );

			lPendingSeekMs = startMs;
			bPendingPause = startPaused;
			mediaPlayer.Play( currentMedia );

			if( oldMedia != null ) oldMedia.Dispose();
		}

		private bool ShouldVideoRepeat()
		{
			// when auto advance waits for the video to end, it must be allowed to end
			return settings.VideoLoop && !( bAutoAdvanceEnabled && settings.VideoAutoAdvanceWaitForEnd );
		}

		private bool IsVideoStopped()
		{
			VLCState s = mediaPlayer.State;
			return s == VLCState.Ended || s == VLCState.Stopped || s == VLCState.Error || s == VLCState.NothingSpecial;
		}

		/// <summary>
		/// The repeat mode is fixed when a video is opened. If loop or auto advance changes while playing,
		/// reopen the video at the same position so the change takes effect immediately.
		/// </summary>
		private void SyncVideoRepeatMode()
		{
			if( !bVideoActive || mediaPlayer == null ) return;

			if( IsVideoStopped() )
			{
				if( mediaPlayer.State == VLCState.Ended && ShouldVideoRepeat() ) OpenVideo( 0, false );	// turning loop on at the end restarts the video
			}
			else if( bCurrentMediaRepeats != ShouldVideoRepeat() )
			{
				OpenVideo( mediaPlayer.Time, mediaPlayer.State == VLCState.Paused );
			}
		}


		// -- Player events (always on the GUI thread, see PostToGui) ---------------------------------------------------

		private void OnVideoPlaying()
		{
			if( !bVideoActive ) return;

			ApplyVideoAudio();	// volume can't be set reliably before audio output exists
			if( lPendingSeekMs > 0 ) mediaPlayer.Time = lPendingSeekMs;
			if( bPendingPause ) mediaPlayer.SetPause( true );
			lPendingSeekMs = 0;
			bPendingPause = false;
			UpdateVideoControls();
		}

		private void OnVideoEnded()
		{
			if( !bVideoActive || mediaPlayer.State != VLCState.Ended ) return;	// ignore stale events from a previous file

			if( bAutoAdvanceEnabled && settings.VideoAutoAdvanceWaitForEnd )
				ShowNextPic( 1 );
			else if( settings.VideoLoop )
				OpenVideo( 0, false );	// fallback in case the video was opened without repeat
			else
				UpdateVideoControls();
		}

		private void OnVideoError()
		{
			if( !bVideoActive ) return;

			lblHeader.Text = "Error playing video:\n" + Path.GetFileName( sCurrentVideo );
			lblHeader.Visible = true;
			lblHeader.BringToFront();
			Util.CenterControl( lblHeader );
		}


		// -- Video commands -------------------------------------------------------------------------------------------

		private void ToggleVideoPlayPause()
		{
			if( !bVideoActive ) return;

			if( IsVideoStopped() )
			{
				OpenVideo( 0, false );	// play again from the start
				ResumeAutoAdvance();
			}
			else if( mediaPlayer.State == VLCState.Paused )
			{
				mediaPlayer.SetPause( false );
				ResumeAutoAdvance();
			}
			else
			{
				mediaPlayer.SetPause( true );
				HaltAutoAdvance();	// don't move on from a video the user is looking at
			}
			UpdateVideoControls();
		}

		private void ToggleVideoLoop()
		{
			settings.VideoLoop = !settings.VideoLoop;
			ShowStatusMessage( "Loop videos: " + ( settings.VideoLoop ? "On" : "Off" ) );
			SyncVideoRepeatMode();
			UpdateVideoControls();
		}

		private void ToggleVideoMute()
		{
			settings.VideoMuted = !settings.VideoMuted;
			ShowStatusMessage( "Video sound: " + ( settings.VideoMuted ? "Muted" : "On" ) );
			ApplyVideoAudio();
			UpdateVideoControls();
		}

		private void SeekVideo( int seconds )
		{
			if( !bVideoActive ) return;

			long length = mediaPlayer.Length;
			long target = Math.Max( 0, mediaPlayer.Time + seconds * 1000L );
			if( length > 0 ) target = Math.Min( target, length - 250 );

			if( IsVideoStopped() )
				OpenVideo( target, false );
			else
				mediaPlayer.Time = target;

			ShowStatusMessage( "Seek: " + Util.FormatTime( target ) + ( length > 0 ? " / " + Util.FormatTime( length ) : "" ) );
			UpdateVideoControls();
		}

		private void ApplyVideoAudio()
		{
			if( mediaPlayer == null ) return;
			mediaPlayer.Volume = Math.Max( 0, Math.Min( 100, settings.VideoVolume ) );
			mediaPlayer.Mute = settings.VideoMuted;
		}


		// -- Control bar ----------------------------------------------------------------------------------------------

		/// <summary>
		/// Windowed: the bar sits below the video. Fullscreen: the bar overlays the video and shows when the mouse is near the bottom.
		/// </summary>
		private void LayoutVideoControls()
		{
			if( panelVideoControls == null || settings == null ) return;

			if( !bVideoActive || !settings.VideoShowControls )
			{
				panelVideoControls.Visible = false;
			}
			else if( bFullScreen )
			{
				panelVideoControls.Dock = DockStyle.None;
				panelVideoControls.Bounds = new Rectangle( 0, panelMain.ClientSize.Height - panelVideoControls.Height, panelMain.ClientSize.Width, panelVideoControls.Height );
				panelVideoControls.BringToFront();
				panelVideoControls.Visible = false;	// until the mouse goes to the bottom of the screen
			}
			else
			{
				panelVideoControls.Dock = DockStyle.Bottom;
				panelVideoControls.SendToBack();
				panelVideoControls.Visible = true;
			}
			lblStatus.Top = GetStatusLabelTop();
		}

		/// <summary>
		/// In fullscreen, shows the control bar while the mouse is near the bottom of the screen
		/// </summary>
		private void UpdateVideoControlsVisibility( int mouseX, int mouseY )
		{
			if( !bFullScreen || !bVideoActive || !settings.VideoShowControls ) return;

			Point pt = panelMain.PointToClient( new Point( mouseX, mouseY ) );
			bool bNearBottom = pt.X >= 0 && pt.X <= panelMain.ClientSize.Width && pt.Y <= panelMain.ClientSize.Height
				&& pt.Y >= panelMain.ClientSize.Height - panelVideoControls.Height * 2;
			bool bShow = bNearBottom || seekVideo.IsDragging || seekVolume.IsDragging;
			if( panelVideoControls.Visible != bShow ) panelVideoControls.Visible = bShow;
		}

		/// <summary>
		/// Status label sits just above the control bar when the bar is docked below the video
		/// </summary>
		private int GetStatusLabelTop()
		{
			int bottom = panelMain.ClientSize.Height;
			if( panelVideoControls != null && panelVideoControls.Visible && panelVideoControls.Dock == DockStyle.Bottom ) bottom = panelVideoControls.Top;
			return bottom - lblStatus.Height - 2;
		}

		private void UpdateVideoControls()
		{
			if( panelVideoControls == null ) return;

			mnuVideoLoop.Checked = settings.VideoLoop;
			mnuVideoMute.Checked = settings.VideoMuted;

			Color fore = panelVideoControls.ForeColor;
			Color dim = Color.FromArgb( 255, ( fore.R + panelVideoControls.BackColor.R * 2 ) / 3, ( fore.G + panelVideoControls.BackColor.G * 2 ) / 3, ( fore.B + panelVideoControls.BackColor.B * 2 ) / 3 );
			lblVideoLoop.ForeColor = settings.VideoLoop ? fore : dim;
			SetTextIfChanged( lblVideoMute, settings.VideoMuted ? GlyphMuted : GlyphSound );
			lblVideoMute.ForeColor = settings.VideoMuted ? dim : fore;
			if( !seekVolume.IsDragging ) seekVolume.Value = settings.VideoVolume / 100f;

			if( !bVideoActive || mediaPlayer == null ) return;

			bool bShowPlay = IsVideoStopped() || mediaPlayer.State == VLCState.Paused;
			SetTextIfChanged( lblVideoPlayPause, bShowPlay ? GlyphPlay : GlyphPause );

			long length = mediaPlayer.Length;
			long time = IsVideoStopped() && mediaPlayer.State == VLCState.Ended ? length : mediaPlayer.Time;
			SetTextIfChanged( lblVideoTime, Util.FormatTime( time ) + " / " + Util.FormatTime( length ) );
			if( !seekVideo.IsDragging ) seekVideo.Value = length > 0 ? (float)time / length : 0;
		}

		private static void SetTextIfChanged( Control c, string text )
		{
			if( c.Text != text ) c.Text = text;
		}

		private void seekVideo_UserChanged( object sender, EventArgs e )
		{
			if( !bVideoActive ) return;
			long length = mediaPlayer.Length;
			if( length <= 0 ) return;

			long target = (long)( seekVideo.Value * length );
			if( IsVideoStopped() )
				OpenVideo( target, false );
			else
				mediaPlayer.Time = target;
			UpdateVideoControls();
		}

		private void seekVolume_UserChanged( object sender, EventArgs e )
		{
			settings.VideoVolume = (int)Math.Round( seekVolume.Value * 100 );
			settings.VideoMuted = false;
			ApplyVideoAudio();
			UpdateVideoControls();
		}

		private void timerVideo_Tick( object sender, EventArgs e )
		{
			if( !bVideoActive || mediaPlayer == null ) return;

			// VLC draws the video in its own window on its own thread, so the mouse hook doesn't see the mouse
			// while it's over the video. Poll it instead, to keep fullscreen menu/controls/cursor hiding working.
			Point p = Cursor.Position;
			if( p != LastVideoMousePos )
			{
				LastVideoMousePos = p;
				if( Form.ActiveForm == this ) UpdateMouse( p.X, p.Y );
			}

			UpdateVideoControls();
		}

		private void videoView_MouseUp( object sender, MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) ToggleVideoPlayPause();
		}

		private void videoView_MouseDoubleClick( object sender, MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left ) ToggleFullscreen();
		}
	}
}
