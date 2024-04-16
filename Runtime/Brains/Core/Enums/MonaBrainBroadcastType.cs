namespace Mona.SDK.Brains.Core.Enums
{
    public enum MonaBrainBroadcastType
    {
        Tag = 0,
        Self = 10,
        Parent = 20,
        Children = 30,
        ThisBodyOnly = 35,
        MessageSender = 40,
        OnConditionTarget = 50,
        OnHitTarget = 60,
        MySpawner = 70,
        LastSpawnedByMe = 80,
        AllSpawnedByMe = 90
    }
}
