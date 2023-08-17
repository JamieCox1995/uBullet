using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotalDistraction.uBullet.Data;
using TotalDistraction.uBullet.Effects;
using TotalDistraction.uBullet.Utilities;

namespace TotalDistraction.uBullet.WeaponSystems
{
    public class Gun : Equippable
    {
        public GunSettings m_GunSettings;

        [Header("Aiming Settings: ")]
        public Vector3 defaultAimLocation;
        public Vector3 aimDownSightsLocation;
        public float aimSpeed = 28f;
        public Vector2 aimedSensitivity = new Vector2(1.5f, 1.5f);

        [Header("FOV Settings: ")]
        //public float standardFOV = 90f;
        public float aimedFOV = 55f;

        [Header("Weapon Components: ")]
        [Tooltip("The Transform should be placed at the end of the visual barrel of the gun. It should be positioned so that the forward of the transform is positioned in the direction of the barrel.")]
        public Transform barrelTransform;
        [Tooltip("Should be positioned where bullet casings are ejected from the weapon. The Transform's forward direction should be aligned with the bullet direction.")]
        public Transform shellEjector;
        public Transform sightForward;
        public GameObject muzzleFlash;

        [Space, Header("Animation and Audio: ")]
        public Animator weaponAnimator;
        public AudioSource weaponAudioSource;

        public WeaponAudioClips weaponAudioClips;
        private int remainingShots, remainingCarriedAmmo, currentFireMode = 0;
        private float timeBetweenShots, currentWeaponSpread, spreadTimer = 0f;
        private float movementInaccuracyModifier = 1f, hipfireInaccuracyModifier = 1f;
        private bool isBoltAction = false, readyToFire = false, reloadRequired = false, isReloading = false, isRunning = false, isAiming = false, resetSpread = false;

        private bool cycleBoltRequired = false;
        private bool resettingTrigger = false;

        // Use this for initialization
        void Start()
        {
            RetrieveAmmunitionObjects();

            m_GunSettings.currentFireType = m_GunSettings.supportedFireTypes[0];
            currentFireMode = 0;

            remainingCarriedAmmo = m_GunSettings.startingCarriedAmmo;
            remainingShots = m_GunSettings.magazineCapacity;

            timeBetweenShots = 1f / (m_GunSettings.roundsPerMinute / 60f);
            currentWeaponSpread = m_GunSettings.weaponSpread;

            if (m_GunSettings.supportedFireTypes.Contains(FireType.BoltAction))
            {
                isBoltAction = true;
            }

            if (weaponAnimator != null)
            {
                weaponAnimator.SetBool("Is Bolt Action", isBoltAction);
                weaponAnimator.SetBool("Reload In Progress", false);
            }

            muzzleFlash.SetActive(false);

            readyToFire = true;
        }

        private void OnEnable()
        {
            if (weaponAnimator != null)
            {
                weaponAnimator.SetBool("Is Bolt Action", isBoltAction);

                if (cycleBoltRequired && isBoltAction)
                {
                    weaponAnimator.Play("Cycle Bolt");
                }
            }
        }

        private void RetrieveAmmunitionObjects()
        {
            MunitionOptions options = Settings.Instance.InstanceMunitions.GetMunitionOptions(m_GunSettings.ammoType);
            m_GunSettings.bulletPrefab = options.munitionPrefab;
            m_GunSettings.casingPrefab = options.munitionCasing;
        }

        // Update is called once per frame
        void Update()
        {
            if (remainingShots == 0)
            {
                reloadRequired = true;
            }

            if (resetSpread)
            {
                spreadTimer += Time.deltaTime;

                if (spreadTimer >= m_GunSettings.recoilRecoveryDelay)
                {
                    float duration = spreadTimer - m_GunSettings.recoilRecoveryDelay;

                    currentWeaponSpread = Mathf.Lerp(currentWeaponSpread, m_GunSettings.weaponSpread, duration / m_GunSettings.recoilRecoveryDuration);
                }

                if (spreadTimer >= (m_GunSettings.recoilRecoveryDelay + m_GunSettings.recoilRecoveryDuration))
                {
                    resetSpread = false;
                    spreadTimer = 0f;
                }
            }

            HandleInput();
        }

        private void HandleInput()
        {
            #region Handle Player Movement
            Vector2 playerMovement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            movementInaccuracyModifier = Mathf.Lerp(1f, m_GunSettings.inaccuracyWhenWalking, playerMovement.magnitude);

            isRunning = Input.GetKey(KeyCode.LeftShift);
            /*weaponAnimator.SetFloat("Move Amount", playerMovement.magnitude);
            weaponAnimator.SetBool("Running", isRunning);*/
            #endregion

            #region Handle Weapon Firing
            // Checking to see if the player has tried to fire the gun.
            if (Input.GetMouseButton(0))
            {
                if (readyToFire == true && reloadRequired == false)
                {
                    Fire();
                }
            }

            // Checking to see if the player has released the trigger.
            if (Input.GetMouseButtonUp(0))
            {
                // If they have and the gun is in FireType.Single, we want to start the trigger reset.
                if (m_GunSettings.currentFireType == FireType.Single && readyToFire == false && resettingTrigger == false)
                {
                    if (m_GunSettings.currentFireType != FireType.BoltAction)
                    {
                        StartCoroutine(CycleWeaponTrigger(timeBetweenShots));                               // We use the gun's hypothetical fastest cycle time so that players with quick trigger fingers can shoot the gun as fast as possible.
                    }
                }
            }
            #endregion

            // Checking to see fi the player is trying to switch fire mode.
            if (Input.GetKeyDown(KeyCode.V) && m_GunSettings.supportedFireTypes.Length > 1)
            {
                ToggleFiremode();
            }

            // Checking to see if the player is trying to reload
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (remainingShots != m_GunSettings.magazineCapacity && remainingCarriedAmmo != 0)
                {
                    InitiateReload();
                }
            }

            // Checking to see if the player is trying to ADS (Aim Down Sights). This is an Animator ONLY action.
            if (weaponAnimator != null)
            {
                if (Input.GetMouseButton(1))
                {
                    if (isAiming == false)
                    {
                        isAiming = true;
                        weaponAnimator.SetBool("ADS", isAiming);

                        isRunning = false;
                        weaponAnimator.SetBool("Running", isRunning);
                    }

                    // Moving the Arm's location so the player is looking right down the sights
                    transform.localPosition = Vector3.Lerp(transform.localPosition, aimDownSightsLocation, Time.deltaTime * aimSpeed);
                    hipfireInaccuracyModifier = Mathf.Lerp(hipfireInaccuracyModifier, 1f, Time.deltaTime * aimSpeed);
                    viewCamera.fieldOfView = Mathf.Lerp(viewCamera.fieldOfView, aimedFOV, Time.deltaTime * aimSpeed);

                    // We should assume that the player cannot sprint and ADS at the same time and restrict the player's ability to sprint.
                }
                else
                {
                    if (isAiming == true)
                    {
                        isAiming = false;
                        weaponAnimator.SetBool("ADS", isAiming);
                    }

                    transform.localPosition = Vector3.Lerp(transform.localPosition, defaultAimLocation, Time.deltaTime * aimSpeed);
                    hipfireInaccuracyModifier = Mathf.Lerp(hipfireInaccuracyModifier, m_GunSettings.hipFireInAccuracy, Time.deltaTime * aimSpeed);
                    viewCamera.fieldOfView = Mathf.Lerp(viewCamera.fieldOfView, Settings.Instance.PlayerFOV, Time.deltaTime * aimSpeed);
                }
            }
        }

        public void Fire()
        {
            // We will handle each type of firemode in here.
            switch (m_GunSettings.currentFireType)
            {
                case FireType.BoltAction:
                    FireSingle();
                    break;
                case FireType.Single:
                    FireSingle();
                    break;
                case FireType.Burst:
                    FireBurst();
                    break;
                case FireType.FullAuto:
                    FireFullAuto();
                    break;
            }

            if (isBoltAction) weaponAnimator.GetBehaviour<AnimatorPlaySound>().audioClip = weaponAudioClips.boltCycle;
            cycleBoltRequired = true;
        }

        #region Bullet Firing
        /// <summary>
        /// Fires a single bullet from the gun. For the weapon to continue shooting, the player must release the fire button.
        /// </summary>
        private void FireSingle()
        {
            // Spawn Bullet, Muzzle Effect and Casing
            SpawnBullet();

            // Randomize Bullet Direction

            // Decrement Remaining Ammo
            remainingShots--;

            // Gun to no longer ready to shoot
            readyToFire = false;
        }

        /// <summary>
        /// Fires a set number of bullets in quick succession.
        /// </summary>
        private void FireBurst()
        {
            // The functionality of burst fire is handled by a coroutine so that the bursts cannot be interrupted.
            StartCoroutine(BurstFire());
        }

        /// <summary>
        /// Fires the weapon for as long as the weapon has ammo and as quickly as it can shoot.
        /// </summary>
        private void FireFullAuto()
        {
            // Spawn Bullet, Muzzle Effect and Casing
            SpawnBullet();

            // Randomize Bullet Direction

            // Decrement Remaining Ammo
            remainingShots--;

            // Gun to no longer ready to shoot
            StartCoroutine(CycleWeaponTrigger(timeBetweenShots));
        }

        private IEnumerator BurstFire()
        {
            readyToFire = false;

            int leftToShoot = (remainingShots < m_GunSettings.shotsPerBurst) ? remainingShots : m_GunSettings.shotsPerBurst;

            while (leftToShoot > 0)
            {
                // Spawn Bullet, Muzzle Effect and Casing
                SpawnBullet();

                // Randomize Bullet Direction

                // Decrement Remaining Ammo
                leftToShoot--;
                remainingShots--;

                // Wait for the weapon to become ready
                yield return new WaitForSeconds(timeBetweenShots);
            }

            // Now we wait for the tirgger to reset.
            StartCoroutine(CycleWeaponTrigger(m_GunSettings.triggerResetTime));
        }

        private IEnumerator CycleWeaponTrigger(float time)
        {
            readyToFire = false;
            resettingTrigger = true;

            yield return new WaitForSeconds(time);

            readyToFire = true;
            resettingTrigger = false;
        }
        #endregion

        private void ToggleFiremode()
        {
            currentFireMode++;

            if (currentFireMode >= m_GunSettings.supportedFireTypes.Length)
            {
                currentFireMode = 0;
            }

            m_GunSettings.currentFireType = m_GunSettings.supportedFireTypes[currentFireMode];

            if (weaponAnimator != null) weaponAnimator.SetTrigger("Change Firemode");
        }

        private void InitiateReload()
        {
            // If the weapon has an animator, we shall trigger the animation and update the remaining shots.
            if (weaponAnimator != null)
            {
                if (!isBoltAction)
                {
                    weaponAudioSource.clip = weaponAudioClips.reloadClip;
                    weaponAudioSource.Play();
                }
                else
                {
                    AnimatorPlaySound playsound = weaponAnimator.GetBehaviour<AnimatorPlaySound>();
                    playsound.audioClip = weaponAudioClips.reloadClip;
                }

                weaponAnimator.SetInteger("Remaining Ammo", remainingShots);
                weaponAnimator.SetTrigger("Reload");

                weaponAnimator.SetBool("Reload In Progress", true);

                reloadRequired = false;
            }

            // If the animator is null, we just want to call the reload method.
            if (weaponAnimator == null)
            {
                if (isBoltAction) ReloadSingle();
                else Reload();
            }
        }

        public void Reload()
        {
            bool fullReload = remainingShots == 0;

            if (weaponAudioSource.isPlaying) weaponAudioSource.Stop();

            // We are checking to see if the weapon is being fully reloaded, i.e. the magazine has no ammo left.
            if (fullReload == true)
            {
                int avaliable = Mathf.Min(remainingCarriedAmmo, m_GunSettings.magazineCapacity);
                remainingShots = avaliable;

                remainingCarriedAmmo -= avaliable;
            }
            else
            {
                int toReload = m_GunSettings.magazineCapacity - remainingShots;

                int avaliable = Mathf.Min(toReload, remainingCarriedAmmo);
                remainingShots += avaliable;

                remainingCarriedAmmo -= avaliable;
            }

            readyToFire = true;
            reloadRequired = false;
        }

        /// <summary>
        /// Used for reloading a single bullet into a bolt action rifle.
        /// </summary>
        public void ReloadSingle()
        {
            if (weaponAudioSource.isPlaying) weaponAudioSource.Stop();

            remainingShots++;
            remainingCarriedAmmo--;

            readyToFire = true;
            reloadRequired = false;

            // Checking to see if we can continue loading in bullets
            if (remainingShots != m_GunSettings.magazineCapacity)
            {
                InitiateReload();
            }
            else
            {
                weaponAnimator.SetBool("Reload In Progress", false);
            }
        }

        public void CycleBolt()
        {
            readyToFire = true;
            cycleBoltRequired = false;
        }

        private void SpawnBullet()
        {
            if (weaponAnimator != null) weaponAnimator.SetTrigger("Fire");

            if (weaponAudioSource.clip != weaponAudioClips.shotClip) weaponAudioSource.clip = weaponAudioClips.shotClip;
            weaponAudioSource.Play();

            StopCoroutine(MuzzleFlash());
            StartCoroutine(MuzzleFlash());

            Vector3 direction = GenerateRandomDirection();      // Creating a random direction for the bullet to travel in within the weapons spread arc
            GameObject spawnedBullet = Instantiate(m_GunSettings.bulletPrefab, barrelTransform.position, Quaternion.identity);
            spawnedBullet.transform.forward = direction;

            WeaponRecoil();

            if (m_GunSettings.hasTracers == true)
            {
                int shotsFired = m_GunSettings.magazineCapacity - remainingShots;

                if (shotsFired % m_GunSettings.tracerEveryNBullet == 0)
                {
                    spawnedBullet.GetComponent<Projectile>().EnableTracer();
                }
                else
                {
                    spawnedBullet.GetComponent<Projectile>().StopTracer();
                }
            }

            if (m_GunSettings.spawnBulletCasing == true)
            {
                // Spawn Casing
                GameObject casing = Instantiate(m_GunSettings.casingPrefab, shellEjector.position, Quaternion.identity);
                casing.transform.forward = shellEjector.forward;

                Rigidbody rb = casing.GetComponent<Rigidbody>();

                Vector3 randomTorque = new Vector3(Random.Range(m_GunSettings.caseEjectionSettings.rotation.x, m_GunSettings.caseEjectionSettings.rotation.y),
                    Random.Range(m_GunSettings.caseEjectionSettings.rotation.x, m_GunSettings.caseEjectionSettings.rotation.y),
                    Random.Range(m_GunSettings.caseEjectionSettings.rotation.x, m_GunSettings.caseEjectionSettings.rotation.y)) * Time.deltaTime;

                rb.AddRelativeTorque(randomTorque);

                Vector3 randomDirection = new Vector3(Random.Range(m_GunSettings.caseEjectionSettings.xForce.x, m_GunSettings.caseEjectionSettings.xForce.y),
                    Random.Range(m_GunSettings.caseEjectionSettings.yForce.x, m_GunSettings.caseEjectionSettings.yForce.y), 0f);

                rb.AddRelativeForce(randomDirection);
            }
        }

        private bool CheckForQuickCollision()
        {
            bool result = false;

            RaycastHit hit;

            if (UnityEngine.Physics.Raycast(barrelTransform.position, barrelTransform.forward, out hit, m_GunSettings.bulletPrefab.GetComponent<Projectile>().m_ProjectileData.MuzzleVelocity * Time.fixedDeltaTime))
            {
                result = true;
            }

            return result;
        }

        private IEnumerator MuzzleFlash()
        {
            muzzleFlash.SetActive(true);

            float time = Mathf.Min(timeBetweenShots, 0.1f);

            yield return new WaitForSeconds(time);

            muzzleFlash.SetActive(false);
        }

        private Vector3 GenerateRandomDirection()
        {
            Vector3 end = barrelTransform.position + (barrelTransform.forward * 100f);
            Vector3 point = (Random.insideUnitSphere * ((currentWeaponSpread * movementInaccuracyModifier) * hipfireInaccuracyModifier)) + end;

            return point - barrelTransform.position;
        }

        private void WeaponRecoil()
        {
            resetSpread = true;
            float recoilGain = (m_GunSettings.maxSpreadDueToShooting - m_GunSettings.weaponSpread) / m_GunSettings.shotsToReachMax;

            currentWeaponSpread += recoilGain;
            currentWeaponSpread = Mathf.Clamp(currentWeaponSpread, m_GunSettings.weaponSpread, m_GunSettings.maxSpreadDueToShooting);

            spreadTimer = 0f;
        }

        private void OnValidate()
        {
            if (m_GunSettings.startingCarriedAmmo > m_GunSettings.maximumCarriedAmmo)
            {
                m_GunSettings.startingCarriedAmmo = m_GunSettings.maximumCarriedAmmo;
            }
        }

        public override int RemainingShots()
        {
            return remainingShots;
        }

        public override int TotalCarriedAmmo()
        {
            return remainingCarriedAmmo;
        }
    }

    [System.Serializable]
    public class GunSettings
    {
        [Header("Weapon Accuracy Settings: ")]
        [Tooltip("Size of the weapon's grouping at 100m distance when the gun is first fired.")]
        public float weaponSpread = 0.1f;
        public float maxSpreadDueToShooting = 2f;
        public int shotsToReachMax = 5;

        [Header("Recoil Recovery Settings: ")]
        public float recoilRecoveryDelay = 2f;
        public float recoilRecoveryDuration = 0.5f;

        [Header("Inaccuracy Modifiers: ")]
        public float inaccuracyWhenWalking = 1.5f;
        [Tooltip("The change in the weapons accuracy when the player is firing from the hip (Not Aiming Down Sights)")]
        public float hipFireInAccuracy = 3f;

        [Header("Rate Of Fire Settings: ")]
        [Tooltip("The types of firemodes which are avaliable on this weapon.")]
        public FireType[] supportedFireTypes = new FireType[] { FireType.Single };
        [Space]
        public FireType currentFireType = FireType.Single;
        [Tooltip("If the weapon supports the Burst FireType, this is how many shots will be fired per burst.")]
        public int shotsPerBurst = 3;

        [Tooltip("The fastest the weapon can shoot within the space of a minute.")]
        public float roundsPerMinute = 600f;
        [Tooltip("The amount of time it takes for the weapon to reset after the trigger is pulled. This will affect the Burst FireType")]
        public float triggerResetTime = 0.5f;

        [Header("Ammunition Capacity Settings: ")]
        public int magazineCapacity = 15;
        public int maximumCarriedAmmo = 150;
        public int startingCarriedAmmo = 150;

        [Header("Ammunition Object Settings: ")]
        [ProjectileType]
        public string ammoType = "9mm";
        public bool spawnBulletCasing = false;
        public CasingEjectionSettings caseEjectionSettings;
        public GameObject bulletPrefab, casingPrefab;

        [Header("Magazine Settings: ")]
        public bool hasTracers = false;
        public int tracerEveryNBullet = 5;

        [Header("Weapon Parts Settings:")]

        [Header("Effect Settings: ")]
        public GameObject muzzleEffect;
    }

    [System.Serializable]
    public class WeaponAudioClips
    {
        public AudioClip shotClip;
        public AudioClip reloadClip;
        public AudioClip boltCycle;
    }

    [System.Serializable]
    public class CasingEjectionSettings
    {
        public Vector2 xForce;
        public Vector2 yForce;
        public Vector2 rotation;
    }

    [System.Serializable]
    public enum FireType
    {
        BoltAction, Single, Burst, FullAuto
    }
}