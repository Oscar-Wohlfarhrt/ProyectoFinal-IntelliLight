using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Generator : MonoBehaviour
{
    //variables para la activacion y desactivacion voluntaria del generador
    public Button Activer;
    public Text ActiverText;
    bool active = true;

    //contador para la generacion de vehiculos
    float counter = 0;

    // Funcion que se ejecuta unicamente en el primer fotograma de vida del objeto
    void Start()
    {
        //añade una funcion lamda para el boton 
        Activer.onClick.AddListener(() => { active = !active; });
    }

    // Funcion que se ejecuta una ves por fotograma
    void Update()
    {
        if (active)
        {
            // detecta si existen vehiculos a una distancia de 8 unidades
            bool hit = Physics.Raycast(transform.position, transform.forward, 8f);

            //verifica si se alcanzo el tiempo para la generacion del vehiculo
            if (counter > (60 / (FlowControl.FlowRate)) + (FlowControl.FlowRate != 0 ? Random.Range(-FlowControl.ran, FlowControl.ran) : 0))
            {
                counter = 0;
                if (!hit)
                {
                    GameObject.Instantiate(Resources.Load("Car"), transform.position + (transform.forward * 4), transform.rotation);
                }
            }

            // actualizacion del contador
            counter += Time.deltaTime;
        }

        // actualizacion del texto
        ActiverText.text = active ?"Active":"Deactive";
    }
}
