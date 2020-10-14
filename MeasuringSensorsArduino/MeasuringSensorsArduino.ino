#define USE_ARDUINO_INTERRUPTS true
#include <PulseSensorPlayground.h>
#include <SoftwareSerial.h>
const int PulseWire = 1;
const int LED13 = 13;
int Threshold = 700; 

PulseSensorPlayground pulseSensor;

SoftwareSerial BT(0,1);
void setup() {
  pulseSensor.analogInput(PulseWire);   
  pulseSensor.blinkOnPulse(LED13);
  pulseSensor.setThreshold(Threshold);
  pulseSensor.begin();
  BT.begin(9600);
}

void loop() {
  int reading = analogRead(0);
  delay(20);
  int myBPM = pulseSensor.getBeatsPerMinute();
 
  float voltage = reading * 5.0;
  voltage /= 1024.0; 

  float temperatureC = (voltage - 0.5) * 100 ;
  if (pulseSensor.sawStartOfBeat()) {
      String str = "";
      str += "BPM: ";
      str += myBPM;
      str += "  Temp: ";
      str += temperatureC;
      char bytesToSend[5];
      bytesToSend[0] = 33;
      bytesToSend[1] = myBPM;
      bytesToSend[2] = (int) temperatureC;
      bytesToSend[3] = ((int)(temperatureC * 100))%100;
      bytesToSend[4] = 47;
      BT.println(bytesToSend);
  }
  delay(20);
}
