using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.ScriptableObjects;
using Mona.SDK.Core.State.UIElements;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.UIElements
{
    public class MonaBrainGraphVisualElement : VisualElement, IDisposable
    {
        private IMonaBrain _brain;

        private int _selectedTab = 0;

        private TextField _name;
        private EnumField _property;

        private IInstructionTileSet _tileSet;
        private IMonaTags _monaTags;

        private VisualElement _root;
        private MonaBrainPageVisualElement _corePage;
        private VisualElement _tabToolbar;
        private VisualElement _tabs;
        private Button _btnNewPage;
        private MonaBrainPageVisualElement _activeStatePage;
        private Foldout _activePageHeading;
        private Foldout _activePagesHeading;
        private Foldout _defaultValuesHeading;
        private Foldout _corePageContainer;
        private TextField _activePageName;
        private VisualElement _activePageContainer;
        private ListView _monaTagListView;

        private Button _btnDeletePage;

        private Foldout _foldOut;
        private DropdownField _tileSetField;
        private ObjectField _monaTagsField;

        private MonaStateVisualElement _defaultStateVisualElement;

        private Foldout CreateHeading(string text)
        {
            var foldOut = new Foldout();
            foldOut.style.marginLeft = 0;
            foldOut.style.marginTop = 3;
            foldOut.text = text;
            foldOut.RegisterCallback<GeometryChangedEvent>((evt) =>
            {
                var content = foldOut.Q<VisualElement>(className: "unity-foldout__toggle");
                if (content != null)
                {
                    content.style.fontSize = 14;
                    content.style.unityFontStyleAndWeight = FontStyle.Bold;
                    content.style.backgroundColor = Color.HSVToRGB(.48f, .4f, .4f);
                    content.style.paddingBottom = content.style.paddingTop = 3;
                    content.style.borderBottomLeftRadius = content.style.borderBottomRightRadius = content.style.borderTopLeftRadius = content.style.borderTopRightRadius = 3;
                }
                content = foldOut.Q<VisualElement>(className: "unity-foldout__content");
                if(content != null)
                    content.style.marginLeft = 0;
            });
            foldOut.value = false;
            return foldOut;
        }

        private void SetBorder(VisualElement elem, float radius = 0, float width = 1, Color color = default, float padding = 5)
        {
            elem.style.borderTopLeftRadius = _activePageContainer.style.borderTopRightRadius = radius;
            elem.style.borderBottomLeftRadius = _activePageContainer.style.borderBottomRightRadius = radius;
            elem.style.borderLeftWidth = elem.style.borderTopWidth = elem.style.borderRightWidth = elem.style.borderBottomWidth = width;
            elem.style.borderLeftColor = elem.style.borderRightColor = elem.style.borderTopColor = elem.style.borderBottomColor = color;
            elem.style.paddingLeft = elem.style.paddingRight = elem.style.paddingTop = elem.style.paddingBottom = padding;
        }

        private void SetMargin(VisualElement elm, float margin = 10)
        {
            _activePageContainer.style.marginLeft = _activePageContainer.style.marginRight = margin;
            _activePageContainer.style.marginTop = _activePageContainer.style.marginBottom = margin;
        }

        public MonaBrainGraphVisualElement()
        {
            _root = new VisualElement();
            _root.style.flexDirection = FlexDirection.Column;

            _name = new TextField("Brain Name");
            _name.RegisterValueChangedCallback((evt) => _brain.Name = (string)evt.newValue);
            _root.Add(_name);
            _property = new EnumField("Property", MonaBrainPropertyType.Default);
            _property.RegisterValueChangedCallback((evt) => _brain.PropertyType = (MonaBrainPropertyType)evt.newValue);
            _root.Add(_property);

            _monaTagListView = new ListView(null, 120, () => new MonaTagReferenceVisualElement(_brain), (elem, i) => ((MonaTagReferenceVisualElement)elem).SetValue(i, _brain.MonaTags[i]));
            _monaTagListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _monaTagListView.showFoldoutHeader = true;
            _monaTagListView.headerTitle = "Mona Tags";
            _monaTagListView.showAddRemoveFooter = true;
            _monaTagListView.reorderMode = ListViewReorderMode.Animated;
            _monaTagListView.reorderable = true;
            _monaTagListView.itemsAdded += (elems) =>
            {
                foreach (var e in elems)
                {
                    _brain.MonaTags[e] = _brain.MonaTagSource.Tags[0];
                }
            };
            _root.Add(_monaTagListView);

            _corePageContainer = CreateHeading("Brain Core Page Instructions");
            _corePageContainer.value = true;
            _root.Add(_corePageContainer);

            _corePage = new MonaBrainPageVisualElement();
            _corePageContainer.Add(_corePage);

            _activePagesHeading = CreateHeading("All Brain State Pages");
            _root.Add(_activePagesHeading);

            _tabToolbar = new VisualElement();
            _tabToolbar.style.width = Length.Percent(100);
            _tabToolbar.style.marginTop = 5;
            _tabToolbar.style.flexDirection = FlexDirection.Row;
            _activePagesHeading.Add(_tabToolbar);

            _activePageContainer = new VisualElement();
            _activePageContainer.style.backgroundColor = new Color(.3f, .3f, .3f);
            SetMargin(_activePageContainer, 5);
            _activePageContainer.style.marginTop = 10;
            _activePageContainer.style.flexDirection = FlexDirection.Column;
            SetBorder(_activePageContainer, 10, 1, Color.gray, 10);
            _activePageContainer.style.paddingTop = 5;
            _activePagesHeading.Add(_activePageContainer);

            _activePageHeading = CreateHeading($"Active Page");
            _activePageHeading.style.fontSize = 12;
            SetBorder(_activePageHeading, 4, 1, new Color(.1f, .1f, .1f), 2);
            SetMargin(_activePageHeading, 0);
            _activePageHeading.style.marginBottom = 5;
            _activePageContainer.Add(_activePageHeading);


            _tabs = new VisualElement();
            _tabs.style.width = Length.Percent(100);
            _tabs.style.flexDirection = FlexDirection.Row;
            _tabs.style.backgroundColor = new Color(.1f, .1f, .1f);
            _tabs.style.borderBottomColor = Color.black;
            _tabs.style.borderBottomWidth = 1;
            _tabs.style.flexWrap = Wrap.Wrap;
            _tabToolbar.Add(_tabs);

            _activePageName = new TextField("Active Page Name");
            _activePageName.style.width = Length.Percent(100);
            _activePageName.RegisterValueChangedCallback((evt) => {
                _brain.StatePages[_selectedTab].Name = evt.newValue;
            });
            _activePageName.RegisterCallback<BlurEvent>((evt) =>
            {
                Refresh();
            });
            _activePageContainer.Add(_activePageName);

            _btnNewPage = new Button();
            _btnNewPage.text = "Create State Page";
            _btnNewPage.style.height = 20;
            _btnNewPage.clicked += () =>
            {
                AddStatePage();
            };
            _tabToolbar.Add(_btnNewPage);

            _activeStatePage = new MonaBrainPageVisualElement();
            _activePageContainer.Add(_activeStatePage);

            _btnDeletePage = new Button();
            _btnDeletePage.style.width = 100;
            _btnDeletePage.style.backgroundColor = new Color(.6f, 0, 0);
            _btnDeletePage.text = "Delete Page";
            SetBorder(_btnDeletePage, 3, 1, Color.red, 0);
            _btnDeletePage.clicked += () =>
            {
                DeleteStatePage();
            };
            _activePageContainer.Add(_btnDeletePage);
            Add(_root);


            _defaultValuesHeading = CreateHeading("Brain Default Values");
            _root.Add(_defaultValuesHeading);

            _defaultStateVisualElement = new MonaStateVisualElement();
            _defaultValuesHeading.Add(_defaultStateVisualElement);

#if UNITY_EDITOR

            _foldOut = new Foldout();
            _foldOut.text = "Brain Settings";
            _tileSetField = new DropdownField("Latest Version");
            _tileSetField.RegisterValueChangedCallback((changed) =>
            {
                var value = (string)changed.newValue;
                var versions = GetTileSets();
                var tileSet = versions.Find(x => x.Version == value);
                if (tileSet == null)
                    _brain.TileSet = versions[0];
                else
                    _brain.TileSet = tileSet;
                Debug.Log($"Tileset value changed {value}");
            });
            _foldOut.Add(_tileSetField);

            _monaTagsField = new ObjectField("Mona Tags");
            _monaTagsField.objectType = typeof(MonaTags);
            _monaTagsField.RegisterValueChangedCallback((changed) =>
            {
                _brain.MonaTagSource = (MonaTags)changed.newValue;
                Refresh();
            });
            _foldOut.Add(_monaTagsField);
            Add(_foldOut);
#endif
        }

        public void Dispose()
        {
            _corePage.Dispose();
            for (var i = 0; i < _activePageContainer.childCount; i++)
            {
                var child = _activePageContainer.ElementAt(i);
                if(child is IDisposable)
                    ((IDisposable)child).Dispose();
            }
        }

        private void AddStatePage()
        {
            var page = new MonaBrainPage($"State{_brain.StatePages.Count}");
            page.AddInstruction(new Instruction());
            _brain.StatePages.Add(page);
            _selectedTab = _brain.StatePages.Count - 1;
            Refresh();
        }

        private void DeleteStatePage()
        {
            _brain.StatePages.RemoveAt(_selectedTab);
            if (_selectedTab >= _brain.StatePages.Count)
                _selectedTab = _brain.StatePages.Count - 1;
            Refresh();
        }

        private void Refresh()
        {
            if (_brain.MonaTagSource == null || _brain.TileSet == null)
            {
                _root.style.display = DisplayStyle.None;
                _foldOut.value = true;
                return;
            }

            _monaTagsField.value = (MonaTags)_brain.MonaTagSource;
            _tileSetField.value = ((InstructionTileSet)_brain.TileSet) != null ? ((InstructionTileSet)_brain.TileSet).Version : "Select a Tileset";

            _root.style.display = DisplayStyle.Flex;
            _corePage.SetPage(_brain, _brain.CorePage);

            _tabs.Clear();
            for(var i = 0;i < _brain.StatePages.Count; i++)
            {
                var statePage = _brain.StatePages[i];
                var button = new Button();
                button.text = statePage.Name;
                button.tabIndex = i;
                button.clicked += () =>
                {
                    _selectedTab = button.tabIndex;
                    Refresh();
                };
                button.style.backgroundColor = (i == _selectedTab) ? new Color(108f / 255f, 173f / 255f, 201f / 255f) : Color.gray;
                _tabs.Add(button);
            }

            if (_selectedTab > _tabs.childCount)
                _selectedTab = 0;

            _activePagesHeading.text = $"Brain State Pages - {_brain.StatePages.Count}";

            if (_brain.StatePages.Count == 0)
            {
                _activePagesHeading.value = false;
                _activePageHeading.style.display = DisplayStyle.None;
                _activePageContainer.style.display = DisplayStyle.None;
                return;
            }
            else
            {
                _activePagesHeading.value = true;
            }

            _activePageHeading.style.display = DisplayStyle.Flex;
            _activePageContainer.style.display = DisplayStyle.Flex;

            _activePageName.value = _brain.StatePages[_selectedTab].Name;
            _activePageHeading.text = $"Active Page - {_brain.StatePages[_selectedTab].Name}";
            _activeStatePage.SetPage(_brain, _brain.StatePages[_selectedTab]); 
            _activeStatePage.visible = true;

        }

        public void SetBrain(IMonaBrain brain)
        {
            _brain = brain;

            _name.value = _brain.Name;
            _property.value = (MonaBrainPropertyType)_brain.PropertyType;

            if (_brain.MonaTagSource == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:MonaTags", null);
                foreach (string guid in guids)
                {
                    _brain.MonaTagSource = (MonaTags)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(MonaTags));
                    _monaTagsField.value = (MonaTags)_brain.MonaTagSource;
                    break;
                }
            }
            
            var versions = GetTileSets();
            _tileSetField.choices = versions.ConvertAll<string>(x => x.Version);
            _tileSetField.choices.Insert(0, "Latest Version");

            _defaultStateVisualElement.SetState(_brain.DefaultState);

            if (_brain.TileSet == null)
                _brain.TileSet = versions[0];

            if (_brain.MonaTags.Count == 0)
                _brain.MonaTags.Add(_brain.MonaTagSource.Tags[0]);
            _monaTagListView.itemsSource = _brain.MonaTags;
            _monaTagListView.Q<Foldout>().value = false;
            _monaTagListView.Rebuild();


            Refresh();
        }

        private List<InstructionTileSet> GetTileSets()
        {
            string[] guids = AssetDatabase.FindAssets("t:InstructionTileSet", null);
            var versions = new List<InstructionTileSet>();
            foreach (string guid in guids)
            {
                var asset = (InstructionTileSet)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(InstructionTileSet));
                versions.Add(asset);
            }
            versions.Sort((a, b) => -a.Version.CompareTo(b.Version));
            return versions;
        }
    }
}