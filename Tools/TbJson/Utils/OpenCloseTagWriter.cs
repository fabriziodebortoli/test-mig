using System;
using System.Diagnostics;

namespace Microarea.TbJson.Utils
{
    public class OpenCloseTagWriter : IDisposable
    {
        private readonly WebInterfaceGenerator generator;
        private readonly bool inline;
        private readonly string tag;
        private bool beginTagClosed;

        public OpenCloseTagWriter(string tag, WebInterfaceGenerator generator, bool inline)
        {
            this.generator = generator;
            this.tag = tag;
            this.inline = inline;
            generator.BeginTag(tag, false, false);
        }

        public void CloseBeginTag()
        {
            Debug.Assert(!beginTagClosed, "Opening tag already closed with '>'!");
            generator.CloseBeginTag(!inline);
            beginTagClosed = true;
        }

        public void Dispose()
        {
            Debug.Assert(beginTagClosed, "Opening tag not closed with '>'!");
            generator.EndTag(tag, !inline);
        }
    }
}
