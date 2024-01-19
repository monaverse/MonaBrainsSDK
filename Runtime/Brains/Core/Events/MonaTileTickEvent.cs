namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaTileTickEvent
    {
        public float DeltaTime;
        public MonaTileTickEvent(float deltaTime)
        {
            DeltaTime = deltaTime;
        }
    }
}