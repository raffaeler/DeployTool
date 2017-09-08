using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool.Interactivity
{
    public class MenuItem
    {
        public string Description { get; set; }
        public InvokerKind InvokerKind { get; set; }
        public object InternalInvocationParameter { get; set; }
        public Func<ConsoleKeyInfo, MenuItem, string> ExternalInvoker { get; set; }
        public string Parameters { get; set; }
        //public string RelativeUrl { get; set; }
        //public string Verb { get; set; }
        //public string Body { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }
}
