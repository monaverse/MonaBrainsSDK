using Mona.SDK.Brains.Core.ScriptableObjects;
using Mona.SDK.Brains.UIElements;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.UIEditors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MonaTags))]
    public class MonaTagEditor : Editor
    {
        private VisualElement _root;
        private MonaTagsVisualElement _tagEditor;

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            _tagEditor = new MonaTagsVisualElement();
            _tagEditor.SetMonaTags((IMonaTags)target);
            _tagEditor.TrackSerializedObjectValue(serializedObject, HandleCallback);
            _root.Add(_tagEditor);
            return _root;
        }

        public void OnDestroy()
        {
            //if (_tagEditor != null)
            //    _tagEditor.Dispose();
        }

        private void HandleCallback(SerializedObject so)
        {
            so.ApplyModifiedProperties();
            if (target != null)
            {
                EditorUtility.SetDirty(target);
                Undo.RecordObject(target, "change brain");
            }
            Debug.Log($"{nameof(HandleCallback)}");
        }
    }
#endif
}