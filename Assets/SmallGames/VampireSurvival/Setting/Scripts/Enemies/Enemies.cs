using System;
using UnityEngine;

namespace VampireSurvival.Enemies
{
    public abstract class Enemies : MonoBehaviour
    {
        [Header("Basic Value")]
        [SerializeField] protected int maxHP = 4;
        [SerializeField] protected int currentHP;
        [SerializeField] protected int scoreValue = 5;
        [SerializeField] protected float moveSpeed = 0.8f;
        [SerializeField] protected int collisionDamage = 5;

        /// <summary>
        /// 敌人移动
        /// </summary>
        public abstract void Move();

        /// <summary>
        /// 敌人受击处理
        /// </summary>
        public abstract void TakeDamage(int damage);

        /// <summary>
        /// 更新分数
        /// </summary>
        public abstract void UpdateScore();
    }
}
