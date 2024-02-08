using UnityEditor;
using UnityEngine.UIElements;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.UIElements;
using UnityEngine;
using UnityEditor.UIElements;

namespace Mona.SDK.Brains.UIEditors
{
#if UNITY_EDITOR
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
            _brainRunnerElement.TrackSerializedObjectValue(serializedObject, HandleCallback);
            _root.Add(_brainRunnerElement);
            return _root;
        }

        public void OnDestroy()
        {
            if (_brainRunnerElement != null)
                _brainRunnerElement.Dispose();
        }

        private void HandleCallback(SerializedObject so)
        {
            so.ApplyModifiedProperties();
            if (target != null)
            {
                EditorUtility.SetDirty(target);
                Undo.RecordObject(target, "change brain");
            }
            //Debug.Log($"{nameof(HandleCallback)}");
        }
    }
#endif
}