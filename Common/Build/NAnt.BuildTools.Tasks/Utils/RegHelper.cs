using Microsoft.Win32;

namespace NAnt.BuildTools.Tasks.Utils
{
    public static class RegHelper
    {
        public static string GetRegistryValue(string path, string value, params RegistryHive[] hives)
        {
            foreach (RegistryHive hive in hives)
            {
                //Log(Level.Verbose, "Opening {0}:{1}.", (object)hive.ToString(), (object)hive);
                using (var root = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                {
                    RegistryKey registryKey = root.OpenSubKey(path, false);
                    if (registryKey == null)
                        continue;

                    var val = registryKey.GetValue(value, null, RegistryValueOptions.None);
                    if (val == null)
                        continue;
                    return val.ToString();
                }
            }
            return null;
        }
    }
}
