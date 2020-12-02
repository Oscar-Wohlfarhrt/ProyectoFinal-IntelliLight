using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Enumeracion para lo posibles estados del semaforo
/// </summary>
public enum statesEnum
{
	R,
	A,
	V,
	Rtn,
	Atn,
	Vtn,
	Rtr,
	Atr,
	Vtr,
	RAtn,
	RAtr,
	RA,
	AVtn,
	AVtr,
	AtnV,
	AtrV
}

//Clase para la codificacion y decodificacion de los archivos con las instrucciones
public static class StateDecoder
{
	/// <summary>
	/// Devuelve un estado apartir de una string
	/// </summary>
	/// <param name="state">Nombre del estado</param>
	/// <returns></returns>
	static public statesEnum StringToState(string state)
	{
		switch (state)
		{
			case "R":
				return statesEnum.R;
			case "A":
				return statesEnum.A;
			case "V":
				return statesEnum.V;
			case "Rtn":
				return statesEnum.Rtn;
			case "Atn":
				return statesEnum.Atn;
			case "Vtn":
				return statesEnum.Vtn;
			case "Rtr":
				return statesEnum.Rtr;
			case "Atr":
				return statesEnum.Atr;
			case "Vtr":
				return statesEnum.Vtr;
			case "RAtn":
				return statesEnum.RAtn;
			case "RAtr":
				return statesEnum.RAtr;
			case "RA":
				return statesEnum.RA;
			case "AVtn":
				return statesEnum.AVtn;
			case "AVtr":
				return statesEnum.AVtr;
			case "AtnV":
				return statesEnum.AtnV;
			case "AtrV":
				return statesEnum.AtrV;
			default:
				return statesEnum.R;
		}
	}

	/// <summary>
	/// devuelve un texto con nombre del estado
	/// </summary>
	/// <param name="state">Estado a devolver</param>
	/// <returns></returns>
	static public string StateToString(statesEnum state)
	{
		switch (state)
		{
			case statesEnum.R:
				return "R";
			case statesEnum.A:
				return "A";
			case statesEnum.V:
				return "V";
			case statesEnum.Rtn:
				return "Rtn";
			case statesEnum.Atn:
				return "Atn";
			case statesEnum.Vtn:
				return "Vtn";
			case statesEnum.Rtr:
				return "Rtr";
			case statesEnum.Atr:
				return "Atr";
			case statesEnum.Vtr:
				return "Vtr";
			case statesEnum.RAtn:
				return "RAtn";
			case statesEnum.RAtr:
				return "RAtr";
			case statesEnum.RA:
				return "RA";
			case statesEnum.AVtn:
				return "AVtn";
			case statesEnum.AVtr:
				return "AVtr";
			case statesEnum.AtnV:
				return "AtnV";
			case statesEnum.AtrV:
				return "AtrV";
			default:
				return "R";
		}
	}

	/// <summary>
	/// Realiza una conversion masiva de string a estados
	/// </summary>
	/// <param name="stringStates">Lista de estados</param>
	/// <param name="headCount">Cantidad de cabezas que posee el semaforo</param>
	/// <returns></returns>
	static public List<statesEnum> DecoStates(List<string> stringStates, int headCount)
	{
		List<statesEnum> states = new List<statesEnum>();
		for (int i = 0; i < headCount; i++)
		{
			states.Add(StringToState(stringStates[i]));
		}
		return states;
	}

	/// <summary>
	/// Realzia un conversion masiva de estados a string
	/// </summary>
	/// <param name="states">estados a convertir</param>
	/// <param name="time">tiempo que le corresponde a dicho estado</param>
	/// <returns></returns>
	static public List<string> CodeStates(List<statesEnum> states, uint time)
	{
		List<string> stringStates = new List<string>();
		for (int i = 0; i < states.Count; i++)
		{
			stringStates.Add(StateToString(states[i]));
		}
		stringStates.Add(Convert.ToString(time));
		return stringStates;
	}
}

public class Instruction
{
	//lista de los estados para las respectivas cabezas
	public List<statesEnum> head = new List<statesEnum>();
	//tiempo que le corresponde a cada instruccion
	public uint time;

	// constructor
	public Instruction(List<statesEnum> states, uint phaseTime)
	{
		head = states;
		time = phaseTime;
	}
}

/// <summary>
/// Clase para el control general de las fases del semaforo
/// </summary>
public class PhaseControl
{
	//cabezas a controlar
	private GameObject[] Heads;

	//Lista de instrucciones a ejecutar
	private List<Instruction> Phases;

	//Cola de instrucciones temporales  a ejecutar
	private Queue<Instruction> TempPhases;

	//indice de istruccion actual
	int count;

	//cabeza que se encuentra actualmente en verde, si existen dos o mas activas devuelve la de menor indice
	int currentHead;

	//direccion de los archivos con las instrucciones
	string PhasePath = Application.dataPath+"/";
	string BaseFile = "Paths.txt";

	// variables para los temporizadores de parpadeo normal y rapido
	private int millistn = 0, millistr = 0;
	// estado del parpadeo normal y rapido
	private bool tnb = false, trb = false;

	/// <summary>
	/// Constructor del objeto
	/// </summary>
	/// <param name="Heads">Cabezas a controlar</param>
	/// <param name="Instructions">Instrucciones a ejecutar</param>
	/// <param name="tryLoad">Si debe cargar las instrucciones de los archivos (true) o usar las proporcionadas en el argumento anterior (false)</param>
	public PhaseControl(GameObject[] Heads, List<Instruction> Instructions,bool tryLoad=false)
	{
		this.Heads = Heads;

		//Inicializacion de la colo temporal
		TempPhases = new Queue<Instruction>();

		//Prueba y error para la carga de archivos, si es que se encuentran corrmpidos o imcomprencibles
		if (tryLoad)
		{
			if (File.Exists(PhasePath+BaseFile))
			{
				string[] baseFile = File.ReadAllLines(PhasePath + BaseFile);
				int index;
				if(int.TryParse(baseFile[0],out index)&&index<baseFile.Length)
				{
					if (File.Exists(PhasePath + baseFile[index + 1]))
					{
						List<string> InstructionFile = File.ReadAllLines(PhasePath + baseFile[index + 1]).ToList();
						List<List<string>> DecoFormat = InstructionFile.Select(line=> { return line.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries).ToList(); }).ToList();
						int headCount;
						if(int.TryParse(DecoFormat[0][0],out headCount)){
							List<Instruction> LoadedInstructions = new List<Instruction>();
							for (int i =1;i< DecoFormat.Count; i++)
							{
								LoadedInstructions.Add(new Instruction(StateDecoder.DecoStates(DecoFormat[i],headCount), uint.Parse(DecoFormat[i][headCount])));
							}
							Phases = LoadedInstructions;
							goto Success;
						}
						else
						{
							goto ErrorOnLoad;
						}
					}
					else
					{
						goto ErrorOnLoad;
					}
				}
				else
				{
					goto ErrorOnLoad;
				}
			}
			else
			{
				// creacion de los archivos si no existen
				File.WriteAllLines(PhasePath + BaseFile,new string[] {"0","default.txt" });
				List<string> lines = new List<string>();
				lines.Add(Convert.ToString(Instructions[0].head.Count));
				lines.AddRange(Instructions.Select(inst => { return string.Join("\t", StateDecoder.CodeStates(inst.head, inst.time)); }).ToList());
				File.WriteAllLines(PhasePath + "default.txt", lines.ToArray());
				goto ErrorOnLoad;
			}
		}

		//goto en caso de error
	ErrorOnLoad:
		Phases = Instructions;

		//goto en caso de una carga exitosa
	Success:;
	}

	/// <summary>
	/// Pasa a la siguiente instruccion
	/// </summary>
	public void NextInstruction()
	{
		if (TempPhases.Count > 0)
		{
			Instruction instruction = TempPhases.Dequeue();
			count = Phases.FindIndex(x => { return x == instruction; });
		}
		else
		{
			count++;

			if (!(count < Phases.Count))
				count = 0;
		}
	}

	/// <summary>
	/// Selecciona la instruccion especificada
	/// </summary>
	/// <param name="index">indice de la lista de instrucciones</param>
	public void SetInstruction(int index)
	{
		if (index < Phases.Count)
			count = index;
	}

	/// <summary>
	/// Obtiene la instruccion especificada
	/// </summary>
	/// <param name="index">indice de la instruccion a retornar</param>
	/// <returns></returns>
	public Instruction GetInstruction(int index)
	{
		return Phases[index];
	}

	/// <summary>
	/// Obtiene el indice actual en la lista de instrucciones
	/// </summary>
	/// <returns></returns>
	public int GetPosition()
	{
		return count;
	}

	/// <summary>
	/// devuelve el primer indice del la instruccion en la que coincida el estado para la cabeza especificada
	/// </summary>
	/// <param name="head">Cabeza a la que corresponde el estado</param>
	/// <param name="state">Estado a buscar</param>
	/// <returns></returns>
	public int GetStateIndex(int head, statesEnum state)
	{
		for (int i = 0; i < Phases.Count; i++)
		{
			if (Phases[i].head[head] == state)
			{
				return i;
			}
		}

		return 0;
	}

	/// <summary>
	/// devuelve la cabeza que esta en verde
	/// </summary>
	/// <returns></returns>
	public int GetCurrentHead()
	{
		return currentHead;
	}

	/// <summary>
	/// Añade una instruccion temporal
	/// </summary>
	/// <param name="instruction">Instruccion a añadir a la cola temporal</param>
	public void AddTempInstruction(Instruction instruction)
	{
		TempPhases.Enqueue(instruction);
	}

	/// <summary>
	/// Limpia la cola de instrucciones temporales
	/// </summary>
	public void ClearTempInstructions()
	{
		TempPhases.Clear();
	}

	/// <summary>
	/// Devuelve true si la cola de intrucciones temporales se encuentra vacia
	/// </summary>
	/// <returns></returns>
	public bool TempInstructionsEmpty()
	{
		return !(TempPhases.Count > 0);
	}

	/// <summary>
	/// Hace un update de los contadores para el parpadeo normal y rapido
	/// </summary>
	/// <param name="deltaMillis">delta de tiempo correspondiente desde la ultima ejecucion</param>
	public void Update(int deltaMillis)
	{
		millistn += deltaMillis;
		if (millistn >= 500)
		{
			tnb = !tnb;
			millistn = 0;
		}

		millistr += deltaMillis;
		if (millistr >= 250)
		{
			trb = !trb;
			millistr = 0;
		}
	}

	/// <summary>
	/// Actualiza el estado de las cabezas
	/// </summary>
	/// <param name="delta">delta de tiempo correspondiente desde la ultima ejecucion</param>
	public void UpdateHeads(int delta)
	{
		Update(delta);
		if (TempPhases.Count > 0)
		{
			for (int i = 0; i < Heads.Length; i++)
			{
				statesEnum state = TempPhases.Peek().head[i];
				Heads[i].GetComponent<Renderer>().material.color = GetState(state);
				if (state != statesEnum.R)
				{
					currentHead = i;
				}
			}
		}
		else
		{
			for (int i = 0; i < Heads.Length; i++)
			{
				statesEnum state = Phases[count].head[i];
				Heads[i].GetComponent<Renderer>().material.color = GetState(state);
				if (state != statesEnum.R)
				{
					currentHead = i;
				}
			}
		}
	}

	/// <summary>
	/// Devuelve el color que corresponde al estado proporcionado
	/// </summary>
	/// <param name="state">Estado del que se quiere obtener la conversion</param>
	/// <returns></returns>
	public Color GetState(statesEnum state)
	{
		switch (state)
		{
			case statesEnum.R:
				return Color.red;
			case statesEnum.A:
				return Color.yellow;
			case statesEnum.V:
				return Color.green;
			case statesEnum.Rtn:
				return tnb ? Color.red : Color.white;
			case statesEnum.Atn:
				return tnb ? Color.yellow : Color.white;
			case statesEnum.Vtn:
				return tnb ? Color.green : Color.gray;
			case statesEnum.Rtr:
				return trb ? Color.red : Color.white;
			case statesEnum.Atr:
				return trb ? Color.yellow : Color.white;
			case statesEnum.Vtr:
				return trb ? Color.green : Color.gray;
			case statesEnum.RAtn:
				return tnb ? Color.magenta : Color.white;
			case statesEnum.RAtr:
				return trb ? Color.magenta : Color.white;
			case statesEnum.RA:
				return Color.magenta;
			case statesEnum.AVtn:
				return tnb ? Color.blue : Color.yellow;
			case statesEnum.AVtr:
				return trb ? Color.blue : Color.yellow;
			case statesEnum.AtnV:
				return tnb ? Color.blue : Color.green;
			case statesEnum.AtrV:
				return trb ? Color.blue : Color.green;
			default:
				return Color.white;
		}
	}

	/// <summary>
	/// Devuelve los bits codificados que corresponde al estado proporcionado
	/// </summary>
	/// <param name="state">Estado del que se quiere obtener la conversion</param>
	/// <returns></returns>
	public byte GetBitCodification(int head)
	{
		switch (GetActualInstruction().head[head])
		{
			case statesEnum.R:
				return 0b100;
			case statesEnum.A:
				return 0b010;
			case statesEnum.V:
				return 0b001;
			case statesEnum.Rtn:
				return (byte)(tnb ? 0b100 : 0b000);
			case statesEnum.Atn:
				return (byte)(tnb ? 0b010 : 0b000);
			case statesEnum.Vtn:
				return (byte)(tnb ? 0b001 : 0b000);
			case statesEnum.Rtr:
				return (byte)(trb ? 0b100 : 0b000);
			case statesEnum.Atr:
				return (byte)(trb ? 0b010 : 0b000);
			case statesEnum.Vtr:
				return (byte)(trb ? 0b001 : 0b000);
			case statesEnum.RAtn:
				return (byte)(tnb ? 0b110 : 0b000);
			case statesEnum.RAtr:
				return (byte)(trb ? 0b110 : 0b000);
			case statesEnum.RA:
				return 0b110;
			case statesEnum.AVtn:
				return (byte)(tnb ? 0b011 : 0b010);
			case statesEnum.AVtr:
				return (byte)(trb ? 0b011 : 0b010);
			case statesEnum.AtnV:
				return (byte)(tnb ? 0b011 : 0b001);
			case statesEnum.AtrV:
				return (byte)(trb ? 0b011 : 0b001);
			default:
				return 0b000;
		}
	}

	/// <summary>
	/// devuelve la instruccion que se está ejecutando actualmente
	/// </summary>
	/// <returns></returns>
	public Instruction GetActualInstruction()
	{
		if (TempPhases.Count > 0)
		{
			return TempPhases.Peek();
		}
		else
		{
			return Phases[count];
		}
	}
}

