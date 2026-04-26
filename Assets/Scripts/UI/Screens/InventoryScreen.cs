using UnityEngine.UIElements;
using GuJian.Lobby;
using GuJian.Progression;

namespace GuJian.UI.Screens {
    /// <summary>
    /// 背包:展示玩家已拥有的锤子和配件,可单击装备。
    /// </summary>
    public class InventoryScreen : UIScreen {
        Label    _title;
        Button   _close;
        ListView _hammerList, _tuningList;

        ToolCatalog _catalog;
        ToolShop    _shop;       // 可用于 EquipTool/EquipTuning
        MetaProgressSave _saveFallback;

        public void Bind(ToolCatalog catalog, ToolShop shop) {
            _catalog = catalog;
            _shop = shop;
            if (_shop == null) _saveFallback = MetaProgressSave.Load();
            Rebuild();
        }

        MetaProgressSave Save => _shop != null ? _shop.Save : _saveFallback;

        protected override void OnBind(VisualElement root) {
            _title      = root.Q<Label>("title");
            _close      = root.Q<Button>("btn-close");
            _hammerList = root.Q<ListView>("list-hammer");
            _tuningList = root.Q<ListView>("list-tuning");
            if (_close != null) _close.clicked += () => UIRouter.Instance.Pop();
            InitList(_hammerList);
            InitList(_tuningList);
        }

        protected override void OnEnter() { Rebuild(); }

        void Rebuild() {
            if (_catalog == null || Save == null) return;
            var owned = new System.Collections.Generic.List<string>(Save.unlockedToolIds);
            var tunings = new System.Collections.Generic.List<string>(Save.ownedTuningIds);
            if (_hammerList != null) {
                _hammerList.itemsSource = owned;
                _hammerList.bindItem = (ve, i) => {
                    var lbl = ve.Q<Label>();
                    var id  = owned[i];
                    var td  = _catalog.FindTool(id);
                    bool equipped = Save.equippedToolId == id;
                    if (lbl != null) lbl.text = $"{(td != null ? td.displayName : id)}{(equipped ? "  \u25c9\u5df2\u88c5\u5907" : "")}";
                };
                _hammerList.Rebuild();
            }
            if (_tuningList != null) {
                _tuningList.itemsSource = tunings;
                _tuningList.bindItem = (ve, i) => {
                    var lbl = ve.Q<Label>();
                    var id = tunings[i];
                    var tn = _catalog.FindTuning(id);
                    bool on = Save.equippedTuningIds.Contains(id);
                    if (lbl != null) lbl.text = $"{(tn != null ? tn.displayName : id)}{(on ? "  \u25c9\u5df2\u88c5\u5907" : "")}";
                };
                _tuningList.Rebuild();
            }
        }

        void InitList(ListView lv) {
            if (lv == null) return;
            lv.fixedItemHeight = 56;
            lv.makeItem = () => {
                var row = new VisualElement(); row.AddToClassList("inv-row");
                row.Add(new Label());
                return row;
            };
            lv.selectionType = SelectionType.Single;
            lv.selectionChanged += items => {
                foreach (var o in items) {
                    if (o is string id && _shop != null) {
                        // 锤子 就是 EquipTool,配件 toggle
                        if (Save.unlockedToolIds.Contains(id)) { _shop.EquipTool(id); }
                        else if (Save.ownedTuningIds.Contains(id)) {
                            _shop.EquipTuning(id, !Save.equippedTuningIds.Contains(id));
                        }
                        Rebuild();
                    }
                    break;
                }
            };
        }
    }
}
