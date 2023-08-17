using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TotalDistraction.uBullet.Data;
using TotalDistraction.uBullet.WeaponSystems;

namespace TotalDistraction.uBullet.Physics
{
    public class PenetrableObject : MonoBehaviour
    {
        [Header("Object World Settings: ")]
        [Tooltip("If the object is penetrable from any direction, us are going to use the Bounding Box to calculate the ")]
        public bool use3dCollider = true;
        [Tooltip("The thickness of the object in the Z direction.")]
        public float Thickness = 0.1f;

        [Header("Object Material Settings: ")]
        public MaterialType _materialType = MaterialType.Metal;
        [Tooltip("This is the objects relative hardness compared to 100mm of rolled steel.")]
        public float RelativeHardness = 1f;

        [Header("Hit Effect Prefabs: ")]
        public HitEffect _hitEffect;

        // This is storing all of the DamagambleObject Components which are attached to this object.
        private DamagableObject[] damagableObjects;

        private void Start()
        {
            damagableObjects = gameObject.GetComponentsInChildren<DamagableObject>();
        }

        /// <summary>
        /// Retrieves the thickness of the object at an angle of attack.
        /// </summary>
        /// <param name="angleOfAttack"> Direction of penetration entering the object. </param>
        /// <param name="normal"> This is the normal of the object's surface being hit. We use this for simplicity of not having to check every axis of the object. </param>
        /// <returns>Returns the Line of Sight Thickness for the object given the normal and Angle of Attack </returns>
        public float GetObjectThickness(Vector3 angleOfAttack, Vector3 normal)
        {
            // 'lost' refers to the 'Line of Sight Thickness' which will be calcualated
            float lost = 0f;

            // We get the Dot Product of the projectile direction and the normal, we invert the direction of the face normal to get the result in the range of 0-90 degrees.
            float dot = Vector3.Dot(angleOfAttack, -normal);

            lost = Mathf.Abs(Thickness / dot);

            return lost * RelativeHardness;
        }

        private float GetDirectionalThickness(Vector3 normal)
        {
            if (use3dCollider == false)
            {
                return Thickness;
            }

            float thick = 0f;

            // Here we want to do some way of calculating where the other side of the object is at any rotation,
            // This could use another raycast from the opposite side of the object.

            Debug.LogFormat("The 3d Thickness of the object was {0}m", thick);

            return thick * 1000f;
        }

        public float GetNominalThickness()
        {
            return Thickness * RelativeHardness;
        }

        /// <summary>
        /// Called when a projectile impacts this. This can be used to spawn an effect when a projectile hits the object.
        /// </summary>
        public void OnImpact(ImpactData data, HitEffect impactEffects)
        {
            ImpactEffects _impactEffects = impactEffects.GetMaterialImpactEffects(this._materialType);

            if (data._collisionType == CollisionType.Ricochet)
            {
                // By default, we just want to spawn in a ricochet effect and play a sound.
                GameObject effect = Instantiate(_impactEffects.ricochetEffect, transform);

                effect.transform.position = data._impactPosition;
                effect.transform.forward = data._impactedNormal;

                AudioSource.PlayClipAtPoint(_impactEffects.ricochetSFX.clip, data._impactPosition, _impactEffects.ricochetSFX.volume);

                return;
            }
            else if (data._collisionType == CollisionType.Penetration)
            {
                // When a projectile fully penetrates an object, we want to spawn in 2 effects; One at the impact point
                // in the opposite direction to the projectile, and the other at the exit point of the projectile (ImpactPoint + (ProjectileDirection * (Line of Sight Thickness / 1000f)))
                // in the direction of the projectile.

                GameObject effect;
                // Spawning the entry effect
                effect = Instantiate(_impactEffects.penetrationEntranceEffect, data._impactPosition, Quaternion.identity, transform);

                effect.transform.forward = -data._impactedNormal;

                // Now we want to calculate the exit location of the projectile
                // First of all, we want to calculate the distance the projectile has to travel through the object in METRES
                //float travelDistance = Mathf.Abs(data._penetrationDistance / Vector3.Dot(data._penetrationDirection, -data._impactedNormal));
                Vector3 exitLocation = data._impactPosition + (data._penetrationDirection.normalized * data._penetrationDistance);
                effect = Instantiate(_impactEffects.penetrationExitEffect, exitLocation, Quaternion.identity, transform);

                effect.transform.forward = -data._penetrationDirection;
            }
            else
            {
                // By default, we just want to spawn in an impact effect and play a sound. This could be the same as
                // the ricochet effect and sound, but we'll separate them for the moment.
                GameObject effect;
                effect = Instantiate(_impactEffects.hitEffect, data._impactPosition, Quaternion.identity, transform);

                effect.transform.forward = data._impactedNormal;
            }

            PlayRandomSound(_impactEffects.impactClips, data._impactPosition);
        }

        private void PlayRandomSound(AudioEffect[] clips, Vector3 soundPosition)
        {
            int index = UnityEngine.Random.Range(0, clips.Length);
            AudioEffect clip = clips[index];

            AudioSource.PlayClipAtPoint(clip.clip, soundPosition, clip.volume);
        }

        private void OnValidate()
        {
            // Here we are going to set the objects Z scale of the object to Thickness
            if (!use3dCollider)
            {
                Vector3 newScale = transform.localScale;

                newScale.z = Thickness;

                transform.localScale = newScale;
            }
        }

        public void OnDamage(float value, DamageSource damageSource)
        {
            foreach (DamagableObject damagable in damagableObjects)
            {
                damagable.OnDamageTaken(value, damageSource);
            }
        }
    }

    public class ImpactData
    {
        public string _projectileType;
        public Vector3 _impactPosition;
        public Vector3 _impactedNormal;

        public float _penetrationDistance;

        public CollisionType _collisionType;
        public Vector3 _penetrationDirection;

        public ImpactData()
        {
            _impactPosition = Vector3.zero;
            _impactedNormal = Vector3.up;

            _collisionType = CollisionType.Ricochet;
            _penetrationDirection = _impactedNormal;
        }

        public ImpactData(Vector3 position, Vector3 normal, CollisionType colType)
        {
            _impactPosition = position;
            _impactedNormal = normal;

            _collisionType = colType;
        }

        public ImpactData(Vector3 position, Vector3 normal, CollisionType colType, Vector3 penDirection, float penDistance)
        {
            _impactPosition = position;
            _impactedNormal = normal;

            _collisionType = colType;
            _penetrationDirection = penDirection;
            _penetrationDistance = penDistance;
        }
    }
}
