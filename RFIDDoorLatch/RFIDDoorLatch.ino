#include <EasyTransferI2C.h>
#include <Wire.h>

int DoorID = 1;
int RFIDResetPin = 8;
int redPin   = 9;   // Red LED,   connected to digital pin 9
int greenPin = 10;  // Green LED, connected to digital pin 10
int bluePin  = 11;  // Blue LED,  connected to digital pin 11


// Color Modes
int COLOR_MODE_RED = 1;
int COLOR_MODE_GREEN = 2;
int COLOR_MODE_BLUE = 3;
int COLOR_MODE_OFF = 0;

EasyTransferI2C ET;

struct DATA_STRUCTURE {
	int access = 0;
	int door = DoorID;
	char char1;
	char char2;
	char char3;
	char char4;
	char char5;
	char char6;
	char char7;
	char char8;
	char char9;
	char char10;
	char char11;
	char char12;
};

DATA_STRUCTURE tagData;
String tag = "";

void setup()
{
	pinMode(RFIDResetPin, OUTPUT);
	digitalWrite(RFIDResetPin, HIGH);
	Wire.begin(DoorID);
	ET.begin(details(tagData), &Wire);
	Wire.onReceive(Receive);
	Serial.begin(9600);
 
  pinMode(redPin, OUTPUT);
  pinMode(greenPin, OUTPUT);
  pinMode(bluePin, OUTPUT);
  digitalWrite(redPin, LOW);
  digitalWrite(greenPin, LOW);
  digitalWrite(bluePin, LOW);

//  Serial.println(COLOR_MODE_BLUE);
	setLEDMode(COLOR_MODE_BLUE);
//	Serial.print("Door # ");
	Serial.println(DoorID + " initialized.");
//	Serial.println(" initialized.");
}

void loop()
{
	char val = 0; // variable to store the data from the serial port
  int count = 0;

//  Serial.println(Serial.available());
  // Check to see if a tag needs to be checked
  if (tag == "") {
    while (Serial.available() > 0) {
      val = Serial.read();
      if (count == 1) tagData.char1 = val;
      if (count == 2) tagData.char2 = val;
      if (count == 3) tagData.char3 = val;
      if (count == 4) tagData.char4 = val;
      if (count == 5) tagData.char5 = val;
      if (count == 6) tagData.char6 = val;
      if (count == 7) tagData.char7 = val;
      if (count == 8) tagData.char8 = val;
      if (count == 9) tagData.char9 = val;
      if (count == 10) tagData.char10 = val;
      if (count == 11) tagData.char11 = val;
      if (count == 12) tagData.char12 = val;
      if (count > 0) tag += val;
      count++;
    }
    //Serial.println(tag);
    if (tag != "") {
//      tagData.tag.replace("\n", "");
//      tagData.tag = tagData.tag.substring(1, tagData.tag.length() - 1);
//      Serial.println("Read tag " + tag);
      setLEDMode(COLOR_MODE_GREEN);
      delay(2000);
      setLEDMode(COLOR_MODE_BLUE);
      clearTagData();
    }
  }
  delay(500);
}



void Receive(int numBytes)
{
}


void setLEDMode(int color) {
  //Serial.println(color);
	// RED
	if (color == COLOR_MODE_RED) {
		//Serial.println("Red LED");
		digitalWrite(redPin, HIGH);
		digitalWrite(greenPin, LOW);
		digitalWrite(bluePin, LOW);
	}

	// GREEN
	if (color == COLOR_MODE_GREEN) {
		//Serial.println("Green LED");
		digitalWrite(redPin, LOW);
		digitalWrite(greenPin, HIGH);
		digitalWrite(bluePin, LOW);
	}

	// BLUE
	if (color == COLOR_MODE_BLUE) {
		//Serial.println("Blue LED");
		digitalWrite(redPin, LOW);
		digitalWrite(greenPin, LOW);
		digitalWrite(bluePin, HIGH);
	}

}

void clearTagData() {
//	Serial.println("clearTagData()");
	tag = "";
	tagData.access = 0;
	tagData.char1 = '\0';
	tagData.char2 = '\0';
	tagData.char3 = '\0';
	tagData.char4 = '\0';
	tagData.char5 = '\0';
	tagData.char6 = '\0';
	tagData.char7 = '\0';
	tagData.char8 = '\0';
	tagData.char9 = '\0';
	tagData.char10 = '\0';
	tagData.char11 = '\0';
	tagData.char12 = '\0';
}










void sendData() {
	//  String strValue = String(LDRValue);
	char cArr[7]; 
	//strValue.toCharArray(cArr, 6);
	Wire.write(cArr);
}

void receiveData(int bytes) {
	char cArr[(bytes + 1)];
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

