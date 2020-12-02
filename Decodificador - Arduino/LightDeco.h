#pragma once

#include <Arduino.h>

class LightDeco{
    private:
        const uint8_t *_LightPins;//pines del display
        uint8_t _MUXNum;//camtidad de displays
        const uint8_t *_MUXPins;//pines para la multiplexacion
        uint8_t _ms;//milisegundos de retardo para imprimir
    public:
        //constructor
        LightDeco(const uint8_t *LightPins, uint8_t MUXNum, const uint8_t *MUXPins);
        //establece los milisegundos de espera para cada display
        void begin(uint8_t ms);
        //convierte de hz a ms respecto de la cantidad de displays
        uint8_t HzToMs(uint16_t hz);
        //escribe en una unica cabeza (MUX: numero de display)
        void writeHead(uint8_t bits, uint8_t MUX);
        //escribe en todas las cabezas
        void writeAllHeads(uint8_t *bits);
};