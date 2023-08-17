using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalDistraction.uBullet.WeaponSystems
{
    public class BulletCasing : MonoBehaviour
    {
        [Header("Despawn Settings:")]
        public float despawnTime = 5f;
        public bool destroyOnDespawn = true;

        [Header("Audio Settings: ")]
        public AudioClip caseCollisionClip;
        public AudioSource audioSource;

        public void Awake()
        {
            StartCoroutine(Despawn());
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (audioSource != null) audioSource.PlayOneShot(caseCollisionClip);
        }

        private IEnumerator Despawn()
        {
            yield return new WaitForSeconds(despawnTime);

            if (destroyOnDespawn)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
