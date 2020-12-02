using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killer : MonoBehaviour
{
    // Funcion que se ejecuta unicamente en el primer fotograma de vida del objeto
    void Start()
    {
        
    }

    // Funcion que se ejecuta una ves por fotograma
    void Update()
    {
        RaycastHit hit;

        // se elimina el objeto que entra en contracto con el rayo
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10f))
        {
            Debug.DrawLine(transform.position, hit.point,Color.black);
            if (hit.distance < 6) {
                GameObject.Destroy(hit.collider.gameObject);
            }
        }
    }
}
