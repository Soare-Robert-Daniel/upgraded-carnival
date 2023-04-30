using System.Collections.Generic;
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
            Zones
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

        [SerializeField] private float currentMobInfoUpdateTime;

        [SerializeField] private int currentSelectedMobId;
        private Label mobNameLabel;
        private ListView mobTokensListView;
        private Button openTowerStoreBtn;

        private Button openZoneStoreBtn;

        private List<ZoneToken> selectedMobTokens;

        private ScrollView storeContainer;

        private Dictionary<int, VisualElement> storeItems;

        private List<(Stores, int, int)> storeItemsOrder;


        private void Awake()
        {
            storeItems = new Dictionary<int, VisualElement>();
            storeItemsOrder = new List<(Stores, int, int)>();
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

            CreateZoneStoreItems(globalResources.zoneTokenDataScriptableObjects);

            openTowerStoreBtn.clicked += () => ShowStore(Stores.Towers);
            openZoneStoreBtn.clicked += () => ShowStore(Stores.Zones);

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

        private void CreateZoneStoreItems(List<ZoneTokenDataScriptableObject> zoneTokenDataScriptableObjects)
        {
            foreach (var zoneTokenDataScriptableObject in zoneTokenDataScriptableObjects)
            {
                var id = GetNextItemId();
                var item = storeItemTemplate.CloneTree();
                var btn = item.Q<Button>("BuyBtn");
                var priceLabel = item.Q<Label>("ItemPriceLabel");
                var nameLabel = item.Q<Label>("ItemNameLabel");

                priceLabel.text = $"{zoneTokenDataScriptableObject.price.value}";
                nameLabel.text = $"{zoneTokenDataScriptableObject.resourcesScriptableObject.label}";

                btn.clicked += () =>
                {
                    Debug.Log($"Buy {zoneTokenDataScriptableObject.resourcesScriptableObject.label}");
                    mapManager.TryBuyZoneForSelectedZone(zoneTokenDataScriptableObject.zoneTokenType);
                };

                storeItems.Add(id, item);
                storeItemsOrder.Add((Stores.Zones, id, 0));
                storeContainer.Add(item);
            }
        }

        private int GetNextItemId()
        {
            itemIdGenerator++;
            return itemIdGenerator;
        }

        public void ShowStore(Stores store)
        {
            visibleStore = store;
            foreach (var (storeView, itemId, order) in storeItemsOrder)
            {
                storeItems[itemId].style.display = store == storeView ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}