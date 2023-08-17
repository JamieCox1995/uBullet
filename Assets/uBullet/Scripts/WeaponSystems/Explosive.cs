using System.Collections;
using System.Collections.Generic;
using TotalDistraction.uBullet.Physics;
using UnityEngine;

namespace TotalDistraction.uBullet.WeaponSystems
{
    public class Explosive : MonoBehaviour
    {
        public ExplosiveSettings _explosiveSettings;

        private GameObject rootObject;
        private ParticleSystem _shrapnelEmitter;
        private Vector3 _explodePosition;

        public bool IsArmed { get; private set; }

        private IEnumerator FuseBurn()
        {
            IsArmed = true;
            float timeElapsed = 0f;

            while (timeElapsed <= _explosiveSettings._fuseTimer)
            {
                timeElapsed += Time.deltaTime;

                yield return null;
            }

            Explode();
        }

        private void Update()
        {
            _explodePosition = transform.position;
        }

        public void TriggerExplosion(bool delayExplosion)
        {
            if (IsArmed == true) return;

            if (_explosiveSettings.hasExploded == false)
            {
                if (delayExplosion)
                {
                    StartCoroutine(FuseBurn());
                }
                else
                {
                    Explode();
                }

                IsArmed = true;
            }
        }

        private void Explode()
        {
            if (IsArmed == false) IsArmed = true;

            // Spawn in the explosion effect
            Instantiate(_explosiveSettings._effect, _explodePosition, Quaternion.identity);

            if (_explosiveSettings._spawnShrapnel)
            {
                SpawnShrapnelObjects(_explodePosition);
            }

            ApplyExplosionToSurroundings();

            Debug.Log("Boom");

            if (_explosiveSettings.destroyOnExplode) Destroy(gameObject);
            if (rootObject != null) Destroy(rootObject);
        }

        private void ApplyExplosionToSurroundings()
        {
            Collider[] collidersInRange = UnityEngine.Physics.OverlapSphere(transform.position, _explosiveSettings.explosionRadius);

            for (int index = 0; index < collidersInRange.Length; index++)
            {
                GameObject current = collidersInRange[index].gameObject;

                if (current != gameObject && _explosiveSettings.affectableLayers == (_explosiveSettings.affectableLayers | (1 << current.layer)))
                {
                    // We are getting the distance to the object.
                    float distance = Vector3.Distance(transform.position, current.transform.position);

                    // First of all we want to check to see if the detected collider has a DamagableObject and a rigidbody applied to it.
                    DamagableObject damagableObject = current.GetComponentInChildren<DamagableObject>();
                    Rigidbody rb = current.GetComponentInChildren<Rigidbody>();

                    // We want to apply damage to the object based on how far we are from the centre of the explosion
                    if (damagableObject != null)
                    {
                        float damageToTake = CalculateDamageToGive(distance, _explosiveSettings.damageAtCentre);

                        damagableObject.OnDamageTaken(damageToTake, DamageSource.Shockwave);
                    }

                    // We want to apply a force to the object based on how far we are from the object.
                    if (rb != null)
                    {
                        RaycastHit hit;

                        if (UnityEngine.Physics.Raycast(transform.position, (rb.transform.position - transform.position), out hit, (rb.transform.position - transform.position).magnitude) && hit.collider.gameObject == rb.gameObject)
                        {
                            rb.AddExplosionForce(_explosiveSettings.explosionForce, transform.position, _explosiveSettings.explosionRadius, 0f, ForceMode.Impulse);
                        }
                    }
                }
            }
        }

        private float CalculateDamageToGive(float distanceFrom, float damageAtEpicentre)
        {
            return Mathf.Lerp(damageAtEpicentre, 0f, distanceFrom / _explosiveSettings.explosionRadius);
        }

        private void SpawnShrapnelObjects(Vector3 position)
        {
            // Creating a parent object for all of the shrapnel pieces.
            GameObject shrapnelParent = new GameObject("Shrapnel");
            shrapnelParent.transform.position = position;

            // Now we are going to start spawning in the shrapnel pieces.
            for (int index = 0; index < _explosiveSettings._noOfSharpnelPieces; index++)
            {
                // Creating a random direction to spawn the shrapnel in.
                Vector3 randDirection = Random.insideUnitSphere;

                GameObject shrapnelPiece = Instantiate(_explosiveSettings._shrapnelPrefab, shrapnelParent.transform.position, Quaternion.identity, shrapnelParent.transform);
                Rigidbody rb = shrapnelPiece.GetComponent<Rigidbody>();

                float velocity = Random.Range(_explosiveSettings.shrapnelVelocity.x, _explosiveSettings.shrapnelVelocity.y);

                rb.AddForce(velocity * randDirection, ForceMode.Impulse);
            }
        }

        public void SetExplosivePosition(Vector3 position)
        {
            _explodePosition = position;
        }

        public void SetRootGameObject(GameObject root)
        {
            rootObject = root;
        }
    }

    [System.Serializable]
    public class ExplosiveSettings
    {
        [Header("Fuse Settings: ")]
        public float _fuseTimer = 0.2f;

        [Header("Explosion Settings: ")]
        public GameObject _effect;
        public float explosionRadius = 20f;
        public float explosionForce = 30f;

        [Header("Damage Settings: ")]
        public float damageAtCentre = 60f;
        public LayerMask affectableLayers;

        [Header("Sharpnel Settings: ")]
        public bool _spawnShrapnel = false;
        public GameObject _shrapnelPrefab;
        public Vector2 shrapnelVelocity = new Vector2(10f, 15f);
        public int _noOfSharpnelPieces = 10;

        [Header("Additional Settings: ")]
        public bool hasExploded = false;
        public bool destroyOnExplode = false;
    }
}
