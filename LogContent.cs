using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples
{
    public class LogContent
    {
        public string Question { get; set; }
        public string LuisIntent { get; set; }
        public double LuisScore { get; set; }
        public string QnAanswer { get; set; }
        public double QnAscore { get; set; }
        public string User { get; set; }
        public string Channel { get; set; }
        public string Date_Time { get; set; }

    }
}
