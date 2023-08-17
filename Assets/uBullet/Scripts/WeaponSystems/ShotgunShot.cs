using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalDistraction.uBullet.WeaponSystems
{
    public class ShotgunShot : MonoBehaviour
    {
        public GameObject shotPrefab;

        [Header("Shot Settings")]
        public int projectilesToSpawn = 25;
        public float shotSpread = 10f;


        // Use this for initialization
        void Start()
        {
            SpawnShots();
            Destroy(gameObject);
        }

        private void SpawnShots()
        {
            for (int index = 0; index < projectilesToSpawn; index++)
            {
                GameObject shot = Instantiate(shotPrefab, transform.position, Quaternion.identity);
                shot.transform.forward = RandomiseDirection();
            }
        }

        private Vector3 RandomiseDirection()
        {
            Vector3 position = (transform.position + (transform.forward * 100f)) + (Random.insideUnitSphere * shotSpread);

            Vector3 direction = position - transform.position;
            return direction.normalized;
        }
    }
}
