using System.Collections;
using System.Collections.Generic;
using TotalDistraction.uBullet.Physics;
using UnityEngine;

namespace TotalDistraction.uBullet.WeaponSystems
{
    public class Shrapnel : MonoBehaviour
    {
        [Header("Shrapnel Impact Effects: ")]
        public GameObject shrapnelImpactEffect;
        public float chanceToSpawnEffect = 0.1f;

        [Header("Impact Settings: ")]
        public float damageOnImpact = 5f;
        public bool m_SendImpactMessage = false;

        private void OnCollisionEnter(Collision collision)
        {
            // In here we want to spawn in the impact effect when the shrapnel hits and object.
            if (Random.Range(0f, 1f) <= 0.1f)
            {
                GameObject effect = Instantiate(shrapnelImpactEffect, collision.contacts[0].point, Quaternion.identity);
                effect.transform.forward = collision.contacts[0].normal;
            }

            if (m_SendImpactMessage)
            {
                // Now we want to check to see if the object is an IDamagable. **TODO: Change this to see if IDamagable == null, when we have created that interface.** 
                if (m_SendImpactMessage)
                {
                    DamagableObject damagable = collision.gameObject.GetComponentInChildren<DamagableObject>();
                    if (damagable != null)
                    {
                        Debug.LogFormat("Sending Impact Message to Object {0}", collision.collider.gameObject.name);
                        damagable.OnDamageTaken(damageOnImpact, DamageSource.Shrapnel);
                    }
                }
            }

            Destroy(gameObject);
        }
    }
}
