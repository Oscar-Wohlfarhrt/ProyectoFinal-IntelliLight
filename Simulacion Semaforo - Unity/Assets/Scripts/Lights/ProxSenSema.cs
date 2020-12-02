using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Funcion en completo desuso
 * El uso original fue pensado para usarse en ves de los sensores de lazo inductivo
 * mediante un sensor de proximidad
 */

public class ProxSenSema : MonoBehaviour
{
    public Text text;
    float angle = -0.25f;
    float[] umb = { 0, 0 };//{ 8, 13 };

    // Start is called before the first frame update
    void Start()
    {

    }

    bool vehicle = false;
    int counter = 0;

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;

        if(Physics.Raycast(gameObject.transform.position,transform.forward+new Vector3(0,angle,0),out hit,15f))
        {
            text.text = $"{text.text.Substring(0,11)} {hit.distance}";
            //Debug.DrawLine(gameObject.transform.position,hit.point,Color.red);

            /*if (hit.distance < umb[0])
            {
                gameObject.GetComponent<Renderer>().material.color = Color.green;
            }
            else if (hit.distance < umb[1])
            {
                gameObject.GetComponent<Renderer>().material.color = Color.yellow;
            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = Color.red;
            }*/
            vehicle = true;
            if (counter > 200)
            {
                counter = 200;
            }
        }
        else
        {
            text.text = $"Distance 2: no hit";
            //gameObject.GetComponent<Renderer>().material.color = Color.red;
        }

        if (vehicle)
        {
            if (counter < 200)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.red;
            }
            else if (counter < 300)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.green;
            }
            else if (counter > 350)
            {
                vehicle = false;
                counter = 0;
            }
            counter++;
        }
        else 
        {
            gameObject.GetComponent<Renderer>().material.color = Color.red;
        }

        text.text += $"\nCounter: {counter}";
    }
}
