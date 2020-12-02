#include <Arduino.h>
#include <LightDeco.h>

const uint8_t lightPins[3] = {12, 11, 10};//pines para las luces
const uint8_t muxPins[4] = {6, 7, 8, 9};//pines para la multiplexacion de las cabezas

uint8_t initialState = 0b100; //estado inicial para la luz roja

uint8_t headBits[4] = {initialState, initialState, initialState, initialState};//array con los estados iniciales

LightDeco deco(lightPins, 4, muxPins);// Control de las luces

void setup()
{
    //inicializacion del serial y el control
  Serial.begin(9600);
  deco.begin(deco.HzToMs(45));
}

void loop()
{
    //espera que a que haya dos bytes para la lectura
  if (!(Serial.available() < 2))
  {
      //lee y separa la informacion para cada cabeza
    uint8_t in = Serial.read();
    headBits[0] = (in >> 3);
    headBits[1] = (in & 0b111);
    in = Serial.read();
    headBits[2] = (in >> 3);
    headBits[3] = (in & 0b111);
  }
  else
  {
      //escribe un valor especifico para decir que esta a la espera de los datos
    Serial.write(0b1010101);
  }

  //enciende las luces correspondientes a la informacion recibida
  deco.writeAllHeads(headBits);
}