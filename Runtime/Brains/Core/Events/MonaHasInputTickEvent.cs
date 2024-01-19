namespace Mona.Brains.Core.Events
{
    public struct MonaHasInputTickEvent
    {
        public int Frame;
        public MonaHasInputTickEvent(int frame)
        {
            Frame = frame;
        }

        public bool ShouldExpireMessage(int frame)
        {
            return frame - Frame > 0;
        }
    }
}