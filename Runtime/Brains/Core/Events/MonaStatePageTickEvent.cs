namespace Mona.Brains.Core.Events
{ 
    public struct MonaStatePageTickEvent
    {
        public string State;
        public bool IsStarting;
        public bool HasAnyMessage;
        public bool HasInput;

        public MonaStatePageTickEvent(string state, bool isStarting, bool hasAnyMessage, bool hasInput)
        {
            State = state;
            IsStarting = isStarting;
            HasAnyMessage = hasAnyMessage;
            HasInput = hasInput;
        }
    }
}