using MessagePack;

namespace DR.Define
{
    [MessagePackObject]
    public class ErrorResponse
    {
        [Key(0)]
        public int Code { get; set; }

        [Key(1)]
        public string Message { get; set; }
    }
}
