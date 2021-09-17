using System.Collections.Generic;
using UnityEngine;
using Yashlan.audio;
using Yashlan.bullet;
using Yashlan.manage;
using Yashlan.tower;

namespace Yashlan.enemy
{
    public enum EnemyType
    {
        Enemy1,
        Enemy2,
        Enemy3,
        Enemy4
    }

    public class Enemy : MonoBehaviour
    {
        [SerializeField]
        private EnemyType _enemyType;
        [SerializeField]
        private int _maxHealth = 1;
        [SerializeField]
        private float _moveSpeed = 1f;
        [SerializeField]
        private SpriteRenderer _head;
        [SerializeField]
        private SpriteRenderer _healthBar;
        [SerializeField]
        private SpriteRenderer _healthFill;
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
        private Tower _targetTower;
        private Quaternion _targetRotation;
        private int _currentHealth;

        public Vector3 TargetPosition { get; private set; }
        public int CurrentPathIndex { get; private set; }

        public EnemyType EnemyType => _enemyType;

        void OnEnable()
        {
            _currentHealth = _maxHealth;
            _healthFill.size = _healthBar.size;
        }

        public void MoveToTarget() => transform.position = Vector3.MoveTowards(transform.position, TargetPosition, _moveSpeed * Time.deltaTime);

        public void SetCurrentPathIndex(int currentIndex) => CurrentPathIndex = currentIndex;

        public void SetTargetPosition(Vector3 targetPosition)
        {
            TargetPosition = targetPosition;
            _healthBar.transform.parent = null;

            Vector3 distance = TargetPosition - transform.position;

            if (Mathf.Abs(distance.y) > Mathf.Abs(distance.x))
            {
                if (distance.y > 0)
                    transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
                else
                    transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -90f));
            }
            else
            {
                if (distance.x > 0)
                    transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
                else
                    transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
            }

            _healthBar.transform.parent = transform;

        }

        public void ReduceEnemyHealth(int damage)
        {
            _currentHealth -= damage;
            AudioPlayer.Instance.PlaySFX(AudioPlayer.HIT_ENEMY_SFX);

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                gameObject.SetActive(false);
                AudioPlayer.Instance.PlaySFX(AudioPlayer.ENEMY_DIE_SFX);

                if(_enemyType == EnemyType.Enemy1) GameManager.Instance.AddGoldOnEnemyDead(50);
                if(_enemyType == EnemyType.Enemy2) GameManager.Instance.AddGoldOnEnemyDead(100);
                if(_enemyType == EnemyType.Enemy3) GameManager.Instance.AddGoldOnEnemyDead(150);
                if(_enemyType == EnemyType.Enemy4) GameManager.Instance.AddGoldOnEnemyDead(200);
            }

            float healthPercentage = (float)_currentHealth / _maxHealth;

            _healthFill.size = new Vector2(healthPercentage * _healthBar.size.x, _healthBar.size.y);

        }

        public void CheckNearestTower(List<Tower> towers)
        {
            if (_targetTower != null)
            {
                if (!_targetTower.gameObject.activeSelf || Vector3.Distance(transform.position, _targetTower.transform.position) > _shootDistance)
                    _targetTower = null;
                else
                    return;
            }

            float nearestDistance = Mathf.Infinity;
            Tower nearestTower = null;

            foreach (var tower in towers)
            {
                if (tower == null) continue;

                float distance = Vector3.Distance(transform.position, tower.transform.position);

                if (distance > _shootDistance) continue;

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTower = tower;
                }
            }

            _targetTower = nearestTower;
        }


        public void ShootTarget()
        {
            if (_targetTower == null) return;

            _runningShootDelay -= Time.unscaledDeltaTime;

            if (_runningShootDelay <= 0f)
            {
                bool headHasAimed = Mathf.Abs(_head.transform.rotation.eulerAngles.z - _targetRotation.eulerAngles.z) < 10f;

                if (!headHasAimed) return;

                Bullet bullet = LevelManager.Instance.GetBulletFromPool(_bulletPrefab);

                bullet.transform.position = transform.position;
                bullet.SetProperties(_shootPower, _bulletSpeed, _bulletSplashRadius);
                bullet.SetTargetTower(_targetTower);
                bullet.gameObject.SetActive(true);

                _runningShootDelay = _shootDelay;
            }
        }

        public void SeekTarget()
        {
            if (_targetTower == null) return;

            Vector3 direction = _targetTower.transform.position - transform.position;

            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            _targetRotation = Quaternion.Euler(new Vector3(0f, 0f, targetAngle - 90f));

            _head.transform.rotation = Quaternion.RotateTowards(_head.transform.rotation, _targetRotation, Time.deltaTime * 180f);
        }
    }
}
