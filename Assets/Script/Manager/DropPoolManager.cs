using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class DropPoolManager : MonoBehaviour
{
    private List<DropController> _controllersList = new List<DropController>();
    private Dictionary<string, List<GameObject>> _dropDictionary = new Dictionary<string, List<GameObject>>();

    [SerializeField] private ManagerDataSourceSO _dataSourceSO;
    [SerializeField] private float _waitForManager = 1f;

    [SerializeField] private int _poolCount = 5;

    //Managers
    private LevelManager _levelManager;
    private GameManager _gameManager;

    private List<GameObject> _spawnedObjectList = new List<GameObject>();

    private void OnEnable()
    {
        if (_levelManager)
        {
            _levelManager.startLevel += DesactivateDropObjects;
        }
        if (_gameManager)
        {
            _gameManager.resetGame += DesactivateDropObjects;
        }
    }

    private void OnDisable()
    {
        if (_levelManager)
        {
            _levelManager.startLevel -= DesactivateDropObjects;
        }
        if (_gameManager)
        {
            _gameManager.resetGame -= DesactivateDropObjects;
        }
    }

    private void Awake()
    {
        if (!_dataSourceSO)
        {
            Debug.LogError(message: $"{name}: DataSource is null\n Check and assigned one\nDisabling component");
            enabled = false;
            return;
        }
         _dataSourceSO.dropManager = this;
    }

    private void Start()
    {
        StartCoroutine(SetManager());
    }

    private IEnumerator SetManager()
    {
        yield return new WaitForSeconds(_waitForManager);
        if (!_levelManager && _dataSourceSO.levelManager)
        {
            _levelManager = _dataSourceSO.levelManager;
            _levelManager.startLevel += DesactivateDropObjects;
        }
        if (!_gameManager && _dataSourceSO.gameManager)
        {
            _gameManager = _dataSourceSO.gameManager;
            _gameManager.resetGame += DesactivateDropObjects;
        }
    }

    public void SpawnDropObject(string name, GameObject dropObject, Vector3 spawnPosition)
    {
        int count = 0;
        if (_dropDictionary != null)
        {
            if (_dropDictionary.ContainsKey(name))
            {
                for (int i = 0; i < _dropDictionary[name].Count; i++)
                {
                    if (!_dropDictionary[name][i].activeSelf)
                    {
                        _dropDictionary[name][i].SetActive(true);
                        _dropDictionary[name][i].transform.position = spawnPosition;
                        _spawnedObjectList.Add(_dropDictionary[name][i]);
                        count++;
                        return;
                    }
                }
                if (count > 0)
                {
                    GameObject temp;
                    temp = Instantiate(dropObject,spawnPosition,Quaternion.identity);
                    _dropDictionary[name].Add(temp);
                    _spawnedObjectList.Add(temp);
                }
            }
        }
    }

    public void CreatePool(string name, GameObject dropObject)
    {
        if (_dropDictionary == null)
        {
            _dropDictionary.Add(name, CreateList(dropObject));
        }
        else
        {
            if (_dropDictionary.ContainsKey(name))
            {
                return;
            }
            else
            {
                _dropDictionary.Add(name, CreateList(dropObject));
            }
        }
    }

    private List<GameObject> CreateList(GameObject dropObject)
    {
        List<GameObject> tempList = new List<GameObject>();
        GameObject temp;
        for (int i = 0; i < _poolCount; i++)
        {
            temp = Instantiate(dropObject);
            tempList.Add(temp);
            temp.SetActive(false);
        }
        return tempList;
    }

    public void AddController(DropController drop)
    {
        _controllersList.Add(drop);
    }

    public void DeleteController(DropController drop)
    {
        if (_controllersList.Contains(drop))
        {
            _controllersList.Remove(drop);
        }
    }

    private void DesactivateDropObjects()
    {
        foreach (GameObject obj in _spawnedObjectList) 
        {
            if (obj.activeSelf)
            {
                obj.SetActive(false);
            }
        }
        _spawnedObjectList.Clear();
    }
}
