using System;
using System.Collections.Concurrent;
using System.Threading;
using MiniAudioPlayer.Core;
using MiniAudioPlayer.Utilities;
using ImGuiNET;
using TinyFileDialogs;
using MiniAudioPlayer.Graphics;
using OpenTK.Graphics.OpenGL;
using MiniAudioPlayer.Embedded;
using System.IO;

namespace MiniAudioPlayer
{
    public class App : Application
    {
        private struct ConcurrentEvent
        {
            public Type type;
            public IntPtr data;

            public enum Type
            {
                SelectFolder,
                SelectFiles,
                TrackEnded,
                LoadShader
            }

            public ConcurrentEvent(Type type, IntPtr data)
            {
                this.type = type;
                this.data = data;
            }
        }

        private AudioPlayer audioPlayer;
        private Visualizer visualizer;
        private Texture2D audioTexture;
        private ConcurrentQueue<ConcurrentEvent> eventQueue;
        private int selectedTrackIndex;
        private float[] waveformData;
        private Complex[] fftData;
        private float[] textureData;
        private float seekPreviewValue;
        private bool isDraggingSlider;
        private string shaderError = string.Empty;
        private string currentShaderPath;
        private readonly char[] currentTimeBuffer = new char[12];
        private readonly char[] totalTimeBuffer = new char[12];

        public App(int width, int height, string title, WindowFlags flags = WindowFlags.VSync)
            : base(width, height, title, flags)
        {

        }

        protected override void OnLoad()
        {
            ImGuiStyle.UseCatppuccinMochaStyle();

            audioPlayer = new AudioPlayer();
            visualizer = new Visualizer();
            eventQueue = new ConcurrentQueue<ConcurrentEvent>();
            selectedTrackIndex = -1;

            audioPlayer.TrackEnded += OnTrackEnded;

            TextureSettings textureSettings = new TextureSettings()
            {
                wrapS = TextureWrapMode.ClampToEdge,
                wrapT = TextureWrapMode.ClampToEdge,
                minFilter = TextureFilterMode.Linear,
                magFilter = TextureFilterMode.Linear
            };

            waveformData = new float[audioPlayer.PeriodSize];
            fftData = new Complex[audioPlayer.PeriodSize / 2];
            textureData = new float[audioPlayer.PeriodSize / 2 * 3];

            audioTexture = new Texture2D(audioPlayer.PeriodSize / 2, 1, InternalFormat.Rgb32f, PixelFormat.Rgb, PixelType.Float, textureSettings, false);
            visualizer.AddTexture(audioTexture, "uAudio");

            SetLayout();
        }

        protected override void OnClose()
        {
            audioPlayer.Dispose();
        }

        protected override void OnUpdate()
        {
            if (eventQueue.Count > 0)
            {
                while (eventQueue.TryDequeue(out ConcurrentEvent e))
                {
                    switch (e.type)
                    {
                        case ConcurrentEvent.Type.SelectFiles:
                            SetInputEnabled(true);
                            string files = NativeString.Get(e.data);
                            NativeString.Free(e.data);

                            if (!string.IsNullOrEmpty(files))
                            {
                                if (files.Contains("|"))
                                {
                                    string[] fileList = files.Split('|');
                                    for (int i = 0; i < fileList.Length; i++)
                                        audioPlayer.AddTrack(fileList[i]);
                                }
                                else
                                {
                                    audioPlayer.AddTrack(files);
                                }
                            }

                            SetInputEnabled(true);
                            break;
                        case ConcurrentEvent.Type.SelectFolder:
                            string directory = NativeString.Get(e.data);
                            NativeString.Free(e.data);
                            audioPlayer.AddTracks(directory);
                            SetInputEnabled(true);
                            break;
                        case ConcurrentEvent.Type.LoadShader:
                            string shaderPath = NativeString.Get(e.data);
                            NativeString.Free(e.data);
                            if(!string.IsNullOrEmpty(shaderPath))
                            {
                                if(File.Exists(shaderPath))
                                {
                                    SetShader(shaderPath);
                                }
                            }
                            
                            SetInputEnabled(true);
                            break;
                        case ConcurrentEvent.Type.TrackEnded:
                            int trackIndex = (int)e.data + 1;
                            if (trackIndex >= audioPlayer.TrackCount)
                                trackIndex = 0;
                            if (trackIndex >= 0 && trackIndex < audioPlayer.TrackCount)
                            {
                                selectedTrackIndex = trackIndex;
                                audioPlayer.PlayNext();
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            var outputBuffer = audioPlayer.OutputBuffer;
            int available = outputBuffer.GetAvailableCount();

            // If the audio thread has pushed multiple periods, 
            // we keep reading until we have the latest one.
            if (available >= waveformData.Length)
            {
                while (available >= waveformData.Length)
                {
                    outputBuffer.Read(waveformData);
                    available = outputBuffer.GetAvailableCount();
                }

                int frameCount = waveformData.Length / 2;
                GetWaveformData(frameCount);
                GetFrequencyData(frameCount);

                var span = new ReadOnlySpan<float>(textureData);
                audioTexture.Bind();
                audioTexture.SubImage2D(0, 0, frameCount, 1, PixelFormat.Rgb, PixelType.Float, span);
                audioTexture.Unbind();
            }
        }

        protected override void OnGUI()
        {
            visualizer.Render();
            ImGui.DockSpaceOverViewport();
            ShowMenu();
            ShowVisualizer();
            ShowPlaylist();
            ShowLog();
            ShowPlayBar();
        }

        private void ShowMenu()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open File"))
                    {
                        SetInputEnabled(false);
                        Thread worker = new Thread(ShowFileChooserDialog);
                        worker.IsBackground = true;
                        worker.Start();
                    }
                    if (ImGui.MenuItem("Open Folder"))
                    {
                        SetInputEnabled(false);
                        Thread worker = new Thread(ShowFolderChooserDialog);
                        worker.IsBackground = true;
                        worker.Start();
                    }
                    if (ImGui.MenuItem("Load Shader"))
                    {
                        SetInputEnabled(false);
                        Thread worker = new Thread(ShowShaderFileChooserDialog);
                        worker.IsBackground = true;
                        worker.Start();
                    }
                    if (ImGui.MenuItem("Exit"))
                    {
                        Application.Quit();
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Playlist"))
                {
                    if (ImGui.MenuItem("Clear"))
                    {
                        audioPlayer.Clear();
                    }
                    if (ImGui.MenuItem("Shuffle"))
                    {
                        audioPlayer.Shuffle();
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }
        }

        private void ShowVisualizer()
        {
            IntPtr texId = new IntPtr(visualizer.Texture);
            ImGui.Begin("Visualizer");
            var size = ImGui.GetContentRegionAvail();
            visualizer.SetSize((int)size.X, (int)size.Y);
            ImGui.Image(texId, size, new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
            ImGui.End();
        }

        private void ShowPlaylist()
        {
            ImGui.Begin("Playlist");

            if (ImGui.BeginListBox("##Tracks", new System.Numerics.Vector2(-1, -1)))
            {
                for (int i = 0; i < audioPlayer.TrackCount; i++)
                {
                    bool isSelected = (selectedTrackIndex == i);
                    if (ImGui.Selectable(audioPlayer.GetTrackName(i), isSelected))
                    {
                        selectedTrackIndex = i;
                    }

                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        audioPlayer.Play(i);
                    }
                }
                ImGui.EndListBox();
            }

            ImGui.End();
        }

        private void ShowPlayBar()
        {
            float progress = 0.0f;

            if (audioPlayer.GetTrackInfo(out float lengthSeconds, out UInt64 lengthInPCMFrames, out UInt64 cursorInPCMFrames))
            {
                float currentTime = (float)cursorInPCMFrames / audioPlayer.SampleRate;

                FormatTime(currentTimeBuffer, currentTime);
                FormatTime(totalTimeBuffer, lengthSeconds);

                if (lengthSeconds > 0.0f)
                {
                    progress = currentTime / lengthSeconds;
                }
            }
            else
            {
                FormatTime(currentTimeBuffer, 0.0f);
                FormatTime(totalTimeBuffer, 0.0f);
            }

            var totalTimeText = new ReadOnlySpan<char>(totalTimeBuffer);
            var currentTimeText = new ReadOnlySpan<char>(currentTimeBuffer);

            ImGui.Begin("Play");

            ImGui.AlignTextToFramePadding();
            ImGui.Text(currentTimeText);
            ImGui.SameLine();

            float spacing = ImGui.GetStyle().ItemSpacing.X;
            float rightLabelWidth = ImGui.CalcTextSize(totalTimeText).X;
            float availableWidth = ImGui.GetContentRegionAvail().X;

            ImGui.PushItemWidth(availableWidth - rightLabelWidth - spacing);

            float sliderValue = isDraggingSlider ? seekPreviewValue : progress;

            if (ImGui.SliderFloat("##AudioProgress", ref sliderValue, 0.0f, 1.0f, ""))
            {
                isDraggingSlider = true;
                seekPreviewValue = sliderValue;
            }

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                audioPlayer.Seek(seekPreviewValue);
                isDraggingSlider = false;
            }

            ImGui.PopItemWidth();
            ImGui.SameLine();

            ImGui.Text(totalTimeText);


            // 1. Define padding and calculate button widths based on font style
            float styleItemSpacingX = ImGui.GetStyle().ItemSpacing.X;
            float buttonPlayPauseWidth = ImGui.CalcTextSize(audioPlayer.IsPlaying ? FontAwesome.ICON_FA_PAUSE : FontAwesome.ICON_FA_PLAY).X + (ImGui.GetStyle().FramePadding.X * 2.0f);
            float buttonStopWidth = ImGui.CalcTextSize(FontAwesome.ICON_FA_STOP).X + (ImGui.GetStyle().FramePadding.X * 2.0f);

            // 2. Sum up total width of the control group
            float totalGroupWidth = buttonPlayPauseWidth + styleItemSpacingX + buttonStopWidth;

            // 3. Calculate center starting position relative to the window content region
            float startPosX = (ImGui.GetContentRegionAvail().X - totalGroupWidth) * 0.5f;

            if (startPosX > 0.0f)
            {
                ImGui.SetCursorPosX(startPosX);
            }

            if(audioPlayer.IsPlaying)
            {
                if(ImGui.Button(FontAwesome.ICON_FA_PAUSE))
                {
                    audioPlayer.Pause();
                }
            }
            else
            {
                if(ImGui.Button(FontAwesome.ICON_FA_PLAY))
                {
                    audioPlayer.Play(selectedTrackIndex);
                }
            }

            ImGui.SameLine();

            if(ImGui.Button(FontAwesome.ICON_FA_STOP))
            {
                audioPlayer.Stop();
            }

            ImGui.End();
        }

        private void ShowLog()
        {
            ImGui.Begin("Log");

            if(ImGui.Button("Clear"))
            {
                shaderError = string.Empty;
            }

            ImGui.SameLine();
            if(ImGui.Button("Reload Shader"))
            {
                SetShader(currentShaderPath);
            }
            
            var size = ImGui.GetContentRegionAvail();
            size.X -= 5;
            size.Y -= 5;

            ImGui.InputTextMultiline("##Log", ref shaderError, 1024, size, ImGuiInputTextFlags.ReadOnly);
            ImGui.End();
        }

        private void SetInputEnabled(bool enabled)
        {
            var io = ImGui.GetIO();

            if (enabled)
            {
                io.ConfigFlags &= ~ImGuiConfigFlags.NoMouse;
                io.ConfigFlags &= ~ImGuiConfigFlags.NoKeyboard;
            }
            else
            {
                io.ConfigFlags |= ImGuiConfigFlags.NoMouse;
                io.ConfigFlags |= ImGuiConfigFlags.NoKeyboard;
            }
        }

        private void ShowFolderChooserDialog()
        {
            string directory = TinyFileDialog.SelectFolderDialog("Select Folder", "/home/wesley/Desktop/FLProjects/");
            ConcurrentEvent e = new ConcurrentEvent(ConcurrentEvent.Type.SelectFolder, NativeString.Allocate(directory));
            eventQueue.Enqueue(e);
        }

        private void ShowFileChooserDialog()
        {
            string[] filter = {
                "*.mp3",
                "*.ogg",
                "*.flac",
                "*.wav",
            };

            string files = TinyFileDialog.OpenFileDialog("Select Files", "/home/wesley/Desktop/FLProjects/", filter, "Audio Files", true);
            ConcurrentEvent e = new ConcurrentEvent(ConcurrentEvent.Type.SelectFiles, NativeString.Allocate(files));
            eventQueue.Enqueue(e);
        }

        private void ShowShaderFileChooserDialog()
        {
            string[] filter = {
                "*.txt",
                "*.glsl",
                "*.c"
            };

            string file = TinyFileDialog.OpenFileDialog("Select Shader", "/home/wesley/Documents/development/dotnet/audio/MiniAudioPlayer/shaders/", filter, "Shader Files", false);
            ConcurrentEvent e = new ConcurrentEvent(ConcurrentEvent.Type.LoadShader, NativeString.Allocate(file));
            eventQueue.Enqueue(e);
        }

        private void OnTrackEnded(int trackIndex)
        {
            ConcurrentEvent e = new ConcurrentEvent(ConcurrentEvent.Type.TrackEnded, new IntPtr(trackIndex));
            eventQueue.Enqueue(e);
        }

        private int FormatTime(char[] buffer, float totalSeconds)
        {
            int seconds = (int)Math.Round(totalSeconds);
            int h = seconds / 3600;
            int m = (seconds % 3600) / 60;
            int s = seconds % 60;

            buffer[0] = (char)('0' + (h / 10));
            buffer[1] = (char)('0' + (h % 10));
            buffer[2] = ':';
            buffer[3] = (char)('0' + (m / 10));
            buffer[4] = (char)('0' + (m % 10));
            buffer[5] = ':';
            buffer[6] = (char)('0' + (s / 10));
            buffer[7] = (char)('0' + (s % 10));

            return 8; // Length of "HH:mm:ss"
        }

        private void GetWaveformData(int frameCount)
        {
            for (int i = 0; i < frameCount; i++)
            {
                textureData[i * 3] = (waveformData[i * 2] + waveformData[i * 2 + 1]) * 0.5f;
            }
        }

        private void GetFrequencyData(int frameCount)
        {
            // Transfer from waveform (Red) to FFT input
            for (int i = 0; i < frameCount; i++)
            {
                fftData[i].Real = textureData[i * 3];
                fftData[i].Imag = 0.0f;
            }

            // Compute FFT
            FFT.Compute(fftData);

            // Populate Green channel
            int binCount = frameCount / 2;
            float scale = 2.0f / frameCount; // 2.0 compensates for Hann window energy loss
            float minDb = -60.0f;
            float attackSpeed = 0.8f;
            float falloffSpeed = 0.92f;

            for (int i = 0; i < binCount; i++)
            {
                float re = fftData[i].Real;
                float im = fftData[i].Imag;

                // Magnitude normalized to 0.0 - 1.0 range
                float mag = MathF.Sqrt(re * re + im * im) * scale;

                float db = 20.0f * MathF.Log10(mag + 1e-6f);

                float target = (db - minDb) / (-minDb);
                target = MathF.Max(0.0f, MathF.Min(target, 1.0f));

                float tilt = (float)i / (float)(i / 2);
                target *= (1.0f + tilt * 2.0f);

                float current = re;

                if (target > current)
                {
                    textureData[(i * 3) + 1] = current + (target - current) * attackSpeed;
                }
                else
                {
                    textureData[(i * 3) + 1] = current * falloffSpeed;
                }

                //textureData[(i * 3) + 1] = mag;
            }

            // Zero out the unused half of the Green channel
            for (int i = binCount; i < frameCount; i++)
            {
                textureData[(i * 3) + 1] = 0.0f;
            }
        }

        private void SetShader(string shaderPath)
        {
            if(string.IsNullOrEmpty(shaderPath))
                return;
            currentShaderPath = shaderPath;
            string fragmentSource = File.ReadAllText(shaderPath);
            fragmentSource = BasicShader.HeaderSource + "\n#line 1\n" + fragmentSource;
            Shader shader = new Shader();
            if(shader.Generate(BasicShader.VertexSource, fragmentSource, out shaderError))
            {
                visualizer.SetShader(shader);
            }
        }

        private void SetLayout()
        {
            if(File.Exists("imgui.ini"))
                return;

            string layout = @"[Window][WindowOverViewport_11111111]
Pos=0,21
Size=800,579
Collapsed=0

[Window][Debug##Default]
Pos=60,60
Size=400,400
Collapsed=0

[Window][Visualizer]
Pos=0,21
Size=560,477
Collapsed=0
DockId=0x00000003,0

[Window][Playlist]
Pos=562,21
Size=238,477
Collapsed=0
DockId=0x00000004,0

[Window][Play]
Pos=0,500
Size=800,100
Collapsed=0
DockId=0x00000002,0

[Window][Log]
Pos=0,500
Size=800,100
Collapsed=0
DockId=0x00000002,1

[Docking][Data]
DockSpace     ID=0x08BD597D Window=0x1BBC0F80 Pos=0,21 Size=800,579 Split=Y
  DockNode    ID=0x00000001 Parent=0x08BD597D SizeRef=800,484 Split=X
    DockNode  ID=0x00000003 Parent=0x00000001 SizeRef=560,484 CentralNode=1 Selected=0x5C1B5396
    DockNode  ID=0x00000004 Parent=0x00000001 SizeRef=238,484 Selected=0x77DC22F9
  DockNode    ID=0x00000002 Parent=0x08BD597D SizeRef=800,100 Selected=0x54F9C7E2";

            ImGui.LoadIniSettingsFromMemory(layout);
        }
    }
}