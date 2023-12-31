using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void SpawnEnemy(int column, int row, int seed);
public class LevelManager : MonoBehaviour
{
    const int DIFF_MATRIX_ENEMY = 1;

    //Delegates
    public Action startLevel;
    public SpawnEnemy spawnEnemy;
    public Action endLevel;
    public Action<int> showActualWave;
    public Action bossFight;
    public Action endBossFight;

    //Level variables for dificult
    [SerializeField] private int _minEnemy = 1;
    [SerializeField] private List<GameObject> _dataPrefabList;

    //Managers
    [SerializeField] private float _waitForManager = 1f;
    [SerializeField] private ManagerDataSourceSO _dataSourceSO;
    private EnemyManager _enemyManager;
    private ViewMapManager _viewMapManager;
    private GameManager _gameManager;
    [SerializeField] private DropController _dropController;

    [SerializeField] private Stair _stair;
    [SerializeField] private float _stairSpawnDelay = 1f;

    private List<ArrayLayout> _dataList = new List<ArrayLayout>();
    private ArrayLayout _dataLayout;

    //Variables for start and end game
    private List<BaseEnemy> _enemySpawnedList = new List<BaseEnemy>();
    [SerializeField] private int _maxWave = 10;
    private int _bossWave;
    private int _actualWave = 0;

    private void OnEnable()
    {
        ReSpawnStair();
        _actualWave = 0;
        _bossWave = _maxWave + 2;

        //Delegate
        _stair.nextLevel += StartLevel;
        if (_viewMapManager)
        {
            _viewMapManager.stairObject += SetStair;
        }
        if (_enemyManager)
        {
            _enemyManager.spawnedEnemies += SpawnedEnemiesCount;
        }
        if (_dropController)
        {
            _dropController.DelegateSuscriptionDrop(this);
        }
        if (_gameManager)
        {
            _gameManager.resetGame += ResetGame;
        }
    }

    private void OnDisable()
    {
        _viewMapManager.stairObject -= SetStair;
        _enemyManager.spawnedEnemies -= SpawnedEnemiesCount;
        _stair.nextLevel -= StartLevel;
        if (_gameManager)
        {
            _gameManager.resetGame -= ResetGame;
        }
    }

    private void Awake()
    {
        _bossWave = _maxWave + 2;
        NullReferenceControll();
        SetDataList();
        _dataSourceSO.levelManager = this;
        StartCoroutine(SetManagers());
    }

    private IEnumerator SetManagers()
    {
        yield return new WaitForSeconds(_waitForManager);
        if (_dataSourceSO.enemyManager && !_enemyManager)
        {
            _enemyManager = _dataSourceSO.enemyManager;
            _enemyManager.spawnedEnemies += SpawnedEnemiesCount;

        }
        if (_dataSourceSO.viewMapManager && !_viewMapManager)
        {
            _viewMapManager = _dataSourceSO.viewMapManager;
            _viewMapManager.stairObject += SetStair;
        }
        if (_dataSourceSO.gameManager && !_gameManager)
        {
            _gameManager = _dataSourceSO.gameManager;
            _gameManager.resetGame += ResetGame;
        }
    }

    private void NullReferenceControll()
    {
        if (!_dataSourceSO)
        {
            Debug.LogError(message: $"{name}: Data source is null \n Check and assigned one\nDisabling component");
            enabled = false;
            return;
        }
        if (_dataPrefabList.Count == 0)
        {
            Debug.LogError(message: $"{name}: DataPrefabList is null \n Check and assigned one\nDisabling component");
            enabled = false;
            return;
        }
        if (!_stair)
        {
            Debug.LogError(message: $"{name}: Stair is null \n Check and assigned one\nDisabling component");
            enabled = false;
            return;
        }
        if (_minEnemy < 0)
        {
            Debug.LogError(message: $"{name}: Min enemy is negative \n Check that and assigned positive number\nChange value to 1");
            _minEnemy = 1;
            return;
        }
        if (!_dropController)
        {
            Debug.LogError(message: $"{name}: DropController is null \n Check and assigned one\nDisabling component");
            enabled = false;
            return;
        }
    }

    private void SetDataList()
    {
        foreach (GameObject data in _dataPrefabList)
        {
            if (data == null)
            {
                Debug.LogError(message: $"{name}: GameObject is null\n Check and assigned one\nDisabling component");
                enabled = false;
                return;
            }
            if (data.gameObject.GetComponent<LevelMapData>() == null)
            {
                Debug.LogError(message: $"{name}: {data.name} dont have a MapData\n Check and assigned one\nDisabling component");
                enabled = false;
                return;
            }
            else
            {
                _dataList.Add(data.gameObject.GetComponent<LevelMapData>().ArrayLayout);
            }
        }
    }

    //TODO: TP2 - Unclear name(Done)
    private void EnemiesSpawnLogic(int f_column, int f_row, int seed)
    {
        if (_minEnemy == 1)
        {
            spawnEnemy?.Invoke(f_column - DIFF_MATRIX_ENEMY, f_row - DIFF_MATRIX_ENEMY, seed);
            return;
        }
        else
        {
            for (int i = 0; i <= _minEnemy; i++)
            {
                spawnEnemy?.Invoke(f_column - DIFF_MATRIX_ENEMY, f_row - DIFF_MATRIX_ENEMY, seed);
            }
        }
    }

    private void SearchEnemiesSpawners()
    {
        ArrayLayout.State state;
        int seed = UnityEngine.Random.Range(0, 100);
        RandomLevel();
        for (int f_row = _dataLayout.rows.Length - 1; f_row >= 0; f_row--)
        {
            for (int f_column = 0; f_column < _dataLayout.rows[f_row].row.Length; f_column++)
            {
                state = _dataLayout.rows[f_row].row[f_column];
                switch (state)
                {
                    case ArrayLayout.State.EmptyFloor:
                        break;
                    case ArrayLayout.State.Rock:
                        break;
                    case ArrayLayout.State.Spawner:
                        EnemiesSpawnLogic(f_column, f_row, seed);
                        break;
                    case ArrayLayout.State.ObjectSpawner:
                        break;
                    case ArrayLayout.State.WallRight:
                        break;
                    case ArrayLayout.State.WallLeft:
                        break;
                    case ArrayLayout.State.WallDown:
                        break;
                    case ArrayLayout.State.WallTop:
                        break;
                    case ArrayLayout.State.WallLeftDown:
                        break;
                    case ArrayLayout.State.WallRightDown:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Randomly level selection
    /// </summary>
    private void RandomLevel()
    {
        int index = UnityEngine.Random.Range(0, _dataList.Count);
        _dataLayout = _dataList[index];
    }

    [ContextMenu("StartLevel")]
    private void StartLevel()
    {
        _actualWave++;
        startLevel?.Invoke();
        _stair.isActiveStair = false;
        _stair.gameObject.SetActive(false);
        Debug.Log($"{name}: Event StartLevel is called");
        if (_actualWave == _maxWave + 1) SpawnBoss();
        else
        {
            showActualWave?.Invoke(_actualWave);
            SearchEnemiesSpawners();
        }
    }

    [ContextMenu("EndLevel")]
    private void EndLevel()
    {
        endLevel?.Invoke();
        ReSpawnStair();
    }

    private void SpawnedEnemiesCount(BaseEnemy enemy)
    {
        _enemySpawnedList.Add(enemy);
        enemy.enemyKill += KilledEnemiesCount;
    }

    private void KilledEnemiesCount(BaseEnemy enemy)
    {
        Debug.Log($"{enemy.name} was killed and call KilledEnemiesCount event.");
        enemy.enemyKill -= KilledEnemiesCount;
        _enemySpawnedList.Remove(enemy);
        if (_enemySpawnedList.Count <= 0)
        {
            Debug.Log("Last enemy killed, start end level moment.");
            if (_actualWave == _maxWave + 1) endBossFight?.Invoke();
            else EndLevel();
        }
    }

    private void SetStair(Stair stair)
    {
        _stair = stair;
        _stair.nextLevel += StartLevel;
    }

    private void ReSpawnStair()
    {
        StartCoroutine(SpawnStair());
    }

    private void SpawnBoss()
    {
        bossFight?.Invoke();
    }

    private IEnumerator SpawnStair()
    {
        _stair.isActiveStair = true;
        yield return new WaitForSeconds(_stairSpawnDelay);
        _stair.gameObject.SetActive(true);
    }

    private void ResetGame()
    {
        if (_stair.gameObject.activeSelf)
        {
            return;
        }
        else
        {
            ReSpawnStair();
        }
        if (_enemySpawnedList.Count > 0)
        {
            for (int i = 0; i < _enemySpawnedList.Count; i++)
            {
                _enemySpawnedList[i].enemyKill -= KilledEnemiesCount;
            }
            _enemySpawnedList.Clear();
        }
    }
}
