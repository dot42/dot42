using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Dot42.Utility;
using Microsoft.Win32;
using TallComponents.Common.Extensions;

namespace Dot42.BarDeployLib
{
    /// <summary>
    /// BlackBerry device identification
    /// </summary>
    public sealed class BlackBerryDevices : IEnumerable<BlackBerryDevice>
    {
        private const string DeviceIdTemplate = "deviceId{0}";
        private const string DeviceSaltTemplate = "deviceSalt{0}";
        private const string DevicePasswordTemplate = "devicePassword{0}";

        private static readonly byte[] Key = {
            (byte) 207, (byte) 172, (byte) 107, (byte) 137, (byte) 53, (byte) 133,
            (byte) 140, (byte) 121, (byte) 128, (byte) 100, (byte) 50, (byte) 12, (byte) 245, (byte) 224, (byte) 128,
            (byte) 143, (byte) 37, (byte) 63, (byte) 136, (byte) 8, (byte) 213, (byte) 161, (byte) 19, (byte) 110,
            (byte) 229, (byte) 7, (byte) 0, (byte) 254, (byte) 77, (byte) 50, (byte) 175, (byte) 116
        };

        private static readonly byte[] IV = {
            (byte) 31, (byte) 49, (byte) 76, (byte) 231, (byte) 170, (byte) 74,
            (byte) 231, (byte) 185, (byte) 115, (byte) 64, (byte) 222, (byte) 115, (byte) 254, (byte) 134, (byte) 85,
            (byte) 46
        };

        /// <summary>
        /// Single instance.
        /// </summary>
        public static readonly BlackBerryDevices Instance = new BlackBerryDevices();

        /// <summary>
        /// Fired when a device has been added/removed.
        /// </summary>
        public event EventHandler Changed;

        private readonly object devicesLock = new object();
        private readonly List<BlackBerryDevice> devices = new List<BlackBerryDevice>();

        /// <summary>
        /// Default ctor
        /// </summary>
        private BlackBerryDevices()
        {            
            LoadFromRegistry();
        }

        /// <summary>
        /// Gets the default device.
        /// </summary>
        public BlackBerryDevice DefaultDevice
        {
            get { return this.FirstOrDefault(); }
        }

        /// <summary>
        /// Add a device.
        /// </summary>
        public BlackBerryDevice Add(string deviceIp, string password)
        {
            BlackBerryDevice result;
            lock (devicesLock)
            {
                var existing = devices.FirstOrDefault(x => string.Equals(x.UniqueId, deviceIp, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    // Alter existing
                    existing.Password = password;
                    result = existing;
                }
                else
                {

                    // Add new
                    var newItem = new BlackBerryDevice(deviceIp, password);
                    devices.Add(newItem);
                    result = newItem;
                }
                SaveToRegistry();
            }
            Changed.Fire(this);
            return result;
        }

        /// <summary>
        /// Remove the given device from this list.
        /// </summary>
        public void Remove(BlackBerryDevice device)
        {
            lock (devicesLock)
            {
                if (!devices.Remove(device))
                    return;
                SaveToRegistry();
            }
            Changed.Fire(this);
        }

        /// <summary>
        /// Force a reload.
        /// </summary>
        public void Reload()
        {
            var changed = LoadFromRegistry();
            if (changed)
            {
                Changed.Fire(this);
            }
        }

        /// <summary>
        /// Initialize me from the registry
        /// </summary>
        private bool LoadFromRegistry()
        {
            lock (devicesLock)
            {
                var oldHash = string.Join(",", devices.Select(x => x.Hash));
                var list = new List<BlackBerryDevice>();
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryConstants.BLACKBERRY_DEVICES))
                {
                    if (key != null)
                    {
                        var i = 0;
                        while (true)
                        {
                            var id = key.GetValue(string.Format(DeviceIdTemplate, i)) as string;
                            var salt = key.GetValue(string.Format(DeviceSaltTemplate, i)) as string;
                            var pw = key.GetValue(string.Format(DevicePasswordTemplate, i)) as string;

                            if ((id == null) || (salt == null) || (pw == null))
                            {
                                break;
                            }

                            string password;
                            if (TryDecodePassword(id, salt, pw, out password))
                            {
                                var device = new BlackBerryDevice(id, password);
                                if (!list.Any(x => string.Equals(x.UniqueId, id, StringComparison.OrdinalIgnoreCase)))
                                {
                                    list.Add(device);
                                }
                            }
                            i++;
                        }
                    }
                }
                var newHash = string.Join(",", list.Select(x => x.Hash));
                if (newHash != oldHash)
                {
                    devices.Clear();
                    devices.AddRange(list);
                }
                return (oldHash != newHash);
            }
        }

        /// <summary>
        /// Save me from the registry
        /// </summary>
        private void SaveToRegistry()
        {
            lock (devicesLock)
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryConstants.BLACKBERRY_DEVICES))
                {
                    for (var i = 0; i < devices.Count; i++)
                    {
                        string salt;
                        string pw;
                        EncodePassword(devices[i].UniqueId, devices[i].Password, out salt, out pw);
                        key.SetValue(string.Format(DeviceIdTemplate, i), devices[i].UniqueId);
                        key.SetValue(string.Format(DeviceSaltTemplate, i), salt);
                        key.SetValue(string.Format(DevicePasswordTemplate, i), pw);
                    }
                    // Remove obsolete entries
                    for (var i = devices.Count; i < devices.Count + 50; i++)
                    {
                        key.DeleteValue(string.Format(DeviceIdTemplate, i), false);
                        key.DeleteValue(string.Format(DeviceSaltTemplate, i), false);
                        key.DeleteValue(string.Format(DevicePasswordTemplate, i), false);                        
                    }
                }
            }
        }

        /// <summary>
        /// Decode a password entry into the actual password.
        /// </summary>
        private static bool TryDecodePassword(string deviceIp, string salt, string pw, out string decodedPassword)
        {
            var data = new byte[pw.Length / 2];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = byte.Parse(pw.Substring(i * 2, 2), NumberStyles.HexNumber);
            }
            var cryptor = Rijndael.Create();
            string decodedData;
            using (var cStream = new CryptoStream(new MemoryStream(data), cryptor.CreateDecryptor(Key, IV), CryptoStreamMode.Read))
            {
                var buffer = new byte[data.Length * 4];
                var length = cStream.Read(buffer, 0, buffer.Length);
                decodedData = Encoding.UTF8.GetString(buffer, 0, length);
            }

            var parts = decodedData.Split(new[] { '|' }, 3);
            decodedPassword = null;
            if (parts.Length != 3)
                return false;
            if (parts[0] != deviceIp)
                return false;
            if (parts[1] != salt)
                return false;
            decodedPassword = parts[2];
            return true;
        }

        /// <summary>
        /// Decode a password entry into the actual password.
        /// </summary>
        private static void EncodePassword(string deviceIp, string password, out string salt, out string pw)
        {
            salt = Guid.NewGuid().ToString("N");
            var formattedData = string.Format("{0}|{1}|{2}", deviceIp, salt, password);

            var cryptor = Rijndael.Create();
            var memStream = new MemoryStream();
            using (var cStream = new CryptoStream(memStream, cryptor.CreateEncryptor(Key, IV), CryptoStreamMode.Write))
            {
                var buffer = Encoding.UTF8.GetBytes(formattedData);
                cStream.Write(buffer, 0, buffer.Length);
                cStream.Flush();
            }
            pw = string.Join("", memStream.ToArray().Select(x => x.ToString("x2")).ToArray());
        }

        public IEnumerator<BlackBerryDevice> GetEnumerator()
        {
            lock (devicesLock)
            {
                return new List<BlackBerryDevice>(devices).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
