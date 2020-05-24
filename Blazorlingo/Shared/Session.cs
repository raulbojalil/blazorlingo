using System;
using System.Collections.Generic;
using System.Text;

namespace Blazorlingo.Shared
{
    public class Session
    {
        public string Id { get; set; }
        public SimpleChallenge[] Challenges { get; set; }
    }
}
