using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Specialized;
using Microsoft.Win32;
using TallComponents.Common.Extensions;

// NOTE: This provider uses Assembly metadata such as ProductName, etc. 
// to determine a workable registry path in which to store settings. 
// Note that these are NOT secure metadata elements, however they are 
// reasonably safe from collision but not at all safe from malicious tampering.  
// A robust implementation of the provider would include a better pathing algorithm.

namespace TallComponents.Common.Configuration
{
    /// <summary>
    /// Settings provider that stores per application data in the registry.
    /// </summary>
    internal class RegistrySettingsProvider : SettingsProvider
    {
        private string subKeyPath;
        private string productVersion;

        /// <summary>
        /// Default ctor
        /// </summary>
        public RegistrySettingsProvider()
        {
        }

        /// <summary>
        /// Gets the application name
        /// </summary>
        public override string ApplicationName
        {
            get { return Application.ProductName; }
            set { }
        }

        public override void Initialize(string name, NameValueCollection col)
        {
            base.Initialize(this.ApplicationName, col);
        }

        // SetPropertyValue is invoked when ApplicationSettingsBase.Save is called
        // ASB makes sure to pass each provider only the values marked for that provider -
        // though in this sample, since the entire settings class was marked with a SettingsProvider
        // attribute, all settings in that class map to this provider
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection propvals)
        {
            // Iterate through the settings to be stored
            // Only IsDirty=true properties should be included in propvals
            foreach (SettingsPropertyValue propval in propvals)
            {
                // NOTE: this provider allows setting to both user- and application-scoped
                // settings. The default provider for ApplicationSettingsBase - 
                // LocalFileSettingsProvider - is read-only for application-scoped setting. This 
                // is an example of a policy that a provider may need to enforce for implementation,
                // security or other reasons.
                if (propval.IsDirty)
                {
                    object serializedValue = propval.SerializedValue;
                    try
                    {
                        using (RegistryKey key = GetRegKey(propval.Property, true))
                        {
                            if (key != null)
                            {
                                if (serializedValue != null)
                                {
                                    key.SetValue(propval.Name, propval.SerializedValue);
                                }
                                else if (key.GetValue(propval.Name) != null)
                                {
                                    key.DeleteValue(propval.Name, false);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(string.Format("Error in SetPropertyValues: {0}\n{1}", ex.GetMessage(), ex.StackTrace));
                    }
                }
            }
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection props)
        {

            // Create new collection of values
            SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();

            // Iterate through the settings to be retrieved
            foreach (SettingsProperty setting in props)
            {
                SettingsPropertyValue value = new SettingsPropertyValue(setting);
                value.IsDirty = false;
                try
                {
                    using (RegistryKey key = GetRegKey(setting, false))
                    {
                        if (key != null)
                        {
                            value.SerializedValue = key.GetValue(setting.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("Error in GetPropertyValues: {0}\n{1}", ex.GetMessage(), ex.StackTrace));
                }
                values.Add(value);
            }
            return values;
        }

        // Helper method: fetches correct registry subkey.
        // HKLM is used for settings marked as application-scoped.
        // HKLU is used for settings marked as user-scoped.
        private RegistryKey GetRegKey(SettingsProperty prop, bool writable)
        {
            RegistryKey regKey;

            if (IsUserScoped(prop))
            {
                regKey = Registry.CurrentUser;
            }
            else
            {
                regKey = Registry.LocalMachine;
            }

            string path = GetSubKeyPath(writable);
            if (writable)
            {
                return regKey.CreateSubKey(path);
            }
            else
            {
                return regKey.OpenSubKey(path);
            }
        }

        // Helper method: walks the "attribute bag" for a given property
        // to determine if it is user-scoped or not.
        // Note that this provider does not enforce other rules, such as 
        //   - unknown attributes
        //   - improper attribute combinations (e.g. both user and app - this implementation
        //     would say true for user-scoped regardless of existence of app-scoped)
        private bool IsUserScoped(SettingsProperty prop)
        {
            foreach (DictionaryEntry d in prop.Attributes)
            {
                Attribute a = (Attribute)d.Value;
                if (a.GetType() == typeof(UserScopedSettingAttribute))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the used subkey in the registry
        /// </summary>
        /// <returns></returns>
        protected virtual string GetSubKeyPath(bool writable)
        {
            if (subKeyPath == null)
            {
                subKeyPath = CreateSubKeyPath();
            }
            return subKeyPath;
        }

        /// <summary>
        /// Create the sub key path.
        /// </summary>
        /// <returns></returns>
        protected virtual string CreateSubKeyPath()
        {
            return string.Format(@"SOFTWARE\{0}\{1}\{2}",
                Application.CompanyName,
                Application.ProductName,
                ProductVersion);
        }

        /// <summary>
        /// Gets the relevant part of the product version.
        /// </summary>
        protected string ProductVersion
        {
            get
            {
                if (productVersion == null)
                {
                    productVersion = this.GetType().Assembly.GetName().Version.Major.ToString();
                    /*string v = Application.ProductVersion;
                    int idx = v.IndexOf('.');
                    if (idx > 0)
                    {
                        productVersion = v.Substring(0, idx);
                    }
                    else
                    {
                        productVersion = v;
                    }*/
                }
                return productVersion;
            }
        }               
    }

    /// <summary>
    /// Setting provider that stores per application+assembly settings in
    /// the registry.
    /// </summary>
    internal class RegistryAssemblySettingsProvider : RegistrySettingsProvider 
    {
        private readonly string companyName;
        private readonly string productName;

        /// <summary>
        /// Default ctor
        /// </summary>
        public RegistryAssemblySettingsProvider()
            : this(Application.ProductName, Application.CompanyName)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        protected RegistryAssemblySettingsProvider(string productName)
            : this(productName, Application.CompanyName)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        protected RegistryAssemblySettingsProvider(string productName, string companyName)
        {
            this.productName = productName;
            this.companyName = companyName;
        }

        /// <summary>
        /// Get the used subkey in the registry
        /// </summary>
        /// <returns></returns>
        protected override string CreateSubKeyPath()
        {
            Assembly me = typeof(RegistrySettingsProvider).Assembly;
            return string.Format(@"SOFTWARE\{0}\{1}\{2}\{3}",
                companyName,
                productName,
                me.GetName().Name,
                ProductVersion);
        }
    }
}
