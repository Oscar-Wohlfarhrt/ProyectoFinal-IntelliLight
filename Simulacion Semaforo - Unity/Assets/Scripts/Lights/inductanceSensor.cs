using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inductanceSensor : MonoBehaviour
{
    // variable publica para la lectura de datos
    public bool vehicle=false;

    // Funcion que se ejecuta unicamente en el primer fotograma de vida del objeto
    void Start()
    {
        
    }

    // Funcion que se ejecuta una ves por fotograma
    void Update()
    {
        // rayo para la deteccion de vehiculos
        Ray ray = new Ray(transform.position, transform.up);
        RaycastHit hit;
        vehicle = Physics.Raycast(ray,out hit);

        // grafico de debug
        if (vehicle)
        {
            Debug.DrawLine(transform.position,hit.point,Color.gray);
        }
    }
}
