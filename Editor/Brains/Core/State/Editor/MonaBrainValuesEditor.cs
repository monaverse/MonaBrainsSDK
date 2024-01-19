using Mona.SDK.Brains.Core.State.UIElements;
using Mona.SDK.Core.State.UIEditors;
using UnityEditor;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.Core.State.UIEditors
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