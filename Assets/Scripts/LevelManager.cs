using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yashlan.audio;
using Yashlan.bullet;
using Yashlan.enemy;
using Yashlan.tower;
using Yashlan.util;

namespace Yashlan.manage
{
    public class LevelManager : SingletonBehaviour<LevelManager>
    {
        [SerializeField]
        private Transform _towerUIParent;
        [SerializeField]
        private GameObject _towerUIPrefab;
        [SerializeField]
        private Tower[] _towerPrefabs;
        [SerializeField] 
        private Enemy[] _enemyPrefabs;
        [SerializeField] 
        private Transform[] _enemyPaths;
        [SerializeField] 
        private float _spawnDelay = 5f;
        [SerializeField] 
        private int _maxLives = 3;
        [SerializeField] 
        private int _totalEnemy = 15;
        [SerializeField] 
        private GameObject _panel;
        [SerializeField] 
        private Text _statusInfo;
        [SerializeField] 
        private Text _livesInfo;
        [SerializeField] 
        private Text _totalEnemyInfo;

        private List<Tower> _spawnedTowers = new List<Tower>();
        private List<Enemy> _spawnedEnemies = new List<Enemy>();
        private List<Bullet> _spawnedBullets = new List<Bullet>();

        private float _runningSpawnDelay;

        private int _currentLives;
        private int _enemyCounter;
        private int randomIndex = 0;

        void Start()
        {
            SetCurrentLives(_maxLives);
            SetTotalEnemy(_totalEnemy);
            InstantiateAllTowerUI();
        }

        void Update()
        {
            if(GameManager.Instance.GameState == GameState.Start)
            {
                _runningSpawnDelay -= Time.unscaledDeltaTime;

                if (_runningSpawnDelay <= 0f)
                {
                    SpawnEnemy();
                    _runningSpawnDelay = _spawnDelay;
                }

                foreach (Tower tower in _spawnedTowers)
                {
                    if (tower == null) continue;
                    tower.CheckNearestEnemy(_spawnedEnemies);
                    tower.SeekTarget();
                    tower.ShootTarget();
                }

                foreach (Enemy enemy in _spawnedEnemies)
                {
                    if (!enemy.gameObject.activeSelf) continue;

                    if (enemy.EnemyType != EnemyType.Enemy1)
                    {
                        enemy.CheckNearestTower(_spawnedTowers);
                        enemy.SeekTarget();
                        enemy.ShootTarget();
                    }

                    if (Vector2.Distance(enemy.transform.position, enemy.TargetPosition) < 0.1f)
                    {
                        enemy.SetCurrentPathIndex(enemy.CurrentPathIndex + 1);

                        if (enemy.CurrentPathIndex < _enemyPaths.Length)
                            enemy.SetTargetPosition(_enemyPaths[enemy.CurrentPathIndex].position);
                        else
                        {
                            ReduceLives(1);
                            enemy.gameObject.SetActive(false);
                        }
                    }
                    else
                        enemy.MoveToTarget();
                }
            }

            if (GameManager.Instance.GameState == GameState.Lose || GameManager.Instance.GameState == GameState.Win)
            {
                if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(0);
            }
        }

        public Bullet GetBulletFromPool(Bullet prefab)
        {
            GameObject newBulletObj = _spawnedBullets.Find(b => !b.gameObject.activeSelf && b.name.Contains(prefab.name))?.gameObject;

            if (newBulletObj == null) newBulletObj = Instantiate(prefab.gameObject);

            Bullet newBullet = newBulletObj.GetComponent<Bullet>();

            if (!_spawnedBullets.Contains(newBullet)) _spawnedBullets.Add(newBullet);

            return newBullet;
        }

        public void ExplodeAt(Vector2 point, float radius, int damage)
        {
            foreach (Enemy enemy in _spawnedEnemies)
            {
                if (enemy.gameObject.activeSelf)
                    if (Vector2.Distance(enemy.transform.position, point) <= radius) enemy.ReduceEnemyHealth(damage);
            }
        }

        public void RegisterSpawnedTower(Tower tower) => _spawnedTowers.Add(tower);

        private void InstantiateAllTowerUI()
        {
            foreach (Tower tower in _towerPrefabs)
            {
                GameObject newTowerUIObj = Instantiate(_towerUIPrefab.gameObject, _towerUIParent);
                TowerUI newTowerUI = newTowerUIObj.GetComponent<TowerUI>();
                newTowerUI.SetTowerPrefab(tower);
                newTowerUI.transform.name = tower.name;
            }
        }

        private void GetSpawnIndex()
        {
            if (_enemyCounter >= 70)
            {
                randomIndex = Random.Range(0, 2);
                if (randomIndex == 2) randomIndex = 1;
                _spawnDelay = 5;
            }
            else if (_enemyCounter >= 40 && _enemyCounter < 70)
            {
                randomIndex = Random.Range(0, 3);
                if (randomIndex == 3) randomIndex = 2;
                _spawnDelay = 4;
            }
            else
            {
                randomIndex = Random.Range(1, 4);
                if (randomIndex == 4) randomIndex = 3;
                _spawnDelay = 3;
            }
        }

        private void SpawnEnemy()
        {
            SetTotalEnemy(--_enemyCounter);

            if (_enemyCounter < 0)
            {
                bool isAllEnemyDestroyed = _spawnedEnemies.Find(e => e.gameObject.activeSelf) == null;
                if (isAllEnemyDestroyed) SetGameState(GameState.Win);
                return;
            }

            GetSpawnIndex();

            string enemyIndexString = (randomIndex + 1).ToString();
            
            GameObject newEnemyObj = _spawnedEnemies.Find(e => !e.gameObject.activeSelf && e.name.Contains(enemyIndexString))?.gameObject;
            
            if (newEnemyObj == null) newEnemyObj = Instantiate(_enemyPrefabs[randomIndex].gameObject);
            
            Enemy newEnemy = newEnemyObj.GetComponent<Enemy>();

            if (!_spawnedEnemies.Contains(newEnemy)) _spawnedEnemies.Add(newEnemy);

            newEnemy.transform.position = _enemyPaths[0].position;
            newEnemy.SetTargetPosition(_enemyPaths[1].position);
            newEnemy.SetCurrentPathIndex(1);
            newEnemy.gameObject.SetActive(true);
        }

        private void ReduceLives(int value)
        {
            SetCurrentLives(_currentLives - value);
            if (_currentLives <= 0) SetGameState(GameState.Lose);
        }

        private void SetCurrentLives(int currentLives)
        {
            _currentLives = Mathf.Max(currentLives, 0);
            _livesInfo.text = $"Lives: {_currentLives}";
        }

        private void SetTotalEnemy(int totalEnemy)
        {
            _enemyCounter = totalEnemy;
            _totalEnemyInfo.text = $"Total Enemy: {Mathf.Max(_enemyCounter, 0)}";
        }

        private void SetGameState(GameState gameState)
        {
            AudioPlayer.Instance.StopBGM();
            GameManager.Instance.GameState = gameState;
            _statusInfo.text = GameManager.Instance.GameState == GameState.Win ? "You Win!" : "You Lose!";
            
            if (GameManager.Instance.GameState == GameState.Win)
                AudioPlayer.Instance.PlaySFX(AudioPlayer.GAME_WIN_SFX);
            else
                AudioPlayer.Instance.PlaySFX(AudioPlayer.GAME_LOSE_SFX);

            _panel.gameObject.SetActive(true);
        }

        void OnDrawGizmos()
        {
            for (int i = 0; i < _enemyPaths.Length - 1; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_enemyPaths[i].position, _enemyPaths[i + 1].position);
            }
        }
    }
}

