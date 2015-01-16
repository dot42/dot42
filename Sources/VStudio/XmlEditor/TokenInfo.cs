using Microsoft.XmlEditor;
using VSTokenInfo = Microsoft.VisualStudio.Package.TokenInfo;

namespace Dot42.VStudio.XmlEditor
{
    internal class TokenInfo : ITokenInfo
    {
        private readonly VSTokenInfo tokenInfo;

        public TokenInfo(VSTokenInfo tokenInfo)
        {
            this.tokenInfo = tokenInfo;
        }

        public int StartIndex { get { return tokenInfo.StartIndex; } }
        public int EndIndex { get { return tokenInfo.EndIndex; } }
        public bool IsStartOfTag { get { return ((Token)tokenInfo.Token == Token.StartOfTag); } }
        public bool IsWhitespace { get { return ((Token)tokenInfo.Token == Token.Whitespace); } }
        public bool IsStringLiteral { get { return ((Token)tokenInfo.Token == Token.StringLiteral); } }
        public bool IsStartStringLiteral { get { return ((Token)tokenInfo.Token == Token.StartStringLiteral); } }
    }
}
