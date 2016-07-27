#include <Wire.h>

int LDR = 0;
int LDRValue = 0;
int LightSensitivity = 600;

void setup()
{
	Serial.begin(9600);
  Wire.begin(0x40);
	pinMode(13, OUTPUT);
}

void loop()
{
	LDRValue = analogRead(LDR);
	delay(100);

  Wire.onRequest(sendData);
  Wire.onReceive(receiveData);
}

void sendData() {
  String strValue = String(LDRValue);
  char cArr[7]; 
  strValue.toCharArray(cArr, 6);
  Wire.write(cArr);
  //Serial.println(LDRValue);
  //Serial.println(cArr);
}

void receiveData(int bytes) {
  char cArr[bytes + 1];
  int index = 0;
  while (Wire.available()) {
    cArr[index] = Wire.read();
    index++;
  }
  if(cArr[0] == '1') {
    digitalWrite(13, HIGH);
  }
  else {
    digitalWrite(13, LOW);
  }
}

