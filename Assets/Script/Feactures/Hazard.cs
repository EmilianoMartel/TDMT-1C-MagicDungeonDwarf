using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    [SerializeField] private int _damage = 10;
    private bool _canHazard = true;

    //TODO: TP2 - Unclear name
    public bool canHazard { set { _canHazard = value; } }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log($"{name} collided with {col.name}");
        //TODO: TP2 - Optimization - TryGetComponent
        if (col.gameObject.GetComponent<HealthPoints>() != null && _canHazard)
        {
            Debug.Log($"{name} try damaged {col.name}");
            HealthPoints healthPoints = col.gameObject.GetComponent<HealthPoints>();
            healthPoints.GetDamage(_damage);
        }
    }
}
