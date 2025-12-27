using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace DR.Define
{
    [MessagePackObject]
    public class Dmge
    {

        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public string Name { get; set; }
    }
}
