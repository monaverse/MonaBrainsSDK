using Mona.Brains.Core.State.UIElements;
using Mona.Core.State.UIEditors;
using UnityEditor;
using UnityEngine.UIElements;

namespace Mona.Brains.Core.State.UIEditors
{
    [CustomEditor(typeof(MonaBrainValues))]
    public class MonaBrainValuesEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var self = (IMonaBrainState)target;
            var root = new MonaBrainValuesVisualElement();
            root.SetState(self);
            return root;
        }
    }
}