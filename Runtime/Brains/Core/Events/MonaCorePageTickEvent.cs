namespace Mona.Brains.Core.Events
{
    public struct MonaCorePageTickEvent
    {
        public bool IsStarting;
        public bool HasAnyMessage;
        public bool HasInput;

        public MonaCorePageTickEvent(bool isStarting, bool hasAnyMessage, bool hasInput)
        {
            IsStarting = isStarting;
            HasAnyMessage = hasAnyMessage;
            HasInput = hasInput;
        }
    }
}