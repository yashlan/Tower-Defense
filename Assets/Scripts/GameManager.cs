using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        private Text _textPriceBuySlot;
        [SerializeField]
        private Button _buttonBuySlot;


        private GameObject notEnoughInfo = null;

        public GameState GameState
        {
            get => _gameState;
            set => _gameState = value;
        }

        public void StartGameOnClick() => _gameState = GameState.Start;


        void Start()
        {
            GameObject canvas = GameObject.Find("Canvas");
            var offset = new Vector3(0, 3.5f, 0);
            Vector3 canvasPos = canvas.transform.position + offset;

            notEnoughInfo = Instantiate(_goldNotEnoughPrefab, canvasPos, Quaternion.identity, canvas.transform);
            notEnoughInfo.SetActive(false);

            _textPriceBuySlot.text = "Buy Slot Tower (" + _priceSlotTower.ToString() + " Gold)";

            _textCurrentGold.text = null;

            _panelInstruction.SetActive(true);
        }

        void Update()
        {
            if(_gameState == GameState.Start) _textCurrentGold.text = "Gold : " + _currentGold.ToString();
        }

        public IEnumerator ShowNotEnoughGoldInfo()
        {
            notEnoughInfo.SetActive(true);
            yield return new WaitForSeconds(2f);
            notEnoughInfo.SetActive(false);
            yield break;
        }

        public int CurrentGold => _currentGold;

        int index = 0;
        public void BuySlotTowerOnClick()
        {
            if ((_currentGold - _priceSlotTower) >= 0)
            {
                if (!_lockedTowerList[index].activeSelf)
                {
                    _lockedTowerList[index].SetActive(true);
                    _currentGold -= _priceSlotTower;
                    _textPriceBuySlot.text = "Buy Slot Tower (" + _priceSlotTower.ToString() + "Gold)";
                    index++;
                    if (index > 3)
                    {
                        _textPriceBuySlot.text = "Slot Tower Max!";
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

        public void UpgradeTower(int priceUpgrade)
        {
            if ((_currentGold - priceUpgrade) >= 0) _currentGold -= priceUpgrade;
        }

        public void AddGoldOnEnemyDead(int amount) => _currentGold += amount;

    }
}