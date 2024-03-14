using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlocEncryptCmdServer
{
    public class User
    {
        public string Name { get; set; } = null!;

        public string ChatSecret { get; set; } = null!;
    }
}
