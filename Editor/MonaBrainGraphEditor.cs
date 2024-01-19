using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.ScriptableObjects;
using Mona.SDK.Brains.UIElements;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

namespace Mona.SDK.Brains.UIEditors
{
#if UNITY_EDITOR
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