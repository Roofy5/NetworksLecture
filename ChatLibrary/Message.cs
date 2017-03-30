using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatLibrary
{
    [Serializable]
    public class Message
    {
        public string Nick { get; set; }
        public string Text { get; set; }
        public DateTime SendTime { get; set; }
    }
}
