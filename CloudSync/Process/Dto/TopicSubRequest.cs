namespace CloudSync.Process.Dto
{
    public class TopicSubRequest
    {
        public string Topic { get; set; }
        public string ExecuteApi { get; set; }
        public string ForwardUrl { get; set; }
        public string ForwardAuthKey { get; set; }
    }
}
