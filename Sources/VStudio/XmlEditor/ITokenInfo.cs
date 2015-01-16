using System;

namespace Dot42.VStudio.XmlEditor
{
    public interface ITokenInfo
    {
        int StartIndex { get; }
        int EndIndex { get; }
        bool IsStartOfTag { get; }
        bool IsWhitespace { get; }
        bool IsStringLiteral { get; }
        bool IsStartStringLiteral { get; }
    }
}
