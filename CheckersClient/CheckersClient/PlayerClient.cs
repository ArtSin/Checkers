using CheckersLib;

namespace CheckersClient
{
    class PlayerClient : BaseClient
    {
        private static PlayerClient instance;

        public static PlayerClient GetInstance()
        {
            if (instance == null)
                instance = new PlayerClient();
            return instance;
        }
    }
}
