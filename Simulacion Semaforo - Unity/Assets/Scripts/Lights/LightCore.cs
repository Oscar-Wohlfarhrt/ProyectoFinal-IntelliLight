using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LightCore : MonoBehaviour
{
    // variables de entorno para unity
    public GameObject[] Heads;// array de las cabezas del semaforo
    public GameObject[] SensorsSet1;// set de sensores internos
    public GameObject[] SensorsSet2;// set de sensores externos
    public Text deltasDebug;// texto para el debug de los deltas


    int oldMillis = 0;// variable para la funcion deltamillis

    int deltaFastChange = 2500;// umbral para el cambio rapido
    int lastFastChange;// variable para el seguro del cambio rapido
    bool fastChangeActive = false;// seguro para el cambio rapido
    int[] fastChangeDelta = { 0, 0, 0, 0 };// contadores de cambio rapido

    bool sequence = false;// variable para el estado de la secuencia
    int millisToNext = 0;// variable para el conteo del cambio de fase
    int[] laneDelta = { 0, 0, 0, 0 };// contadores de espera

    int resetTime = 0;// variable para el seguro del reset

    // variables con datos binarios para el envio al decodificador externo
    public byte output1 = 0, output2 = 0;

    // constante para la inicializacion de las instrucciones
    public const uint greenTime = 30;

    // array de instrucciones
    public Instruction[] instructions = {
        new Instruction(new List<statesEnum>{statesEnum.RA,statesEnum.R,statesEnum.R,statesEnum.R},2),
        new Instruction(new List<statesEnum>{statesEnum.V,statesEnum.R,statesEnum.R,statesEnum.R},greenTime),
        new Instruction(new List<statesEnum>{statesEnum.Vtn,statesEnum.R,statesEnum.R,statesEnum.R},2),
        new Instruction(new List<statesEnum>{statesEnum.A,statesEnum.R,statesEnum.R,statesEnum.R},3),
        new Instruction(new List<statesEnum>{statesEnum.R,statesEnum.RA,statesEnum.R,statesEnum.R},2),
        new Instruction(new List<statesEnum>{statesEnum.R,statesEnum.V,statesEnum.R,statesEnum.R},greenTime),
        new Instruction(new List<statesEnum>{statesEnum.R,statesEnum.Vtn,statesEnum.R,statesEnum.R},2),
        new Instruction(new List<statesEnum>{statesEnum.R,statesEnum.A,statesEnum.R,statesEnum.R},3),
        new Instruction(new List<statesEnum>{statesEnum.R,statesEnum.R,statesEnum.RA,statesEnum.R},2),
        new Instruction(new List<statesEnum>{statesEnum.R,statesEnum.R,statesEnum.V,statesEnum.R},greenTime),
        new Instruction(new List<statesEnum>{statesEnum.R,statesEnum.R,statesEnum.Vtn,statesEnum.R},2),
        new Instruction(new List<statesEnum>{statesEnum.R,statesEnum.R,statesEnum.A,statesEnum.R},3),
        new Instruction(new List<statesEnum>{statesEnum.R,statesEnum.R,statesEnum.R,statesEnum.RA},2),
        new Instruction(new List<statesEnum>{statesEnum.R,statesEnum.R,statesEnum.R,statesEnum.V},greenTime),
        new Instruction(new List<statesEnum>{statesEnum.R,statesEnum.R,statesEnum.R,statesEnum.Vtn},2),
        new Instruction(new List<statesEnum>{statesEnum.R,statesEnum.R,statesEnum.R,statesEnum.A},3)
    };

    PhaseControl Control;// objeto de control del semaforo

    // Funcion que se ejecuta unicamente en el primer fotograma de vida del objeto
    void Start()
    {
        Control = new PhaseControl(Heads, instructions.ToList(), true);
    }

    // Funcion que se ejecuta una ves por fotograma
    void Update()
    {
        // obtencion de los valores de los sensores
        bool[] hitsSet1 = GiveSensorState(SensorsSet1);// sensores internos
        bool[] hitsSet2 = GiveSensorState(SensorsSet2);// sensores externos

        // actualizacion del valor delta y conteo para el cambio de fase
        int delta = deltamillis();
        millisToNext += delta;

        // trigger o disparador del modo secuencia
        if (!sequence)
        {
            for (int i = 0; i < 4; i++)
            {
                // se verifican los sensores externos apra identificar si eisten vehiculos
                if (hitsSet2[i])
                {
                    // si existen vehiculos se añaden las instrucciones temporales necesarias para dar verde en esa direccion
                    sequence = true;
                    Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(Control.GetCurrentHead(), statesEnum.A)));
                    Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(i, statesEnum.RA)));
                    Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(i, statesEnum.V)));
                    Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(i, statesEnum.Vtn)));
                    Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(i, statesEnum.A)));

                    // reset para evitar fallos y abiguedades en el ciclo
                    resetTime = 0;
                    for (int l = 0; l < fastChangeDelta.Length; l++)
                    {
                        fastChangeDelta[l] = 0;
                    }

                    break;
                }
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (hitsSet1[i])
            {
                laneDelta[i] += delta;
            }
            else
            {
                laneDelta[i] = 0;
            }
        }

        if ((Control.GetActualInstruction().time * 1000) < millisToNext)
        {
            // si existe una secuencia activa se añaden instrucciones para los vehiculos restantes
            if (sequence)
            {
                int nextHead = NextHead(laneDelta);
                int currentHead = Control.GetCurrentHead();

                // para evitar la saturacion del sistema solo se añaden si no quedan secuencias por ejecutar
                if (Control.TempInstructionsEmpty())
                {
                    Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(nextHead, statesEnum.RA)));
                    Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(nextHead, statesEnum.RA)));
                    Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(nextHead, statesEnum.V)));
                    Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(nextHead, statesEnum.Vtn)));
                    Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(nextHead, statesEnum.A)));
                }
            }

            // se pasa a la siguiente instruccion
            Control.NextInstruction();

            // reset del conteo para cambio de fase
            millisToNext = 0;

            // reset para evitar fallos y abiguedades en el ciclo
            for (int i = 0; i < fastChangeDelta.Length; i++)
            {
                fastChangeDelta[i] = 0;
            }
        }

        // se actualizan los deltas del cambio rapido
        for (int i = 0; i < Heads.Length; i++)
        {
            if (hitsSet1[i] || hitsSet2[i])
            {
                fastChangeDelta[i] = 0;
            }
            else
            {
                fastChangeDelta[i] += delta;
            }
        }

        // secuencia de reset con seguro contra multiples ejecuciones
        if (laneDelta.Max() == 0 && sequence)
        {
            resetTime += delta;
            if (resetTime > 2500)
            {
                sequence = false;
                resetTime = 0;
            }
        }
        else
        {
            resetTime = 0;
        }

        // seguro contra multiples ejecuciones del cambio rapido
        if (Control.GetCurrentHead() != lastFastChange)
        {
            fastChangeActive = false;
        }

        // ejecucion del cambio rapido si se alcansa el umbral
        if (fastChangeDelta[Control.GetCurrentHead()] >= deltaFastChange && sequence && !fastChangeActive)
        {
            // reset del seguro
            fastChangeActive = true;

            int nextHead = NextHead(laneDelta);
            int currentHead = lastFastChange = Control.GetCurrentHead();

            // se limpian y añaden nuevas instrucciones
            Control.ClearTempInstructions();
            Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(currentHead, statesEnum.Vtn)));
            Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(currentHead, statesEnum.A)));
            Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(nextHead, statesEnum.RA)));
            Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(nextHead, statesEnum.V)));
            Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(nextHead, statesEnum.Vtn)));
            Control.AddTempInstruction(Control.GetInstruction(Control.GetStateIndex(nextHead, statesEnum.A)));

            // reset para evitar fallos y abiguedades en el ciclo
            for (int l = 0; l < fastChangeDelta.Length; l++)
            {
                fastChangeDelta[l] = 0;
            }

            // reset del conteo para cambio de fase
            millisToNext = 0;
        }

        // textos de debug para verificacion de errores
        deltasDebug.text = $"millisToNext: {millisToNext}\n" +
            $"LaneDelta[0]: {laneDelta[0]}\n" +
            $"LaneDelta[1]: {laneDelta[1]}\n" +
            $"LaneDelta[2]: {laneDelta[2]}\n" +
            $"LaneDelta[3]: {laneDelta[3]}\n" +
            $"FastChangeDelta[0]: {fastChangeDelta[0]}\n" +
            $"FastChangeDelta[1]: {fastChangeDelta[1]}\n" +
            $"FastChangeDelta[2]: {fastChangeDelta[2]}\n" +
            $"FastChangeDelta[3]: {fastChangeDelta[3]}\n";

        Control.UpdateHeads(delta);// Actualizacion de las luces

        // variables para la transmision al decodificador
        output1 = (byte)((Control.GetBitCodification(0) << 3) | Control.GetBitCodification(1));
        output2 = (byte)((Control.GetBitCodification(2) << 3) | Control.GetBitCodification(3));
    }

    /// <summary>
    /// Retorna el tiempo de simulacion en milisegundos
    /// </summary>
    /// <returns></returns>
    int millis()
    {
        return (int)(Time.time * 1000);
    }

    /// <summary>
    /// Obtiene el delta de la ejecucion anterior y lo debuelve en milisegundos
    /// </summary>
    /// <returns></returns>
    int deltamillis()
    {
        int temp = oldMillis;
        int mil = millis();
        oldMillis = mil;
        return mil - temp;
    }

    //variables auxiliares
    int cicle = 0;
    //int lastHead = 0;

    /// <summary>
    /// Devuelve el indice de la cabeza en la que por mas tiempo se estubo esperando
    /// </summary>
    /// <param name="deltas">tiempos de espera de todas las cabezas</param>
    /// <returns></returns>
    int NextHead(int[] deltas)
    {
        int index = (int)deltas.ToList().IndexOf(deltas.Max());
        if (deltas.Max() != 0)
        {
            cicle = index;
            return index;
        }
        else
        {
            if (!(++cicle < Heads.Length))
                cicle = 0;

            return cicle;
        }
    }

    /// <summary>
    /// Debuelve un array con la informacion de los sensores
    /// </summary>
    /// <param name="sensorSet">Array de los sensores para extrar los datos</param>
    /// <returns></returns>
    bool[] GiveSensorState(GameObject[] sensorSet)
    {
        List<bool> hits = new List<bool>();
        for (int i = 0; i < sensorSet.Length; i++)
        {
            hits.Add(sensorSet[i].GetComponent<inductanceSensor>().vehicle);
        }
        return hits.ToArray();
    }
}

