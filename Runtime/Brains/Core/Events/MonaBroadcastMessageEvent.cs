using Mona.SDK.Brains.Core.Brain;

namespace Mona.SDK.Brains.Core.Events
{
    public interface IBrainMessageEvent { }

    public struct MonaBroadcastMessageEvent : IBrainMessageEvent
    {
        public string Message;
        public IMonaBrain Sender;
        public int Frame;
        public MonaBroadcastMessageEvent(string message, IMonaBrain sender, int frame)
        {
            Message = message;
            Sender = sender;
            Frame = frame;
        }

        public bool ShouldExpireMessage(int frame)
        {
            return frame - Frame > 0;
        }
    }
}