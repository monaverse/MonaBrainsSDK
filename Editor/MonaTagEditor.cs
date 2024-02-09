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

public class MonaTagsVisualElement : VisualElement
{
    private IMonaTags _monaTags;
    private ListView _monaTagListView;

    public MonaTagsVisualElement()
    {
        _monaTagListView = new ListView(null, 20, () => new MonaTagEditItemVisualElement(_monaTags), (elem, i) => ((MonaTagEditItemVisualElement)elem).SetValue(i, _monaTags.AllTags[i]));
        _monaTagListView.showFoldoutHeader = true;
        _monaTagListView.headerTitle = "Mona Tags";
        _monaTagListView.showAddRemoveFooter = true;
        _monaTagListView.reorderMode = ListViewReorderMode.Animated;
        _monaTagListView.reorderable = true;
        _monaTagListView.itemsAdded += (elems) =>
        {
            foreach (var e in elems)
            {
                _monaTags.AllTags[e] = (IMonaTagItem)new MonaTagItem("Default", false, true);
            }
        };
        Add(_monaTagListView);
    }

    public void SetMonaTags(IMonaTags monaTags)
    {
        _monaTags = monaTags;
        _monaTagListView.itemsSource = _monaTags.AllTags;
        _monaTagListView.Rebuild();
    }

}

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