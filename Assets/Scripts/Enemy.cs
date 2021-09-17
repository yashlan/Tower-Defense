﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yashlan.audio;

namespace Yashlan.enemy
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField]
        private int _maxHealth = 1;
        [SerializeField]
        private float _moveSpeed = 1f;
        [SerializeField]
        private SpriteRenderer _healthBar;
        [SerializeField]
        private SpriteRenderer _healthFill;

        private int _currentHealth;

        public Vector3 TargetPosition { get; private set; }
        public int CurrentPathIndex { get; private set; }

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
            }

            float healthPercentage = (float)_currentHealth / _maxHealth;

            _healthFill.size = new Vector2(healthPercentage * _healthBar.size.x, _healthBar.size.y);

        }
    }
}
