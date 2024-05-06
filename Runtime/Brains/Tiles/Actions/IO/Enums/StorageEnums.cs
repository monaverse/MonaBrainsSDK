using System;

namespace Mona.SDK.Brains.Tiles.Actions.IO
{
    [Serializable]
    public enum StorageVariableType
    {
        Number = 0,
        Bool = 10,
        String = 20,
        Vector2 = 30,
        Vector3 = 40
    }

    [Serializable]
    public enum StorageStringFormatType
    {
        VariableName = 0,
        VariableAndBrainName = 10,
        CustomName = 20,
        CustomAndBrainName = 30
    }

    [Serializable]
    public enum StorageBrainNameType
    {
        ThisBrain = 0,
        CustomBrainName = 10
    }
}