using Mona.SDK.Brains.Core.State.UIElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.Core.State.UIEditors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MonaBrainVariablesBehaviour))]
    public class MonaBrainVariablesBehaviourEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var self = (IMonaBrainVariables)target;
            var root = new MonaBrainValuesVisualElement();
            root.SetState(self);
            return root;
        }
    }
#endif
}