using System;

namespace Dot42.VStudio.XmlEditor
{
    public interface ISource
    {
        void Save(Action saveAction);
        ITokenInfo GetTokenInfo(int line, int column);
    }
}
