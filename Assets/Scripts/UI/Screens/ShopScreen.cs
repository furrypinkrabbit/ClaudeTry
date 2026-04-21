using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using GuJian.Lobby;
using GuJian.Progression;
using GuJian.Tools;

namespace GuJian.UI.Screens {
    /// <summary>
    /// 商店界面。从外部 Bind(catalog, shop) 注入业务,不直接耦合系统启动顺序。
    /// 两个标签:锤子 / 配件。ListView 虚拟化,只有可见项有真实 VisualElement,高效。
    /// </summary>
    public class ShopScreen : UIScreen {
        Label    _silverLabel;
        Button   _tabHammer, _tabTuning, _closeBtn;
        ListView _hammerList, _tuningList;

        ToolCatalog _catalog;
        ToolShop    _shopSvc;
        MetaProgressSave _saveFallback;

        // 锤子 price 由 catalog 里 ToolData 不含 price —— 这里以 (kind) 估一个；后续 ToolData 加字段再替换
        const int DefaultHammerPrice = 200;

        public void Bind(ToolCatalog catalog, ToolShop service) {
            _catalog = catalog;
            _shopSvc = service;
            if (_shopSvc == null) _saveFallback = MetaProgressSave.Load();
            Rebuild();
        }

        protected override void OnBind(VisualElement root) {
            _silverLabel = root.Q<Label>("label-silver");
            _closeBtn    = root.Q<Button>("btn-close");
            _tabHammer   = root.Q<Button>("tab-hammer");
            _tabTuning   = root.Q<Button>("tab-tuning");
            _hammerList  = root.Q<ListView>("list-hammer");
            _tuningList  = root.Q<ListView>("list-tuning");

            if (_closeBtn  != null) _closeBtn.clicked   += () => UIRouter.Instance.Pop();
            if (_tabHammer != null) _tabHammer.clicked  += () => SetTab(true);
            if (_tabTuning != null) _tabTuning.clicked  += () => SetTab(false);

            InitList(_hammerList, BuildHammerItem, BindHammerItem);
            InitList(_tuningList, BuildTuningItem, BindTuningItem);
            SetTab(true);
        }

        protected override void OnEnter() { Rebuild(); }

        MetaProgressSave Save => _shopSvc != null ? _shopSvc.Save : _saveFallback;

        void Rebuild() {
            if (_silverLabel != null) _silverLabel.text = $"匠银 · {Save?.matterSilver ?? 0}";
            if (_catalog != null) {
                if (_hammerList != null) { _hammerList.itemsSource = _catalog.tools;   _hammerList.Rebuild(); }
                if (_tuningList != null) { _tuningList.itemsSource = _catalog.tunings; _tuningList.Rebuild(); }
            }
        }

        void SetTab(bool hammer) {
            if (_hammerList != null) _hammerList.style.display = hammer ? DisplayStyle.Flex : DisplayStyle.None;
            if (_tuningList != null) _tuningList.style.display = hammer ? DisplayStyle.None : DisplayStyle.Flex;
            _tabHammer?.EnableInClassList("tab--active", hammer);
            _tabTuning?.EnableInClassList("tab--active", !hammer);
        }

        void InitList(ListView lv, System.Func<VisualElement> make, System.Action<VisualElement,int> bind) {
            if (lv == null) return;
            lv.fixedItemHeight = 72;
            lv.makeItem = make;
            lv.bindItem = bind;
            lv.selectionType = SelectionType.Single;
        }

        // 锤子 row
        VisualElement BuildHammerItem() {
            var row = new VisualElement(); row.AddToClassList("shop-row");
            var name = new Label();   name.AddToClassList("shop-row__name");
            var price= new Label();   price.AddToClassList("shop-row__price");
            var buy  = new Button();  buy.text = "购入"; buy.AddToClassList("shop-row__buy");
            row.Add(name); row.Add(price); row.Add(buy);
            return row;
        }
        void BindHammerItem(VisualElement row, int idx) {
            if (_catalog == null || idx < 0 || idx >= _catalog.tools.Count) return;
            var t = _catalog.tools[idx];
            var nameL = row.Q<Label>(className:"shop-row__name");
            var priceL= row.Q<Label>(className:"shop-row__price");
            var buyB  = row.Q<Button>(className:"shop-row__buy");
            if (nameL  != null) nameL.text = t.displayName;
            if (priceL != null) priceL.text = $"{DefaultHammerPrice}";
            bool owned = Save != null && Save.unlockedToolIds.Contains(t.toolId);
            if (buyB != null) {
                buyB.text = owned ? "已持有" : "购入";
                buyB.SetEnabled(!owned);
                buyB.clickable = new Clickable(() => {
                    if (_shopSvc != null && _shopSvc.TryBuyTool(t.toolId, DefaultHammerPrice)) Rebuild();
                });
            }
        }

        // 配件 row
        VisualElement BuildTuningItem() {
            var row = new VisualElement(); row.AddToClassList("shop-row");
            var name = new Label();   name.AddToClassList("shop-row__name");
            var price= new Label();   price.AddToClassList("shop-row__price");
            var buy  = new Button();  buy.AddToClassList("shop-row__buy");
            row.Add(name); row.Add(price); row.Add(buy);
            return row;
        }
        void BindTuningItem(VisualElement row, int idx) {
            if (_catalog == null || idx < 0 || idx >= _catalog.tunings.Count) return;
            var t = _catalog.tunings[idx];
            var nameL = row.Q<Label>(className:"shop-row__name");
            var priceL= row.Q<Label>(className:"shop-row__price");
            var buyB  = row.Q<Button>(className:"shop-row__buy");
            if (nameL  != null) nameL.text = t.displayName;
            if (priceL != null) priceL.text = $"{t.price}";
            bool owned = Save != null && Save.ownedTuningIds.Contains(t.tuningId);
            if (buyB != null) {
                buyB.text = owned ? "已持有" : "购入";
                buyB.SetEnabled(!owned);
                buyB.clickable = new Clickable(() => {
                    if (_shopSvc != null && _shopSvc.TryBuyTuning(t.tuningId, t.price)) Rebuild();
                });
            }
        }
    }
}
