using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.Brains.Core.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Utils/Mona Tags")]
    public class MonaTags : ScriptableObject, IMonaTags
    {
        private List<string> _default = new List<string>()
        {
            "Default",
            "Player",
            "Enemy",
            "Friendly",
            "Collectible"
        };

        [SerializeField]
        private List<string> _tags = new List<string>();

        public List<string> Tags {
            get {
                var allTags = new List<string>();
                allTags.AddRange(_default);
                allTags.AddRange(_tags);
                return allTags;
            }
        }
    }
}
