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
        OnSelectTarget = 60,
        MySpawner = 70,
        LastSpawnedByMe = 80,
        AllSpawnedByMe = 90,
        MyPoolPreviouslySpawned = 100,
        MyPoolNextSpawned = 110,
        MyBodyArray = 120,
        LastSkin = 130,
    }

    public enum MonaBrainTransformTargetType
    {
        Tag = 0,
        Self = 10,
        Parent = 20,
        Children = 30,
        ThisBodyOnly = 35,
        MessageSender = 40,
        OnConditionTarget = 50,
        OnSelectTarget = 60,
        MySpawner = 70,
        LastSpawnedByMe = 80,
        AllSpawnedByMe = 90,
        MyPoolPreviouslySpawned = 100,
        MyPoolNextSpawned = 110,
        MyBodyArray = 120
    }

    public enum MonaBrainHovererTargetType
    {
        Tag = 0,
        MousePointer = 12,
        WorldPosition = 13,
        MySpawner = 70,
        LastSpawnedByMe = 80,
        AllSpawnedByMe = 90,
        MyPoolPreviouslySpawned = 100,
        MyPoolNextSpawned = 110,
        MyBodyArray = 120
    }

    public enum MonaBrainPausableTargetType
    {
        Tag = 0,
        All = 5,
        Self = 10,
        Parent = 20,
        Parents = 24,
        Children = 30,
        ThisBodyOnly = 35,
        MessageSender = 40,
        OnConditionTarget = 50,
        OnSelectTarget = 60,
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
        OnSelectTarget = 60,
        MySpawner = 70,
        LastSpawnedByMe = 80,
        AnySpawnedByMe = 90,
        MyPoolPreviouslySpawned = 100,
        MyPoolNextSpawned = 110
    }

    public enum MonaBrainOrbitTargetType
    {
        Tag = 0,
        WorldSpace = 14,
        LocalSpace = 16,
        Parent = 20,
        MessageSender = 40,
        OnConditionTarget = 50,
        OnSelectTarget = 60,
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

    public enum MonaBrainTargetMaterialType
    {
        Tag = 0,
        Self = 10,
        Skybox = 12,
        Parent = 20,
        Parents = 24,
        Children = 30,
        ThisBodyOnly = 35,
        MessageSender = 40,
        OnConditionTarget = 50,
        OnSelectTarget = 60,
        MySpawner = 70,
        LastSpawnedByMe = 80,
        AllSpawnedByMe = 90,
        MyPoolPreviouslySpawned = 100,
        MyPoolNextSpawned = 110
    }

    public enum MonaBrainTargetColorType
    {
        Tag = 0,
        Self = 10,
        //Skybox = 12,
        GlobalFog = 13,
        GlobalShadows = 14,
        GlobalAmbience = 15,
        CameraBackground = 16,
        Parent = 20,
        Parents = 24,
        Children = 30,
        ThisBodyOnly = 35,
        MessageSender = 40,
        OnConditionTarget = 50,
        OnSelectTarget = 60,
        MySpawner = 70,
        LastSpawnedByMe = 80,
        AllSpawnedByMe = 90,
        MyPoolPreviouslySpawned = 100,
        MyPoolNextSpawned = 110
    }

    public enum MonaBrainTargetLayoutType
    {
        Tag = 0,
        AllBodies = 5,
        //Self = 10,
        //Parent = 20,
        //Parents = 24,
        //Children = 30,
        ThisBodyOnly = 35
        //MessageSender = 40,
        //OnConditionTarget = 50,
        //OnHitTarget = 60,
        //MySpawner = 70,
        //LastSpawnedByMe = 80,
        //AllSpawnedByMe = 90,
        //MyPoolPreviouslySpawned = 100,
        //MyPoolNextSpawned = 110
    }
}
