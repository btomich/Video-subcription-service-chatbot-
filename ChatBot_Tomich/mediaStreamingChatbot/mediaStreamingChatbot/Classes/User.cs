using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mediaStreamingChatbot
{
    public class User
    {
        public string first { get; set; }
        public string last { get; set; }

        public string description { get; set; }

        public string bug { get; set; }

        public int userID { get; set; }
        public bool isCreator { get; set; }
    }
}
