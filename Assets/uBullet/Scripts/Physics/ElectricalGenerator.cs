using System.Collections;
using System.Collections.Generic;
using TotalDistraction.uBullet.WeaponSystems;
using UnityEngine;

namespace TotalDistraction.uBullet.Physics
{
    [RequireComponent(typeof(Explosive))]
    public class ElectricalGenerator : DamagableObject
    {
        [Header("Generator Settings: ")]
        public List<Light> connectedLights;
        private Explosive _explosive;

        [Header("Additional Damage Settings: "), Range(0f, 1f)]
        public float chanceToExplode = 0.05f;

        protected override void Start()
        {
            base.Start();

            _explosive = GetComponent<Explosive>();

            if (conditionMatrix.Length == 0) throw new System.Exception(string.Format("{0} has no Conditions set in the ConditionMatrix. This may lead to some unwanted behaviours", gameObject.name));
        }

        public override void OnDamageTaken(float damageTaken, DamageSource damageSource)
        {
            if (isDamagable)
            {
                //Debug.LogWarningFormat("{0}, has taken {1} points worth of damage!", gameObject.name, damageTaken);

                remainingHitPoints -= damageTaken * (1f - damageResistance);

                if (remainingHitPoints < 0f) remainingHitPoints = 0f;

                CheckObjectCondition();

                if (damageSource == DamageSource.Projectile)
                {
                    if (Random.Range(0f, 1f) <= chanceToExplode)
                    {
                        _explosive.TriggerExplosion(false);
                    }
                }

                onObjectDamaged.Invoke();
            }
        }

        private IEnumerator LightsMalfunction()
        {
            while (m_ObjectCondition == ObjectCondition.Malfunctioning)
            {
                yield return new WaitForSeconds(0.05f);

                float chanceToToggleLight = 0.15f;

                foreach (Light light in connectedLights)
                {
                    if (Random.Range(0f, 1f) <= chanceToToggleLight)
                    {
                        light.enabled = !light.enabled;
                    }
                }
            }
        }

        private void DisableLights()
        {
            foreach (Light light in connectedLights)
            {
                light.enabled = false;
            }
        }

        public override void OnMalfunctioning()
        {
            StartCoroutine(LightsMalfunction());
        }

        public override void OnDamaged()
        {
            //throw new System.NotImplementedException();
        }

        public override void OnDestroyed()
        {
            StopAllCoroutines();
            DisableLights();

            isDamagable = false;
        }
    }
}
