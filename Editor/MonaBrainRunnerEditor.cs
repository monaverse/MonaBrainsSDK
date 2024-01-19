using UnityEditor;
using UnityEngine.UIElements;
using Mona.Brains.Core.Brain;
using Mona.Brains.UIElements;
using UnityEngine;

namespace Mona.Brains.UIEditors
{
    [CustomEditor(typeof(MonaBrainRunner))]
    public class MonaBrainRunnerEditor : Editor
    {
        private VisualElement _root;
        private MonaBrainRunnerVisualElement _brainRunnerElement;

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            _brainRunnerElement = new MonaBrainRunnerVisualElement();
            _brainRunnerElement.SetRunner((MonaBrainRunner)target);
            _root.Add(_brainRunnerElement);
            return _root;
        }
    }
}