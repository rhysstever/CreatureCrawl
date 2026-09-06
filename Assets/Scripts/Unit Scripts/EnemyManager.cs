using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EnemyType
{
    Boar,
    Mushroom,
    Fairy,
    Ent,
    Hag
}

public class EnemyManager : MonoBehaviour
{
    // Singleton
    public static EnemyManager instance = null;

    // Instantiated in inspector
    [SerializeField]
    private Transform enemiesParent;
    [SerializeField]
    private GameObject boarEnemyPrefab, mushroomEnemyPrefab, fairyEnemyPrefab, entEnemyPrefab, hagEnemyPrefab;
    //oozeEnemyPrefab, batSwarmEnemyPrefab, zombieEnemyPrefab, shadowEnemyPrefab, necromancerEnemyPrefab;

    // Instantiated in code
    private List<EnemyWave> enemyWaves;
    private int currentWaveNum;

    private IEnumerator enemyActionsCoroutine;
    private IEnumerator enemyEffectsCoroutine;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        enemyWaves = SetEnemyWaves();
        Reset();
    }

    private List<EnemyWave> SetEnemyWaves()
    {
        List<EnemyWave> combatRounds = new() {
            // Tutorial Level
            new EnemyWave(EnemyType.Boar, 1),   
            // Area 1 enemies
            new EnemyWave(EnemyType.Boar, 2),
            new EnemyWave(EnemyType.Mushroom, 2),
            new EnemyWave(EnemyType.Fairy, 3),
            new EnemyWave(EnemyType.Ent, 1),
            new EnemyWave(EnemyType.Hag, 1, true),
            // Area 2 enemies
            //new EnemyWave(EnemyType.Ooze, 1),
            //new EnemyWave(EnemyType.Bat, 3),
            //new EnemyWave(EnemyType.Zombie, 2),
            //new EnemyWave(EnemyType.Shadow, 1),
            //new EnemyWave(EnemyType.Necromancer, #, true),
        };

        return combatRounds;
    }

    public List<Action> GetEnemyActions(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Boar => new List<Action>
            {
                new Action(ActionType.Defend, 2, TargetType.Self),
                new Action(ActionType.WeaponAttack, 2, TargetType.Player),
                new Buff(ActionType.WeaponAttack, 2),
            },
            EnemyType.Mushroom => new List<Action>
            {
                new Action(ActionType.Defend, 2, TargetType.Self),
                new Action(ActionType.Poison, 2, TargetType.Player),
                new Action(ActionType.WeaponAttack, 2, TargetType.Player),
                new Buff(ActionType.Poison, 1),
            },
            EnemyType.Fairy => new List<Action>
            {
                new Action(ActionType.Defend, 2, TargetType.Self),
                new Action(ActionType.Burn, 1, TargetType.Player),
                new EnemySummon(1, EnemyType.Fairy),
                new Buff(ActionType.Burn, 1),
            },
            EnemyType.Ent => new List<Action>
            {
                new Action(ActionType.Poison, 6, TargetType.Player),
                new Action(ActionType.Defend, 5, TargetType.Self),
                new Action(ActionType.Defend, 5, TargetType.Self),
                new Action(ActionType.WeaponAttack, 15, TargetType.Player),
                new Buff(ActionType.WeaponAttack, 10),
            },
            EnemyType.Hag => new List<Action>
            {
                new EnemySummon(2, EnemyType.Mushroom),
                new Action(ActionType.Defend, 5, TargetType.Self),
                new Action(ActionType.Burn, 5, TargetType.Player),
                new Action(ActionType.Poison, 10, TargetType.Player),
                new Action(ActionType.Heal, 10, TargetType.Self),
            },
            _ => new List<Action>()
        };
    }

    public GameObject GetEnemyPrefabByType(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Boar => boarEnemyPrefab,
            EnemyType.Mushroom => mushroomEnemyPrefab,
            EnemyType.Fairy => fairyEnemyPrefab,
            EnemyType.Ent => entEnemyPrefab,
            EnemyType.Hag => hagEnemyPrefab,
            _ => null,
        };
    }

    public void StartEnemyCombatTurn()
    {
        enemyActionsCoroutine = PerformEnemyRoundActions(GetCurrentEnemies());
        StartCoroutine(enemyActionsCoroutine);
    }

    private IEnumerator PerformEnemyRoundActions(List<Enemy> enemies)
    {
        WaitForSeconds enemyDelayWait = new WaitForSeconds(1);
        WaitForSeconds endOfEnemyTurnsDelayWait = new WaitForSeconds(1);
        WaitForSeconds turnBannerDelayWait = new WaitForSeconds(UIManager.instance.TurnBannerVisibleTime);

        UIManager.instance.ToggleEnemyTurnBanner(true);
        yield return turnBannerDelayWait;
        UIManager.instance.ToggleEnemyTurnBanner(false);

        int enemyIndex = 0;
        while(enemyIndex < enemies.Count && GameManager.instance.Player.CurrentLife > 0)
        {
            if(enemies[enemyIndex].CurrentLife > 0)
            {
                yield return enemyDelayWait;
                enemies[enemyIndex].PerformRoundAction();
            }
            enemyIndex++;
        }

        if(GameManager.instance.Player.CurrentLife > 0)
        {
            yield return endOfEnemyTurnsDelayWait;
            // Check if enemies are still alive, if so, move to the Player's turn
            // If there are no enemies alive, an action needed to have had been processed (like spikes),
            // thus remaining enemies will be checked once the action has been processed, and the combat will be ended
            if(!IsWaveOver())
            {
                GameManager.instance.ChangeCombatState(CombatState.PlayerTurn);
            }
        }
    }

    public bool IsWaveOver()
    {
        // Check all enemies and if none have health, the wave is over
        for(int i = 0; i < enemiesParent.childCount; i++)
        {
            Enemy enemy = GetEnemyAtPosition(i);
            if(enemy != null && enemy.CurrentLife > 0)
            {
                return false;
            }
        }

        return true;
    }

    public void IncrementWaveNum()
    {
        currentWaveNum++;
    }

    public void SpawnNextWave()
    {
        IncrementWaveNum();

        // If there are no more waves to spawn, end the game
        if(currentWaveNum >= enemyWaves.Count)
        {
            GameManager.instance.ChangeMenuState(MenuState.GameEnd);
            return;
        }

        EnemyWave wave = enemyWaves[currentWaveNum];
        GameObject enemyToSpawn = GetEnemyPrefabByType(wave.EnemyType);
        switch(wave.EnemyCount)
        {
            case 3:
                // Spawn one enemy in the middle position and
                // 2 enemies on the edge positions
                SpawnEnemy(enemyToSpawn, 0).IncrementStartingRound();
                SpawnEnemy(enemyToSpawn, 2);
                SpawnEnemy(enemyToSpawn, 4).IncrementStartingRound();
                break;
            case 2:
                // Spawn both enemies in the second and fourth positions
                SpawnEnemy(enemyToSpawn, 1);
                SpawnEnemy(enemyToSpawn, 3).IncrementStartingRound();
                break;
            case 1:
                if(wave.IsSummoningBossWave)
                {
                    // Spawn the boss on the edge to prepare for when it summons
                    SpawnEnemy(enemyToSpawn, 4);
                }
                else
                {
                    if(TutorialManager.instance.IsInTutorial)
                    {
                        // Spawn the tutorial enemy in the middle right spot
                        SpawnEnemy(enemyToSpawn, 3).IncrementRound();
                    }
                    else
                    {
                        // Spawn the only enemy in the center spot
                        SpawnEnemy(enemyToSpawn, 2);
                    }
                }
                break;
            default:
                Debug.Log(string.Format("Error! Incorrect number of enemies: {0}!", wave.EnemyCount));
                break;
        }
    }

    public void SpawnSummon(GameObject enemy)
    {
        // Find current open spots
        List<int> enemiesSpots = new List<int>();
        for(int i = 0; i < enemiesParent.childCount; i++)
        {
            // If there is no enemy at that spot, the spot is open
            if(GetEnemyAtPosition(i) == null)
            {
                enemiesSpots.Add(i);
            }
        }

        // Choose a random open spots
        int randomSpotIndex = -1;
        if(enemiesSpots.Count > 0)
        {
            randomSpotIndex = enemiesSpots[UnityEngine.Random.Range(0, enemiesSpots.Count)];
        }

        // Spawn the enemy at that position
        if(randomSpotIndex != -1)
        {
            SpawnEnemy(enemy, randomSpotIndex);
        }
        else
        {
            Debug.Log("Error! No available positions to spawn summon!");
        }
    }

    private Enemy SpawnEnemy(GameObject enemy, int positionIndex)
    {
        GameObject newEnemyObject = Instantiate(enemy, enemiesParent.GetChild(positionIndex));
        newEnemyObject.name = enemy.name + positionIndex;

        Enemy newEnemy = newEnemyObject.GetComponent<Enemy>();
        newEnemy.SetPositionIndex(positionIndex);
        return newEnemy;
    }

    public bool IsLastWave()
    {
        return currentWaveNum == enemyWaves.Count - 1;
    }

    public Enemy GetRandomEnemy()
    {
        List<Enemy> currentEnemies = GetCurrentEnemies();
        int randomEnemyIndex = UnityEngine.Random.Range(0, currentEnemies.Count);
        return currentEnemies[randomEnemyIndex];
    }

    private Enemy GetEnemyAtPosition(int positionIndex)
    {
        if(enemiesParent.GetChild(positionIndex).childCount > 0)
        {
            return enemiesParent.GetChild(positionIndex).GetChild(0).GetComponent<Enemy>();
        }
        else
        {
            return null;
        }
    }

    public List<Enemy> GetCurrentEnemies()
    {
        List<Enemy> enemies = new List<Enemy>();
        for(int i = 0; i < enemiesParent.childCount; i++)
        {
            Enemy enemy = GetEnemyAtPosition(i);
            if(enemy != null)
            {
                enemies.Add(enemy);
            }            
        }
        return enemies;
    }

    public IEnumerator ProcessEffectsOnEnemies()
    {
        WaitForSeconds enemyEffectsDelayWait = new WaitForSeconds(1);
        int enemyIndex = 0;
        List<Enemy> enemies = GetCurrentEnemies();

        while(enemyIndex < enemies.Count)
        {
            if(enemies[enemyIndex].HasEffectsToProcess())
            {
                yield return enemyEffectsDelayWait;
                enemyEffectsCoroutine = enemies[enemyIndex].ProcessEffects();
                StartCoroutine(enemyEffectsCoroutine);
            }
            else
            {
                enemies[enemyIndex].MarkProcessed();
            }
            enemyIndex++;
        }

        // Wait for all enemies to process all of their effects
        while(enemies.Where(e => !e.HasBeenProcessed).Count() > 0)
        {
            yield return enemyEffectsDelayWait;
        }

        if(IsWaveOver())
        {
            GameManager.instance.ChangeCombatState(CombatState.End);
        }
        else
        {
            StartEnemyCombatTurn();
        }
    }

    public void Reset()
    {
        currentWaveNum = -1;

        for(int i = 0; i < enemiesParent.childCount; i++)
        {
            Enemy enemy = GetEnemyAtPosition(i);
            if(enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
    }
}
