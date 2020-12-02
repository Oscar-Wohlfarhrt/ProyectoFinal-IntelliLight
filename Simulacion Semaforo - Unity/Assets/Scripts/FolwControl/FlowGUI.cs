using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlowGUI : MonoBehaviour
{
    //variables para los objetos de la intefaz
    public Slider slide;
    public Text debug;
    public InputField Rate;
    public InputField Ran;

    // Funcion que se ejecuta unicamente en el primer fotograma de vida del objeto
    void Start()
    {
        //Se añaden las funciones para los eventos correspondientes
        slide.onValueChanged.AddListener(SliderChange);
        Rate.onValueChanged.AddListener(RateInputChange);
        Ran.onValueChanged.AddListener(RanInputChange);
    }

    // Funcion que se ejecuta una ves por fotograma
    void Update()
    {
        // actualizacion de la interfaz y de la variable estatica de flujo vehicular
        FlowControl.FlowRate = slide.value;
        debug.text=$"Generación: {FlowControl.FlowRate} cars/min";
    }

    /// <summary>
    /// Funcion para el evento de actualizacion del campo Slider
    /// </summary>
    /// <param name="input"></param>
    public void SliderChange(float value)
    {
        if (float.Parse(Rate.text) != value)
            Rate.text = value.ToString();
    }

    /// <summary>
    /// Funcion para el evento de actualizacion del campo Rate
    /// </summary>
    /// <param name="input"></param>
    public void RateInputChange(string input)
    {  
        float f;
        if (float.TryParse(input,out f)) {
            if (f >= 0 && f <= 60 && (f != slide.value))
                slide.value = f;
        }
    }

    /// <summary>
    /// Funcion para el evento de actualizacion del campo Ran
    /// </summary>
    /// <param name="input"></param>
    public void RanInputChange(string input)
    {
        float f;
        if (float.TryParse(input, out f))
            FlowControl.ran = f;
            
    }
}
