using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.SocialUser.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/UserServer/UserServerUpdatePassword", fileName = "UserServerUpdatePassword")]
    public class SocialUserUpdatePasswordInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new SocialUserUpdatePasswordInstructionTile();
        [SerializeField] protected string _id = SocialUserUpdatePasswordInstructionTile.ID;
        [SerializeField] protected string _name = SocialUserUpdatePasswordInstructionTile.NAME;
        [SerializeField] protected string _category = SocialUserUpdatePasswordInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
