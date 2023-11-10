using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] protected BaseEnemy _baseEnemy;
    protected EnemyManager _enemyManager;

    //Positions
    protected Vector2 p_currentPosition;
    protected Vector2 p_nextPosition;
    protected Vector2 p_directionToNextPosition;
    protected int p_row;
    protected int p_column;
    protected int p_currentRowPosition;
    protected int p_currentColumnPosition;
    protected Vector2 p_direction;

    [SerializeField] protected float _treshold = 0.0001f;

    public Vector2 direction { get { return p_direction; } }

    private void Start()
    {
        if (_enemyManager == null)
        {
            Debug.LogError(message: $"{name}: EnemyManager is null\n Check and assigned one\nDisabling component");
            enabled = false;
            return;
        }
        p_row = _enemyManager._positionMatriz.GetLength(1);
        p_column = _enemyManager._positionMatriz.GetLength(0);
        p_currentColumnPosition = _baseEnemy.col;
        p_currentRowPosition = _baseEnemy.row;
        GetRandomNextPosition();
    }

    protected void DirectionAssigned(Vector2 direction)
    {
        p_direction = direction;
    }

    public void GetValueEnemyManager(EnemyManager enemyManager)
    {
        _enemyManager = enemyManager;
    }

    protected virtual void GetRandomNextPosition()
    {

    }
}
