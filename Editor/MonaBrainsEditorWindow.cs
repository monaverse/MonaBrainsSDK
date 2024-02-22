using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.ScriptableObjects;
using Mona.SDK.Brains.UIElements;
using Mona.SDK.Core.Body;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using UnityEditor.UIElements;

#if UNITY_EDITOR

using UnityEditor;

namespace Mona.SDK.Brains.UIEditors
{

    public class BrainItemVisualElement : VisualElement
    {
        private VisualElement _container;
        private Label _label;
        private Button _button;
        private GameObject _gameObject;
        private IMonaBrainRunner _runner;
        private MonaBrainGraph _graph;
        private MonaBrainsEditorWindow _window;

        public BrainItemVisualElement(MonaBrainsEditorWindow window)
        {
            _window = window;
            _container = new VisualElement();
            _container.style.flexDirection = FlexDirection.Row;
            _container.style.flexGrow = 1;
            _container.style.borderBottomWidth = 2;
            _container.style.borderBottomColor = new Color(.1f, .1f, .1f);

            _label = new Label();
            _label.style.unityFontStyleAndWeight = FontStyle.Bold;
            _label.style.height = Length.Percent(100);
            _label.style.unityTextAlign = TextAnchor.MiddleLeft;
            _label.style.flexGrow = 1;

            _button = new Button();
            _button.text = "+";
            _button.style.width = 30;
            _button.style.height = Length.Percent(100);
            _button.style.borderBottomLeftRadius = _button.style.borderTopLeftRadius = 0;
            _button.style.borderTopRightRadius = _button.style.borderBottomRightRadius = 0;
            _button.style.marginLeft = 0;
            _button.style.marginTop = _button.style.marginBottom = 1;
            _button.style.backgroundColor = Color.white;
            _button.style.color = Color.black;
            _button.clicked += () =>
            {
                if (_runner != null && _graph != null && _runner.BrainGraphs.Contains(_graph))
                {
                    _runner.BrainGraphs.Remove(_graph);
                    _window.Refresh();
                }
                else
                {
                    if (_gameObject.GetComponent<IMonaBody>() == null)
                        _gameObject.AddComponent<MonaBody>();

                    if (_gameObject.GetComponent<IMonaBrainRunner>() == null)
                        _gameObject.AddComponent<MonaBrainRunner>();

                    var runner = _gameObject.GetComponent<IMonaBrainRunner>();
                    if (!runner.BrainGraphs.Contains(_graph))
                        runner.BrainGraphs.Add(_graph);
                    _window.Refresh();
                }
            };

            _container.Add(_button);
            _container.Add(_label);
            Add(_container);
            
        }

        public void SetValue(MonaBrainGraph graph, GameObject gameObject, IMonaBrainRunner runner)
        {
            _graph = graph;
            _gameObject = gameObject;
            _runner = runner;
            _label.text = (string.IsNullOrEmpty(graph.Name) ? graph.Name : graph.name);
            _button.SetEnabled(gameObject != null);
            if (runner != null && runner.BrainGraphs.Contains(graph))
            {
                _button.text = "-";
                _button.style.width = 30;
                _button.style.backgroundColor = Color.white;
            }
            else
            {
                _button.text = "+";
                _button.style.width = 30;
                _button.style.backgroundColor = Color.HSVToRGB(347f / 360f, .1f, .9f);
            }
        }

    }

    public class MonaBrainsEditorWindow : EditorWindow
    {
        [MenuItem("Mona/Mona Brains")]
        public static void ShowExample()
        {
            MonaBrainsEditorWindow wnd = GetWindow<MonaBrainsEditorWindow>();
            wnd.titleContent = new GUIContent("Mona Brains Editor");
        }

        private GameObject _target;
        private IMonaBody _body;
        private IMonaBrainRunner _runner;

        private List<MonaBrainGraph> _items = new List<MonaBrainGraph>();

        private ListView _listView;
        private ListView _attachedView;

        private MonaBrainGraphVisualElement _brainEditor;
        private TextField _search;
        private Label _header;
        private Label _selectedHeader;
        private MonaTagsVisualElement _tagsEditor;
        private MonaTags _monaTags;
        private IMonaBrain _brain;

        private Color _darkRed = Color.HSVToRGB(347f / 360f, .66f, .3f);
        private Color _lightRed = Color.HSVToRGB(347f / 360f, .80f, .66f);
        private Color _brightPink = Color.HSVToRGB(351f / 360f, .79f, .98f);

        private Label CreateTab(string text)
        {
            var tab = new Label();
            tab.style.width = 150f;
            tab.style.borderTopLeftRadius = tab.style.borderTopRightRadius = 5;
            tab.style.backgroundColor = _darkRed;
            tab.style.fontSize = 16;
            tab.style.paddingLeft = 10;
            tab.style.height = 30;
            tab.style.marginRight = 5;
            tab.style.unityTextAlign = TextAnchor.MiddleLeft;
            tab.style.unityFontStyleAndWeight = FontStyle.Bold;
            tab.text = text;
            return tab;
        }

        public void CreateGUI()
        {
            Texture banner = (Texture)AssetDatabase.LoadAssetAtPath("Packages/com.monaverse.brainssdk/Runtime/Resources/mona.png", typeof(Texture));

            VisualElement root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow = 1;

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.backgroundColor = Color.black;
            header.style.width = Length.Percent(100);
            header.style.minHeight = 50;
            header.style.alignItems = Align.FlexEnd;
            header.style.borderBottomWidth = 5;
            header.style.borderBottomColor = _darkRed;
            header.style.paddingTop = header.style.paddingBottom = 0;

            var split = new TwoPaneSplitView();
            split.style.flexGrow = 1;

            _tagsEditor = new MonaTagsVisualElement();
            _tagsEditor.style.flexGrow = 1;

            var tagsWarning = new Label();
            tagsWarning.text = "Please Import Starter Sample from Brains SDK Package.";
            tagsWarning.style.display = DisplayStyle.None;

            var brainsTab = CreateTab("Brains");
            var tagsTab = CreateTab("Tags");
            header.Add(brainsTab);
            brainsTab.AddManipulator(new Clickable(() => {
                split.style.display = DisplayStyle.Flex;
                brainsTab.style.backgroundColor = _darkRed;
                _tagsEditor.style.display = DisplayStyle.None;
                tagsTab.style.backgroundColor = _lightRed;
                tagsWarning.style.display = DisplayStyle.None;
            }));

            tagsTab.style.backgroundColor = _lightRed;

            header.Add(tagsTab);
            tagsTab.AddManipulator(new Clickable(() => {
                split.style.display = DisplayStyle.None;
                brainsTab.style.backgroundColor = _lightRed;
                _tagsEditor.style.display = DisplayStyle.Flex;
                tagsTab.style.backgroundColor = _darkRed;

                string[] guids = AssetDatabase.FindAssets("t:MonaTags", null);
                foreach (string guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    _monaTags = (MonaTags)AssetDatabase.LoadAssetAtPath(path, typeof(MonaTags));
                    if (!path.Contains("com.monaverse.brainssdk"))
                    {
                        _tagsEditor.SetMonaTags(_monaTags);
                        tagsWarning.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        tagsWarning.style.display = DisplayStyle.Flex;
                    }
                    break;
                }
            }));

            var space = new VisualElement();
            space.style.flexGrow = 1;
            header.Add(space);

            var logo = new Image();
            logo.image = banner;
            logo.style.width = 100;
            logo.style.height = 40;
            header.Add(logo);

            root.Add(header);

            split.orientation = TwoPaneSplitViewOrientation.Horizontal;
            split.fixedPaneInitialDimension = 200;

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.backgroundColor = _darkRed;

            _header = new Label();
            _header.text = "Select a GameObject";
            _header.style.fontSize = 16;
            _header.style.unityFontStyleAndWeight = FontStyle.Bold;
            _header.style.flexWrap = Wrap.Wrap;
            _header.style.paddingLeft = _header.style.paddingRight = _header.style.paddingTop = 5;

            _selectedHeader = new Label();
            _selectedHeader.text = "";
            _selectedHeader.style.whiteSpace = WhiteSpace.Normal;
            _selectedHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            _selectedHeader.style.paddingLeft = _header.style.paddingRight = 5;

            container.Add(_header);
            container.Add(_selectedHeader);

            var searchContainer = new VisualElement();
            searchContainer.style.flexDirection = FlexDirection.Column;

            var s = searchContainer.style;
            s.borderBottomWidth = s.borderTopWidth = s.borderRightWidth = s.borderBottomWidth = 1;
            s.marginLeft = s.marginTop = s.marginRight = s.marginBottom = 5;

            var searchLabel = new Label("Search Brains");
            searchContainer.Add(searchLabel);

            _search = new TextField();
            _search.RegisterValueChangedCallback((evt) =>
            {
                if (!string.IsNullOrEmpty(evt.newValue))
                {
                    var items = _items.FindAll(x =>
                    {
                        if (_runner != null && _runner.BrainGraphs.Contains(x)) return true;
                        if (x.Name.ToLower().Contains(evt.newValue.ToLower())) return true;
                        if (x.HasMonaTag(evt.newValue)) return true;
                        return false;
                    });

                    items.Sort((a, b) =>
                    {
                        if (_runner != null)
                        {
                            if (_runner.BrainGraphs.Contains(a)) return -1;
                            if (_runner.BrainGraphs.Contains(b)) return 1;
                        }
                        return a.Name.CompareTo(b.Name);
                    });

                    _listView.itemsSource = items;
                    _listView.Rebuild();
                }
                else
                {
                    _listView.itemsSource = _items;
                    _listView.Rebuild();
                }
            });
            _search[0].style.backgroundColor = Color.black;
            _search.style.color = Color.white;
            searchContainer.Add(_search);

            _attachedView = new ListView(null, 24, () => new BrainItemVisualElement(this), (elem, i) => BindItemAttached(elem, i));
            _attachedView.style.backgroundColor = Color.HSVToRGB(347f / 360f, .66f, .1f);
            _attachedView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _attachedView.showAddRemoveFooter = false;
            _attachedView.style.borderBottomColor = _lightRed;
            _attachedView.style.borderBottomWidth = 5;
            _attachedView.selectionChanged += (items) =>
            {
                if(_attachedView.selectedIndex > 0)
                    _listView.selectedIndex = -1;
                ListSelectionChangedAttached();
            };

            container.Add(_attachedView);

            container.Add(searchContainer);

            _listView = new ListView(null, 24, () => new BrainItemVisualElement(this), (elem, i) => BindItem(elem, i));
            _listView.style.backgroundColor = Color.HSVToRGB(347f/360f, .66f, .1f);
            _listView.showAddRemoveFooter = false;
            _listView.style.flexGrow = 1;
            _listView.selectionChanged += (items) =>
            {
                if (_listView.selectedIndex > 0)
                    _attachedView.selectedIndex = -1;
                ListSelectionChanged();
            };
            
            container.Add(_listView);

            split.Add(container);

            _brainEditor = new MonaBrainGraphVisualElement(SetDirtyCallback);
            split.Add(_brainEditor);

            root.Add(split);
            root.Add(_tagsEditor);
            _tagsEditor.style.display = DisplayStyle.None;

            root.Add(tagsWarning);

            Refresh();
            OnSelectionChange();
        }

        private void ListSelectionChanged()
        {
            UpdateSelectedObject();

            if (_listView.selectedItem != null)
            {
                _brain = (MonaBrainGraph)_listView.selectedItem;
                _brainEditor.SetBrain(_brain);
                _brainEditor.style.visibility = Visibility.Visible;
            }
            else
            {
                _brainEditor.style.visibility = Visibility.Hidden;
            }
        }

        private void ListSelectionChangedAttached()
        {
            UpdateSelectedObject();

            if (_attachedView.selectedItem != null)
            {
                _brainEditor.SetBrain((MonaBrainGraph)_attachedView.selectedItem);
                _brainEditor.style.visibility = Visibility.Visible;
            }
            else
            {
                _brainEditor.style.visibility = Visibility.Hidden;
            }
        }

        private void UpdateSelectedObject()
        {
            _target = Selection.activeGameObject;
            if (_target != null)
            {
                _body = _target.GetComponent<IMonaBody>();
                _runner = _target.GetComponent<IMonaBrainRunner>();
                if (_runner != null)
                    _selectedHeader.text = $"{_runner.BrainGraphs.Count} brains attached.";
                else
                    _selectedHeader.text = $"no brains attached.";

                _header.text = $"{_target.name}";
                _brainEditor.style.visibility = Visibility.Visible;
            }
            else
            {
                _header.text = "Select a GameObject";
                _selectedHeader.text = "";
                _brainEditor.style.visibility = Visibility.Hidden;
            }
        }

        private void BindItemAttached(VisualElement elem, int i)
        {
            Debug.Log($"{nameof(BindItemAttached)} {i}");
            ((BrainItemVisualElement)elem).SetValue((MonaBrainGraph)_attachedView.itemsSource[i], _target, _runner);
        }

        private void BindItem(VisualElement elem, int i)
        {
            //Debug.Log($"{nameof(BindItem)} {i}");
            ((BrainItemVisualElement)elem).SetValue((MonaBrainGraph)_listView.itemsSource[i], _target, _runner);
        }

        private void OnHierarchyChange()
        {
            Refresh();
        }

        private void OnDisable()
        {

            _listView.itemsSource = null;
            _listView.Rebuild();
        }

        public void Refresh()
        {
            ListSelectionChanged();
            ListSelectionChangedAttached();
            _items.Clear();

            string[] guids = AssetDatabase.FindAssets("t:MonaBrainGraph", null);
            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var graph = (MonaBrainGraph)AssetDatabase.LoadAssetAtPath(path, typeof(MonaBrainGraph));
                var fileName = Path.GetFileName(path);
                graph.name = fileName.Substring(0, fileName.LastIndexOf("."));
                if(string.IsNullOrEmpty(graph.Name))
                    graph.Name = graph.name;
                _items.Add(graph);
            }

            _items.Sort((a, b) =>
            {
                return a.Name.CompareTo(b.Name);
            });

            var items = _items.FindAll(x => _runner != null && _runner.BrainGraphs.Contains(x));

            var nonAttached = _items.FindAll(x => _runner == null || !_runner.BrainGraphs.Contains(x));
            
            _attachedView.itemsSource = items;
            _attachedView.Rebuild();

            _listView.itemsSource = nonAttached;
            _listView.Rebuild();
        }

        void OnSelectionChange()
        {
            
            Refresh();
        }

        private void HandleCallback(SerializedObject obj)
        {
            obj.ApplyModifiedProperties();
            if (_monaTags != null)
            {
                EditorUtility.SetDirty(_monaTags);
                Undo.RecordObject(_monaTags, "change tags");
            }
        }

        private void HandleBrainCallback(SerializedObject obj)
        {
            //serializedObject.ApplyModifiedProperties();
            if (_brain != null)
            {
                EditorUtility.SetDirty((MonaBrainGraph)_brain);
                Undo.RecordObject((MonaBrainGraph)_brain, "change brain");
            }
        }

        private void SetDirtyCallback()
        {
            //serializedObject.ApplyModifiedProperties();
            if (_brain != null)
            {
                EditorUtility.SetDirty((MonaBrainGraph)_brain);
                Undo.RecordObject((MonaBrainGraph)_brain, "change brain");
            }
        }
    }
}
#endif