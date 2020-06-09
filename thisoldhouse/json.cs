using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thisoldhouse
{
    public class ffmpeg
    {
        public class Json
        {
            public Bin bin { get; set; }
        }

        public class Bin
        {
            [JsonProperty(PropertyName = "windows-32")]
            public Windows32 windows32 { get; set; }
        }

        public class Windows32
        {
            public string ffmpeg { get; set; }
        }
    }
}
