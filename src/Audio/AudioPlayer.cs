
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using MiniAudioEx.Native;
using MiniAudioPlayer.Utilities;
using static MiniAudioEx.Native.MiniAudioNative;

namespace MiniAudioPlayer
{
    public class TrackInfo
    {
        public string filePath;
        public string fileName;
        public float durationSeconds;

        public TrackInfo(string filePath)
        {
            this.filePath = filePath;
            fileName = Path.GetFileName(filePath);
            durationSeconds = 0;
        }
    }

    public delegate void TrackEndedEventHandler(int trackIndex);

    public unsafe sealed class AudioPlayer : IDisposable
    {
        public event TrackEndedEventHandler TrackEnded;

        private const UInt32 periodSize = 2048;
        private List<TrackInfo> tracks;
        private RingBuffer ringBuffer;
        private ma_engine_ptr pEngine;
        private ma_context_ptr pContext;
        private ma_device_ptr pDevice;
        private ma_resource_manager_ptr pResourceManager;
        private ma_sound_ptr pSound;
        private ma_effect_node_ptr pEffectNode;
        private ma_device_data_proc deviceDataProc;
        private ma_effect_node_process_proc effectProcessProc;
        private ma_sound_end_proc soundEndProc;
        private UInt32 sampleRate;
        private UInt32 channels;
        private bool soundIsPaused;
        private int currentTrackIndex;
        private float currentLengthSeconds;
        private UInt64 currentLengthInPCMFrames;

        public Int32 PeriodSize => (Int32)periodSize;
        public Int32 TrackCount => tracks.Count;
        public UInt32 SampleRate => sampleRate;
        public UInt32 Channels => channels;
        public RingBuffer OutputBuffer => ringBuffer;
        public bool IsPlaying
        {
            get
            {
                if(pSound.Get()->pDataSource.pointer == IntPtr.Zero)
                    return false;
                return ma_sound_is_playing(pSound) > 0;
            }
        }

        public AudioPlayer()
        {
            tracks = new List<TrackInfo>();
            ringBuffer = new RingBuffer(16384);
            currentTrackIndex = 0;
            pEngine = new ma_engine_ptr(true);
            pContext = new ma_context_ptr(true);
            pDevice = new ma_device_ptr(true);
            pResourceManager = new ma_resource_manager_ptr(true);
            pSound = new ma_sound_ptr(true);
            pEffectNode = new ma_effect_node_ptr(true);
            deviceDataProc = OnDeviceData;
            effectProcessProc = OnEffectProcess;
            soundEndProc = OnSoundEnd;
            soundIsPaused = false;

            if (ma_context_init(null, pContext) != ma_result.success)
            {
                Console.WriteLine("Failed to create context");
                Dispose();
                return;
            }

            ma_device_config deviceConfig = ma_device_config_init(ma_device_type.playback);
            deviceConfig.playback.format = ma_format.f32;
            deviceConfig.playback.channels = 2;
            deviceConfig.sampleRate = 44100;
            deviceConfig.periodSizeInFrames = periodSize;
            deviceConfig.SetDataCallback(deviceDataProc);

            if (ma_context_get_devices(pContext, out ma_device_info[] ppPlaybackDeviceInfos, out ma_device_info[] ppCaptureDeviceInfos) != ma_result.success)
            {
                Console.WriteLine("Failed to get devices");
                Dispose();
                return;
            }

            if (ppPlaybackDeviceInfos?.Length > 0)
            {
                for (int i = 0; i < ppPlaybackDeviceInfos.Length; i++)
                {
                    if (ppPlaybackDeviceInfos[i].isDefault > 0)
                    {
                        deviceConfig.playback.pDeviceID = new ma_device_id_ptr(true);
                        unsafe
                        {
                            *deviceConfig.playback.pDeviceID.Get() = ppPlaybackDeviceInfos[i].id;
                        }
                        Console.WriteLine("Selected default device: " + ppPlaybackDeviceInfos[i].GetName());
                        break;
                    }
                }
            }

            if (ma_device_init(pContext, ref deviceConfig, pDevice) != ma_result.success)
            {
                if(deviceConfig.playback.pDeviceID.pointer != IntPtr.Zero)
                    deviceConfig.playback.pDeviceID.Free();
                Console.WriteLine("Failed to initialize device");
                Dispose();
                return;
            }

            if(deviceConfig.playback.pDeviceID.pointer != IntPtr.Zero)
                deviceConfig.playback.pDeviceID.Free();

            ma_decoding_backend_vtable_ptr[] vtables = {
                ma_libvorbis_get_decoding_backend_ptr()
            };

            ma_resource_manager_config resourceManagerConfig = ma_resource_manager_config_init();
            resourceManagerConfig.SetCustomDecodingBackendVTables(vtables);

            if (ma_resource_manager_init(ref resourceManagerConfig, pResourceManager) != ma_result.success)
            {
                resourceManagerConfig.FreeCustomDecodingBackendVTables();
                Console.WriteLine("Failed to initialize ma_resource_manager");
                Dispose();
                return;
            }

            resourceManagerConfig.FreeCustomDecodingBackendVTables();

            ma_engine_config engineConfig = ma_engine_config_init();
            engineConfig.listenerCount = MA_ENGINE_MAX_LISTENERS;
            engineConfig.pDevice = pDevice;
            engineConfig.pResourceManager = pResourceManager;

            if (ma_engine_init(ref engineConfig, pEngine) != ma_result.success)
            {
                Console.WriteLine("Failed to initialize ma_engine");
                Dispose();
                return;
            }

            ma_device* device = (ma_device*)pDevice.pointer;
            device->pUserData = pEngine.pointer;

            if (ma_device_start(pDevice) != ma_result.success)
            {
                Console.WriteLine("Failed to start ma_device");
                Dispose();
                return;
            }

            sampleRate = device->sampleRate;
            ma_device_playback_ptr playback = ma_device_get_playback(pDevice);
            channels = playback.Get()->channels;
        }

        public void Dispose()
        {
			ma_sound_uninit(pSound);
            ma_effect_node_uninit(pEffectNode);
			ma_engine_uninit(pEngine);
			ma_device_uninit(pDevice);
			ma_context_uninit(pContext);
			ma_resource_manager_uninit(pResourceManager);

            pEngine.Free();
            pContext.Free();
            pDevice.Free();
            pResourceManager.Free();
            pSound.Free();
            pEffectNode.Free();
        }

        public void AddTrack(string filePath)
        {
            if(string.IsNullOrEmpty(filePath))
                return;
            if(!IsAudioExtension(filePath))
                return;
            tracks.Add(new TrackInfo(filePath));
        }

        public void AddTracks(string directoryPath)
        {
            if(string.IsNullOrEmpty(directoryPath))
                return;
            var files = Directory.GetFiles(directoryPath);

            if(files?.Length == 0)
                return;

            files.Sort(new NaturalFileInfoComparer());

            for(int i = 0; i < files.Length; i++)
            {
                if(!IsAudioExtension(files[i]))
                    continue;
                tracks.Add(new TrackInfo(files[i]));
            }
        }

        public void Clear()
        {
            tracks.Clear();
            currentTrackIndex = 0;
        }

        public void Shuffle()
        {
            if(tracks.Count == 0)
                return;
            
            Shuffle(tracks);
        }

        public void PlayNext()
        {
            currentTrackIndex++;
            if(currentTrackIndex >= tracks.Count)
                currentTrackIndex = 0;
            Play(currentTrackIndex);
        }

        public void Play(int index)
        {
            if(index < 0 || index >= tracks.Count)
                return;

            if(soundIsPaused && currentTrackIndex == index)
            {
                ma_sound_start(pSound);
                return;
            }

            currentTrackIndex = index;
            
            ma_sound_uninit(pSound);
            ma_effect_node_uninit(pEffectNode);

            if(ma_sound_init_from_file(pEngine, tracks[currentTrackIndex].filePath, 0, default, default, pSound) == ma_result.success)
            {
                ma_sound_get_length_in_seconds(pSound, out currentLengthSeconds);
                ma_sound_get_length_in_pcm_frames(pSound, out currentLengthInPCMFrames);

                ma_device_playback_ptr playback = ma_device_get_playback(pDevice);

                ma_effect_node_config effectNodeConfig = ma_effect_node_config_init(playback.Get()->channels, pDevice.Get()->sampleRate, effectProcessProc, IntPtr.Zero);

                if (ma_effect_node_init(ma_engine_get_node_graph(pEngine), ref effectNodeConfig, pEffectNode) == ma_result.success)
                {
                    ma_node_attach_output_bus(new ma_node_ptr(pEffectNode.pointer), 0, ma_engine_get_endpoint(pEngine), 0);
                    ma_node_attach_output_bus(new ma_node_ptr(pSound.pointer), 0, new ma_node_ptr(pEffectNode.pointer), 0);
                }

                ma_sound_set_end_callback(pSound, soundEndProc, IntPtr.Zero);

                if(ma_sound_start(pSound) == ma_result.success)
                {
                    currentTrackIndex = index;
                }
            }
            
            soundIsPaused = false;
        }

        public void Pause()
        {
            if(ma_sound_is_playing(pSound) > 0)
            {
                ma_sound_stop(pSound);
                soundIsPaused = true;
            }
        }

        public void Stop()
        {
            if(ma_sound_is_playing(pSound) > 0)
            {
                ma_sound_stop(pSound);
                ma_sound_seek_to_pcm_frame(pSound, 0);
            }
            soundIsPaused = false;
        }

        public bool Seek(float percentage)
        {
            float time = percentage * currentLengthSeconds;
            return ma_sound_seek_to_second(pSound, time) == ma_result.success;
        }

        public ReadOnlySpan<char> GetTrackName(int index)
        {
            if(index < 0 || index >= tracks.Count)
                return null;
            
            return tracks[index].fileName;
        }

        public bool GetTrackInfo(out float lengthInSeconds, out UInt64 lengthInPCMFrames, out UInt64 cursorInPCMFrames)
        {
            lengthInSeconds = currentLengthSeconds;
            lengthInPCMFrames = currentLengthInPCMFrames;
            
            if(ma_sound_get_cursor_in_pcm_frames(pSound, out cursorInPCMFrames) != ma_result.success)
                return false;
            return true;
        }

        private bool IsAudioExtension(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();

            if (ext == ".mp3" || ext == ".wav" || ext == ".flac" || ext == ".ogg")
                return true;
            return false;
        }

        private void OnDeviceData(ma_device_ptr pDevice, IntPtr pOutput, IntPtr pInput, UInt32 frameCount)
        {
            ma_device* device = pDevice.Get();

            if (device == null)
                return;

            ma_engine_ptr pEngine = new ma_engine_ptr(device->pUserData);
            ma_engine_read_pcm_frames(pEngine, pOutput, frameCount);
        }

        private void OnEffectProcess(ma_node_ptr pNode, IntPtr ppFramesIn, IntPtr pFrameCountIn, IntPtr ppFramesOut, IntPtr pFrameCountOut)
        {
            if (pNode.pointer == IntPtr.Zero)
                return;

            ma_effect_node_ptr pEffectNode = new ma_effect_node_ptr(pNode.pointer);

            UInt32* frameCountIn = (UInt32*)pFrameCountIn;
            UInt32* frameCountOut = (UInt32*)pFrameCountOut;
            UInt32 channels = pEffectNode.Get()->config.channels;

            float** framesIn = (float**)ppFramesIn;
            float** framesOut = (float**)ppFramesOut;

            NativeArray<float> bufferIn = new NativeArray<float>(framesIn[0], (int)(*frameCountIn * channels));
            NativeArray<float> bufferOut = new NativeArray<float>(framesOut[0], (int)(*frameCountOut * channels));

            bufferIn.CopyTo(bufferOut);

            *frameCountOut = *frameCountIn;

            ringBuffer.Write(new ReadOnlySpan<float>(framesOut[0], (int)*frameCountOut));
        }

        private void OnSoundEnd(IntPtr pUserData, ma_sound_ptr pSound)
        {
            TrackEnded?.Invoke(currentTrackIndex);
        }

        private static void Shuffle<T>(List<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}