namespace Mona.SDK.Brains.Core.Events
{ 
    public struct MonaVariablesPageTickEvent
    {
        public string State;
        public bool IsStarting;
        public bool HasAnyMessage;
        public bool HasInput;

        public MonaVariablesPageTickEvent(string state, bool isStarting, bool hasAnyMessage, bool hasInput)
        {
            State = state;
            IsStarting = isStarting;
            HasAnyMessage = hasAnyMessage;
            HasInput = hasInput;
        }
    }
}