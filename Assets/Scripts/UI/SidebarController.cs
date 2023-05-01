using System;
using System.Collections.Generic;
using System.Linq;
using Map;
using Mobs;
using Towers;
using Towers.Zones;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class SidebarController : MonoBehaviour
    {
        public enum Stores
        {
            Towers,
            Zones,
            Upgrades
        }

        [SerializeField] private MapManager mapManager;
        [SerializeField] private MobsController mobsController;
        [SerializeField] private TowerManager towerManager;
        [SerializeField] private EventChannel eventChannel;

        [SerializeField] private GlobalResources globalResources;

        [SerializeField] private Stores visibleStore;

        [SerializeField] private UIDocument sidebarRoot;
        [SerializeField] private VisualTreeAsset storeItemTemplate;
        [SerializeField] private VisualTreeAsset mobTokenTemplate;
        [SerializeField] private float updateMobInfoInterval;

        [Header("Internal")]
        [SerializeField] private int itemIdGenerator;

        [SerializeField] private int itemVisualIdGenerator;

        [SerializeField] private float currentMobInfoUpdateTime;

        [SerializeField] private int currentSelectedMobId;
        private List<int> availableStoreItemsVisuals;
        private Label mobNameLabel;
        private ListView mobTokensListView;

        private Button openTowerStoreBtn;
        private Button openUpgradeStoreBtn;
        private Button openZoneStoreBtn;

        private List<ZoneToken> selectedMobTokens;

        private ScrollView storeContainer;

        private List<StoreItem> storeItems;

        private Dictionary<int, VisualElement> storeItemsElements;


        private void Awake()
        {
            storeItemsElements = new Dictionary<int, VisualElement>();
            storeItems = new List<StoreItem>();
            availableStoreItemsVisuals = new List<int>();
            currentSelectedMobId = -1;
        }

        private void Start()
        {
            mobNameLabel = sidebarRoot.rootVisualElement.Q<Label>("MobNameLabel");

            mobTokensListView = sidebarRoot.rootVisualElement.Q<ListView>("MobTokensListView");
            mobTokensListView.makeItem = () =>
            {
                var item = mobTokenTemplate.CloneTree();
                return item;
            };
            mobTokensListView.bindItem = (element, index) =>
            {
                if (currentSelectedMobId == -1) return;

                var token = selectedMobTokens[index];
                var label = element.Q<Label>("TokenInfoLabel");
                label.text = $"{token.zoneTokenType} ({token.rank}) - {token.remainingDuration:0.0}";
            };

            storeContainer = sidebarRoot.rootVisualElement.Q<ScrollView>("StoreContainer");
            openTowerStoreBtn = sidebarRoot.rootVisualElement.Q<Button>("OpenTowerStoreBtn");
            openZoneStoreBtn = sidebarRoot.rootVisualElement.Q<Button>("OpenZoneStoreBtn");
            openUpgradeStoreBtn = sidebarRoot.rootVisualElement.Q<Button>("OpenUpgradesStoreBtn");

            CreateZoneStoreItems(globalResources.zoneTokenDataScriptableObjects);
            CreateTowerStoreItems(globalResources.towerDataScriptableObjects);
            RefreshStoreItems();

            openTowerStoreBtn.clicked += () => ShowStore(Stores.Towers);
            openZoneStoreBtn.clicked += () => ShowStore(Stores.Zones);
            openUpgradeStoreBtn.clicked += () => ShowStore(Stores.Upgrades);

            eventChannel.OnSelectedMobChanged += OnSelectedMobChanged;
        }

        private void Update()
        {
            currentMobInfoUpdateTime += Time.deltaTime;
            if (currentMobInfoUpdateTime >= updateMobInfoInterval)
            {
                currentMobInfoUpdateTime = 0f;
                OnSelectedMobChanged(currentSelectedMobId);
            }
        }
        private void OnSelectedMobChanged(int mobId)
        {
            currentSelectedMobId = mobId;
            if (!mobsController.Mobs.TryGetValue(mobId, out var mob))
            {
                currentSelectedMobId = -1;
                return;
            }
            ;
            if (!mobsController.MobsData.TryGetValue(mobId, out var mobDataScriptableObject)) return;

            mobNameLabel.text =
                $"{mobDataScriptableObject.mobResourcesScriptableObject.label} ({mob.Health}/{mob.baseStats.baseHealth})";
            mobTokensListView.Clear();

            if (!towerManager.TokenZoneSystem.TryGetMobTokens(mobId, out var mobTokens)) return;
            selectedMobTokens = mobTokens;
            mobTokensListView.itemsSource = mobTokens;
            mobTokensListView.RefreshItems();
        }

        private void CreateZoneStoreItems(IEnumerable<ZoneTokenDataScriptableObject> zoneTokenDataScriptableObjects)
        {
            foreach (var storeItem in from zoneTokenDataScriptableObject in zoneTokenDataScriptableObjects
                     let id = GetNextItemId()
                     select new StoreItem
                     {
                         id = id,
                         label = zoneTokenDataScriptableObject.resourcesScriptableObject.label,
                         price = zoneTokenDataScriptableObject.price.value.ToString("0"),
                         store = Stores.Zones,
                         order = 0,
                         visualId = -1,
                         onClick = () =>
                         {
                             Debug.Log($"Buy {zoneTokenDataScriptableObject.resourcesScriptableObject.label}");
                             mapManager.TryBuyZoneForSelectedZone(zoneTokenDataScriptableObject.zoneTokenType);
                         }
                     })
            {
                storeItems.Add(storeItem);
            }
        }

        private void CreateTowerStoreItems(IEnumerable<TowerDataScriptableObject> towerDataScriptableObjects)
        {
            foreach (var towerDataScriptableObject in towerDataScriptableObjects)
            {
                var id = GetNextItemId();
                var storeItem = new StoreItem
                {
                    id = id,
                    label = towerDataScriptableObject.resources.label,
                    description = towerDataScriptableObject.resources.description,
                    price = towerDataScriptableObject.price.value.ToString("0"),
                    store = Stores.Towers,
                    order = 0,
                    visualId = -1,
                    onClick = () =>
                    {

                        if (!mapManager.EconomyController.CanSpend(towerDataScriptableObject.price.resource.currency,
                                towerDataScriptableObject.price.value)) return;
                        Debug.Log($"Buy {towerDataScriptableObject.resources.label}");
                        mapManager.EconomyController.SpendCurrency(towerDataScriptableObject.price.resource.currency,
                            towerDataScriptableObject.price.value);
                        towerManager.TowerRowController.CreateTower(towerDataScriptableObject);
                    }
                };

                storeItems.Add(storeItem);
            }
        }

        private void CreateStoreItemVisualElement()
        {
            var id = GetNextItemVisualId();
            var item = storeItemTemplate.CloneTree();
            storeContainer.Add(item);
            storeItemsElements.Add(id, item);
            availableStoreItemsVisuals.Add(id);
        }

        private void RefreshStoreItems()
        {

            while (storeItems.Count > availableStoreItemsVisuals.Count)
            {
                CreateStoreItemVisualElement();
            }


            // Order store items
            storeItems.Sort((a, b) => a.order.CompareTo(b.order));

            // Zip store items with visuals
            var storeItemsOrder = storeItems.Zip(availableStoreItemsVisuals, (storeItem, visualId) => (storeItem, visualId));
            foreach (var (storeItem, visualId) in storeItemsOrder)
            {
                var item = storeItemsElements[visualId];
                var btn = item.Q<Button>("BuyBtn");
                var priceLabel = item.Q<Label>("ItemPriceLabel");
                var nameLabel = item.Q<Label>("ItemNameLabel");

                priceLabel.text = $"{storeItem.price}";
                nameLabel.text = $"{storeItem.label}";
                storeItem.visualId = visualId;

                btn.clicked += () =>
                {
                    storeItem.onClick?.Invoke();
                };
            }
        }

        private int GetNextItemId()
        {
            itemIdGenerator++;
            return itemIdGenerator;
        }

        private int GetNextItemVisualId()
        {
            itemVisualIdGenerator++;
            return itemVisualIdGenerator;
        }

        public void ShowStore(Stores store)
        {
            visibleStore = store;
            // foreach (var (storeView, itemId, order) in storeItemsOrder)
            // {
            //     storeItemsElements[itemId].style.display = store == storeView ? DisplayStyle.Flex : DisplayStyle.None;
            // }

            foreach (var storeItem in storeItems)
            {
                if (!storeItemsElements.TryGetValue(storeItem.visualId, out var item)) continue;
                item.style.display = store == storeItem.store ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public class StoreItem
        {
            public string description;
            public int id;
            public string label;
            public Action onClick;
            public int order;
            public string price;
            public Stores store;
            public int visualId;
        }
    }
}