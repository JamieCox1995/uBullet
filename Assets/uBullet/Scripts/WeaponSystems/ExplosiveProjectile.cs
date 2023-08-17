using System.Collections;
using System.Collections.Generic;
using TotalDistraction.uBullet.Physics;
using UnityEngine;

namespace TotalDistraction.uBullet.WeaponSystems
{
    [RequireComponent(typeof(Explosive))]
    public class ExplosiveProjectile : Projectile
    {
        [Header("Projectile Charge Settings: ")]
        public bool _explodeOnImpact = false;
        private Explosive _explosive;

        protected override void Start()
        {
            base.Start();

            _explosive = GetComponent<Explosive>();
        }

        protected override void Update()
        {
            //_explosive.SetExplosivePosition(_projectileNose.transform.position);

            base.Update();
        }

        protected override void OnProjectileCollision()
        {
            _explosive.TriggerExplosion(!_explodeOnImpact);

            base.OnProjectileCollision();
        }

        protected override void OnImpact(PenetrableObject penetratedObject, bool addForceToObject)
        {
            // In here do we want to spawn an object that is a delayed explosion? This would mean that there would still be an explosion
            // occuring even if the projectile impacts?
            //if (!_explodeOnImpact && _explosive.IsArmed == false)
            //{
            //GameObject delayed = new GameObject("Delayed Explosion");

            //Explosive explo = delayed.AddComponent<Explosive>();
            //explo._explosiveSettings = _explosive._explosiveSettings;

            //explo.SetRootGameObject(delayed);
            //explo.TriggerExplosion(true);
            //}

            //base.OnImpact(penetratedObject, addForceToObject);

            // When a projectile IMPACTS but does not penetrate an object, we want to just make the projectile "stick" into the surface.
            // Setting the position to the predicted impact location, and setting our velocity to 0.

            transform.position = _predictedImpactPosition;
            transform.parent = penetratedObject.transform;

            ApplyForceToHitObject(penetratedObject.GetComponent<Rigidbody>(), 0.8f);

            _rigidbody.velocity = Vector3.zero;
            _rigidbody.isKinematic = true;
            transform.forward = -_predictedImpactNormal;

            penetratedObject.OnDamage(m_ProjectileData.damageOnImpact, DamageSource.Projectile);

            this.enabled = false;
        }

        protected override void Destroy()
        {
            _explosive.TriggerExplosion(false);
        }
    }
}
