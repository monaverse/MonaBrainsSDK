using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.ScriptableObjects;
using Mona.SDK.Brains.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

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
        private VisualElement _contentViewPort;

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            
            _brainEditor = new MonaBrainGraphVisualElement(SetDirtyCallback);
            _brainEditor.SetBrain((IMonaBrain)target);
            _brainEditor.TrackSerializedObjectValue(serializedObject, HandleCallback);
            _root.Clear();
            _root.Add(_brainEditor);

            //https://forum.unity.com/threads/make-an-editor-expand-to-the-inspector-size.1346489/
            // Register a geometry changed event to update the root size to match the entire inspector.
            _root.RegisterCallback<GeometryChangedEvent, VisualElement>((evt, rootArgs) =>
            {
                // Only do this if the view port is null to save resources.
                if (_contentViewPort == null)
                {
                    // Find the template container.
                    TemplateContainer rootVisualContainer = FindParent<TemplateContainer>(rootArgs);
                    if (rootVisualContainer != null)
                    {
                        // Find the view port element.
                        _contentViewPort = rootVisualContainer.Q<VisualElement>("unity-content-viewport");
                    }
                }

                // The viewport exists.
                if (_contentViewPort != null)
                {
                    // Update the root size to match the entire inspector.
                    rootArgs.style.height = _contentViewPort.resolvedStyle.height - 70;
                }
            }, _root);

            return _root;
        }

        private static T FindParent<T>(VisualElement element, string name = null) where T : VisualElement
        {
            VisualElement parent = element;
            do
            {
                parent = parent.parent;
                if (parent != null && parent.GetType() == typeof(T))
                {
                    if (!string.IsNullOrEmpty(name) && parent.name != name)
                    {
                        continue;
                    }

                    return (T)parent;
                }
            } while (parent != null);

            return null;
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

        private void SetDirtyCallback()
        {
            serializedObject.ApplyModifiedProperties();
            if (target != null)
            {
                EditorUtility.SetDirty(target);
                Undo.RecordObject(target, "change brain");
            }
        }
    }
#endif
}