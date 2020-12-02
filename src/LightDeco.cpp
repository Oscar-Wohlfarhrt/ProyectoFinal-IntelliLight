#include <LightDeco.h>

//constructor
LightDeco::LightDeco(const uint8_t *dispPins, uint8_t MUXNum, const uint8_t *MUXPins)
{
    _LightPins = dispPins;
    _MUXNum = MUXNum;
    _MUXPins = MUXPins;
}

//convierte de hz a ms respecto de la cantidad de displays
uint8_t LightDeco::HzToMs(uint16_t hz)
{
    return ((1000 / hz) / _MUXNum);
}

//establece los milisegundos de espera para cada display
void LightDeco::begin(uint8_t ms)
{
    _ms = ms;
    for (int i = 0; i < 3; i++)
    {
        pinMode(_LightPins[i], OUTPUT);
        digitalWrite(_LightPins[i], false);
    }

    for (int i = 0; i < _MUXNum; i++)
    {
        pinMode(_MUXPins[i], OUTPUT);
        digitalWrite(_MUXPins[i], false);
    }
}

//escribe en una unica cabeza (MUX: numero de display)
void LightDeco::writeHead(uint8_t bits, uint8_t MUX)
{
    for (int i = 0; i < 3; i++)
    {
        digitalWrite(_LightPins[i], !(bits & (1 << i)));
    }

    digitalWrite(_MUXPins[MUX], true);
    delay(_ms);
    digitalWrite(_MUXPins[MUX], false);
}

//escribe en todas las cabezas
void LightDeco::writeAllHeads(uint8_t *bits)
{
    for (int i = 0; i < _MUXNum; i++)
    {
        writeHead(bits[i], i);
    }
}
