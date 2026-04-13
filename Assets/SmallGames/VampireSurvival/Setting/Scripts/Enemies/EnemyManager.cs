using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VampireSurvival.Core;
using UnityTV.Core;
using UnityTV.Player;


namespace VampireSurvival.Enemies
{
    /// <summary>
    /// 实现动态敌人生成的功能，根据当前关卡阶段（Mission）调整生成的敌人类型和数量，
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        private Transform minPos, maxPos; // 生成区域的最小和最大坐标，用于屏幕外生成敌人位置

        [Header("Spawn Settings")]
        [SerializeField] private string Mission;
        [SerializeField] private int maxSpawnCount = 0; // 当前关卡最大生成数量，根据关卡难度调整
        [SerializeField] private int maxNormalEnemiesCount = 0; // 最大普通敌人数量
        [SerializeField] private int maxElitEnemiesCount = 0; // 最大精英敌人数量
        [SerializeField] private int maxBossEnemiesCount = 0; // 最大Boss敌人数量

        [SerializeField] private int normalEnemiesCount = 0; // 当前生成的普通敌人数量
        [SerializeField] private int elitEnemiesCount = 0; // 当前生成的精英敌人数量
        [SerializeField] private int bossEnemiesCount = 0; // 当前生成的Boss敌人数量
        [SerializeField] private int spawnKills = 0; // 已击败的敌人数量

        [SerializeField] private float timePast = 0f; // 已经过的时间，用于控制生成间隔
        [SerializeField] private float timeInterval = 2f; // 生成间隔，单位为秒，后续可根据关卡难度调整

        [SerializeField] private Transform minSpawnPos, maxSpawnPos; // 生成区域的最小和最大坐标，用于屏幕外生成敌人位置

        [Header("Enemy Prefab")]
        [SerializeField] private GameObject normalEnemyPrefab; // 普通敌人预制体
        [SerializeField] private GameObject elitEnemyPrefab; // 精英敌人预制体
        [SerializeField] private GameObject bossEnemyPrefab; // Boss敌人预制体

        private void Start()
        {
            // 初始化部分变量
            minPos = transform.parent.Find("minPos");
            maxPos = transform.parent.Find("maxPos");

            Mission = VampireSurvivalData.instance.CurrentMission;
            maxSpawnCount = VampireSurvivalData.instance.MissionNum[Mission];
            spawnKills = 0;
            timePast = 0f;
            timeInterval = 2f;
            InitEnemyCount();
            InitEnemyPrefab();

            // 启动生成协程
            StartCoroutine(SpawnEnemy());
        }

        private void Update()
        {
            // 更新时间，控制生成间隔
            TimeIntervalUpdate();
        }

        /// <summary>
        /// 依据关卡阶段，初始化敌人的生成数量
        /// </summary>
        private void InitEnemyCount()
        {
            switch(Mission)
            {
                case "1_1":
                case "1_2":
                case "2_1":
                case "2_2":
                case "3_1":
                case "3_2":
                    bossEnemiesCount = 0;
                    elitEnemiesCount = maxSpawnCount / 4;
                    normalEnemiesCount = maxSpawnCount - elitEnemiesCount - bossEnemiesCount;
                    
                    Debug.Log("Mission: " + Mission + 
                        ", Max Spawn Count: " + maxSpawnCount + 
                        ", Normal Enemies: " + normalEnemiesCount + 
                        ", Elite Enemies: " + elitEnemiesCount + 
                        ", Boss Enemies: " + bossEnemiesCount);
                    break;
                case "1_3":
                case "2_3":
                case "3_3":
                    bossEnemiesCount = 1;
                    elitEnemiesCount = maxSpawnCount / 4;
                    normalEnemiesCount = maxSpawnCount - elitEnemiesCount - bossEnemiesCount;
                    
                    Debug.Log("Mission: " + Mission +
                        ", Max Spawn Count: " + maxSpawnCount +
                        ", Normal Enemies: " + normalEnemiesCount +
                        ", Elite Enemies: " + elitEnemiesCount +
                        ", Boss Enemies: " + bossEnemiesCount);
                    break;
                default:
                    Debug.LogError("Invalid Mission Name: " + Mission); 
                    break;
            }
        }

        /// <summary>
        /// 实现敌人预制体的初始化，确保在生成敌人时能够正确实例化对应类型的敌人，
        /// </summary>
        private void InitEnemyPrefab()
        {
            try
            {
                normalEnemyPrefab = Addressables.LoadAssetAsync<GameObject>("NormalEnemy").WaitForCompletion();
                bossEnemyPrefab = Addressables.LoadAssetAsync<GameObject>("BossEnemy").WaitForCompletion();
                switch (GameManager.Instance.PlayerData.FearChoice)
                {
                    case FearType.Gaze:
                        elitEnemyPrefab = Addressables.LoadAssetAsync<GameObject>("ElitEnemy_Gaze").WaitForCompletion();
                        break;
                    case FearType.Darkness:
                        elitEnemyPrefab = Addressables.LoadAssetAsync<GameObject>("ElitEnemy_Darkness").WaitForCompletion();
                        break;
                    case FearType.Insects:
                        elitEnemyPrefab = Addressables.LoadAssetAsync<GameObject>("ElitEnemy_Insects").WaitForCompletion();
                        break;
                    case FearType.Death:
                        elitEnemyPrefab = Addressables.LoadAssetAsync<GameObject>("ElitEnemy_Death").WaitForCompletion();
                        break;
                }

                Debug.Log("Enemy prefabs loaded successfully: " +
                    "\nNormal Enemy: " + (normalEnemyPrefab != null) +
                    "\nElite Enemy: " + (elitEnemyPrefab != null) +
                    "\nBoss Enemy: " + (bossEnemyPrefab != null));
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to load enemy prefabs: " + ex.Message);
            }
        }

        /// <summary>
        /// 更新时间，在每帧调用，增加经过的时间，用于控制生成间隔
        /// </summary>
        private void TimeIntervalUpdate()
        {
            if(timePast >= 0f && timePast < 10f)
            {
                timeInterval = 2f;
            }
            else if (timePast >= 10f && timePast < 30f)
            {
                timeInterval = 1f;
            }
            else if (timePast >= 30f)
            {
                timeInterval = 0.5f;
            }

            timePast += Time.deltaTime * Time.timeScale;
        }

        /// <summary>
        /// 实现敌人生成的逻辑，根据当前生成数量和时间间隔来决定是否生成新的敌人，
        /// 根据关卡难度调整生成的敌人数量（普通、精英、Boss）
        /// </summary>
        private IEnumerator SpawnEnemy()
        {
            float tempChoose = Random.Range(0f, 1f);

            if (tempChoose < 0.8f && normalEnemiesCount < maxNormalEnemiesCount)
            {
                // 生成普通敌人
                Instantiate(normalEnemyPrefab, GetSpawnPos(), Quaternion.identity);
                normalEnemiesCount++;
            }
            else if (tempChoose < 1f && elitEnemiesCount < maxElitEnemiesCount)
            {
                // 生成精英敌人
                Instantiate(elitEnemyPrefab, GetSpawnPos(), Quaternion.identity);
                elitEnemiesCount++;
            }
            else
            {
                // 生成Boss敌人
                Instantiate(bossEnemyPrefab, GetSpawnPos(), Quaternion.identity);
                bossEnemiesCount++;
            }

            yield return new WaitForSeconds(timeInterval);
        }

        /// <summary>
        /// 实现生成位置的逻辑，确保敌人生成在屏幕外的随机位置
        /// </summary>
        /// <returns></returns>
        private Vector3 GetSpawnPos()
        {
            Vector3 spawnPoint = Vector3.zero;

            bool spawnOnX = Random.value > 0.5f; // 随机决定生成在X轴还是Y轴

            if (spawnOnX)
            {
                spawnPoint.x = Random.Range(minSpawnPos.position.x, maxSpawnPos.position.x);
                spawnPoint.y = Random.value > 0.5f ? maxSpawnPos.position.y : minSpawnPos.position.y; // 随机决定生成在上方还是下方
            }
            else
            {
                spawnPoint.y = Random.Range(minSpawnPos.position.y, maxSpawnPos.position.y);
                spawnPoint.x = Random.value > 0.5f ? maxSpawnPos.position.x : minSpawnPos.position.x; // 随机决定生成在右侧还是左侧  
            }
            return spawnPoint;
        }
    }
}