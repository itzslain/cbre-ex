namespace CBRE.Settings
{
    public class General
    {
        public static bool CheckUpdatesOnStartup { get; set; }

        static General()
        {
            CheckUpdatesOnStartup = true;
        }
    }
}
