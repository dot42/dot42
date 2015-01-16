using System;

namespace Dot42.ApkSpy.Tree
{
    internal class ExceptionNode : TextNode
    {
        private readonly Exception error;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ExceptionNode(Exception error)
        {
            this.error = error;
            Text = error.GetType().Name;
        }

        /// <summary>
        /// Load the text to display
        /// </summary>
        protected override string LoadText(ISpyContext settings)
        {
            return string.Format("Error: {0}{1}{2}", error.Message, Environment.NewLine, error.StackTrace);
        }
    }
}
