namespace MiniAudioPlayer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using(var application = new Application(800, 600, 3, 3, true, "MiniAudioPlayer"))
            {
                application.Run();
            }
        }
    }
}