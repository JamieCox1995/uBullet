using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TotalDistraction.uBullet.Physics
{
    public abstract class DamagableObject : MonoBehaviour
    {
        [Header("Damage Settings: ")]
        public bool isDamagable = false;
        public ObjectCondition m_ObjectCondition;
        public float startingHitPoints = 100f;
        protected float remainingHitPoints;

        [Tooltip("Holds all of the Conditions this object can enter.")]
        public ConditionMatrix[] conditionMatrix;

        [Range(0f, 1f)]
        public float damageResistance = 1f;

        [Header("On Damage Event Information: ")]
        public UnityEvent onObjectDamaged;

        protected virtual void Start()
        {
            remainingHitPoints = startingHitPoints;
        }

        public abstract void OnDamageTaken(float damageTaken, DamageSource damageSource);

        /// <summary>
        /// Called when the object is damaged, this method checks to see if the condition of
        /// the object has degraded.
        /// </summary>
        protected virtual void CheckObjectCondition()
        {
            ObjectCondition old = m_ObjectCondition;
            float conditionValue = remainingHitPoints / startingHitPoints;

            for (int i = 0; i < conditionMatrix.Length; i++)
            {
                if (i != 0)
                {
                    if (conditionValue >= conditionMatrix[i - 1].maxHitPoints && conditionValue < conditionMatrix[i].maxHitPoints)
                    {
                        m_ObjectCondition = conditionMatrix[i].condition;
                    }
                }
                else
                {
                    if (conditionValue >= 0 && conditionValue < conditionMatrix[i].maxHitPoints)
                    {
                        m_ObjectCondition = conditionMatrix[i].condition;
                    }
                }
            }

            if (m_ObjectCondition != old)
            {
                switch (m_ObjectCondition)
                {
                    case ObjectCondition.Destroyed:
                        OnDestroyed();
                        break;

                    case ObjectCondition.Damaged:
                        OnDamaged();
                        break;

                    case ObjectCondition.Malfunctioning:
                        OnMalfunctioning();
                        break;
                }
            }
        }

        /// <summary>
        /// Called when the object enters the Malfunctioning state.
        /// </summary>
        public abstract void OnMalfunctioning();

        /// <summary>
        /// Called when the object enters the Damaged state.
        /// </summary>
        public abstract void OnDamaged();

        /// <summary>
        /// Called when the object enters the Destroyed state.
        /// </summary>
        public abstract void OnDestroyed();
    }

    [Serializable]
    public class ConditionMatrix
    {
        public ObjectCondition condition = ObjectCondition.Working;
        [Range(0f, 1f), Tooltip("Maximum Hit Points considered to be in this state. This is a proportion of the ")]
        public float maxHitPoints = 1f;
    }

    [Serializable]
    public enum ObjectCondition
    {
        Working, Damaged, Malfunctioning, Destroyed
    }

    public enum DamageSource
    {
        Projectile, Shrapnel, Shockwave
    }
}