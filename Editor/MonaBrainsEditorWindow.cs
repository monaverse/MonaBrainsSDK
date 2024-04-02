using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.ScriptableObjects;
using Mona.SDK.Brains.UIElements;
using Mona.SDK.Core.Body;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif
using System;

#if UNITY_EDITOR

using UnityEditor;
using Mona.SDK.Brains.Core.Control;

namespace Mona.SDK.Brains.UIEditors
{

    public class BrainItemVisualElement : VisualElement
    {
        private VisualElement _container;
        private Label _label;
        private Button _button;
        private GameObject _gameObject;
        private IMonaBrainRunner _runner;
        private MonaGlobalBrainRunner _globalRunner;
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
            _button.style.backgroundColor = Color.HSVToRGB(347f / 360f, .1f, .9f);
            _button.style.color = Color.black;
            _button.clicked += () =>
            {
                if (_globalRunner != null && _graph != null && _globalRunner.PlayerBrainGraphs.Contains(_graph))
                {
                    _globalRunner.PlayerBrainGraphs.Remove(_graph);
                    _window.Refresh();
                }
                else if (_runner != null && _graph != null && _runner.BrainGraphs.Contains(_graph))
                {
                    _runner.BrainGraphs.Remove(_graph);
                    _window.Refresh(_graph);
                }
                else
                {
                    if (_globalRunner != null)
                    {
                        if (!_globalRunner.PlayerBrainGraphs.Contains(_graph))
                            _globalRunner.PlayerBrainGraphs.Add(_graph);
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
                    }
                    _window.Refresh(_graph);
                }
            };

            _container.Add(_button);
            _container.Add(_label);
            Add(_container);
            
        }

        public void SetValue(MonaBrainGraph graph, GameObject gameObject, IMonaBrainRunner runner, MonaGlobalBrainRunner globalRunner)
        {
            _graph = graph;
            _gameObject = gameObject;
            _runner = runner;

            _globalRunner = globalRunner;
            _label.text = (string.IsNullOrEmpty(graph.Name) ? graph.Name : graph.name);
            _button.SetEnabled(gameObject != null);
            if ((runner != null && runner.BrainGraphs.Contains(graph)) || (_globalRunner != null && _globalRunner.PlayerBrainGraphs.Contains(graph)))
            {
                _button.text = "-";
                _button.style.width = 30;
            }
            else
            {
                _button.text = "+";
                _button.style.width = 30;
            }
        }

    }

    public class MonaBrainNameVisualElement : VisualElement
    {
        public event Action<string> OnSubmit = delegate { };

        private VisualElement _container;
        private Label _prompt;
        private TextField _brainName;
        private Button _button;

        public MonaBrainNameVisualElement()
        {
            style.flexDirection = FlexDirection.Column;
            style.flexGrow = 1;
            style.alignItems = Align.Center;

            _container = new VisualElement();
            _container.style.marginTop = 100;
            _container.style.width = Length.Percent(50);
            _container.style.height = 100;

            _prompt = new Label();
            _prompt.text = "Name your new brain!";
            _container.Add(_prompt);

            _brainName = new TextField();
            SetBorder(_brainName, 3, 1, Color.white, 2);
            _brainName.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return)
                    OnSubmit?.Invoke(GetName());
            });
            _container.Add(_brainName);

            _button = new Button();
            _button.text = "Save";
            _button.style.color = Color.black;
            _button.style.backgroundColor = Color.white;
            _button.style.width = 100;
            _button.RegisterCallback<ClickEvent>((evt) =>
            {
                OnSubmit?.Invoke(GetName());
            });
            _container.Add(_button);
            Add(_container);
        }

        public string GetName()
        {
            return _brainName.value;
        }

        private void SetBorder(VisualElement elem, float radius = 0, float width = 1, Color color = default, float padding = 5)
        {
            elem.style.borderTopLeftRadius = elem.style.borderTopRightRadius = radius;
            elem.style.borderBottomLeftRadius = elem.style.borderBottomRightRadius = radius;
            elem.style.borderLeftWidth = elem.style.borderTopWidth = elem.style.borderRightWidth = elem.style.borderBottomWidth = width;
            elem.style.borderLeftColor = elem.style.borderRightColor = elem.style.borderTopColor = elem.style.borderBottomColor = color;
            elem.style.paddingLeft = elem.style.paddingRight = elem.style.paddingTop = elem.style.paddingBottom = padding;
        }

    }

    public class MonaBrainsEditorWindow : EditorWindow
    {
        [MenuItem("Mona/Mona Brains")]
        public static void ShowWindow()
        {
            MonaBrainsEditorWindow wnd = GetWindow<MonaBrainsEditorWindow>();
            wnd.titleContent = new GUIContent("Mona Brains Editor");
            wnd.Show();
        }

        private GameObject _target;
        private IMonaBody _body;
        private IMonaBrainRunner _runner;
        private MonaGlobalBrainRunner _globalRunner;

        private List<MonaBrainGraph> _items = new List<MonaBrainGraph>();

        private Label _status;
        private ListView _listView;
        private ListView _attachedView;

        private MonaBrainGraphVisualElement _brainEditor;
        private MonaBrainNameVisualElement _brainNewContainer;
        private TextField _brainNewName;
        private TextField _search;
        private Label _header;
        private Label _selectedHeader;
        private MonaTagsVisualElement _tagsEditor;
        private MonaTags _monaTags;
        private IMonaBrain _brain;

        private Color _darkRed = Color.HSVToRGB(347f / 360f, .66f, .3f);
        private Color _darkerRed = Color.HSVToRGB(347f / 360f, .66f, .2f);
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

        private void SetBorder(VisualElement elem, float radius = 0, float width = 1, Color color = default, float padding = 5)
        {
            elem.style.borderTopLeftRadius = elem.style.borderTopRightRadius = radius;
            elem.style.borderBottomLeftRadius = elem.style.borderBottomRightRadius = radius;
            elem.style.borderLeftWidth = elem.style.borderTopWidth = elem.style.borderRightWidth = elem.style.borderBottomWidth = width;
            elem.style.borderLeftColor = elem.style.borderRightColor = elem.style.borderTopColor = elem.style.borderBottomColor = color;
            elem.style.paddingLeft = elem.style.paddingRight = elem.style.paddingTop = elem.style.paddingBottom = padding;
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
                tagsTab.style.backgroundColor = _darkerRed;
                tagsWarning.style.display = DisplayStyle.None;
            }));

            tagsTab.style.backgroundColor = _lightRed;

            header.Add(tagsTab);
            tagsTab.AddManipulator(new Clickable(() => {
                split.style.display = DisplayStyle.None;
                brainsTab.style.backgroundColor = _darkerRed;
                _tagsEditor.style.display = DisplayStyle.Flex;
                tagsTab.style.backgroundColor = _darkRed;

                if (_brain != null)
                {
                    _monaTags = (MonaTags)_brain.MonaTagSource;
                    _tagsEditor.SetMonaTags(_monaTags);
                    tagsWarning.style.display = DisplayStyle.Flex;
                }
                else
                {
                    string[] guids = AssetDatabase.FindAssets("t:MonaTags", null);
                    foreach (string guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        if (!path.Contains("com.monaverse.brainssdk"))
                        {
                            _monaTags = (MonaTags)AssetDatabase.LoadAssetAtPath(path, typeof(MonaTags));
                            _tagsEditor.SetMonaTags(_monaTags);
                            tagsWarning.style.display = DisplayStyle.None;
                        }
                        else
                        {
                            tagsWarning.style.display = DisplayStyle.Flex;
                        }
                    }
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
            _header.style.whiteSpace = WhiteSpace.Normal;
            _header.style.unityFontStyleAndWeight = FontStyle.Bold;
            _header.style.flexWrap = Wrap.Wrap;
            _header.style.paddingLeft = _header.style.paddingRight = _header.style.paddingTop = 10;

            _selectedHeader = new Label();
            _selectedHeader.text = "";
            _selectedHeader.style.whiteSpace = WhiteSpace.Normal;
            _selectedHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            _selectedHeader.style.paddingLeft = _header.style.paddingRight = 10;

            container.Add(_header);
            container.Add(_selectedHeader);

            var searchContainer = new VisualElement();
            searchContainer.style.flexDirection = FlexDirection.Column;

            var s = searchContainer.style;
            s.borderBottomWidth = s.borderTopWidth = s.borderRightWidth = s.borderBottomWidth = 1;
            s.marginLeft = s.marginTop = s.marginRight = s.marginBottom = 5;

            var searchLabel = new Label("Search Local Brains");
            searchLabel.style.fontSize = 13;
            searchLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            searchContainer.Add(searchLabel);

            _search = new TextField();
            SetBorder(_search, 3, 1, Color.white, 2);
            _search.RegisterValueChangedCallback((evt) =>
            {
                if (!string.IsNullOrEmpty(evt.newValue))
                {
                    var items = _items.FindAll(x =>
                    {
                        if (_globalRunner != null && _globalRunner.PlayerBrainGraphs.Contains(x)) return true;
                        if (_runner != null && _runner.BrainGraphs.Contains(x)) return true;
                        if (x.Name.ToLower().Contains(evt.newValue.ToLower())) return true;
                        if (x.HasMonaTag(evt.newValue)) return true;
                        return false;
                    });

                    items.Sort((a, b) =>
                    {
                        if(_globalRunner != null)
                        {
                            if (_globalRunner.PlayerBrainGraphs.Contains(a)) return -1;
                            if (_globalRunner.PlayerBrainGraphs.Contains(b)) return 1;
                        }
                        if (_runner != null)
                        {
                            if (_runner.BrainGraphs.Contains(a)) return -1;
                            if (_runner.BrainGraphs.Contains(b)) return 1;
                        }
                        return a.Name.CompareTo(b.Name);
                    });

                    _listView.itemsSource = items;
                    _listView.Rebuild();
                    _status.text = $"({items.Count}) Brain{(items.Count > 1 ? "s" : "")} Found";
                }
                else
                {
                    _listView.itemsSource = _items;
                    _listView.Rebuild();
                    _status.text = $"({_items.Count}) Brain{(_items.Count > 1 ? "s" : "")} Found";
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
            _attachedView.style.marginTop = 10;
            _attachedView.style.minHeight = 100;
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

#if UNITY_EDITOR
            var bottomToolBar = new Toolbar();
            bottomToolBar.style.alignContent = Align.FlexEnd;
            container.Add(bottomToolBar);

            var newButton = new ToolbarButton();
            newButton.text = "New Brain";
            newButton.style.color = Color.black;
            newButton.style.backgroundColor = Color.white;
            newButton.clicked += () =>
            {
                _brainNewContainer.style.display = DisplayStyle.Flex;
                _brainEditor.style.display = DisplayStyle.None;
            };

            _status = new Label();
            _status.text = "No Brains Found";
            _status.style.unityTextAlign = TextAnchor.MiddleLeft;
            _status.style.unityFontStyleAndWeight = FontStyle.Italic;
            _status.style.flexGrow = 1;
            _status.style.paddingLeft = 5;

            bottomToolBar.Add(_status);
            bottomToolBar.Add(newButton);
#endif

            split.Add(container);

            var brainContainer = new VisualElement();
            brainContainer.style.flexDirection = FlexDirection.Column;

            _brainEditor = new MonaBrainGraphVisualElement(SetDirtyCallback);
            brainContainer.Add(_brainEditor);

            _brainNewContainer = new MonaBrainNameVisualElement();
            _brainNewContainer.OnSubmit += (string newName) =>
            {
                if (string.IsNullOrEmpty(newName)) return;
                var brain = ScriptableObject.CreateInstance<MonaBrainGraph>();
                    brain.CorePage.Instructions.Add(new Instruction());
                    brain.name = newName;

                if (!AssetDatabase.IsValidFolder("Assets/Brains"))
                    AssetDatabase.CreateFolder("Assets", "Brains");

                AssetDatabase.CreateAsset(brain, "Assets/Brains/" + brain.name + ".asset");
                AssetDatabase.SaveAssets();

                if (_target != null)
                {
                    if (_globalRunner != null)
                    {
                        if (!_globalRunner.PlayerBrainGraphs.Contains(brain))
                            _globalRunner.PlayerBrainGraphs.Add(brain);

                        Refresh(brain);
                    }
                    else
                    {
                        if (_target.GetComponent<IMonaBody>() == null)
                            _target.AddComponent<MonaBody>();

                        if (_target.GetComponent<IMonaBrainRunner>() == null)
                            _target.AddComponent<MonaBrainRunner>();

                        var runner = _target.GetComponent<IMonaBrainRunner>();
                        if (!runner.BrainGraphs.Contains(brain))
                            runner.BrainGraphs.Add(brain);

                        Refresh(brain);
                    }
                }

                _brainNewContainer.style.display = DisplayStyle.None;
                _brainEditor.style.display = DisplayStyle.Flex;

            };
            brainContainer.Add(_brainNewContainer);
            _brainNewContainer.style.display = DisplayStyle.None;

            split.Add(brainContainer);

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

            if (_listView == null)
                return;

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

            if (_attachedView == null)
                return;

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
            var newTarget = Selection.activeGameObject;
            if (newTarget != null)
            {
                _target = newTarget;
                _body = _target.GetComponent<IMonaBody>();
                _globalRunner = _target.GetComponent<MonaGlobalBrainRunner>();
                _runner = _target.GetComponent<IMonaBrainRunner>();
                if (_selectedHeader != null)
                {
                    if (_runner != null)
                        _selectedHeader.text = $"{_runner.BrainGraphs.Count} brains attached.";
                    else
                        _selectedHeader.text = $"no brains attached.";
                }
                _header.text = $"{_target.name}";
                _brainEditor.style.visibility = Visibility.Visible;

            }
        }

        private void BindItemAttached(VisualElement elem, int i)
        {
            //Debug.Log($"{nameof(BindItemAttached)} {i}");
            ((BrainItemVisualElement)elem).SetValue((MonaBrainGraph)_attachedView.itemsSource[i], _target, _runner, _globalRunner);
        }

        private void BindItem(VisualElement elem, int i)
        {
            //Debug.Log($"{nameof(BindItem)} {i}");
            ((BrainItemVisualElement)elem).SetValue((MonaBrainGraph)_listView.itemsSource[i], _target, _runner, _globalRunner);
        }

        private void OnHierarchyChange()
        {
            Refresh();
        }

        private void OnDisable()
        {
            if (_listView == null) return;
            _listView.itemsSource = null;
            _listView.Rebuild();
        }

        public void Refresh(MonaBrainGraph lastAdded = null)
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

            var items = _items.FindAll(x => (_runner != null && _runner.BrainGraphs.Contains(x)) || (_globalRunner != null && _globalRunner.PlayerBrainGraphs.Contains(x)));

            var nonAttached = _items.FindAll(x => (_runner == null || !_runner.BrainGraphs.Contains(x) || _globalRunner == null || !_globalRunner.PlayerBrainGraphs.Contains(x)));
            
            _attachedView.itemsSource = items;
            _attachedView.Rebuild();

            if(lastAdded != null && items.Contains(lastAdded))
            {
                _attachedView.SetSelection(items.IndexOf(lastAdded));
            }

            ListSelectionChanged();
            ListSelectionChangedAttached();

            _listView.itemsSource = nonAttached;
            _listView.Rebuild();
            _status.text = $"({nonAttached.Count}) Brain{(nonAttached.Count > 1 ? "s" : "")} Found";
        }

        void OnSelectionChange()
        {            
            Refresh();

            rootVisualElement.schedule.Execute(() =>
            {
                if (_attachedView.itemsSource != null && _attachedView.itemsSource.Count > 0)
                {
                    _attachedView.selectedIndex = -1;
                    _attachedView.selectedIndex = 0;
                }
            }).ExecuteLater(200);
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