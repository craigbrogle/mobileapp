using System;
using System.Linq;
using Foundation;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Daneel.Services
{
    internal sealed class UserDefaultsStorage : IKeyValueStorage
    {
        public bool GetBool(string key)
            => NSUserDefaults.StandardUserDefaults.BoolForKey(key);

        public string GetString(string key)
            => NSUserDefaults.StandardUserDefaults.StringForKey(key);

        public void SetBool(string key, bool value)
        {
            NSUserDefaults.StandardUserDefaults.SetBool(value, key);
        }

        public void SetString(string key, string value)
        {
            NSUserDefaults.StandardUserDefaults.SetString(value, key);
        }

        public void SetInt(string key, int value)
            => NSUserDefaults.StandardUserDefaults.SetInt(value, key);

        public int GetInt(string key, int defaultValue)
        {
            var objectForKey = NSUserDefaults.StandardUserDefaults.ValueForKey(new NSString(key));
            if (objectForKey == null)
                return defaultValue;

            return (int)NSUserDefaults.StandardUserDefaults.IntForKey(key);
        }

        public void Remove(string key)
        {
            NSUserDefaults.StandardUserDefaults.RemoveObject(key);
        }

        public void RemoveAllWithPrefix(string prefix)
        {
            var keys = NSUserDefaults.StandardUserDefaults
                .ToDictionary()
                .Keys
                .Select(key => key.ToString())
                .Where(key => key.StartsWith(prefix, StringComparison.Ordinal));

            foreach (var key in keys)
            {
                Remove(key);
            }
        }
    }
}
