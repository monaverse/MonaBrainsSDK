using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.SocialUser.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/UserServer/RegisterNewUser", fileName = "RegisterNewUser")]
    public class SocialUserRegisterInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new SocialUserRegisterInstructionTile();
        [SerializeField] protected string _id = SocialUserRegisterInstructionTile.ID;
        [SerializeField] protected string _name = SocialUserRegisterInstructionTile.NAME;
        [SerializeField] protected string _category = SocialUserRegisterInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
