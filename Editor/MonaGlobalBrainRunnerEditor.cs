using UnityEditor;
using UnityEngine.UIElements;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.UIElements;
using UnityEngine;

namespace Mona.SDK.Brains.UIEditors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MonaGlobalBrainRunner))]
    public class MonaGlobalBrainRunnerEditor : Editor
    {
        private VisualElement _root;
        private MonaGlobalBrainRunnerVisualElement _brainRunnerElement;

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            _brainRunnerElement = new MonaGlobalBrainRunnerVisualElement();
            _brainRunnerElement.SetRunner((MonaGlobalBrainRunner)target);
            _root.Add(_brainRunnerElement);
            return _root;
        }

        public void OnDestroy()
        {
            if (_brainRunnerElement != null)
                _brainRunnerElement.Dispose();
        }
    }
#endif
}