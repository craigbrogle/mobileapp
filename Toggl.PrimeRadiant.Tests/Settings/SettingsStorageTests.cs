using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Toggl.PrimeRadiant.Settings;
using Xunit;

namespace Toggl.PrimeRadiant.Tests.Settings
{
    public sealed class SettingsStorageTests
    {
        public abstract class SettingsStorageTest
        {
            protected const string VersionString = "1.0";

            protected IKeyValueStorage Storage { get; } = new InMemoryKeyValueStorage();

            protected SettingsStorage SettingsStorage { get; } 

            public SettingsStorageTest()
            {
                SettingsStorage = new SettingsStorage(Version.Parse(VersionString), Storage);
            }

            private sealed class InMemoryKeyValueStorage : IKeyValueStorage
            {
                private readonly Dictionary<string, bool> bools = new Dictionary<string, bool>();
                private readonly Dictionary<string, string> strings = new Dictionary<string, string>();
                private readonly Dictionary<string, int> ints = new Dictionary<string, int>();

                public bool GetBool(string key)
                {
                    bools.TryGetValue(key, out var value);
                    return value;
                }

                public string GetString(string key)
                {
                    strings.TryGetValue(key, out var value);
                    return value;
                }

                public void SetBool(string key, bool value)
                {
                    bools[key] = value;
                }

                public void SetString(string key, string value)
                {
                    strings[key] = value;
                }

                public void SetInt(string key, int value)
                {
                    ints[key] = value;
                }

                public int GetInt(string key, int defaultValue)
                {
                    int value;
                    if (ints.TryGetValue(key, out value))
                        return value;
                    return defaultValue;
                }

                public void Remove(string key)
                {
                }

                public void RemoveAllWithPrefix(string prefix)
                {
                    var keys = bools.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal));
                    foreach (var key in keys)
                        bools.Remove(key);

                    keys = strings.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal));
                    foreach (var key in keys)
                        strings.Remove(key);
                }
            }
        }

        public abstract class DeprecationCheckTest : SettingsStorageTest
        {
            internal abstract bool CallOutdatedMethod();

            protected abstract string Key { get; }

            [Fact, LogIfTooSlow]
            public void ReturnsTrueIfTheStoredVersionIsEqualToTheVersionPassedDuringConstruction()
            {
                Storage.SetString(Key, VersionString);

                var isOutdated = CallOutdatedMethod();

                isOutdated.Should().BeTrue();
            }

            [Theory, LogIfTooSlow]
            [InlineData("0.1")]
            [InlineData("0.2")]
            [InlineData("0.7.5")]
            [InlineData("0.9.7")]
            [InlineData("0.11.9")]
            public void ReturnsFalseIfTheStoredVersionIsSmallerThanTheVersionPassedDuringConstruction(string version)
            {
                Storage.SetString(Key, version);

                var isOutdated = CallOutdatedMethod();

                isOutdated.Should().BeFalse();
            }
        }

        public sealed class TheIsClientOutdatedMethod : DeprecationCheckTest
        {
            protected override string Key => "OutdatedClient";

            internal override bool CallOutdatedMethod()
                => SettingsStorage.IsClientOutdated();
        }

        public sealed class TheIsApiOutdatedMethod : DeprecationCheckTest
        {
            protected override string Key => "OutdatedApi";

            internal override bool CallOutdatedMethod()
                => SettingsStorage.IsApiOutdated();
        }

        public sealed class TheIsUnauthorizedMethod : SettingsStorageTest
        {
            [Property]
            public void ReturnsTrueIfTheApiTokenPassedIsTheSameAsTheOneStored(NonEmptyString key)
            {
                var apiToken = key.Get;
                SettingsStorage.SetUnauthorizedAccess(apiToken);

                var isUnauthorized = SettingsStorage.IsUnauthorized(apiToken);

                isUnauthorized.Should().BeTrue();
            }
            [Property]
            public void ReturnsFalseIfTheApiTokenPassedIsDifferentThanTheOneStored(NonEmptyString key)
            {
                var apiToken = key.Get;
                SettingsStorage.SetUnauthorizedAccess($"{apiToken}{apiToken}");

                var isUnauthorized = SettingsStorage.IsUnauthorized(apiToken);

                isUnauthorized.Should().BeFalse();
            }
        }

        public sealed class TheSetLastOpenedMethod : SettingsStorageTest
        {
            [Property]
            public void SetsTheStorageParameterToTheSerializedValueOfTheDatePassed(DateTimeOffset date)
            {
                SettingsStorage.SetLastOpened(date);

                SettingsStorage.GetLastOpened().Should().Be(date.ToString());
            }
        }
    }
}
