using System.Collections;
using System.Collections.Generic;
using TotalDistraction.uBullet.Data;
using TotalDistraction.uBullet.Physics;
using UnityEngine;
using UnityEngine.Events;

namespace TotalDistraction.uBullet.WeaponSystems
{
    //using CollisionType = TotalDistraction.uBullet.WeaponSystems.CollisionType;

    [RequireComponent(typeof(Rigidbody), typeof(TrailRenderer))]
    public class Projectile : MonoBehaviour
    {
        public string type = "9mm";
        public ProjectileData m_ProjectileData;

        [Space]
        public LayerMask penetrableLayers;

        protected Collider _collider;
        protected Rigidbody _rigidbody;
        protected TrailRenderer _tracer;
        public Transform _projectileNose;
        private Vector3 _lastPos;
        private float currentPenetration = 0f;
        private float startingEnergy = 0f;
        private float kEnergy = 0f;         // We want to store the projectiles KE, as using distance as the measure for penetration capabilities may not be the most accurate after the projectile has hit multiple objects.
        private float velocity;

        [Space]
        public UnityEvent onImpact;

        public float DistanceTravelled { get; private set; }
        public float TimeAlive { get; private set; }
        private const float energyLostThroughTravel = 1500f;
        private const float MaxDistanceTravelled = 250f, MaxTimeAlive = 180f;

        // These are used for storing information about a predicted collision that we want to process next frame.
        private bool _predictedCollision = false;
        protected Vector3 _predictedImpactNormal;
        protected Vector3 _predictedImpactPosition;
        protected Collider _impactedCollider;

        public bool debug;

        // Use this for initialization
        protected virtual void Start()
        {
            CheckForRigidbody();
            _collider = GetComponent<Collider>();

            _rigidbody.mass = m_ProjectileData.Weight;
            _rigidbody.AddForce(_rigidbody.transform.forward * m_ProjectileData.MuzzleVelocity, ForceMode.VelocityChange);

            velocity = m_ProjectileData.MuzzleVelocity;

            _tracer = GetComponent<TrailRenderer>();

            if (_tracer != null)
            {
                if (m_ProjectileData.tracerActivationTime == 0f) _tracer.enabled = m_ProjectileData.isTracer;
                else StartCoroutine(IgniteTracer());

                _tracer.colorGradient = m_ProjectileData.tracerColour;
            }

            _lastPos = transform.position;

            kEnergy = (0.5f * m_ProjectileData.Weight) * (m_ProjectileData.MuzzleVelocity * m_ProjectileData.MuzzleVelocity);
            startingEnergy = kEnergy;

            currentPenetration = m_ProjectileData.maxPenetration;

            DistanceTravelled = 0f;

            PredictImpactLocation(m_ProjectileData.MuzzleVelocity * Time.fixedDeltaTime);
        }

        protected virtual void Update()
        {
            TimeAlive += Time.deltaTime;

            if (DistanceTravelled >= MaxDistanceTravelled || TimeAlive >= MaxTimeAlive)
            {
                Destroy();
            }

            if (_rigidbody.velocity.normalized != Vector3.zero)
            {
                transform.forward = _rigidbody.velocity.normalized;
            }
        }

        protected virtual void FixedUpdate()
        {
            // First of all we are going to update the projectiles distance travelled for accurate values for penetration
            float distFromLastUpdate = Vector3.Distance(_lastPos, transform.position);
            DistanceTravelled += distFromLastUpdate;

            // Now we are updating the 'last position' of the projectile and re-calculating the current KE for the projectile
            _lastPos = transform.position;
            velocity = _rigidbody.velocity.magnitude;

            kEnergy = (0.5f * m_ProjectileData.Weight) * (velocity * velocity);
            if (m_ProjectileData.constantPenetration == false) currentPenetration = m_ProjectileData.maxPenetration * (kEnergy / startingEnergy);       // Here we are tying the penetration into the kE of the projectile, as we can then use penetration to
                                                                                                                                                        // slow down, and decrease the penetration of projectiles.
            if (_predictedCollision)
            {
                // We want to set our posiiton to the predicted impact location, and we want to run the penetration/ricochet logic
                transform.position = _predictedImpactPosition - (transform.forward * Vector3.Distance(transform.position, _projectileNose.position));

                OnProjectileCollision();

                _predictedCollision = false;
            }

            // After we've run the stuff for handling a collision, we want to run the normal FixedUpdate stuff so that we can check if the projectile is going to collide again
            PredictImpactLocation(velocity * Time.fixedDeltaTime);
        }

        /// <summary>
        /// Handles the Collision for the Projectile against an object on the penetrable layers
        /// TODO: Update the Collision Logic to include varying Projectile Types, i.e AP, APCR, APBC, HE, HEAT, APHE, etc. ** These could be different Classes which inherit from this class **
        /// </summary>
        protected virtual void OnProjectileCollision()
        {
            // Here we are grabbing the PenetrableObject script so that we can grab the thickness of the object, but to also call the Impact() method on it
            PenetrableObject penObject = _impactedCollider.gameObject.GetComponent<PenetrableObject>();

            float collisionAngle = Mathf.Acos(Vector3.Dot(transform.forward, -_predictedImpactNormal)) * Mathf.Rad2Deg;
            ImpactData impactData = new ImpactData(_predictedImpactPosition, _predictedImpactNormal.normalized, CollisionType.Ricochet);

            float nominalThickness, objectThickness;

            CalculateObjectThickness(penObject, out nominalThickness, out objectThickness);

            if (m_ProjectileData.Calibre >= (nominalThickness * 3f) && currentPenetration >= objectThickness)
            {
                impactData._collisionType = CollisionType.Penetration;
                impactData._penetrationDirection = transform.forward;

                OnPenetration(penObject, penObject.GetObjectThickness(transform.forward, _predictedImpactNormal));
            }
            else
            {
                if (collisionAngle >= m_ProjectileData.MinimumRicochetAngle)
                {
                    OnRicochet(penObject);
                }
                else
                {
                    // Here is where we want to normalize the projectile and then check the if it can penetrate the object with the new angle. *** NOW I NEED TO WORK OUT HOW TO APPLY THE NORMALIZATION TO THE PROJECTILE *** 
                    Vector3 normalizedDirection = CalculateNormalization(collisionAngle);

                    // Here we want to add the normalization value of the projectile to the collision Angle
                    _rigidbody.velocity = normalizedDirection.normalized * velocity;
                    transform.forward = _rigidbody.velocity.normalized;
                    transform.position = _predictedImpactPosition - (transform.forward * Vector3.Distance(transform.position, _projectileNose.position));

                    impactData._penetrationDirection = transform.forward;

                    // Now that we have calculated the normalization angle of the projectile, we want to get the thickness of the object at the new angle
                    if (penObject != null)
                    {
                        if (penObject.use3dCollider)
                        {
                            objectThickness = PenetrationUtility.Get3DLineOfSightThickness(_impactedCollider.gameObject, _predictedImpactPosition, transform.forward, penetrableLayers, penObject.RelativeHardness);
                        }
                        else
                        {
                            objectThickness = penObject.GetObjectThickness(transform.forward, _predictedImpactNormal);
                        }
                    }

                    if (currentPenetration >= objectThickness)
                    {
                        // We have penetrated the object and the projectile passed through. We shall want to decrease the velocity of the projectile, as energy will
                        // have been lost trying to go through the object.
                        impactData._collisionType = CollisionType.Penetration;
                        impactData._penetrationDistance = objectThickness;

                        OnPenetration(penObject, objectThickness);
                    }
                    else
                    {
                        // We have hit the object but not penetrated it. We shall just delete the projectile at this point.
                        impactData._collisionType = CollisionType.Hit;
                        if (penObject != null) OnImpact(penObject, penObject.GetComponent<Rigidbody>());
                    }
                }
            }

            if (penObject != null) penObject.OnImpact(impactData, Settings.Instance.InstanceMunitions.GetMunitionHitEffect(type));

            onImpact.Invoke();
        }

        private void CalculateObjectThickness(PenetrableObject penObject, out float nominalThickness, out float objectThickness)
        {
            nominalThickness = Mathf.Infinity;
            objectThickness = Mathf.Infinity;
            if (penObject != null)
            {
                if (penObject.use3dCollider)
                {
                    nominalThickness = PenetrationUtility.GetNominalThickness(_impactedCollider.gameObject, _predictedImpactPosition, -_predictedImpactNormal, penetrableLayers, penObject.RelativeHardness);
                    objectThickness = PenetrationUtility.Get3DLineOfSightThickness(_impactedCollider.gameObject, _predictedImpactPosition, transform.forward, penetrableLayers, penObject.RelativeHardness);
                }
                else
                {
                    nominalThickness = penObject.Thickness;
                    objectThickness = penObject.GetObjectThickness(transform.forward, _predictedImpactNormal);
                }
            }
        }

        protected virtual void OnRicochet(PenetrableObject penetrableObject)
        {
            bool worldObject = penetrableObject == null;

            // Here we are reducing the energy of the projectile based on whether the object hit the ground, or a penetrable object.
            float energyLoss = (worldObject == false) ? 0.01f : 0.035f;
            kEnergy *= energyLoss;

            if (kEnergy <= startingEnergy * 0.001f) Destroy(gameObject);

            ApplyForceToHitObject(penetrableObject, energyLoss * 0.005f);

            Vector3 inDir = transform.forward;
            float newVelocity = Mathf.Sqrt(2f * kEnergy / m_ProjectileData.Weight);

            // Here we will want to apply some de-normalization to the reflected angle to add some variation to the bullet's reflected path.
            _rigidbody.velocity = Vector3.Reflect(_rigidbody.velocity.normalized, _predictedImpactNormal) * newVelocity;      // Here we want to just decrease the kE on the projectile slightly

            transform.forward = _rigidbody.velocity.normalized;

            // We are just going to manually set the position of the projectile as we are getting inconsistancies in the position and the refected angle of the projectile
            transform.position = _predictedImpactPosition + (_rigidbody.velocity.normalized * (newVelocity * Time.fixedDeltaTime));

            if (debug) DebugRicochet(inDir);
        }

        protected virtual void OnPenetration(PenetrableObject penetratedObject, float thickness)
        {
            // The amount of energy we lose should be related to the thickness of the object we hit, and it's relative hardness. So an object with 1.0 hardness will slow a projectile down more than one with 0.7 hardness
            float energyLoss = ((energyLostThroughTravel * m_ProjectileData.ProjectileNoseShape) * (thickness)) / (1f / penetratedObject.RelativeHardness);
            float newVelocity = Mathf.Sqrt(Mathf.Abs(2f * (kEnergy - energyLoss) / m_ProjectileData.Weight));

            _rigidbody.velocity = _rigidbody.velocity.normalized * newVelocity;

            // Here we update the kE of the projectile, and work out the new penetration value for the object.
            kEnergy -= energyLoss;

            DealDamage(penetratedObject, m_ProjectileData.damageOnPenetration);
            ApplyForceToHitObject(penetratedObject, 0.001f);

            if (m_ProjectileData.constantPenetration == false) currentPenetration = m_ProjectileData.maxPenetration * (kEnergy / startingEnergy);
        }

        protected virtual void OnImpact(PenetrableObject penetratedObject, bool addForceToObject)
        {
            ApplyForceToHitObject(addForceToObject, 0.008f);

            DealDamage(penetratedObject, m_ProjectileData.damageOnImpact);

            Destroy(gameObject);
        }

        protected void ApplyForceToHitObject(bool applyForce, float energyToTransfer)
        {
            if (applyForce)
            {
                Rigidbody rb = _impactedCollider.gameObject.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    // kE = 1/2 * mass * velocity^2
                    // v = sqrt(2 * kE / m)
                    float velocity = Mathf.Sqrt(Mathf.Abs((2 * (kEnergy * energyToTransfer)) / rb.mass));

                    rb.AddForceAtPosition(-_predictedImpactNormal.normalized * velocity, _predictedImpactPosition, ForceMode.Impulse);
                }
            }
        }

        private Vector3 CalculateNormalization(float angle)
        {
            // In here we want to work out what the new direction of the projectile should be when it penetrates an object.
            Vector3 normalized = Vector3.RotateTowards(transform.forward, -_predictedImpactNormal, (m_ProjectileData.normalizationOnPenetration * (angle / m_ProjectileData.MinimumRicochetAngle)) * Mathf.Deg2Rad, 0.0f);

            if (debug) DebugNormalization(normalized);

            return normalized;
        }

        #region Debugging Methods
        private void DebugRicochet(Vector3 inDirection)
        {
            Debug.DrawRay(_predictedImpactPosition, inDirection, Color.red, 1f);
            Debug.DrawRay(_predictedImpactPosition, _predictedImpactNormal, Color.cyan, 1f);

            Debug.DrawRay(_predictedImpactPosition, transform.forward, Color.white, 1f);
        }

        private void DebugNormalization(Vector3 normalizedDirection)
        {

            Debug.DrawRay(_predictedImpactPosition, -_predictedImpactNormal, Color.cyan, 1f);
            Debug.DrawRay(_predictedImpactPosition, normalizedDirection, Color.white, 1f);
            Debug.DrawRay(_predictedImpactPosition, transform.forward, Color.red, 1f);
        }
        #endregion

        private void CheckForRigidbody()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();

                if (_rigidbody != null)
                {
                    return;
                }

                _rigidbody = gameObject.AddComponent<Rigidbody>();
            }
        }

        private void PredictImpactLocation(float distance)
        {
            // TO FIX THE ISSUE OF THE PROJECTILES PASSING THROUGH OBJECTS WE CAN
            // 1) Fire a raycast out from the projectile to see if will collide with an object in the next frame (Rays length is velocity * Time.fixedDeltaTime)
            float rayLength = velocity * Time.fixedDeltaTime;

            RaycastHit hit;

            if (UnityEngine.Physics.SphereCast(_projectileNose.position, m_ProjectileData.Calibre, transform.forward, out hit, distance, penetrableLayers))
            {
                _predictedCollision = true;
                _predictedImpactPosition = hit.point;   // Not sure if we want to set the position to the hit.point - (the projectiles forward direction and colliders Z length)

                _predictedImpactNormal = hit.normal;    // We are predicting what the angle of the surface we hit is going to be. **THIS IS BY NO MEANS ACCURATE** 

                _impactedCollider = hit.collider;       // We are storing the collider of the object we're anticipating to hit so that we can send messages and shit to it.
            }

            // 2) If the ray has hit something, we shall store the hit location and the normal data/angle of attack and set a boolean telling us that next frame we want to override the Unity calculated position
            // 3) In the next update, check to see if the collision boolean is true, and handle collision/ricochet/penetration logic.
        }

        private void DealDamage(PenetrableObject penetrableObject, float damage, float variation = 0.1f)
        {
            float damageToTake = Random.Range(damage * (1f - variation), damage * (1f + variation));

            penetrableObject.OnDamage(damageToTake, DamageSource.Projectile);
        }

        public void EnableTracer()
        {
            if (_tracer == null) _tracer = GetComponent<TrailRenderer>();

            if (m_ProjectileData.tracerActivationTime == 0f) _tracer.enabled = m_ProjectileData.isTracer = true;
            else StartCoroutine(IgniteTracer());
        }

        private IEnumerator IgniteTracer()
        {
            yield return new WaitForSeconds(m_ProjectileData.tracerActivationTime);

            _tracer.enabled = m_ProjectileData.isTracer = true;
        }

        public void StopTracer()
        {
            StopCoroutine("IgniteTracer");
            GetComponent<TrailRenderer>().enabled = false;
        }

        protected virtual void Destroy()
        {
            Destroy(gameObject);
        }
    }



    [System.Serializable]
    public class ProjectileData
    {
        [Header("General Parameters: ")]
        public float Weight;            // Measured in KG
        public float Calibre;           // Measured in Metres
        public float MuzzleVelocity;    // Measured in Metres Per Second

        [Tooltip("Refers to the roundness of the Projectile's nose. A value of 0; a blunt nosed projectile. A value of 1; completely round (Spherical)"), Range(0f, 1f), Space()]
        public float ProjectileNoseShape = 0.8f;

        [Header("Penetration Parameters: ")]
        public float MinimumRicochetAngle = 55f;
        [Tooltip("Projectile types such as HE and HEAT have constant penetration values over ANY distance.")]
        public bool constantPenetration = false;
        [Tooltip("The maximum thickness of an object this projectile can penetrate. (Metres)")]
        public float maxPenetration = 200.0f;

        [Header("Normalization Parameters: ")]
        [Tooltip("Change in the projectile's path (in °'s) either towards (positive) or away (negative) away from the impacted normal on collision with an object.")]
        public float normalizationOnPenetration = 5f;

        [Header("Projectile Damage Settings: ")]
        public float damageOnPenetration = 50f;
        public float damageOnImpact = 100f;

        [Header("Tracer Settings: ")]
        public bool isTracer = false;       // Cheers, Love! The cavalery's here!
        public Gradient tracerColour;
        public float tracerActivationTime = 0f;
    }

    public enum CollisionType
    {
        Ricochet,
        Penetration,
        Hit
    }
}