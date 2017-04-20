using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExViewer.ViewModels.SearchingParser
{
    internal class Parser : IEnumerable<Term>
    {
        private readonly string input;

        private struct Token
        {
            internal Token(Parser parent, int start, int length)
            {
                this.StartPosition = start;
                this.Length = length;
                this.value = parent.input;
            }

            public int StartPosition { get; }
            public int Length { get; }
            private readonly string value;

            public string Value => this.value.Substring(this.StartPosition, this.Length);
        }

        public Parser(string input)
        {
            this.input = input ?? "";
        }

        private IEnumerable<Token> getTokens()
        {
            var currentPos = 0;
            var startPos = 0;
            var inQuatation = false;
            while(currentPos < this.input.Length)
            {
                var current = this.input[currentPos];
                if(char.IsWhiteSpace(current))
                {
                    if(!inQuatation)
                    {
                        if(startPos != currentPos)
                            yield return new Token(this, startPos, currentPos - startPos);
                        startPos = currentPos + 1;
                    }
                }
                else if(current == '"')
                    inQuatation = !inQuatation;
                else
                {

                }
                currentPos++;
            }
            if(startPos != currentPos)
                yield return new Token(this, startPos, currentPos - startPos);
        }

        public IEnumerator<Term> GetEnumerator()
        {
            return getTokens().Select(t => new Term(t.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    internal class Term : IStringable
    {
        public Term(string value)
        {
            // TODO:
        }

        public string Keyword { get; set; }
        public string Namespace { get; set; }
        public bool Exact { get; set; }
        public bool Exclusion { get; set; }
        public bool LeftQuotation { get; set; }
        public bool RightQuotation { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if(Exclusion)
                sb.Append('-');
            sb.Append(Namespace);
            if(Namespace != null)
                sb.Append(':');
            if(LeftQuotation)
                sb.Append('"');
            sb.Append(Keyword);
            if(Exact)
                sb.Append('$');
            if(RightQuotation)
                sb.Append('"');
            return sb.ToString();
        }
    }
}
