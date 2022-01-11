namespace TSStickyWindow.Messages
{
    public class WindowUnstickedMessage
    {
        public WindowUnstickedMessage(string sourceId, 
            string targetTopId = "", string targetRightId = "", 
            string targetBottomId = "", string targetLeftId = "")
        {
            SourceId = sourceId;
            TargetTopId = targetTopId;
            TargetRightId = targetRightId;
            TargetBottomId = targetBottomId;
            TargetLeftId = targetLeftId;
        }

        public string SourceId { get; }

        public string TargetTopId { get; }
        public string TargetRightId { get; }
        public string TargetBottomId { get; }
        public string TargetLeftId { get; }
    }
}