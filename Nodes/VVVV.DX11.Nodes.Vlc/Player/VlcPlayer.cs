using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibVlcWrapper;
using System.Runtime.InteropServices;
using System.IO;

namespace VVVV.DX11.Vlc.Player
{
    public unsafe class VlcPlayer
    {
        private static IntPtr _libvlc = IntPtr.Zero;

        private static IntPtr LibVLC
        {
            get
            {
                if (_libvlc == IntPtr.Zero)
                {
                    string[] argv = {
					"--no-video-title",
					"--no-one-instance",
					"--directx-audio-speaker=5.1"
				    };
                    _libvlc = LibVlcMethods.libvlc_new(argv.GetLength(0), argv);
                }
                return _libvlc;
            }

        }

        private IntPtr opaqueForCallbacks;
        private VlcVideoLockHandlerDelegate vlcVideoLockHandlerDelegate;
        private VlcVideoUnlockHandlerDelegate vlcVideoUnlockHandlerDelegate;
        private VlcVideoDisplayHandlerDelegate vlcVideoDisplayHandlerDelegate;

        private IntPtr media;
        private IntPtr mediaPlayer;

        private int w;
        private int h;
        private float fps;
        private int currentFrame = 0;
        private bool valid = false;
        private bool loop = false;
        private bool isplaying = false;
        private float duration;

        private IntPtr buffer0 = IntPtr.Zero;
        private IntPtr buffer1 = IntPtr.Zero;

        public bool SizeChanged { get; protected set; }


        public int Width { get { return w; } }
        public int Height { get { return h; } }
        public float Fps { get { return this.fps; } }
        public float Position { get; protected set; }
        public int CurrentFrame { get { return this.currentFrame; } }
        public float Duration { get { return this.duration; } }

        public string FileName { get; protected set; }
        public bool IsValid { get { return this.valid; } }
        public int Pitch { get; private set; }

        private bool hasreset = true;

        public bool HasReset { get { return this.hasreset; } }

        public void Unset() { this.hasreset = false; }

        public IntPtr frontBuffer { get { return this.buffer1; } }

        public event EventHandler FrameReady;

        public void Stop()
        {
            LibVlcMethods.libvlc_media_player_stop(mediaPlayer);
        }

        public bool Play
        {
            get { return this.isplaying; }
            set
            {
                if (this.IsValid)
                {
                    if (value)
                    {
                        if (!isplaying) { LibVlcMethods.libvlc_media_player_play(mediaPlayer); this.currentFrame = 0; }
                    }
                    else
                    {
                        if (isplaying) { LibVlcMethods.libvlc_media_player_set_pause(mediaPlayer,1); }
                    }
                }
            }
        }

        public bool Loop
        {
            set
            {
                this.loop = value;
            }
        }

        public void SetPosition(float position)
        {
            libvlc_state_t mpState = LibVlcMethods.libvlc_media_player_get_state(mediaPlayer);

            if (mpState == libvlc_state_t.libvlc_Ended)
            {
                LibVlcMethods.libvlc_media_player_stop(mediaPlayer);
                LibVlcMethods.libvlc_media_player_play(mediaPlayer);
                LibVlcMethods.libvlc_media_player_set_time(mediaPlayer, (long)(position * 1000));
            }
            else
            {
                LibVlcMethods.libvlc_media_player_set_time(mediaPlayer, (long)(position * 1000));
            }
        }

        public void SetSpeed(float speed)
        {
            LibVlcMethods.libvlc_media_player_set_rate(mediaPlayer,speed);
        }

        public void SetVolume(float volume)
        {
            LibVlcMethods.libvlc_audio_set_volume(mediaPlayer, Convert.ToInt32(volume * 100.0f));
        }


        public VlcPlayer()
        {
            this.Initialize();
        }

        public void ResetSizeChanged()
        {
            this.SizeChanged = false;
        }

        private void Initialize()
        {
            opaqueForCallbacks = Marshal.AllocHGlobal(4);
            vlcVideoLockHandlerDelegate = VlcVideoLockCallBack;
            vlcVideoUnlockHandlerDelegate = VlcVideoUnlockCallBack;
            vlcVideoDisplayHandlerDelegate = VlcVideoDisplayCallBack;
            media = new IntPtr();


            mediaPlayer = LibVlcMethods.libvlc_media_player_new(LibVLC);
            LibVlcMethods.libvlc_media_player_retain(mediaPlayer);

            //Handle some VLC events!
            VlcEventHandlerDelegate h = VlcEventHandler;
            IntPtr ptr = Marshal.GetFunctionPointerForDelegate(h);
            IntPtr eventManager = LibVlcMethods.libvlc_media_player_event_manager(mediaPlayer);
        }

        public void SetFileName(string file)
        {
            this.valid = false;
            this.media = this.ParseFilename(file);
            this.FileName = file;
            LibVlcMethods.libvlc_media_parse(media);

            IntPtr trackInfoArray;
            int nrOfStreams;

            nrOfStreams = LibVlcMethods.libvlc_media_get_tracks_info(media, out trackInfoArray);

            for (int i = 0; i < nrOfStreams; i++)
            {
                LibVlcWrapper.libvlc_media_track_info_t trackInfo = ((LibVlcWrapper.libvlc_media_track_info_t*)trackInfoArray)[i];


                if (trackInfo.i_type == LibVlcWrapper.libvlc_track_type_t.libvlc_track_audio)
                {
                    /*br = trackInfo.audio.i_rate;
                    ch = trackInfo.audio.i_channels;
                    hasAudio = true;*/
                }
                else if (trackInfo.i_type == LibVlcWrapper.libvlc_track_type_t.libvlc_track_video)
                {
                    //setting w+h is important !!!
                    int neww = trackInfo.video.i_width;
                    int newh = trackInfo.video.i_height;

                    this.SizeChanged = this.Realloc(neww, newh);
                    this.valid = neww > 0 && newh > 0;

                    this.w = neww;
                    this.h = newh;
                }
            }

            if (nrOfStreams > 0)
            {
                Marshal.DestroyStructure(trackInfoArray, typeof(LibVlcWrapper.libvlc_media_track_info_t*));
            }

            

            LibVlcMethods.libvlc_media_player_stop(mediaPlayer);

            if (this.valid)
            {
                LibVlcMethods.libvlc_media_player_set_media(mediaPlayer, media);
                LibVlcMethods.libvlc_video_set_callbacks(mediaPlayer, Marshal.GetFunctionPointerForDelegate(vlcVideoLockHandlerDelegate), Marshal.GetFunctionPointerForDelegate(vlcVideoUnlockHandlerDelegate), Marshal.GetFunctionPointerForDelegate(vlcVideoDisplayHandlerDelegate), opaqueForCallbacks);


                int pitch = this.w * 4;
                LibVlcMethods.libvlc_video_set_format(mediaPlayer, Encoding.UTF8.GetBytes("RV32"), this.w, this.h, pitch);

                LibVlcMethods.libvlc_media_player_play(mediaPlayer);


                
                this.fps = LibVlcMethods.libvlc_media_player_get_fps(mediaPlayer);
            }
        }

        public void GetStatus()
        {
            this.fps = LibVlcMethods.libvlc_media_player_get_fps(mediaPlayer);
            libvlc_state_t mpState = LibVlcMethods.libvlc_media_player_get_state(mediaPlayer);

            if ((mpState == libvlc_state_t.libvlc_Ended) && this.loop)
            {
                LibVlcMethods.libvlc_media_player_stop(mediaPlayer);
                currentFrame = 0;
                LibVlcMethods.libvlc_media_player_play(mediaPlayer);
            }

            mpState = LibVlcMethods.libvlc_media_player_get_state(mediaPlayer);

            this.isplaying = mpState == libvlc_state_t.libvlc_Playing || mpState == libvlc_state_t.libvlc_Buffering;

            //this.Position = Convert.ToSingle(LibVlcMethods.libvlc_media_player_get_position(mediaPlayer));

           // this.Position = (float)currentFrame / this.fps;  
            this.duration = Convert.ToSingle(LibVlcMethods.libvlc_media_player_get_length(mediaPlayer)) / 1000.0f;

            if (mpState == libvlc_state_t.libvlc_Ended)
            {
                this.Position = this.duration;
            }
            else
            {
                this.Position = Convert.ToSingle(LibVlcMethods.libvlc_media_player_get_position(mediaPlayer)) * this.duration;
            }

            
        }

        public IntPtr VlcVideoLockCallBack(ref IntPtr data, ref IntPtr pixelPlane)
        {
            currentFrame++;

            pixelPlane = this.buffer0;

            return this.buffer0;
        }


        public void VlcAudioPlayCallBack(ref IntPtr data, IntPtr samples, UInt32 count, Int64 pts)
        {
        }

        public void VlcVideoDisplayCallBack(ref IntPtr data, ref IntPtr id)
        {
            IntPtr temp = this.buffer0;
            this.buffer0 = this.buffer1;
            this.buffer1 = temp;
            if (this.FrameReady != null) { this.FrameReady(this, new EventArgs()); }
            
        }

        public void VlcVideoUnlockCallBack(ref IntPtr data, ref IntPtr id, ref IntPtr pixelPlane)
        {
            
        }

        private void VlcEventHandler(ref libvlc_event_t libvlc_event, IntPtr userData)
        {
        }

        private IntPtr ParseFilename(string fileName)
        {
            if (fileName.Length == 0)
            {
                return IntPtr.Zero;
            }
            string[] mediaOptions = fileName.Split("|".ToCharArray());
            if (mediaOptions[0].TrimEnd().Length == 0 || (!mediaOptions[0].Contains("://") && !File.Exists(mediaOptions[0].TrimEnd())))
            {
                return IntPtr.Zero;
            }

            IntPtr retVal = new IntPtr();
            try
            {
                if (!mediaOptions[0].Contains("://"))
                {
                    retVal = LibVlcMethods.libvlc_media_new_path(LibVLC, Encoding.UTF8.GetBytes(mediaOptions[0].TrimEnd()));
                }
                else
                {
                    retVal = LibVlcMethods.libvlc_media_new_location(LibVLC, Encoding.UTF8.GetBytes(mediaOptions[0].TrimEnd()));
                }
                for (int moIndex = 1; moIndex < mediaOptions.Length; moIndex++)
                {
                    LibVlcMethods.libvlc_media_add_option(retVal, Encoding.UTF8.GetBytes(mediaOptions[moIndex].Trim()));
                }

            }
            catch
            {
                retVal = IntPtr.Zero;
            }

            return retVal;
        }

        private bool Realloc(int w, int h)
        {
            if (this.w != w || this.h != h)
            {
                if (this.buffer0 != IntPtr.Zero) { Marshal.FreeHGlobal(this.buffer0); this.buffer0 = IntPtr.Zero; }
                if (this.buffer1 != IntPtr.Zero) { Marshal.FreeHGlobal(this.buffer1); this.buffer1 = IntPtr.Zero; }
                this.buffer0 = Marshal.AllocHGlobal(w * h * 4 + 32);
                this.buffer1 = Marshal.AllocHGlobal(w * h * 4 + 32);

                return true;
            }

            return false;
        }

        public void Dispose()
        {
            LibVlcMethods.libvlc_media_player_stop(mediaPlayer);
            if (this.buffer0 != IntPtr.Zero) { Marshal.FreeHGlobal(this.buffer0); this.buffer0 = IntPtr.Zero; }
            if (this.buffer1 != IntPtr.Zero) { Marshal.FreeHGlobal(this.buffer1); this.buffer1 = IntPtr.Zero; }
            this.valid = false;

            try
            {
                LibVlcMethods.libvlc_media_player_release(mediaPlayer);
            }
            catch
            {
            }


            try
            {
                Marshal.FreeHGlobal(opaqueForCallbacks);
            }
            catch { }
        }
    }
}
