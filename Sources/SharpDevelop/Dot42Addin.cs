using System;
using System.Linq;
using System.Reflection;
using Dot42.Ide;
using Dot42.Utility;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace Dot42.SharpDevelop
{
	/// <summary>
	/// Description of Dot42Addin.
	/// </summary>
	public static class Dot42Addin
	{
		private static IIde ide = new Services.Ide();
		private static bool fixedAddinTreeConditions;
		
		static Dot42Addin() {
			InitializeLocations();
		}
		
		internal static void InitializeLocations() {
			Locations.SetTarget("android");			
		}
		
		/// <summary>
		/// Gets the IDE implementation.
		/// </summary>
		internal static IIde Ide { get{return ide; }}
		
		internal static IServiceProvider ServiceProvider { get{return null;}}
		
		/// <summary>
		/// Invoke the action on the main thread in a fire-and-forget mode.
		/// </summary>
		internal static void InvokeAsyncAndForget(Action action) {
			#if SD5
			SD.MainThread.InvokeAsyncAndForget(action);
			#else
			WorkbenchSingleton.SafeThreadAsyncCall(action);
			#endif
		}
		
		/// <summary>
		/// Invoke the action on the main thread and return after it is done.
		/// WARNING: This may cause a deadlock if the main thread is locked on the current thread!
		/// </summary>
		internal static void Invoke(Action action) {
			#if SD5
			SD.MainThread.Invoke(action);
			#else
			WorkbenchSingleton.SafeThreadCall(action);
			#endif
		}
		
		/// <summary>
		/// Fix the conditions for existing #Develop elements in the addin tree.
		/// </summary>
		internal static void FixAddinTreeConditions() {
			if (fixedAddinTreeConditions)
				return;
			fixedAddinTreeConditions = true;
			var node = AddInTree.GetTreeNode("/SharpDevelop/BackendBindings/ProjectOptions/C#");
			foreach (var codon in node.Codons) {
				switch (codon.Id) {
					case "Application":
					case "Signing":
					case "BuildOptions":
					case "DebugOptions":
						var props = new Properties();
						props["guid"] = "{337B7DB7-2D1E-448D-BEBF-17E887A46E37}";
						AddCondition(codon, new NegatedCondition(new Condition("ProjectBehaviorSupported", props)));
						break;
				}
			}
		}
		
		/// <summary>
		/// Add the given condition to the given codon.
		/// </summary>
		private static void AddCondition(Codon codon, ICondition condition) {
			var field = codon.GetType().GetField("conditions", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
			if (field == null)
				throw new InvalidOperationException("Cannot find conditions field");
			var conditions = (ICondition[])field.GetValue(codon);
			if (conditions == null) {
				conditions = new[] { condition };
			} else {
				Array.Resize(ref conditions, conditions.Length + 1);
				conditions[conditions.Length - 1] = condition;
			}
			field.SetValue(codon, conditions);
		}
	}
}
