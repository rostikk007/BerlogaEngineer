using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance { get { return instance; } }

    [Header("Звуки игрока")]
    [Tooltip("Источник звука для шагов")]
    public AudioSource footstepsSource;
    [Tooltip("Массив звуков шагов")]
    public AudioClip[] footstepSounds;
    [Tooltip("Минимальный интервал между звуками шагов")]
    public float footstepInterval = 0.3f;
    [Tooltip("Громкость шагов")]
    [Range(0, 1)]
    public float footstepsVolume = 0.5f;

    [Header("Звуки врагов")]
    [Tooltip("Звук атаки жука")]
    public AudioClip bugAttackSound;
    [Tooltip("Звук смерти жука")]
    public AudioClip bugDeathSound;
    [Tooltip("Звук атаки жука дальнего боя")]
    public AudioClip rangedBugAttackSound;
    [Tooltip("Звук смерти жука дальнего боя")]
    public AudioClip rangedBugDeathSound;
    [Tooltip("Звук появления жука")]
    public AudioClip bugSpawnSound;
    [Tooltip("Звук появления жука дальнего боя")]
    public AudioClip rangedBugSpawnSound;
    [Tooltip("Громкость звуков жуков")]
    [Range(0, 1)]
    public float bugSoundsVolume = 0.7f;

    [Header("Звуки урона игрока")]
    [Tooltip("Звук получения урона игроком")]
    public AudioClip playerDamageSound;
    [Tooltip("Звук смерти игрока")]
    public AudioClip playerDeathSound;
    [Tooltip("Громкость звуков урона игрока")]
    [Range(0, 1)]
    public float playerDamageSoundsVolume = 0.7f;

    [Header("Звуки турелей")]
    [Tooltip("Звук выстрела обычной турели")]
    public AudioClip basicTurretShot;
    [Tooltip("Звук выстрела огнемета")]
    public AudioClip flamethrowerShot;
    [Tooltip("Звук выстрела снайперской турели")]
    public AudioClip sniperTurretShot;
    [Tooltip("Звук отталкивателя")]
    public AudioClip repulsorSound;
    [Tooltip("Звук смерти турели")]
    public AudioClip turretDeath;
    [Tooltip("Звук появления турели")]
    public AudioClip turretSpawn;
    [Tooltip("Громкость звуков турелей")]
    [Range(0, 1)]
    public float turretSoundsVolume = 0.7f;

    [Header("Звуки пасеки")]
    [Tooltip("Звук добычи меда")]
    public AudioClip honeyHarvest;
    [Tooltip("Звук смерти пасеки")]
    public AudioClip apiaryDeath;
    [Tooltip("Звук появления пасеки")]
    public AudioClip apiarySpawn;
    [Tooltip("Громкость звуков пасеки")]
    [Range(0, 1)]
    public float apiarySoundsVolume = 0.6f;

    [Header("Звуки переработчика железа")]
    [Tooltip("Звук добычи железа")]
    public AudioClip ironProcessingSound;
    [Tooltip("Звук появления переработчика")]
    public AudioClip ironProcessorSpawn;
    [Tooltip("Звук уничтожения переработчика")]
    public AudioClip ironProcessorDestroy;
    [Tooltip("Громкость звуков переработчика")]
    [Range(0, 1)]
    public float ironProcessorSoundsVolume = 0.6f;

    [Header("Звуки стен")]
    [Tooltip("Звук разрушения стены")]
    public AudioClip wallDeath;
    [Tooltip("Звук появления стены")]
    public AudioClip wallSpawn;
    [Tooltip("Громкость звуков стен")]
    [Range(0, 1)]
    public float wallSoundsVolume = 0.5f;

    [Header("Общие звуки построек")]
    [Tooltip("Звук появления постройки")]
    public AudioClip buildingSpawnSound;
    [Tooltip("Звук получения урона постройкой")]
    public AudioClip buildingDamagedSound;
    [Tooltip("Звук уничтожения постройки")]
    public AudioClip buildingDestroyedSound;
    [Tooltip("Громкость общих звуков построек")]
    [Range(0, 1)]
    public float buildingSoundsVolume = 0.6f;

    [Header("Звуки повреждений построек")]
    [Tooltip("Звук получения урона турелью")]
    public AudioClip turretDamaged;
    [Tooltip("Звук получения урона пасекой")]
    public AudioClip apiaryDamaged;
    [Tooltip("Звук получения урона стеной")]
    public AudioClip wallDamaged;
    [Tooltip("Минимальный интервал между звуками повреждений")]
    public float damageSoundInterval = 0.2f;
    [Tooltip("Громкость звуков повреждений")]
    [Range(0, 1)]
    public float damageSoundsVolume = 0.6f;

    [Header("Звуки попаданий")]
    [Tooltip("Звук попадания пули по врагу")]
    public AudioClip bulletHitEnemy;
    [Tooltip("Звук попадания снаряда врага по игроку")]
    public AudioClip projectileHitPlayer;
    [Tooltip("Звук попадания снаряда врага по постройке")]
    public AudioClip projectileHitBuilding;
    [Tooltip("Громкость звуков попадания")]
    [Range(0, 1)]
    public float hitSoundsVolume = 0.6f;

    [Header("Звуки строительства")]
    [Tooltip("Звук процесса строительства")]
    public AudioClip buildingConstructionSound;
    [Tooltip("Громкость звуков строительства")]
    [Range(0, 1)]
    public float constructionSoundsVolume = 0.7f;

    [Header("Звуки ИВИ-21")]
    [Tooltip("Звук работы антенны")]
    public AudioClip antennaWorkingSound;
    [Tooltip("Громкость звуков антенны")]
    [Range(0, 1)]
    public float antennaSoundsVolume = 0.6f;

    private float lastTurretDamageTime;
    private float lastApiaryDamageTime;
    private float lastWallDamageTime;
    private float lastBuildingDamageTime;
    private float lastIronProcessorDamageTime;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Player Sounds
    public void PlayFootstep()
    {
        if (footstepSounds.Length > 0 && footstepsSource != null)
        {
            AudioClip randomFootstep = footstepSounds[Random.Range(0, footstepSounds.Length)];
            footstepsSource.PlayOneShot(randomFootstep, footstepsVolume);
        }
    }

    public void PlayPlayerDamage(Vector3 position)
    {
        if (playerDamageSound != null)
        {
            AudioSource.PlayClipAtPoint(playerDamageSound, position, playerDamageSoundsVolume);
        }
    }

    public void PlayPlayerDeath(Vector3 position)
    {
        if (playerDeathSound != null)
        {
            AudioSource.PlayClipAtPoint(playerDeathSound, position, playerDamageSoundsVolume);
        }
    }
    #endregion

    #region Enemy Sounds
    public void PlayBugAttack(Vector3 position)
    {
        if (bugAttackSound != null)
        {
            AudioSource.PlayClipAtPoint(bugAttackSound, position, bugSoundsVolume);
        }
    }

    public void PlayBugDeath(Vector3 position)
    {
        if (bugDeathSound != null)
        {
            AudioSource.PlayClipAtPoint(bugDeathSound, position, bugSoundsVolume);
        }
    }

    public void PlayRangedBugAttack(Vector3 position)
    {
        if (rangedBugAttackSound != null)
        {
            AudioSource.PlayClipAtPoint(rangedBugAttackSound, position, bugSoundsVolume);
        }
    }

    public void PlayRangedBugDeath(Vector3 position)
    {
        if (rangedBugDeathSound != null)
        {
            AudioSource.PlayClipAtPoint(rangedBugDeathSound, position, bugSoundsVolume);
        }
    }

    public void PlayBugSpawn(Vector3 position)
    {
        if (bugSpawnSound != null)
        {
            AudioSource.PlayClipAtPoint(bugSpawnSound, position, bugSoundsVolume);
        }
    }

    public void PlayRangedBugSpawn(Vector3 position)
    {
        if (rangedBugSpawnSound != null)
        {
            AudioSource.PlayClipAtPoint(rangedBugSpawnSound, position, bugSoundsVolume);
        }
    }
    #endregion

    #region Turret Sounds
    public void PlayBasicTurretShot(Vector3 position)
    {
        if (basicTurretShot != null)
        {
            AudioSource.PlayClipAtPoint(basicTurretShot, position, turretSoundsVolume);
        }
    }

    public void PlayFlamethrowerShot(Vector3 position)
    {
        if (flamethrowerShot != null)
        {
            AudioSource.PlayClipAtPoint(flamethrowerShot, position, turretSoundsVolume);
        }
    }

    public void PlaySniperTurretShot(Vector3 position)
    {
        if (sniperTurretShot != null)
        {
            AudioSource.PlayClipAtPoint(sniperTurretShot, position, turretSoundsVolume);
        }
    }

    public void PlayRepulsorSound(Vector3 position)
    {
        if (repulsorSound != null)
        {
            AudioSource.PlayClipAtPoint(repulsorSound, position, turretSoundsVolume);
        }
    }

    public void PlayTurretDeath(Vector3 position)
    {
        if (turretDeath != null)
        {
            AudioSource.PlayClipAtPoint(turretDeath, position, turretSoundsVolume);
        }
    }

    public void PlayTurretSpawn(Vector3 position)
    {
        if (turretSpawn != null)
        {
            AudioSource.PlayClipAtPoint(turretSpawn, position, turretSoundsVolume);
        }
    }
    #endregion

    #region Apiary Sounds
    public void PlayHoneyHarvest(Vector3 position)
    {
        if (honeyHarvest != null)
        {
            AudioSource.PlayClipAtPoint(honeyHarvest, position, apiarySoundsVolume);
        }
    }

    public void PlayApiaryDeath(Vector3 position)
    {
        if (apiaryDeath != null)
        {
            AudioSource.PlayClipAtPoint(apiaryDeath, position, apiarySoundsVolume);
        }
    }

    public void PlayApiarySpawn(Vector3 position)
    {
        if (apiarySpawn != null)
        {
            AudioSource.PlayClipAtPoint(apiarySpawn, position, apiarySoundsVolume);
        }
    }
    #endregion

    #region Iron Processor Sounds
    public void PlayIronProcessing(Vector3 position)
    {
        if (ironProcessingSound != null)
        {
            AudioSource.PlayClipAtPoint(ironProcessingSound, position, ironProcessorSoundsVolume);
        }
    }

    public void PlayIronProcessorSpawn(Vector3 position)
    {
        if (ironProcessorSpawn != null)
        {
            AudioSource.PlayClipAtPoint(ironProcessorSpawn, position, ironProcessorSoundsVolume);
        }
    }

    public void PlayIronProcessorDestroy(Vector3 position)
    {
        if (ironProcessorDestroy != null)
        {
            AudioSource.PlayClipAtPoint(ironProcessorDestroy, position, ironProcessorSoundsVolume);
        }
    }
    #endregion

    #region Wall Sounds
    public void PlayWallDeath(Vector3 position)
    {
        if (wallDeath != null)
        {
            AudioSource.PlayClipAtPoint(wallDeath, position, wallSoundsVolume);
        }
    }

    public void PlayWallSpawn(Vector3 position)
    {
        if (wallSpawn != null)
        {
            AudioSource.PlayClipAtPoint(wallSpawn, position, wallSoundsVolume);
        }
    }
    #endregion

    #region General Building Sounds
    public void PlayBuildingConstruction(Vector3 position)
    {
        if (buildingConstructionSound != null)
        {
            AudioSource.PlayClipAtPoint(buildingConstructionSound, position, constructionSoundsVolume);
        }
    }

    public void PlayBuildingSpawn(Vector3 position)
    {
        if (buildingSpawnSound != null)
        {
            AudioSource.PlayClipAtPoint(buildingSpawnSound, position, buildingSoundsVolume);
        }
    }

    public void PlayBuildingDamaged(Vector3 position)
    {
        if (buildingDamagedSound != null && Time.time - lastBuildingDamageTime >= damageSoundInterval)
        {
            AudioSource.PlayClipAtPoint(buildingDamagedSound, position, buildingSoundsVolume);
            lastBuildingDamageTime = Time.time;
        }
    }

    public void PlayBuildingDestroyed(Vector3 position)
    {
        if (buildingDestroyedSound != null)
        {
            AudioSource.PlayClipAtPoint(buildingDestroyedSound, position, buildingSoundsVolume);
        }
    }
    #endregion

    #region Damage Sounds
    public void PlayTurretDamaged(Vector3 position)
    {
        if (turretDamaged != null && Time.time - lastTurretDamageTime >= damageSoundInterval)
        {
            AudioSource.PlayClipAtPoint(turretDamaged, position, damageSoundsVolume);
            lastTurretDamageTime = Time.time;
        }
    }

    public void PlayApiaryDamaged(Vector3 position)
    {
        if (apiaryDamaged != null && Time.time - lastApiaryDamageTime >= damageSoundInterval)
        {
            AudioSource.PlayClipAtPoint(apiaryDamaged, position, damageSoundsVolume);
            lastApiaryDamageTime = Time.time;
        }
    }

    public void PlayWallDamaged(Vector3 position)
    {
        if (wallDamaged != null && Time.time - lastWallDamageTime >= damageSoundInterval)
        {
            AudioSource.PlayClipAtPoint(wallDamaged, position, damageSoundsVolume);
            lastWallDamageTime = Time.time;
        }
    }
    #endregion

    #region Hit Sounds
    public void PlayBulletHitEnemy(Vector3 position)
    {
        if (bulletHitEnemy != null)
        {
            AudioSource.PlayClipAtPoint(bulletHitEnemy, position, hitSoundsVolume);
        }
    }

    public void PlayProjectileHitPlayer(Vector3 position)
    {
        if (projectileHitPlayer != null)
        {
            AudioSource.PlayClipAtPoint(projectileHitPlayer, position, hitSoundsVolume);
        }
    }

    public void PlayProjectileHitBuilding(Vector3 position)
    {
        if (projectileHitBuilding != null)
        {
            AudioSource.PlayClipAtPoint(projectileHitBuilding, position, hitSoundsVolume);
        }
    }
    #endregion
}