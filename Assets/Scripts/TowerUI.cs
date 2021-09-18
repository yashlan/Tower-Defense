using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Yashlan.audio;
using Yashlan.manage;

namespace Yashlan.tower
{
    public enum TowerUIType
    {
        TowerGreen,
        TowerYellow,
        TowerRed,
    }
    public class TowerUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public TowerUIType TowerUIType;

        [SerializeField]
        private bool _isDropped = false;
        [SerializeField]
        private Image _towerIcon;
        [SerializeField]
        private Text _priceText;

        private int _towerPrice;
        private Tower _towerPrefab;
        private Tower _currentSpawnedTower;

        public bool IsDropped
        {
            get => _isDropped;
            set => _isDropped = value;
        }

        void Start()
        {
            switch (_towerPrefab.name)
            {
                case "tower 1":
                    TowerUIType = TowerUIType.TowerGreen;
                    break;

                case "tower 2":
                    TowerUIType = TowerUIType.TowerYellow;
                    break;

                case "tower 3":
                    TowerUIType = TowerUIType.TowerRed;
                    break;
            }
        }

        public void SetTowerPrefab(Tower tower)
        {
            _towerPrice = tower.Price;
            _priceText.text = _towerPrice.ToString();
            _towerPrefab = tower;
            _towerIcon.sprite = tower.GetTowerHeadIcon();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if(GameManager.Instance.GameState == GameState.Start)
            {
                _isDropped = false;

                if (GameManager.Instance.CurrentGold >= _towerPrice)
                {
                    GameObject newTowerObj = Instantiate(_towerPrefab.gameObject);
                    _currentSpawnedTower = newTowerObj.GetComponent<Tower>();
                    _currentSpawnedTower.ToggleOrderInLayer(true);
                }
                else
                    StartCoroutine(GameManager.Instance.ShowNotEnoughGoldInfo());
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(GameManager.Instance.GameState == GameState.Start)
            {
                _isDropped = false;

                if (GameManager.Instance.CurrentGold >= _towerPrice)
                {
                    Camera mainCamera = Camera.main;
                    Vector3 mousePosition = Input.mousePosition;
                    mousePosition.z = -mainCamera.transform.position.z;
                    Vector3 targetPosition = mainCamera.ScreenToWorldPoint(mousePosition);
                    if(_currentSpawnedTower != null)
                        _currentSpawnedTower.transform.position = targetPosition;
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if(GameManager.Instance.GameState == GameState.Start)
            {
                if (_currentSpawnedTower == null) return;

                if (_currentSpawnedTower.PlacePosition != null)
                {
                    GameManager.Instance.BuyTower(_towerPrice, isSucces =>
                    {
                        if (isSucces)
                        {
                            _currentSpawnedTower.LockPlacement();
                            _currentSpawnedTower.ToggleOrderInLayer(false);
                            LevelManager.Instance.RegisterSpawnedTower(_currentSpawnedTower);
                            _currentSpawnedTower = null;
                            _isDropped = true;
                            AudioPlayer.Instance.PlaySFX(AudioPlayer.DROP_TOWER_SFX);
                        }
                        else
                            _isDropped = false;
                    });
                }
                else
                    Destroy(_currentSpawnedTower.gameObject);
            }
        }
    }
}
