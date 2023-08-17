using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotalDistraction.uBullet.WeaponSystems;
using TotalDistraction.uBullet.Physics;

[RequireComponent(typeof(Explosive))]
public class FuelContainer : DamagableObject
{
    [Header("Additional Settings:"), Range(0f, 1f)]
    public float chanceToExplodeOnDamaged = 0.025f;

    private Explosive _explosive;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        _explosive = GetComponent<Explosive>();
    }

    public override void OnDamageTaken(float damageTaken, DamageSource damageSource)
    {
        if (isDamagable)
        {
            remainingHitPoints -= damageTaken * (1f - damageResistance);

            CheckObjectCondition();

            if (damageSource == DamageSource.Projectile && Random.Range(0, 1f) <= chanceToExplodeOnDamaged)
            {
                _explosive.TriggerExplosion(false);
            }

            onObjectDamaged.Invoke();
        }
    }

    public override void OnDamaged()
    {
        //throw new System.NotImplementedException();
    }

    public override void OnDestroyed()
    {
        _explosive._explosiveSettings.hasExploded = true;
        //throw new System.NotImplementedException();
    }

    public override void OnMalfunctioning()
    {
        //throw new System.NotImplementedException();
    }
}
