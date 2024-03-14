using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlocEncryptCmdServer
{
    public class MessageS
    {
        public string MessageData { get; set; } = null!;
        public string Secret { get; set; } = null!;
        public string Username { get; set; } = null!;
    }
}
