using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class LightGrid
{
    // recuperacion de los prefabricados en variables
    static private GameObject Light = Resources.Load("Semaforo") as GameObject, Generator = Resources.Load("Generator") as GameObject;
    
    //distancia estandar entre celdas
    static private float scale = 110;

    //Lista de celdas
    static List<List<LightCell>> Cells = new List<List<LightCell>>();

    /// <summary>
    /// Funcion para la generacion del entorno
    /// </summary>
    /// <param name="x">Cordenada X de inicio</param>
    /// <param name="z">Cordenada Z de inicio</param>
    /// <param name="xSize">Tamaño X de la cuadricula</param>
    /// <param name="zSize">Tamaño Z de la cuadricula</param>
    /// <param name="_scale">Distancia entre las celdas</param>
    static public void GenerateCells(float x, float z, int xSize, int zSize, float _scale)
    {
        scale = _scale;

        Cells = new List<List<LightCell>>();

        for (int i = 0; i < xSize + 2; i++)
        {
            Cells.Add(new List<LightCell>());
            for (int l = 0; l < zSize + 2; l++)
            {
                if (i == 0 && l == 0)
                {
                    Cells[i].Add(new LightCell());
                }
                else if (i == 0 && l == zSize + 1)
                {
                    Cells[i].Add(new LightCell());
                }
                else if (i == xSize + 1 && l == 0)
                {
                    Cells[i].Add(new LightCell());
                }
                else if (i == xSize + 1 && l == zSize + 1)
                {
                    Cells[i].Add(new LightCell());
                }
                else if (i == 0)
                {
                    Cells[i].Add(new LightCell(GetCellToWorld(new Vector2Int(i, l)) + new Vector3(x, 1.4f, z), Quaternion.Euler(0, 90, 0), Generator));
                }
                else if (l == 0)
                {
                    Cells[i].Add(new LightCell(GetCellToWorld(new Vector2Int(i, l)) + new Vector3(x, 1.4f, z), Quaternion.Euler(0, 0, 0), Generator));
                }
                else if (i == xSize + 1)
                {
                    Cells[i].Add(new LightCell(GetCellToWorld(new Vector2Int(i, l)) + new Vector3(x, 1.4f, z), Quaternion.Euler(0, -90, 0), Generator));
                }
                else if (l == zSize + 1)
                {
                    Cells[i].Add(new LightCell(GetCellToWorld(new Vector2Int(i, l)) + new Vector3(x, 1.4f, z), Quaternion.Euler(0, 180, 0), Generator));
                }
                else
                {
                    Cells[i].Add(new LightCell(GetCellToWorld(new Vector2Int(i, l)) + new Vector3(x, 0, z), new Quaternion(), Light));
                }
            }
        }
    }

    /// <summary>
    /// Devuelve la celda en la que esta el punto especificado
    /// </summary>
    /// <param name="cords">Cordenadas del mundo</param>
    /// <returns></returns>
    static public Vector2Int WorldToCell(Vector3 cords)
    {
        return new Vector2Int((int)(cords.x/scale), (int)(cords.z/scale));
    }

    /// <summary>
    /// Devuelve la celda solicitada
    /// </summary>
    /// <param name="cords">Cordenadas de la celda a devolver</param>
    /// <returns></returns>
    static public LightCell GetCell(Vector2Int cords)
    {
        return Cells[cords.x][cords.y];
    }

    /// <summary>
    /// Devuelve el punto en el mundo del centro de la celda
    /// </summary>
    /// <param name="cords">Cordenadas de la celda</param>
    /// <returns></returns>
    static public Vector3 GetCellCenterToWorld(Vector2Int cords)
    {
        return new Vector3((cords.x) * scale + (scale / 2), 0.1f, (cords.y) * scale + (scale / 2));
    }

    /// <summary>
    /// Debuelve el punto en el mundo de la celda
    /// </summary>
    /// <param name="cords">Cordenadas de la celda</param>
    /// <returns></returns>
    static public Vector3 GetCellToWorld(Vector2Int cords)
    {
        return new Vector3((cords.x) * scale, 0.1f, (cords.y) * scale);
    }

    /// <summary>
    /// Funcion en desuso para obtener el patron de luz verde para al menos dos direcciones
    /// </summary>
    /// <param name="x">Cordenada X del semaforo</param>
    /// <param name="y">Cordenada Y del semaforo</param>
    /// <returns></returns>
    static public uint PerfectGreenPattern(int x,int y)
    {
        int i = x + y;
        return (uint)(3-(i-((int)(i/4))*4));
    }

    /// <summary>
    /// Retorna el punto de giro para el vehiculo especificado
    /// </summary>
    /// <param name="car">Vehiculo para el que se deben obtener los puntos</param>
    /// <param name="direction">Direccion de giro del vehiculo</param>
    /// <returns></returns>
    static public Vector3 GetTurnPoint(GameObject car, Turn direction)
    {
        if (direction == Turn.Forward)
        {
            return car.transform.position;
        }
        else if (direction == Turn.Left)
        {
            Vector3 pos = LightGrid.GetCellCenterToWorld(LightGrid.WorldToCell(car.transform.position)) - car.transform.forward * 2.5f + car.transform.right * 2.5f;
            pos.y = car.transform.position.y;
            return pos;
        }
        else if (direction == Turn.Right)
        {
            Vector3 pos = LightGrid.GetCellCenterToWorld(LightGrid.WorldToCell(car.transform.position)) + car.transform.forward * 2.5f + car.transform.right * 2.5f;
            pos.y = car.transform.position.y;
            return pos;
        }
        return default;
    }

    /// <summary>
    /// Destruye todos los objetos de las celdas
    /// </summary>
    static public void DestroyAllCells()
    {
        Cells.ForEach(x=>
        {
            x.ForEach(y=>
            {
                y.Destroy();
            });
        });

    }

    /// <summary>
    /// Destruye todos los vehiculos en el entorno
    /// </summary>
    static public void DestroyAllCars()
    {
        GameObject.FindGameObjectsWithTag("Cars").ToList().ForEach(x => { Object.Destroy(x); });
    }
}

public class LightCell
{
    public GameObject Light;// objeto instanciado
    public GameObject prefab;// prefabricado del objeto instanciado

    /// <summary>
    /// Contructor con inicializacion de objetos
    /// </summary>
    /// <param name="cords">Cordenadas del objeto</param>
    /// <param name="rota">Rotacion del objeto</param>
    /// <param name="obj">Objeto a instanciar</param>
    public LightCell(Vector3 cords, Quaternion rota, GameObject obj)
    {
        prefab = obj;
        Light = GameObject.Instantiate(prefab, cords, rota);
    }

    /// <summary>
    /// constructor vacio
    /// </summary>
    public LightCell()
    {

    }

    // funcion para la verificacion del objeto
    public bool IsLight()
    {
        if (prefab == (Resources.Load("Semaforo") as GameObject))
            return true;
        else
            return false;
    }

    // destructor del objeto
    public void Destroy()
    {
        Object.Destroy(Light);
    }
}

/// <summary>
/// Enumeracion para direccion de giro de los vehiculos
/// </summary>
public enum Turn
{
    Left,
    Right,
    Forward
}