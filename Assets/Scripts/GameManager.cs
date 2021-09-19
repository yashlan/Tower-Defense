using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yashlan.audio;
using Yashlan.util;

namespace Yashlan.manage
{
    public enum GameState
    {
        Ready,
        Start,
        Win,
        Lose
    }

    public class GameManager : SingletonBehaviour<GameManager>
    {
        [SerializeField]
        private GameState _gameState;

        [Header("locked Slot Tower")]
        [SerializeField]
        private int _priceSlotTower;
        [SerializeField]
        private List<GameObject> _lockedTowerList;

        [Header("Gold")]
        [SerializeField]
        private int _currentGold;

        [Header("UI")]
        [SerializeField]
        private GameObject _panelInstruction;
        [SerializeField]
        private Text _textCurrentGold;
        [SerializeField]
        private GameObject _goldNotEnoughPrefab;
        [SerializeField]
        private Button _buttonBuySlot;
        [SerializeField]
        private Text[] _textPriceSlots;
        [SerializeField]
        private Text _textSlotTowerMax;

        private GameObject notEnoughInfo = null;

        public int CurrentGold => _currentGold;

        public GameState GameState
        {
            get => _gameState;
            set => _gameState = value;
        }

        public void StartGameOnClick() => _gameState = GameState.Start;

        public void AddGoldOnEnemyDead(int amount) => _currentGold += amount;


        void Start()
        {
            GameObject canvas = GameObject.Find("Canvas");
            var offset = new Vector3(0, 4f, 0);
            Vector3 canvasPos = canvas.transform.position + offset;

            notEnoughInfo = Instantiate(_goldNotEnoughPrefab, canvasPos, Quaternion.identity, canvas.transform);
            notEnoughInfo.SetActive(false);
            _panelInstruction.SetActive(true);
        }

        void Update()
        {
            _textCurrentGold.text = "Gold : " + _currentGold.ToString();
        }

        public IEnumerator ShowNotEnoughGoldInfo()
        {
            notEnoughInfo.SetActive(true);
            yield return new WaitForSeconds(2f);
            notEnoughInfo.SetActive(false);
            yield break;
        }

        int index = 0;
        public void BuySlotTowerOnClick()
        {
            if ((_currentGold - _priceSlotTower) >= 0)
            {
                if (!_lockedTowerList[index].activeSelf)
                {
                    AudioPlayer.Instance.PlaySFX(AudioPlayer.UNLOCKED_SLOT_SFX);
                    _lockedTowerList[index].SetActive(true);
                    _currentGold -= _priceSlotTower;
                    index++;
                    if (index > 3)
                    {
                        foreach (var text in _textPriceSlots)
                        {
                            text.gameObject.SetActive(false);
                        }
                        _textSlotTowerMax.gameObject.SetActive(true);
                        _buttonBuySlot.interactable = false;
                    }
                }
            }
            else
                StartCoroutine(ShowNotEnoughGoldInfo());
        }

        public void BuyTower(int priceTower, Action<bool> isSucces)
        {
            if ((_currentGold - priceTower) >= 0)
            {
                isSucces(true);
                _currentGold -= priceTower;
            }
        }
    }
}