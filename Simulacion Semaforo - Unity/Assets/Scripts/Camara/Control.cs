using System;
using System.IO;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml.Serialization;

public class Control : MonoBehaviour
{
    // varibles para la configuracion del movimiento de la camara
    public float sensibility, rotSen, shiftBoost;
    public Camera cam;

    // objetos de la interfaz
    public Text debug;

    // botones para la gestion del puerto COM
    public Button connect;
    public Button disconnect;

    // Lista de puertos
    public Dropdown portSel;

    // cordenadas para la transmicion de datos
    public InputField cordsX;
    public InputField cordsZ;

    // campos para la regeneracion del entorno
    public Button Gen;
    public InputField XCount;
    public InputField ZCount;


    // variables para la comunicacion serie
    SerialPort sp = new SerialPort();
    List<string> Ports = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        // generacion inicial del entorno
        float scale = 110;
        LightGrid.GenerateCells((scale/2f), (scale/2f),1,1, scale);

        // actualizacion de puertos COM
        UpdateDropdown();

        // asignacion de funciones para los eventos
        connect.onClick.AddListener(Connect);
        disconnect.onClick.AddListener(Disconnect);
        Gen.onClick.AddListener(Generate);
    }

    // Update is called once per frame
    void Update()
    {
        // control de la camara
        if (Input.GetMouseButton(1))
        {
            if (Input.GetKey(KeyCode.W))
            {
                cam.transform.Translate(0, 0, 1 * sensibility * Time.deltaTime*(Input.GetKey(KeyCode.LeftShift)?shiftBoost:1), Space.Self);
            }
            if (Input.GetKey(KeyCode.S))
            {
                cam.transform.Translate(0, 0, -1 * sensibility * Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? shiftBoost : 1), Space.Self);
            }
            if (Input.GetKey(KeyCode.A))
            {
                cam.transform.Translate(-1 * sensibility * Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? shiftBoost : 1), 0, 0, Space.Self);
            }
            if (Input.GetKey(KeyCode.D))
            {
                cam.transform.Translate(1 * sensibility * Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? shiftBoost : 1), 0, 0, Space.Self);
            }
            if (Input.GetKey(KeyCode.E))
            {
                cam.transform.Translate(0, 1f * sensibility * Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? shiftBoost : 1), 0, Space.World);
            }
            if (Input.GetKey(KeyCode.Q))
            {
                cam.transform.Translate(0, -1f * sensibility * Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? shiftBoost : 1), 0, Space.World);
            }
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            cam.transform.Rotate(0, Input.GetAxis("Mouse X")*rotSen, 0, Space.World);
            cam.transform.Rotate(-1*Input.GetAxis("Mouse Y")* rotSen, 0, 0, Space.Self);
        }

        // rayo para debug
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit))
        {
            Debug.DrawLine(Camera.main.transform.position,hit.point);

            debug.text = $"CtWP: {hit.point}\nObjectName: {hit.collider.gameObject.name}\nCell: {LightGrid.WorldToCell(hit.point)}";
        }

        // actualizacion de puertos COM
        UpdateDropdown();

        // envio de los datos por el puerto serie, si estubiese abierto
        if (sp.IsOpen)
        {
            int x = int.Parse(cordsX.text);
            int z = int.Parse(cordsZ.text);
            LightCell cell = LightGrid.GetCell(new Vector2Int(x,z));

            if (cell.IsLight())
            {
                if (sp.BytesToRead > 0)
                {
                    if (sp.ReadByte() == 0b1010101)
                    {
                        byte[] bits = { cell.Light.GetComponent<LightCore>().output1, cell.Light.GetComponent<LightCore>().output2 };

                        sp.Write(bits, 0, 2);

                        while (sp.BytesToRead>0) sp.ReadByte();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Funcion para la conexion del puerto COM
    /// </summary>
    public void Connect()
    {
        if (!sp.IsOpen)
        {
            sp.PortName = Ports[portSel.value];
            sp.BaudRate = 9600;
            sp.Open();
        }
    }

    /// <summary>
    /// Funcion para la desconexion del puerto COM
    /// </summary>
    public void Disconnect()
    {
        if (sp.IsOpen)
        {
            sp.Close();
        }
    }

    /// <summary>
    /// Funcion de actualizacion de la lista de puertos
    /// </summary>
    void UpdateDropdown()
    {
        if (Ports.ToArray() != SerialPort.GetPortNames())
        {
            List<Dropdown.OptionData> ShowPorts = new List<Dropdown.OptionData>();

            Ports = new List<string>(SerialPort.GetPortNames());

            foreach (string port in Ports)
            {
                ShowPorts.Add(new Dropdown.OptionData(port));
            }

            portSel.options = ShowPorts;
        }
    }

    /// <summary>
    /// Funcion para regenerar el entorno
    /// </summary>
    public void Generate()
    {
        LightGrid.DestroyAllCells();
        LightGrid.DestroyAllCars();

        int x = int.Parse(XCount.text);
        int z = int.Parse(ZCount.text);
        LightGrid.GenerateCells((110 / 2f), (110 / 2f), x, z, 110);
    }
}
