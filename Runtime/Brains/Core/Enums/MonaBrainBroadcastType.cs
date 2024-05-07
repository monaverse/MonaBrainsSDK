namespace Mona.SDK.Brains.Core.Enums
{
    public enum MonaBrainBroadcastType
    {
        Tag = 0,
        Self = 10,
        Parent = 20,
        Parents = 24,
        Children = 30,
        ThisBodyOnly = 35,
        MessageSender = 40,
        OnConditionTarget = 50,
        OnHitTarget = 60,
        MySpawner = 70,
        LastSpawnedByMe = 80,
        AllSpawnedByMe = 90,
        MyPoolPreviouslySpawned = 100,
        MyPoolNextSpawned = 110
    }

    public enum MonaBrainTransformType
    {
        Tag = 0,
        Self = 10,
        WorldSpace = 14,
        LocalSpace = 16,
        Parent = 20,
        Child = 30,
        MessageSender = 40,
        OnConditionTarget = 50,
        OnHitTarget = 60,
        MySpawner = 70,
        LastSpawnedByMe = 80,
        AnySpawnedByMe = 90,
        MyPoolPreviouslySpawned = 100,
        MyPoolNextSpawned = 110
    }

    public enum MonaBrainSelfTransformType
    {
        CurrentWorld,
        CurrentLocal,
        InitialWorld,
        InitialLocal
    }
}
