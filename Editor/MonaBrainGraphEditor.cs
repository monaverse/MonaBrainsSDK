using Mona.Brains.Core.Brain;
using Mona.Brains.Core.ScriptableObjects;
using Mona.Brains.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mona.Brains.UIEditors
{
    [CustomEditor(typeof(MonaBrainGraph))]
    public class MonaBrainGraphEditor : Editor
    {
        public void OnEnable()
        {

        }

        private VisualElement _root;
        private MonaBrainGraphVisualElement _brainEditor;

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            _brainEditor = new MonaBrainGraphVisualElement();
            _brainEditor.SetBrain((IMonaBrain)target);
            _brainEditor.TrackSerializedObjectValue(serializedObject, HandleCallback);
            _root.Clear();
            _root.Add(_brainEditor);
            return _root;
        }

        private void HandleCallback(SerializedObject so)
        {
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            Undo.RecordObject(target, "change brain");
            //Debug.Log($"{nameof(HandleCallback)}");
        }
    }
}