using System;
using System.Linq;
using Android.Content;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Giskard.Services
{
    public sealed class SharedPreferencesStorage : IKeyValueStorage
    {
        private readonly ISharedPreferences sharedPreferences;

        public SharedPreferencesStorage(ISharedPreferences sharedPreferences)
        {
            this.sharedPreferences = sharedPreferences;
        }

        public bool GetBool(string key)
            => sharedPreferences.GetBoolean(key, false);

        public string GetString(string key)
            => sharedPreferences.GetString(key, null);

        public void SetBool(string key, bool value)
        {
            var editor = sharedPreferences.Edit();
            editor.PutBoolean(key, value);
            editor.Commit();
        }

        public void SetString(string key, string value)
        {
            var editor = sharedPreferences.Edit();
            editor.PutString(key, value);
            editor.Commit();
        }

        public void SetInt(string key, int value)
        {
            var editor = sharedPreferences.Edit();
            editor.PutInt(key, value);
            editor.Commit();
        }

        public int GetInt(string key, int defaultValue)
        => sharedPreferences.GetInt(key, defaultValue);

        public void Remove(string key)
        {
            var editor = sharedPreferences.Edit();
            editor.Remove(key);
            editor.Commit();
        }

        public void RemoveAllWithPrefix(string prefix)
        {
            var keys = sharedPreferences.All.Keys
                .Where(key => key.StartsWith(prefix, StringComparison.Ordinal));

            var editor = sharedPreferences.Edit();

            foreach (var key in keys)
            {
                editor.Remove(key);
            }

            editor.Commit();
        }
    }
}
