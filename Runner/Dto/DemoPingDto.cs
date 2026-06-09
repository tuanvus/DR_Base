using MessagePack;

namespace DR.Dto
{
    [MessagePackObject]
    public class DemoPingRequestDto
    {
        [Key(0)]
        public string Message { get; set; }
    }

    [MessagePackObject]
    public class DemoPingResponseDto
    {
        [Key(0)]
        public bool Success { get; set; }
        
        [Key(1)]
        public string Reply { get; set; }
        
        [Key(2)]
        public long ServerTicksUtc { get; set; }
    }
}
