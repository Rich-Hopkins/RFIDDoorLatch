
#include <SPI.h>
#include <MFRC522.h>
#include <Wire.h>

const int SS_PIN = 10;
const int RST_PIN = 9;
const int DoorID = 8;
const int redPin = 7;  // Red LED
const int greenPin = 6;  // Green LED
const int bluePin = 5;  // Blue LED
String strDoorID;

// Color Modes
const int COLOR_MODE_RED = 1;
const int COLOR_MODE_GREEN = 2;
const int COLOR_MODE_BLUE = 3;
const int COLOR_MODE_OFF = 0;

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
	Serial.begin(9600);
	SPI.begin();
	rfid.PCD_Init();

	strDoorID = String(DoorID);
	while (strDoorID.length() < 2) {
		strDoorID = "0" + strDoorID;
	}

	pinMode(redPin, OUTPUT);
	pinMode(greenPin, OUTPUT);
	pinMode(bluePin, OUTPUT);
	digitalWrite(redPin, LOW);
	digitalWrite(greenPin, LOW);
	digitalWrite(bluePin, LOW);

	time = millis();

	setLEDMode(COLOR_MODE_BLUE);
	Serial.println("Door # " + strDoorID + " initialized");
	clearTagData();
	Wire.onRequest(SendData);
	Wire.onReceive(ReceiveData);
}

void loop()
{
	if (!hasTag) {
		CheckForCard();
	}
	else {
		unsigned long time2 = millis();
		if (time2 - time > 5000 || time2 < time) {
			clearTagData();
		}
	}
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

		//halt PICC
		rfid.PICC_HaltA();
		//stop encryption
		rfid.PCD_StopCrypto1();
		cardSent = false;
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
		Serial.println("clearTagData()");
		tag = "FFFFFFFFFFF";
		cardSent = false;
		hasTag = false;
	}


	void printHex(byte *buffer, byte bufferSize) {
		tag = "0";
		tag += strDoorID;
		for (byte i = 0; i < bufferSize; i++) {
			tag += String(buffer[i], HEX);
		}
		tag.toUpperCase();
		Serial.print("tag variable is : ");
		Serial.print(tag);
		Serial.println();
	}

	void SendData() {
		if (!cardSent) {
			Serial.println("Sending " + tag);
			Wire.write(tag.c_str());
			cardSent = true;
		}
	}

	void ReceiveData(int bytes) {
		char cArr[(bytes + 1)]; //					TRY REMOVING + 1
		int index = 0;
		while (Wire.available()) {
			cArr[index] = Wire.read();
			index++;
		}
		String receivedData = String(cArr);
		Serial.println("Received Data : " + receivedData);
		Serial.println("Received Data without Unlock Bit: " + receivedData.substring(1));
		Serial.println("     Tag Data without Unlock Bit:  " + tag.substring(1));

		if (receivedData.substring(1) != tag.substring(1)) {
			return;
		}
		if (cArr[0] == 1 && receivedData.substring(1) == tag.substring(1)) {
			GrantAccess();
		}
		else {
			DenyAccess();
		}
	}

	void GrantAccess() {
		setLEDMode(COLOR_MODE_GREEN);
		delay(5000);
		setLEDMode(COLOR_MODE_BLUE);
	}

	void DenyAccess() {
		setLEDMode(COLOR_MODE_RED);
		delay(5000);
		setLEDMode(COLOR_MODE_BLUE);
	}

