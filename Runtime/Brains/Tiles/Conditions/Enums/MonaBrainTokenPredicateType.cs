namespace Mona.SDK.Brains.Tiles.Conditions.Enums
{
    public enum MonaBrainTokenPredicateType
    {
        TokenType=0,
        HasTrait=4,
        TraitValue=5,
        Name=10,
        NameContains=15,
        NameStartsWith=16,
        DescriptionContains=17,
        Token =20,
        TokenRange=21,
        Contract=25,
        Collection=30,
        CollectionSymbol = 31,
        CollectionSlug = 32,
        Position =35,
        ChainId=40
    }

    public enum MonaBrainTokenPredicatePositionType
    {
        Index=0,
        First=5,
        Last=10,
        Random=15,
    }
}