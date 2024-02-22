using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.ScriptableObjects;
using Mona.SDK.Brains.UIElements;
using Mona.SDK.Core.Body;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

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
                _button.style.width = 25;
                _container.style.borderLeftWidth = 5;
                _container.style.borderLeftColor = Color.HSVToRGB(0f, 1f, .9f);
                _button.style.backgroundColor = Color.white;
            }
            else
            {
                _button.text = "+";
                _button.style.width = 30;
                _container.style.borderLeftWidth = 0;
                _container.style.borderLeftColor = Color.clear;
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
        private MonaBrainGraphVisualElement _brainEditor;
        private TextField _search;
        private Label _header;
        private Label _selectedHeader;

        private Color _darkRed = Color.HSVToRGB(347f / 360f, .66f, .3f);

        public void CreateGUI()
        {
            Texture banner = (Texture)AssetDatabase.LoadAssetAtPath("Packages/com.monaverse.brainssdk/Runtime/Resources/mona.png", typeof(Texture));

            VisualElement root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.backgroundColor = Color.black;
            header.style.width = Length.Percent(100);
            header.style.alignItems = Align.FlexEnd;
            header.style.borderBottomWidth = 5;
            header.style.borderBottomColor = _darkRed;
            header.style.paddingTop = header.style.paddingBottom = 0;

            var brainsTab = new Label();
            brainsTab.style.width = 150f;
            brainsTab.style.borderTopLeftRadius = brainsTab.style.borderTopRightRadius = 5;
            brainsTab.style.backgroundColor = _darkRed;
            brainsTab.style.fontSize = 16;
            brainsTab.style.paddingLeft = 10;
            brainsTab.style.height = 30;
            brainsTab.style.unityTextAlign = TextAnchor.MiddleLeft;
            brainsTab.style.unityFontStyleAndWeight = FontStyle.Bold;
            brainsTab.text = "Brains";
            header.Add(brainsTab);


            var space = new VisualElement();
            space.style.flexGrow = 1;
            header.Add(space);

            var logo = new Image();
            logo.image = banner;
            logo.style.width = 100;
            logo.style.height = 40;
            header.Add(logo);

            root.Add(header);

            var split = new TwoPaneSplitView();
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
                        if (x.Name.ToLower().Contains(evt.newValue.ToLower())) return true;
                        if (x.HasMonaTag(evt.newValue)) return true;
                        return false;
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

            container.Add(searchContainer);

            _listView = new ListView(null, 24, () => new BrainItemVisualElement(this), (elem, i) => BindItem(elem, i));
            _listView.style.backgroundColor = Color.HSVToRGB(347f/360f, .66f, .1f);
            _listView.showAddRemoveFooter = false;
            _listView.style.flexGrow = 1;
            _listView.selectionChanged += (items) =>
            {
                ListSelectionChanged();
            };
            
            container.Add(_listView);

            split.Add(container);

            _brainEditor = new MonaBrainGraphVisualElement(SetDirtyCallback);
            split.Add(_brainEditor);

            root.Add(split);

            Refresh();
            OnSelectionChange();
        }

        private void ListSelectionChanged()
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

            if (_listView.selectedItem != null)
            {
                _brainEditor.SetBrain((MonaBrainGraph)_listView.selectedItem);
                _brainEditor.style.visibility = Visibility.Visible;
            }
            else
            {
                _brainEditor.style.visibility = Visibility.Hidden;
            }
        }

        private void BindItem(VisualElement elem, int i)
        {
            Debug.Log($"{nameof(BindItem)} {i}");
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

            _listView.itemsSource = _items;
            _listView.Rebuild();
        }

        void OnSelectionChange()
        {
            
            Refresh();
        }

        private void SetDirtyCallback()
        {
            //serializedObject.ApplyModifiedProperties();
            if (_target != null)
            {
                EditorUtility.SetDirty(_target);
                Undo.RecordObject(_target, "change brain");
            }
        }
    }
}
#endif