using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.ScriptableObjects;
using Mona.SDK.Core.State.UIElements;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Core.Assets.Interfaces;
using Mona.SDK.Core;
using Mona.SDK.Brains.Core;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif


namespace Mona.SDK.Brains.UIElements
{
    public class MonaBrainGraphVisualElement : VisualElement, IDisposable
    {
        private IMonaBrain _brain;
        private Action callback;

        private int _selectedTab = 0;

        private TextField _name;
        private EnumField _property;

        private IInstructionTileSet _tileSet;
        private IMonaTags _monaTags;

        private ScrollView _leftColumn;
        private VisualElement _rightColumn;
        private Foldout _brainMetaData;
        private MonaBrainPageVisualElement _corePage;
        private VisualElement _tabToolbar;
        private VisualElement _tabs;
        private Button _btnNewPage;
        private MonaBrainPageVisualElement _activeStatePage;
        private Label _statePageHeading;
        private Foldout _activePagesHeading;
        private Foldout _defaultVariablesHeading;
        private Foldout _corePageContainer;
        private TextField _activePageName;
        private Label _statePageInstructions;
        private VisualElement _activePageContainer;
        private ListView _monaTagListView;
        private ListView _monaAssetsListView;
        private Toggle _toggleAllowLogging;
        private Toggle _toggleLegacyMonaPlatforms;
        private TextField _search;

        private Button _btnDeletePage;
        private Button _btnMoveLeft;
        private Button _btnMoveRight;

        private Foldout _foldOut;
        private DropdownField _tileSetField;
#if UNITY_EDITOR
        private ObjectField _monaTagsField;
#endif

        private MonaVariablesVisualElement _defaultVariablesVisualElement;

        private ListView _tileListView;
        private List<TileMenuItem> _tileSource = new List<TileMenuItem>();
        private List<IInstruction> _selectedInstructions;
        private int _selectedTileIndex = -1;

        private Color _darkRed = Color.HSVToRGB(347f / 360f, .66f, .1f);
        private Color _lightRed = Color.HSVToRGB(347f / 360f, .66f, .3f);

        private Foldout CreateHeading(string text)
        {
            var foldOut = new Foldout();
            foldOut.style.marginLeft = -9;
            foldOut.style.marginTop = 3;
            foldOut.text = text;
            foldOut.RegisterCallback<GeometryChangedEvent>((evt) =>
            {
                var bar = foldOut.Q<VisualElement>(className: "unity-foldout");
                bar.style.borderLeftWidth = bar.style.borderRightWidth = bar.style.borderTopWidth = bar.style.borderBottomWidth = 0;

                var content = foldOut.Q<VisualElement>(className: "unity-foldout__toggle");
                if (content != null)
                {
                    content.style.marginLeft = 12;
                    content.style.fontSize = 14;
                    content.style.height = 28;
                    content.style.unityFontStyleAndWeight = FontStyle.Bold;
                    content.style.backgroundColor = _lightRed;
                    content.style.paddingBottom = content.style.paddingTop = 3;
                    content.style.borderBottomLeftRadius = content.style.borderBottomRightRadius = content.style.borderTopLeftRadius = content.style.borderTopRightRadius = 3;
                }
                content = foldOut.Q<VisualElement>(className: "unity-foldout__content");
               // if(content != null)
                    //content.style.marginLeft = 0;
            });
            foldOut.value = false;
            return foldOut;
        }

        private void SetBorder(VisualElement elem, float radius = 0, float width = 1, Color color = default, float padding = 5)
        {
            elem.style.borderTopLeftRadius = elem.style.borderTopRightRadius = radius;
            elem.style.borderBottomLeftRadius = elem.style.borderBottomRightRadius = radius;
            elem.style.borderLeftWidth = elem.style.borderTopWidth = elem.style.borderRightWidth = elem.style.borderBottomWidth = width;
            elem.style.borderLeftColor = elem.style.borderRightColor = elem.style.borderTopColor = elem.style.borderBottomColor = color;
            elem.style.paddingLeft = elem.style.paddingRight = elem.style.paddingTop = elem.style.paddingBottom = padding;
        }

        private void SetMargin(VisualElement elm, float margin = 10)
        {
            _activePageContainer.style.marginLeft = _activePageContainer.style.marginRight = margin;
            _activePageContainer.style.marginTop = _activePageContainer.style.marginBottom = margin;
        }

        private Label CreateSmallHeading(string text)
        {
            var label = new Label();
            label.text = text;
            label.style.paddingLeft = 5;
            label.style.height = 30;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.color = Color.white;
            label.style.backgroundColor = _darkRed;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.borderTopLeftRadius = label.style.borderTopRightRadius = 5;
            return label;
        }

        public MonaBrainGraphVisualElement(Action newCallback)
        {
            callback = newCallback;
            style.flexDirection = FlexDirection.Row;

            _leftColumn = new ScrollView();
            _leftColumn.style.flexDirection = FlexDirection.Column;
            _leftColumn.style.flexGrow = 1;
            _leftColumn.style.paddingRight = 3;
            _leftColumn.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _leftColumn.verticalScrollerVisibility = ScrollerVisibility.Auto;
            Add(_leftColumn);

            _brainMetaData = CreateHeading("Brain Metadata");
            _brainMetaData.value = false;

            _name = new TextField("Brain Name");
            _name.RegisterValueChangedCallback((evt) => _brain.Name = (string)evt.newValue);
            _brainMetaData.Add(_name);
            _property = new EnumField("Property", MonaBrainPropertyType.Default);
            _property.RegisterValueChangedCallback((evt) => _brain.PropertyType = (MonaBrainPropertyType)evt.newValue);
            _brainMetaData.Add(_property);

            var tagContainer = new VisualElement();
            var s = tagContainer.style;
            s.flexDirection = FlexDirection.Column;
            s.marginLeft = s.marginRight = s.marginTop = s.marginBottom = 5;
            s.paddingLeft = s.paddingRight = s.paddingTop = s.paddingBottom = 5;
            //s.borderBottomLeftRadius = s.borderBottomRightRadius = s.borderTopLeftRadius = s.borderTopRightRadius = 5;
            //s.borderLeftWidth = s.borderRightWidth = s.borderTopWidth = s.borderBottomWidth = 1;
            //s.borderLeftColor = s.borderRightColor = s.borderTopColor = s.borderBottomColor = new Color(.5f, .5f, .5f);
            s.backgroundColor = new Color(.2f, .2f, .2f);
            s.marginBottom = 10;

            var label = CreateSmallHeading("Mona Tags");
            tagContainer.Add(label);

            _monaTagListView = new ListView(null, -1, () => new MonaTagReferenceVisualElement(_brain), (elem, i) => ((MonaTagReferenceVisualElement)elem).SetValue(i, _brain.MonaTags[i]));
            _monaTagListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _monaTagListView.showFoldoutHeader = false;
            _monaTagListView.headerTitle = "Mona Tags";
            _monaTagListView.showAddRemoveFooter = true;
            _monaTagListView.reorderMode = ListViewReorderMode.Animated;
            _monaTagListView.reorderable = true;
            _monaTagListView.style.paddingBottom = 10;
            _monaTagListView.itemsAdded += (elems) =>
            {
                foreach (var e in elems)
                {
                    if(string.IsNullOrEmpty(_brain.MonaTags[e]))
                        _brain.MonaTags[e] = MonaBrainConstants.TAG_DEFAULT;
                }
            };
            tagContainer.Add(_monaTagListView);

            var label2 = CreateSmallHeading("Mona Assets");
            label2.style.marginTop = 10;
            tagContainer.Add(label2);

            _monaAssetsListView = new ListView(null, -1, () => new MonaAssetReferenceVisualElement(), (elem, i) => ((MonaAssetReferenceVisualElement)elem).SetValue(_brain, i));
            _monaAssetsListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _monaAssetsListView.showFoldoutHeader = false;
            _monaAssetsListView.headerTitle = "Mona Assets";
            _monaAssetsListView.showAddRemoveFooter = true;
            _monaAssetsListView.reorderMode = ListViewReorderMode.Animated;
            _monaAssetsListView.reorderable = true;
            _monaAssetsListView.itemsAdded += (elems) =>
            {
                foreach (var e in elems)
                {
                    _brain.MonaAssets[e] = null;
                }
            };
            tagContainer.Add(_monaAssetsListView);
            _brainMetaData.Add(tagContainer);
            _leftColumn.Add(_brainMetaData);
#if UNITY_EDITOR
            _defaultVariablesHeading = CreateHeading("Brain Default Variables");
            _defaultVariablesHeading.value = true;
            _leftColumn.Add(_defaultVariablesHeading);

            _defaultVariablesVisualElement = new MonaVariablesVisualElement(callback);
            _defaultVariablesHeading.Add(_defaultVariablesVisualElement);
#endif
            _corePageContainer = CreateHeading("Brain Core Page Instructions");
            _corePageContainer.value = true;
            _leftColumn.Add(_corePageContainer);

            _corePage = new MonaBrainPageVisualElement();
            _corePage.OnSelectedInstructionsChanged += HandleSelectedInstructions;
            _corePage.OnTileIndexClicked += HandleTileIndexClicked;
            _corePageContainer.Add(_corePage);

            _activePagesHeading = CreateHeading("All Brain State Pages");
            _leftColumn.Add(_activePagesHeading);

            _tabToolbar = new VisualElement();
            _tabToolbar.style.width = Length.Percent(100);
            _tabToolbar.style.marginTop = 5;
            _tabToolbar.style.flexDirection = FlexDirection.Row;
            _activePagesHeading.Add(_tabToolbar);

            _activePageContainer = new VisualElement();
            _activePageContainer.style.backgroundColor = new Color(.2f, .2f, .2f);
            SetMargin(_activePageContainer, 5);
            _activePageContainer.style.marginTop = 10;
            _activePageContainer.style.flexDirection = FlexDirection.Column;
            SetBorder(_activePageContainer, 5, 0, new Color(.2f, .2f, .2f), 5);
            _activePageContainer.style.paddingTop = 5;
            _activePagesHeading.Add(_activePageContainer);

            _statePageHeading = CreateSmallHeading($"Active Page");
            _statePageHeading.style.fontSize = 12;
            SetBorder(_statePageHeading, 5, 1, new Color(.1f, .1f, .1f), 2);
            SetMargin(_statePageHeading, 0);
            _statePageHeading.style.marginBottom = 5;
            _activePageContainer.Add(_statePageHeading);


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

            _statePageInstructions = CreateSmallHeading("Active Page Instructions");
            _activePageContainer.Add(_statePageInstructions);

            _activeStatePage = new MonaBrainPageVisualElement();
            _activeStatePage.OnSelectedInstructionsChanged += HandleSelectedInstructions;
            _activeStatePage.OnTileIndexClicked += HandleTileIndexClicked;
            _activePageContainer.Add(_activeStatePage);

            var btnBar = new VisualElement();
                btnBar.style.flexDirection = FlexDirection.Row;
            _btnDeletePage = new Button();
            _btnDeletePage.style.width = 100;
            _btnDeletePage.style.backgroundColor = new Color(.6f, 0, 0);
            _btnDeletePage.text = "Delete Page";
            SetBorder(_btnDeletePage, 3, 1, Color.red, 0);
            _btnDeletePage.clicked += () =>
            {
                DeleteStatePage();
            };

            _activePageContainer.Add(btnBar);

            btnBar.Add(_btnDeletePage);


            _btnMoveLeft = new Button();
            _btnMoveLeft.text = "<";
            _btnMoveLeft.style.marginLeft = 30;
            _btnMoveLeft.style.width = 30;
            _btnMoveLeft.clicked += () =>
            {
                MoveStatePageLeft();
            };
            btnBar.Add(_btnMoveLeft);

            _btnMoveRight = new Button();
            _btnMoveRight.text = ">";
            _btnMoveRight.style.width = 30;
            _btnMoveRight.clicked += () =>
            {
                MoveStatePageRight();
            };
            btnBar.Add(_btnMoveRight);

#if UNITY_EDITOR

            _foldOut = CreateHeading("Brain Settings");
            _foldOut.value = true;
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
            _leftColumn.Add(_foldOut);

            _toggleAllowLogging = new Toggle();
            _toggleAllowLogging.label = "Allow Console Output";
            _toggleAllowLogging.RegisterValueChangedCallback((changed) =>
            {
                _brain.LoggingEnabled = changed.newValue;
            });
            _foldOut.Add(_toggleAllowLogging);

            _toggleLegacyMonaPlatforms = new Toggle();
            _toggleLegacyMonaPlatforms.label = "Support Legacy Mona Platforms";
            _toggleLegacyMonaPlatforms.RegisterValueChangedCallback((changed) =>
            {
                _brain.LegacyMonaPlatforms = changed.newValue;
            });
            _foldOut.Add(_toggleLegacyMonaPlatforms);

            var container = new VisualElement();
            SetBorder(container, 5, 1, Color.grey, 10f);

            var labelImport = new Label("IMPORT/EXPORT JSON");
            labelImport.style.unityFontStyleAndWeight = FontStyle.Bold;
            labelImport.style.paddingBottom = 10;
            container.Add(labelImport);

            var importRow = new VisualElement();
            importRow.style.flexDirection = FlexDirection.Row;
            importRow.style.flexGrow = 1;

            var importLabel = new Label();
            importLabel.text = "Import From Json";
            importLabel.style.width = 200;
            importRow.Add(importLabel);

            var jsonField = new ObjectField();
            jsonField.objectType = typeof(TextAsset);
            jsonField.style.flexGrow = 1;

            var jsonImport = new Button();
            jsonImport.text = "Import";
            jsonImport.clicked += () =>
            {
                if (jsonField.value == null) return;
                var text = (TextAsset)jsonField.value;
                _brain.FromJson(text.text);
                Refresh();
            };

            importRow.Add(jsonField);
            importRow.Add(jsonImport);

            container.Add(importRow);

            var exportRow = new VisualElement();
            exportRow.style.flexDirection = FlexDirection.Row;
            exportRow.style.flexGrow = 1;

            var exportLabel = new Label();
            exportLabel.text = "Export To Json";
            exportLabel.style.width = 200;
            exportRow.Add(exportLabel);

            var button = new Button();
            button.text = "Export And Save";
            button.style.flexGrow = 1;
            button.clicked += () =>
            {
                Debug.Log($"BRAIN JSON {_brain.ToJson()}");

                if (!AssetDatabase.IsValidFolder("Assets/Brains"))
                    AssetDatabase.CreateFolder("Assets", "Brains");

                if (!AssetDatabase.IsValidFolder("Assets/Brains/Json"))
                    AssetDatabase.CreateFolder("Assets/Brains", "Json");

                var file = _brain.Name;
                if (string.IsNullOrEmpty(file))
                    file = ((MonaBrainGraph)_brain).name;

                var name = "/Brains/Json/" + file + ".json";

                File.WriteAllText(Application.dataPath + name, _brain.ToJson());
                AssetDatabase.Refresh();


                Debug.Log($"BRAIN EXPORTED AS: {name}");
            };

            exportRow.Add(button);
            container.Add(exportRow);
            _foldOut.Add(container);

#endif

            _rightColumn = new VisualElement();
            _rightColumn.style.width = 150;
            _rightColumn.style.backgroundColor = Color.HSVToRGB(347f / 360f, .66f, .1f);
            _rightColumn.style.paddingLeft = 5;
            Add(_rightColumn);

            var searchContainer = new VisualElement();
            var searchLabel = new Label("Search Tiles");
            searchLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            searchLabel.style.fontSize = 13;

            searchContainer.Add(searchLabel);

            _search = new TextField();
            _search.RegisterValueChangedCallback((evt) =>
            {
                if (!string.IsNullOrEmpty(evt.newValue))
                {
                    var items = _tileSource.FindAll(x =>
                    {
                        if (x.IsHeader) return false;
                        if (x.Category != null && x.Category.ToLower().Contains(evt.newValue.ToLower())) return true;
                        if (x.Label.ToLower().Contains(evt.newValue.ToLower())) return true;
                        return false;
                    });
                    items = items.FindAll(x =>
                    {
                        if (x.IsCategory)
                        {
                            return items.Find(f => f.Category == x.Category) != null;
                        }
                        return true;
                    });
                    _tileListView.itemsSource = items;
                    _tileListView.Rebuild();
                }
                else
                {
                    _tileListView.itemsSource = _tileSource;
                    _tileListView.Rebuild();
                }
            });
            _search[0].style.backgroundColor = Color.black;
            _search.style.color = Color.white;
            SetBorder(_search, 3, 1, Color.white, 2);
            searchContainer.Add(_search);
            _rightColumn.Add(searchContainer);

            _tileListView = new ListView(null, 34, () => new TileMenuItemVisualElement(), (elem, i) =>
            {
                var item = (TileMenuItemVisualElement)elem;
                item.SetItem((TileMenuItem)_tileListView.itemsSource[i]);
                //Debug.Log($"Bind: {i} {_tileSource[i]}");
            });
            _tileListView.selectionChanged += (items) =>
            {
                foreach(var item in items)
                {
                    var menuItem = (TileMenuItem)item;
                    if (!menuItem.IsCategory)
                        menuItem.Action();
                }
                _tileListView.selectedIndex = -1;
            };
            _tileListView.style.flexGrow = 1;
            _rightColumn.Add(_tileListView);
            //Add(_root);
        }

        public class TileMenuItem
        {
            public string Label;
            public Action Action;
            public bool IsCategory;
            public string Category;
            public bool IsHeader;
            public bool IsCondition;
            public override string ToString()
            {
                return Label;
            }

        }

        public class TileMenuItemVisualElement : VisualElement
        {
            private TileMenuItem _item;
            public TileMenuItem Item => _item;
            private Label _label;

            private Color _brightPink = Color.HSVToRGB(351f / 360f, .79f, .98f);
            private Color _lightRed = Color.HSVToRGB(347f / 360f, .80f, .66f);
            private Color _textColor = Color.white;

            public TileMenuItemVisualElement()
            {
                style.flexDirection = FlexDirection.Row;
                style.flexGrow = 1;
                _label = new Label();
                _label.style.flexWrap = Wrap.Wrap;
                Add(_label);
            }

            private void SetRadius(float r) => _label.style.borderBottomLeftRadius = _label.style.borderBottomRightRadius = _label.style.borderTopLeftRadius = _label.style.borderTopRightRadius = r;

            public void SetItem(TileMenuItem item)
            {
                _item = item;
                _label.style.flexGrow = 1;
                _label.style.unityFontStyleAndWeight = FontStyle.Normal;
                _label.style.marginBottom = _label.style.marginLeft = _label.style.marginRight = 2;
                _label.style.borderLeftWidth = 5;
                _label.style.borderLeftColor = Color.white;
                _label.style.paddingLeft = 4;
                _label.text = _item.Label;
                _label.style.unityTextAlign = TextAnchor.MiddleLeft;
                SetRadius(3);

                if (_item.IsCondition)
                {
                    _label.style.backgroundColor = _lightRed;
                    _label.style.color = _textColor;
                }
                else
                {
                    _label.style.backgroundColor = _brightPink;
                    _label.style.color = _textColor;
                }

                if (_item.IsCategory)
                {
                    _label.style.borderLeftWidth = 0;
                    _label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    _label.text = _item.Label.ToUpper();
                    SetRadius(0);
                    if (_item.IsHeader)
                    {
                        _label.style.backgroundColor = Color.HSVToRGB(.9f, 0f, .1f);
                        _label.style.color = Color.HSVToRGB(1f, 0f, 1f);
                        _label.style.unityFontStyleAndWeight = FontStyle.Bold;
                    }
                    else
                    {
                        if (_item.IsCondition)
                        {
                            _label.style.backgroundColor = Color.HSVToRGB(.9f, 0f, .1f); 
                            _label.style.color = _textColor;
                        }
                        else
                        {
                            _label.style.backgroundColor = Color.HSVToRGB(.9f, 0f, .1f); 
                            _label.style.color = _textColor;
                        }

                        _label.style.color = Color.HSVToRGB(1f, 0f, 1f);
                        _label.style.unityFontStyleAndWeight = FontStyle.Bold;
                    }
                }
            }
        }

        private void RefreshMenu()
        {
            if (_brain.TileSet == null)
            {
                Debug.LogError($"Please assign a tile set to this brain");
                return;
            }
#if UNITY_EDITOR
            _tileSource = new List<TileMenuItem>();
            _tileSource.Add(new TileMenuItem() { Label = "WHEN TILES", IsCategory = true, IsHeader = true });
            
            string lastCategory = null;
            var tiles = _brain.TileSet.ConditionTiles;
            //tiles.Sort((a, b) =>
            //{
            //    if (a == null || b == null) return -1;
            //    if (a.Category.CompareTo(b.Category) == 0)
            //        return a.Name.CompareTo(b.Name);
            //    else
            //        return a.Category.CompareTo(b.Category);
            //});

            for (var i = 0; i < tiles.Count; i++)
            {
                var def = tiles[i];
                if (def == null) continue;
                CopyToTile(def);
                if(def.Category != lastCategory)
                {
                    _tileSource.Add(new TileMenuItem() { Label = def.Category, Category = def.Category, IsCategory = true, IsCondition = true });
                    lastCategory = def.Category;
                }    
                _tileSource.Add(new TileMenuItem() { Label = $"{def.Name}", Category = def.Category, IsCondition = true, Action = () => AddTileToSelectedInstructions(def.Tile) });
            }
            
            _tileSource.Add(new TileMenuItem() { Label = "THEN DO TILES", IsCategory = true, IsHeader = true });

            var tiles2 = _brain.TileSet.ActionTiles;
            //tiles2.Sort((a, b) =>
            //{
            //    if (a == null || b == null) return -1;
            //    if (a.Category.CompareTo(b.Category) == 0)
            //        return a.Name.CompareTo(b.Name);
            //    else
            //        return a.Category.CompareTo(b.Category);
            //});

            lastCategory = null;
            for (var i = 0; i < tiles2.Count; i++)
            {
                var def = tiles2[i];
                if (def == null) continue;
                CopyToTile(def);
                if (def.Category != lastCategory)
                {
                    _tileSource.Add(new TileMenuItem() { Label = def.Category, IsCategory = true });
                    lastCategory = def.Category;
                }
                _tileSource.Add(new TileMenuItem() { Label = $"{def.Name}", Action = () => AddTileToSelectedInstructions(def.Tile) });
            }
            _tileListView.itemsSource = _tileSource;
            _tileListView.RefreshItems();
#endif
        }

        private void AddTileToSelectedInstructions(IInstructionTile tile)
        {
            if (_selectedInstructions != null && _selectedInstructions.Count > 0)
            {
                for (var i = 0; i < _selectedInstructions.Count; i++)
                    _selectedInstructions[i].AddTile(tile, _selectedTileIndex, FindInstructionPage(_selectedInstructions[i]));
                _selectedTileIndex = -1;
            }
        }

        private IMonaBrainPage FindInstructionPage(IInstruction instruction)
        {
            if (_brain.CorePage.Instructions.IndexOf(instruction) > -1)
                return _brain.CorePage;
            return _brain.StatePages.Find(x => x.Instructions.IndexOf(instruction) > -1);
        }

        private void HandleSelectedInstructions(List<IInstruction> instructions)
        {
            _selectedInstructions = instructions;
            _selectedTileIndex = -1;
            Debug.Log($"selected instructions {instructions.Count}");
        }

        private void HandleTileIndexClicked(IInstruction instruction, int i)
        {
            _selectedInstructions = new List<IInstruction>() { instruction };
            _selectedTileIndex = i;
            Debug.Log($"Clicked tile {i}");
        }

        private void CopyToTile(IInstructionTileDefinition def)
        {
            if (def.Tile == null)
            {
                Debug.LogWarning($"{nameof(CopyToTile)} missing tile instance");
            }
            else { 
                def.Tile.Id = def.Id;
                def.Tile.Name = def.Name;
                def.Tile.Category = def.Category;
            }
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
            var page = new MonaBrainPage($"State{_brain.StatePages.Count}", false);
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

        private void MoveStatePageLeft()
        {
            var item = _brain.StatePages[_selectedTab];
            _brain.StatePages.RemoveAt(_selectedTab);
            _brain.StatePages.Insert(_selectedTab-1, item);
            _selectedTab = _selectedTab - 1;
            Refresh();
        }

        private void MoveStatePageRight()
        {
            var item = _brain.StatePages[_selectedTab];
            _brain.StatePages.RemoveAt(_selectedTab);
            _brain.StatePages.Insert(_selectedTab+1, item);
            _selectedTab = _selectedTab + 1;
            Refresh();
        }

        private void Refresh()
        {
#if UNITY_EDITOR
            _toggleAllowLogging.value = _brain.LoggingEnabled;
            _toggleLegacyMonaPlatforms.value = _brain.LegacyMonaPlatforms;

            if (_brain.TileSet == null || _brain.TileSet.ToString() == "null")
            {
                var versions = GetTileSets();
                _brain.TileSet = versions[0];
            }

            if (_brain.MonaTagSource == null || _brain.MonaTagSource.ToString() == "null")
            {
                string[] guids = AssetDatabase.FindAssets("t:MonaTags", null);
                foreach (string guid in guids)
                {
                    _brain.MonaTagSource = (MonaTags)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(MonaTags));
                    _monaTagsField.value = (MonaTags)_brain.MonaTagSource;
                    break;
                }
            }

#endif
            if (_brain.MonaTagSource == null || _brain.TileSet == null)
            {
                _leftColumn.style.display = DisplayStyle.None;
                _foldOut.value = true;
                return;
            }

#if UNITY_EDITOR
            _monaTagsField.value = (MonaTags)_brain.MonaTagSource;
#endif
            _tileSetField.value = ((InstructionTileSet)_brain.TileSet) != null ? ((InstructionTileSet)_brain.TileSet).Version : "Select a Tileset";

            _leftColumn.style.display = DisplayStyle.Flex;
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
                    if(_selectedInstructions != null)
                        _selectedInstructions.Clear();
                    _selectedTileIndex = -1;
                    _selectedTab = button.tabIndex;
                    Refresh();
                };
                button.style.backgroundColor = (i == _selectedTab) ? new Color(108f / 255f, 173f / 255f, 201f / 255f) : Color.gray;
                _tabs.Add(button);
            }

            if (_selectedTab > _tabs.childCount)
                _selectedTab = 0;

            _activePagesHeading.text = $"Brain State Pages - {_brain.StatePages.Count}";

            _activePagesHeading.value = true;
            if (_brain.StatePages.Count == 0)
            {                
                _statePageHeading.style.display = DisplayStyle.None;
                _activePageContainer.style.display = DisplayStyle.None;
            }
            else
            {
                _statePageHeading.style.display = DisplayStyle.Flex;
                _activePageContainer.style.display = DisplayStyle.Flex;

                _activePageName.value = _brain.StatePages[_selectedTab].Name;
                _statePageHeading.text = $"\"{_brain.StatePages[_selectedTab].Name}\" Page Properties";
                _statePageInstructions.text = $"\"{_brain.StatePages[_selectedTab].Name}\" Page Instructions";
                _activeStatePage.SetPage(_brain, _brain.StatePages[_selectedTab]);
                _activeStatePage.visible = true;

                _btnMoveLeft.SetEnabled(_selectedTab > 0);
                _btnMoveRight.SetEnabled(_selectedTab < _brain.StatePages.Count - 1);

            }

        }

        public void SetBrain(IMonaBrain brain)
        {
            _monaTagListView.itemsSource = null;
            _monaTagListView.Rebuild();

            _brain = brain;

            _name.value = _brain.Name;
            _property.value = (MonaBrainPropertyType)_brain.PropertyType;

            var versions = GetTileSets();
            _tileSetField.choices = versions.ConvertAll<string>(x => x.Version);
            _tileSetField.choices.Insert(0, "Latest Version");

            _defaultVariablesVisualElement.SetState(_brain.DefaultVariables);

            if (_brain.MonaTags.Count == 0)
                _brain.MonaTags.Add(MonaBrainConstants.TAG_DEFAULT);
             
            _monaTagListView.itemsSource = _brain.MonaTags;
            _monaTagListView.Rebuild();

            _monaAssetsListView.itemsSource = _brain.MonaAssets;
            _monaAssetsListView.Rebuild();

            Refresh();
            RefreshMenu();
        }

        private List<InstructionTileSet> GetTileSets()
        {
            var versions = new List<InstructionTileSet>();
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("t:InstructionTileSet", null);
            foreach (string guid in guids)
            {
                var asset = (InstructionTileSet)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(InstructionTileSet));
                versions.Add(asset);
            }
            versions.Sort((a, b) => -a.Version.CompareTo(b.Version));
#endif
            return versions;
        }
    }
}