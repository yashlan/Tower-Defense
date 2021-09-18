using UnityEngine;
using Yashlan.enemy;
using Yashlan.manage;
using Yashlan.tower;

namespace Yashlan.bullet
{
    public enum BulletType
    {
        ForAttackEnemy,
        ForAttackTower
    }
    public class Bullet : MonoBehaviour
    {
        [SerializeField]
        private BulletType _bulletType;
        private int _bulletPower;
        private float _bulletSpeed;
        private float _bulletSplashRadius;
        private Enemy _targetEnemy;
        private Tower _targetTower;

        private void FixedUpdate()
        {
            if(GameManager.Instance.GameState == GameState.Start)
            {
                if(_bulletType == BulletType.ForAttackEnemy)
                {
                    if (_targetEnemy != null)
                    {
                        if (!_targetEnemy.gameObject.activeSelf)
                        {
                            gameObject.SetActive(false);
                            _targetEnemy = null;
                            return;
                        }

                        transform.position = Vector3.MoveTowards(transform.position, _targetEnemy.transform.position, _bulletSpeed * Time.fixedDeltaTime);
                        Vector3 direction = _targetEnemy.transform.position - transform.position;
                        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, targetAngle - 90f));
                    }
                    else
                        gameObject.SetActive(false);
                }

                if(_bulletType == BulletType.ForAttackTower)
                {
                    if (_targetTower != null)
                    {
                        if (!_targetTower.gameObject.activeSelf)
                        {
                            gameObject.SetActive(false);
                            _targetTower = null;
                            return;
                        }

                        transform.position = Vector3.MoveTowards(transform.position, _targetTower.transform.position, _bulletSpeed * Time.fixedDeltaTime);
                        Vector3 direction = _targetTower.transform.position - transform.position;
                        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, targetAngle - 90f));
                    }
                    else
                        gameObject.SetActive(false);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(_bulletType == BulletType.ForAttackEnemy)
            {
                if (_targetEnemy == null) return;

                if (collision.gameObject.Equals(_targetEnemy.gameObject))
                {
                    gameObject.SetActive(false);

                    if (_bulletSplashRadius > 0f)
                        LevelManager.Instance.ExplodeAt(transform.position, _bulletSplashRadius, _bulletPower);
                    else
                        _targetEnemy.ReduceEnemyHealth(_bulletPower);

                    _targetEnemy = null;
                }
            }

            if(_bulletType == BulletType.ForAttackTower)
            {
                if (_targetTower == null) return;

                if (collision.gameObject.Equals(_targetTower.gameObject))
                {
                    gameObject.SetActive(false);

                    if (_bulletSplashRadius > 0f)
                        LevelManager.Instance.ExplodeAt(transform.position, _bulletSplashRadius, _bulletPower);
                    else
                        _targetTower.ReduceTowerHealth(_bulletPower);

                    _targetTower = null;
                }
            }

        }

        public void SetProperties(int bulletPower, float bulletSpeed, float bulletSplashRadius)
        {
            _bulletPower = bulletPower;
            _bulletSpeed = bulletSpeed;
            _bulletSplashRadius = bulletSplashRadius;
        }

        public void SetTargetEnemy(Enemy enemy) => _targetEnemy = enemy;
        public void SetTargetTower(Tower tower) => _targetTower = tower;
    }
}
