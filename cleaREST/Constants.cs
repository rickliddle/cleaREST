using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cleaREST
{
    internal static class Protocols
    {
        internal static string Http { get { return "http"; } }

        internal static string Https { get { return "https"; } }
    }

    public enum HttpAuthentication
    {
        Basic,
        //Digest,
        //NTLM
    }    
}
