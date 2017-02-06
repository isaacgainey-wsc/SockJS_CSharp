using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockJS_CSharp
{
    public class SockJSInfo
    {
        public int entropy { get; set; }
        public dynamic[] origins { get; set; }
        public bool cookie_needed {get; set;}
        public bool websocket { get; set; }
    }
}
