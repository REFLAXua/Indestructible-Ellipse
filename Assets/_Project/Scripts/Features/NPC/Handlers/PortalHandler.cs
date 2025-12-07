using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Features.NPC.Handlers
{
    public class PortalHandler : NPCInteractionHandlerBase
    {
        [Header("Portal Settings")]
        [SerializeField] private string _portalId = "game_portal";
        [SerializeField] private float _teleportDelay = 1f;
        [SerializeField] private bool _requireConfirmation = true;
        
        [Header("Available Locations")]
        [SerializeField] private LocationData[] _allLocations;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onPortalOpened;
        [SerializeField] private UnityEvent _onPortalClosed;
        [SerializeField] private UnityEvent<LocationData[]> _onLocationsLoaded;
        [SerializeField] private UnityEvent<string> _onLocationSelected;
        [SerializeField] private UnityEvent _onTeleportStarted;
        [SerializeField] private UnityEvent<string> _onTeleportCompleted;
        [SerializeField] private UnityEvent<string> _onLocationLocked;

        private LocationData? _selectedLocation;
        private HashSet<string> _visitedLocationIds = new HashSet<string>();

        public string PortalId => _portalId;
        public LocationData[] AllLocations => _allLocations;
        public LocationData? SelectedLocation => _selectedLocation;

        private void Awake()
        {
            LoadVisitedLocations();
        }

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[PortalHandler] Відкриття порталу: {_portalId}");
            
            _onPortalOpened?.Invoke();
            
            var availableLocations = GetAvailableLocations();
            _onLocationsLoaded?.Invoke(availableLocations);
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _selectedLocation = null;
            _onPortalClosed?.Invoke();
        }

        public LocationData[] GetAvailableLocations()
        {
            var result = new List<LocationData>();
            
            foreach (var location in _allLocations)
            {
                var locationWithStatus = location;
                locationWithStatus.IsUnlocked = IsLocationUnlocked(location.LocationId);
                result.Add(locationWithStatus);
            }
            
            return result.ToArray();
        }

        public bool IsLocationUnlocked(string locationId)
        {
            var location = System.Array.Find(_allLocations, l => l.LocationId == locationId);
            
            if (location.UnlockByDefault)
            {
                return true;
            }
            
            return _visitedLocationIds.Contains(locationId);
        }

        public void SelectLocation(string locationId)
        {
            var location = System.Array.Find(_allLocations, l => l.LocationId == locationId);
            
            if (string.IsNullOrEmpty(location.LocationId))
            {
                Debug.LogWarning($"[PortalHandler] Локацію не знайдено: {locationId}");
                return;
            }

            if (!IsLocationUnlocked(locationId))
            {
                Debug.Log($"[PortalHandler] Локація заблокована: {location.DisplayName}");
                _onLocationLocked?.Invoke(locationId);
                return;
            }

            _selectedLocation = location;
            Debug.Log($"[PortalHandler] Обрано локацію: {location.DisplayName}");
            _onLocationSelected?.Invoke(locationId);

            if (!_requireConfirmation)
            {
                TeleportToSelected();
            }
        }

        public void TeleportToSelected()
        {
            if (!_selectedLocation.HasValue)
            {
                Debug.LogWarning("[PortalHandler] Локацію не обрано");
                return;
            }

            StartCoroutine(TeleportCoroutine(_selectedLocation.Value));
        }

        public void ConfirmTeleport()
        {
            TeleportToSelected();
        }

        public void CancelTeleport(NPCController npc)
        {
            _selectedLocation = null;
            npc.EndInteraction();
        }

        private System.Collections.IEnumerator TeleportCoroutine(LocationData location)
        {
            Debug.Log($"[PortalHandler] Телепортація на {location.DisplayName} через {_teleportDelay}с...");
            _onTeleportStarted?.Invoke();

            yield return new WaitForSeconds(_teleportDelay);

            UnlockLocation(location.LocationId);

            Debug.Log($"[PortalHandler] Телепорт завершено: {location.SceneName}");
            _onTeleportCompleted?.Invoke(location.LocationId);
        }

        public void UnlockLocation(string locationId)
        {
            if (!_visitedLocationIds.Contains(locationId))
            {
                _visitedLocationIds.Add(locationId);
                SaveVisitedLocations();
                Debug.Log($"[PortalHandler] Локацію розблоковано: {locationId}");
            }
        }

        private void LoadVisitedLocations()
        {
            string saved = PlayerPrefs.GetString($"Portal_{_portalId}_Visited", "");
            if (!string.IsNullOrEmpty(saved))
            {
                string[] ids = saved.Split(',');
                foreach (var id in ids)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        _visitedLocationIds.Add(id);
                    }
                }
            }
            Debug.Log($"[PortalHandler] Завантажено {_visitedLocationIds.Count} відвіданих локацій");
        }

        private void SaveVisitedLocations()
        {
            string toSave = string.Join(",", _visitedLocationIds);
            PlayerPrefs.SetString($"Portal_{_portalId}_Visited", toSave);
            PlayerPrefs.Save();
        }

        public void ResetProgress()
        {
            _visitedLocationIds.Clear();
            PlayerPrefs.DeleteKey($"Portal_{_portalId}_Visited");
            Debug.Log("[PortalHandler] Прогрес локацій скинуто");
        }

        public int GetUnlockedCount()
        {
            int count = 0;
            foreach (var location in _allLocations)
            {
                if (IsLocationUnlocked(location.LocationId))
                {
                    count++;
                }
            }
            return count;
        }

        public int GetTotalCount()
        {
            return _allLocations?.Length ?? 0;
        }
    }

    [System.Serializable]
    public struct LocationData
    {
        public string LocationId;
        public string DisplayName;
        public string SceneName;
        public LocationType Type;
        public int RequiredLevel;
        public bool UnlockByDefault;
        public Sprite Icon;
        public Sprite LockedIcon;
        [TextArea(1, 3)]
        public string Description;
        
        [HideInInspector]
        public bool IsUnlocked;
    }

    public enum LocationType
    {
        Lobby,
        Level,
        Boss,
        Arena,
        SecretArea,
        Event
    }
}

