using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnDestroy : MonoBehaviour
{
    //what object is to spawn on destroy
    [SerializeField] private GameObject prefab;

    private void OnDestroy(){
        Instantiate(prefab, transform.position, Quaternion.identity);
    }
}
