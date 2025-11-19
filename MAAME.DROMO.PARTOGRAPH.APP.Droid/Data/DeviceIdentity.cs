using MAAME.DROMO.PARTOGRAPH.APP.Droid.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SQLite;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Data
{
    public static class DeviceIdentity
    {
        private const string DeviceIdKey = "Device_Key";

        public static string GetOrCreateDeviceId()
        {
            if (Preferences.ContainsKey(DeviceIdKey))
                return Preferences.Get(DeviceIdKey, string.Empty);

            var newId = Guid.NewGuid().ToString();
            Preferences.Set(DeviceIdKey, newId);

            return newId;
        }
    }
}