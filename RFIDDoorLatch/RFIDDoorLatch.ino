
#include <SPI.h>
#include <MFRC522.h>
#include <Wire.h>

const int SS_PIN = 10;
const int RST_PIN = 9;
const int DoorID = 8;
const int redPin = 3; 
const int greenPin = 5;
const int bluePin = 6;  
const int bit0Pin = 7;
const int bit1Pin = 8;

// Color Modes
const int COLOR_MODE_OFF = 0;
const int COLOR_MODE_RED = 1;
const int COLOR_MODE_GREEN = 2;
const int COLOR_MODE_BLUE = 3;
const int COLOR_MODE_PURPLE = 4;

MFRC522 rfid(SS_PIN, RST_PIN); // Instance of the class
MFRC522::MIFARE_Key key;
//byte nuidPICC[3]; // Init array that will store new NUID 

String tag = "";
boolean hasTag = false;
unsigned long time;
boolean cardSent = false;

void setup()
{
	Wire.begin(DoorID);
	Serial.begin(115200);
	SPI.begin();
	rfid.PCD_Init(); //Initialize the RFID reader

	pinMode(redPin, OUTPUT);
	pinMode(greenPin, OUTPUT);
	pinMode(bluePin, OUTPUT);
	pinMode(bit0Pin, INPUT);
	pinMode(bit1Pin, INPUT);
	digitalWrite(redPin, LOW);
	digitalWrite(greenPin, LOW);
	digitalWrite(bluePin, LOW);


	time = millis();

	//setLEDMode(COLOR_MODE_BLUE);

	//Serial.print("Door # ");
	//Serial.print(DoorID);
	//Serial.println(" initialized.");

	clearTagData();
	Wire.onRequest(SendData);
	Wire.onReceive(ReceiveData);
}

void loop()
{
	CheckLEDInput();

	if (!hasTag) {
		CheckForCard();
	}
	else {
		unsigned long time2 = millis();
		if ((time2 - time > 3000 || time2 < time)) {
			int bit0 = digitalRead(bit0Pin);
			int bit1 = digitalRead(bit1Pin);
			if(bit0 == LOW && bit1 == HIGH)	clearTagData();
		}
	}
}

void CheckLEDInput() {
	int bit0 = digitalRead(bit0Pin);
	int bit1 = digitalRead(bit1Pin);

	//granted
	if (bit0 == HIGH && bit1 == HIGH) {
		setLEDMode(COLOR_MODE_GREEN);
		return;
	}

	//denied
	if (bit0 == HIGH && bit1 == LOW) {
		setLEDMode(COLOR_MODE_RED);
	}

	//standby
	if (bit0 == LOW && bit1 == HIGH) {
		setLEDMode(COLOR_MODE_BLUE);
		return;
	}

	//off	
	setLEDMode(COLOR_MODE_OFF);
}

	void CheckForCard() {
		// Look for new cards
		if (!rfid.PICC_IsNewCardPresent())
			return;

		// Verify if the NUID has been readed
		if (!rfid.PICC_ReadCardSerial())
			return;

		//Get PICC type - we only use MIFARE 1k
		MFRC522::PICC_Type piccType = rfid.PICC_GetType(rfid.uid.sak);

		if (piccType != MFRC522::PICC_TYPE_MIFARE_1K) {
			Serial.println("This is not the right card type.");
			return;
		}

		//Serial.print("Card ID : ");
		printHex(rfid.uid.uidByte, rfid.uid.size);
		hasTag = true;
		time = millis();
		//setLEDMode(COLOR_MODE_PURPLE);

		//halt PICC
		rfid.PICC_HaltA();
		//stop encryption
		rfid.PCD_StopCrypto1();
		cardSent = false;
	}

	void clearTagData() {
		//Serial.println("clearTagData()");
		tag = "FFFFFFFFFFF";
		cardSent = false;
		hasTag = false;
		//setLEDMode(COLOR_MODE_BLUE);
	}


	void printHex(byte *buffer, byte bufferSize) {
		tag = "";
		for (byte i = 0; i < bufferSize; i++) {
			tag += String(buffer[i], HEX);
		}
		tag.toUpperCase();
		/*Serial.print("tag variable is : ");
		Serial.print(tag);
		Serial.println();*/
	}

	void SendData() {
		if (!cardSent) {
			Serial.println("Sending Data :  " + tag);
			Wire.write(tag.c_str());
			cardSent = true;
		}
	}

	void ReceiveData(int bytes) {
		//char cArr[(bytes + 1)]; 
		//int index = 0;
		//while (Wire.available()) {
		//	cArr[index] = Wire.read();
		//	index++;
		//}
		//String receivedData = String(cArr);
		//receivedData = receivedData.substring(0, 11);
		//Serial.println("Received Data : " + receivedData);

		///*if (receivedData.substring(1) != tag.substring(1)) {
		//	return;
		//}*/
		//if (receivedData.substring(0, 1) == "1" && receivedData.substring(1) == tag.substring(1)) {
		//	GrantAccess();
		//}
		//else {
		//	DenyAccess();
		//}
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

		//PURPLE
		if (color == COLOR_MODE_PURPLE) {
			analogWrite(redPin, 127);
			analogWrite(greenPin, 127);
			analogWrite(bluePin, 127);
		}

		//OFF
		if (color == COLOR_MODE_OFF) {
			digitalWrite(redPin, LOW);
			digitalWrite(greenPin, LOW);
			digitalWrite(bluePin, LOW);
		}

	}

