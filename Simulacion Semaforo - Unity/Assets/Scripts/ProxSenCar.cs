using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Bienvenido al lado oscuro
 * 
 * Creo que todos tenenos uno ¿no?
 */


public class ProxSenCar : MonoBehaviour
{
    float mps = 16.666666666f;// velocidad en metros por segundo
    float angle = 0.8f;// angulo para el rayo que verifica el color del semaforo
    Vector2Int LastCell;// celda anterior a la celda en la que actualmente se encuentra el vehiculo
    Turn direction = Turn.Forward;// variable para la direccion de giro
    bool tTry=true,turn=false;// variable de seguridad para el giro del vehiculo

    //texto de debug que se encuentra encima del vehiculo
    public Text debug;

    // Funcion que se ejecuta unicamente en el primer fotograma de vida del objeto
    void Start()
    {
        //actualiza la variable lastcell con la posision actual
        LastCell = LightGrid.WorldToCell(transform.position);
    }

    // Funcion que se ejecuta una ves por fotograma
    void Update()
    {
        // textos y graficos de debug
        Debug.DrawLine(transform.position, LightGrid.GetTurnPoint(gameObject, direction));
        debug.text = $"WPos: {(direction==Turn.Forward?"Forward":(direction==Turn.Left?"Left":(direction==Turn.Right?"Right":"None")))}\nCPos: {LightGrid.WorldToCell(transform.position)}\nTurnP: {LightGrid.GetTurnPoint(gameObject, direction)}\nDistance: {Vector3.Distance(LightGrid.GetTurnPoint(gameObject, direction), transform.position)}";

        // informacion del choque del rayo
        RaycastHit hit;

        // variable para indicar el movimiento del vehiculo
        bool move = false;

        //si detecta un semafor verifica su estado y actualiza la variable move
        if (Physics.Raycast(gameObject.transform.position, transform.forward + new Vector3(0, angle, 0), out hit))
        {
            Debug.DrawLine(gameObject.transform.position, hit.point, Color.red);

            Renderer headRend = hit.collider.gameObject.GetComponent<Renderer>();
            move = (headRend.material.color == Color.red || headRend.material.color == Color.yellow || headRend.material.color == Color.magenta || headRend.material.color == Color.white);
        }
        else
        {

        }

        //verifica si existe algun objeto que impida el movimiento enfrente del vehiculo
        if (Physics.SphereCast(gameObject.transform.position, 0.8f, transform.forward, out hit, 10f))
        {
            Debug.DrawLine(gameObject.transform.position, hit.point, Color.red);

            if (hit.distance < 3)
            {
                move = true;
            }
        }
        else
        {

        }

        // si no existe ningun impedimento para el avance el vehiculo avanza
        if (move)
        {
            // y si, si la variable move es true quiere decir que el vehiculo no se mueve
            // le dije que era el lado oscuro
        }
        else
        {
            // se aplica un movimiento de 16.6 unidades al segundo
            transform.position += new Vector3(transform.forward.x * mps * Time.deltaTime, transform.forward.y, transform.forward.z * mps * Time.deltaTime);
        }

        // se obtiene la celda actual
        Vector2Int ActualCell = LightGrid.WorldToCell(transform.position);

        // se verifica si ya existio un intento de doblar
        if (!tTry)
        {
            // a traves de un random se determina en que direccion va adoblar el vehiculo a traves del enum Turn
            int ran = Random.Range(0, 6);
            direction = (ran<3? (Turn)ran :Turn.Forward);
            turn = tTry = true;
        }
        else if (ActualCell != LastCell)
        {
            tTry = false;
        }

        //si se recibe la instruccion de doblar se verifica si se esta en la posicion correcta y se efectua el giro
        if (turn)
        {
            Vector3 TurnPoint = LightGrid.GetTurnPoint(gameObject, direction);
            float distance = Vector3.Distance(TurnPoint, transform.position);
            if (distance < 0.2f)
            {
                switch (direction)
                {
                    case Turn.Forward:
                        break;
                    case Turn.Right:
                        transform.rotation *= Quaternion.Euler(0, -90, 0);
                        transform.position = TurnPoint;
                        break;
                    case Turn.Left:
                        transform.rotation *= Quaternion.Euler(0, 90, 0);
                        transform.position = TurnPoint;
                        break;
                    default:
                        break;
                }
                turn = false;
            }
        }

        // se actualiza la variable LastCell
        LastCell = LightGrid.WorldToCell(transform.position);
    }
}
