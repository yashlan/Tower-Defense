using System.Collections.Generic;
using UnityEngine;
using Yashlan.audio;
using Yashlan.bullet;
using Yashlan.enemy;
using Yashlan.manage;

namespace Yashlan.tower 
{
    public class Tower : MonoBehaviour
    {
        [SerializeField]
        private int _price;
        [SerializeField]
        private SpriteRenderer _towerPlace;
        [SerializeField]
        private SpriteRenderer _towerHead;
        [SerializeField]
        private SpriteRenderer _healthBar;
        [SerializeField]
        private SpriteRenderer _healthFill;
        [SerializeField]
        private int _maxHealth = 1;
        [SerializeField]
        private int _currentHealth = 1;
        [SerializeField]
        private int _shootPower = 1;
        [SerializeField]
        private float _shootDistance = 1f;
        [SerializeField]
        private float _shootDelay = 5f;
        [SerializeField]
        private float _bulletSpeed = 1f;
        [SerializeField]
        private float _bulletSplashRadius = 0f;
        [SerializeField] 
        private Bullet _bulletPrefab;


        private float _runningShootDelay;
        private Enemy _targetEnemy;
        private Quaternion _targetRotation;

        public int Price => _price;

        public Sprite GetTowerHeadIcon() => _towerHead.sprite;

        public Vector2? PlacePosition { get; private set; }

        public void SetPlacePosition(Vector2? newPosition) => PlacePosition = newPosition;

        public void LockPlacement() => transform.position = (Vector2)PlacePosition;

        public void ToggleOrderInLayer(bool toFront)
        {
            int orderInLayer = toFront ? 2 : 0;
            _towerPlace.sortingOrder = orderInLayer;
            _towerHead.sortingOrder = orderInLayer;
        }

        public void ReduceTowerHealth(int damage)
        {
            _currentHealth -= damage;
            AudioPlayer.Instance.PlaySFX(AudioPlayer.HIT_ENEMY_SFX);

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                AudioPlayer.Instance.PlaySFX(AudioPlayer.ENEMY_DIE_SFX);
                Destroy(gameObject);
            }

            float healthPercentage = (float)_currentHealth / _maxHealth;

            _healthFill.size = new Vector2(healthPercentage * _healthBar.size.x, _healthBar.size.y);

        }

        public void CheckNearestEnemy(List<Enemy> enemies)
        {
            if (_targetEnemy != null)
            {
                if (!_targetEnemy.gameObject.activeSelf || Vector3.Distance(transform.position, _targetEnemy.transform.position) > _shootDistance)
                    _targetEnemy = null;
                else
                    return;
            }

            float nearestDistance = Mathf.Infinity;
            Enemy nearestEnemy = null;

            foreach (Enemy enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (distance > _shootDistance) continue;

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            _targetEnemy = nearestEnemy;
        }


        public void ShootTarget()
        {
            if (_targetEnemy == null) return;

            _runningShootDelay -= Time.unscaledDeltaTime;

            if (_runningShootDelay <= 0f)
            {
                bool headHasAimed = Mathf.Abs(_towerHead.transform.rotation.eulerAngles.z - _targetRotation.eulerAngles.z) < 10f;
               
                if (!headHasAimed) return;

                Bullet bullet = LevelManager.Instance.GetBulletFromPool(_bulletPrefab);

                bullet.transform.position = transform.position;
                bullet.SetProperties(_shootPower, _bulletSpeed, _bulletSplashRadius);
                bullet.SetTargetEnemy(_targetEnemy);
                bullet.gameObject.SetActive(true);

                _runningShootDelay = _shootDelay;
            }
        }

        public void SeekTarget()
        {
            if (_targetEnemy == null) return;

            Vector3 direction = _targetEnemy.transform.position - transform.position;

            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            _targetRotation = Quaternion.Euler(new Vector3(0f, 0f, targetAngle - 90f));

            _towerHead.transform.rotation = Quaternion.RotateTowards(_towerHead.transform.rotation, _targetRotation, Time.deltaTime * 180f);
        }

        GameObject _placementTemp = null;
        void OnTriggerStay2D(Collider2D collision)
        {
            if(collision.gameObject.tag == "placement")
            {
                var placement = collision.GetComponent<TowerPlacement>();

                _placementTemp = placement.gameObject;

                foreach (var towerUI in FindObjectsOfType<TowerUI>())
                {
                    switch (towerUI.TowerUIType)
                    {
                        case TowerUIType.TowerGreen:
                            if (towerUI.IsDropped)
                            {
                                placement.gameObject.SetActive(false);
                                towerUI.IsDropped = false;
                                return;
                            }
                            break;

                        case TowerUIType.TowerYellow:
                            if (towerUI.IsDropped)
                            {
                                placement.gameObject.SetActive(false);
                                towerUI.IsDropped = false;
                                return;
                            }
                            break;

                        case TowerUIType.TowerRed:
                            if (towerUI.IsDropped)
                            {
                                placement.gameObject.SetActive(false);
                                towerUI.IsDropped = false;
                                return;
                            }
                            break;
                    }
                }
            }
        }

        bool _showPlacement = false;
        void OnDestroy()
        {
            if (_placementTemp == null) return;

            _showPlacement = true;
            if (_showPlacement) _placementTemp.SetActive(true);
        }
    }
}


