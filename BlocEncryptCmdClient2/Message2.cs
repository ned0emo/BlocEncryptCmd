﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlocEncryptCmdClient2
{
    public class Message2
    {
        public string MessageData { get; set; } = null!;
        public string Secret { get; set; } = null!;
        public string Username { get; set; } = null!;
    }
}
