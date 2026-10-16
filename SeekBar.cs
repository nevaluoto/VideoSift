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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace VideoSift
{
	/// <summary>
	/// Minimal slider used for video seeking and volume.
	/// Unlike TrackBar it never takes keyboard focus, so arrow keys and sort keys keep working.
	/// </summary>
	class SeekBar : Control
	{
		private float value = 0;
		private bool bDragging = false;

		public Color TrackColor { get; set; }
		public Color FillColor { get; set; }

		/// <summary>Raised when the user changes the value with the mouse</summary>
		public event EventHandler UserChanged;

		public SeekBar()
		{
			SetStyle( ControlStyles.Selectable, false );
			SetStyle( ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true );
			TabStop = false;
			Cursor = Cursors.Hand;
			TrackColor = Color.FromArgb( 90, 90, 90 );
			FillColor = Color.FromArgb( 220, 220, 220 );
		}

		/// <summary>Value in the range 0..1</summary>
		public float Value
		{
			get { return value; }
			set
			{
				float v = Math.Max( 0, Math.Min( 1, value ) );
				if( v == this.value ) return;
				this.value = v;
				Invalidate();
			}
		}

		public bool IsDragging { get { return bDragging; } }

		private void SetFromMouse( int x )
		{
			int pad = Height / 2;
			Value = (float)( x - pad ) / Math.Max( 1, Width - pad * 2 );
			if( UserChanged != null ) UserChanged( this, EventArgs.Empty );
		}

		protected override void OnMouseDown( MouseEventArgs e )
		{
			if( e.Button == MouseButtons.Left )
			{
				bDragging = true;
				SetFromMouse( e.X );
			}
			base.OnMouseDown( e );
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			if( bDragging ) SetFromMouse( e.X );
			base.OnMouseMove( e );
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			bDragging = false;
			base.OnMouseUp( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			e.Graphics.Clear( BackColor );
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			int pad = Height / 2;
			int trackWidth = Math.Max( 1, Width - pad * 2 );
			int y = Height / 2;
			int fillX = pad + (int)( value * trackWidth );

			using( Pen track = new Pen( TrackColor, 4 ) ) e.Graphics.DrawLine( track, pad, y, pad + trackWidth, y );
			using( Pen fill = new Pen( FillColor, 4 ) ) e.Graphics.DrawLine( fill, pad, y, fillX, y );
			using( Brush thumb = new SolidBrush( FillColor ) ) e.Graphics.FillEllipse( thumb, fillX - 6, y - 6, 12, 12 );
		}
	}
}
