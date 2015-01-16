using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TallComponents.Common.Util
{
	/// <summary>
	/// Summary description for AssemblyUtil.
	/// </summary>
	internal class AssemblyUtil
	{
		/// <summary>
		/// Gets the informational version of the given assembly.
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		public static string InformationalVersion(Assembly asm) 
		{
			object[] attrs = asm.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
			string config = null;
			if (attrs.Length > 0) 
			{
				AssemblyConfigurationAttribute attr = attrs[0] as AssemblyConfigurationAttribute;
				config = attr.Configuration;
			} 
			string version = asm.GetName().Version.ToString();
            if (string.IsNullOrEmpty(config)) 
			{
				return version;
			} 
			else 
			{
				return string.Format("{0} ({1})", version, config);
			}
		}

		/// <summary>
		/// Gets the copyright of the given assembly.
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		public static string Copyright(Assembly asm) 
		{
			object[] attrs = asm.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
			if (attrs.Length > 0) 
			{
				AssemblyCopyrightAttribute attr = attrs[0] as AssemblyCopyrightAttribute;
				return attr.Copyright;
			} 
			else 
			{
                return string.Format("Copyright © 2001 - {0} TallComponents", DateTime.Now.Year);
			}
		}
	}
}
