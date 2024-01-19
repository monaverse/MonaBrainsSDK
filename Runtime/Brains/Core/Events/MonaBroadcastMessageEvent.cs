using Mona.Brains.Core.Brain;

namespace Mona.Brains.Core.Events
{
    public struct MonaBroadcastMessageEvent
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