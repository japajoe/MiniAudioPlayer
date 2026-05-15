using MiniAudioPlayer.Core;

namespace MiniAudioPlayer
{
    class Program
    {
        static void Main(string[] args)
        {
            App application = new App(800, 600, "MiniAudioPlayer", WindowFlags.VSync);
            application.Run();
        }
    }
}