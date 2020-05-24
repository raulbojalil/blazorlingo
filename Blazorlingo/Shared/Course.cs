using System;
using System.Collections.Generic;
using System.Text;

namespace Blazorlingo.Shared
{
    public class Course
    {
        public string Id { get; set; }
        public string LearningLanguage { get; set; }
        public int Crowns { get; set; }
        public int Xp { get; set; }
        public string Title { get; set; }
        public string FromLanguage { get; set; }
    }
}
