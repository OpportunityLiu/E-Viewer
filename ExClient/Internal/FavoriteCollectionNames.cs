namespace ExClient.Internal
{
    internal class FavoriteCollectionNames
        : ApplicationDataManager.Settings.ApplicationSettingCollection
    {
        public static FavoriteCollectionNames Current
        {
            get;
            private set;
        } = new FavoriteCollectionNames();

        private FavoriteCollectionNames()
            : base("ExClient.FavoriteCollectionNames")
        {
        }

        public string GetName(int index)
        {
            return GetLocal($"favorite {index}", $"Fav{index}");
        }

        public void SetName(int index, string name)
        {
            if(string.IsNullOrWhiteSpace(name))
                name = $"favorite {index}";
            SetLocal(name, $"Fav{index}");
        }
    }
}
