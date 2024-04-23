using Mona.SDK.Brains.Core.State.UIElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.Core.State.UIEditors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MonaBrainVariablesBehaviour))]
    public class MonaBrainVariablesBehaviourEditor : Editor
    {
        private MonaBrainValuesVisualElement _root;
        public override VisualElement CreateInspectorGUI()
        {
            var self = (IMonaBrainVariables)target;
            _root = new MonaBrainValuesVisualElement();
            _root.SetState(self);
            return _root;
        }

        public void OnDestroy()
        {
            _root.Dispose();
        }
    }
#endif
}