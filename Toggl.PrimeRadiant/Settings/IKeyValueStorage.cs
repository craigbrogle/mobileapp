namespace Toggl.PrimeRadiant.Settings
{
    public interface IKeyValueStorage
    {
        bool GetBool(string key);

        string GetString(string key);

        void SetBool(string key, bool value);

        void SetString(string key, string value);

        void SetInt(string key, int value);

        int GetInt(string key, int defaultValue);

        void Remove(string key);

        void RemoveAllWithPrefix(string prefix);
    }
}
