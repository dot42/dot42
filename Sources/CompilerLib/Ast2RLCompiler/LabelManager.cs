using System;
using System.Collections.Generic;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.RL;

namespace Dot42.CompilerLib.Ast2RLCompiler
{
    /// <summary>
    /// Manage labels in a method
    /// </summary>
    internal sealed class LabelManager
    {
        private readonly Dictionary<string, Entry> labels = new Dictionary<string, Entry>();
        private readonly Stack<string> contextStack  = new Stack<string>();
        private int lastContext;

        /// <summary>
        /// 
        /// </summary>
        public LabelManager()
        {
            // Create initial context
            contextStack.Push(string.Empty);
        }

        /// <summary>
        /// Set the target of the given label, resolving any pending actions.
        /// </summary>
        public void SetTarget(AstLabel label, Instruction target)
        {
            GetLabel(label).Target = target;
        }

        /// <summary>
        /// Record an action to be called when the target is known.
        /// </summary>
        public void AddResolveAction(AstLabel label, Action<Instruction> action)
        {
            GetLabel(label).AddResolveAction(action);
        }

        /// <summary>
        /// Push a new context on the context stack.
        /// </summary>
        public IDisposable CreateContext()
        {
            var context = string.Format("{0}_", ++lastContext);
            contextStack.Push(context);
            return new ContextWrapper(this);
        }

        /// <summary>
        /// Remove the current context from the context stack.
        /// </summary>
        private void PopContext()
        {
            contextStack.Pop();
        }

        /// <summary>
        /// Get/create an entry for the given label.
        /// </summary>
        private Entry GetLabel(AstLabel label)
        {
            Entry result;
            var name = contextStack.Peek() + label.Name;
            if (!labels.TryGetValue(name, out result))
            {
                result = new Entry();
                labels.Add(name, result);
            }
            return result;
        }

        /// <summary>
        /// Single label entry
        /// </summary>
        private sealed class Entry
        {
            private Instruction target;
            private readonly List<Action<Instruction>> resolveBranchActions = new List<Action<Instruction>>();

            /// <summary>
            /// Gets/sets the target towards this label points.
            /// </summary>
            public Instruction Target
            {
                set
                {
                    if ((target != null) && (target != value))
                        throw new ArgumentException("Target already set");
                    target = value;
                    while (resolveBranchActions.Count > 0)
                    {
                        var action = resolveBranchActions[0];
                        resolveBranchActions.RemoveAt(0);
                        action(value);
                    }
                }
            }

            /// <summary>
            /// Record an action to be called when the target is known.
            /// </summary>
            public void AddResolveAction(Action<Instruction> action)
            {
                if (target != null)
                {
                    action(target);
                }
                else
                {
                    resolveBranchActions.Add(action);
                }
            }
        }

        private sealed class ContextWrapper : IDisposable
        {
            private readonly LabelManager labelManager;

            /// <summary>
            /// Default ctor
            /// </summary>
            public ContextWrapper(LabelManager labelManager)
            {
                this.labelManager = labelManager;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {
                labelManager.PopContext();                
            }
        }
    }
}
