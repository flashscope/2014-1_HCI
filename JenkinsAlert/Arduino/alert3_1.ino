/*
  Repeating Wifi Web client
 
 This sketch connects to a a web server and makes a request
 using an Arduino Wifi shield.
 
 Circuit:
 * Wifi shield attached to pins 10, 11, 12, 13
 
 created 23 April 2012
 modifide 31 May 2012
 by Tom Igoe
 
 http://arduino.cc/en/Tutorial/WifiWebClientRepeating
 This code is in the public domain.
 */

#include <SPI.h>
#include <WiFi.h>


char ssid[] = "ssid";      //  your network SSID (name) 
char pass[] = "pass";   // your network password

int keyIndex = 0;            // your network key Index number (needed only for WEP)

int status = WL_IDLE_STATUS;

// Initialize the Wifi client library
WiFiClient client;

int MOTOR = 9;
const int ledPin =  8; 

bool isUP = true;
int motorValue = 120;

// server address:
char server[] = "serverAddress";

unsigned long lastConnectionTime = 0;           // last time you connected to the server, in milliseconds
boolean lastConnected = false;                  // state of the connection last time through the main loop
const unsigned long postingInterval = 10*1000;  // delay between updates, in milliseconds

void setup() {
  pinMode(MOTOR, OUTPUT);
  pinMode(ledPin, OUTPUT);
  
  //Initialize serial and wait for port to open:
  Serial.begin(9600); 
  connectWIFI();
}
void connectWIFI() {
    while (!Serial) {
    ; // wait for serial port to connect. Needed for Leonardo only
  }
  
  // check for the presence of the shield:
  if (WiFi.status() == WL_NO_SHIELD) {
    Serial.println("WiFi shield not present"); 
    // don't continue:
    while(true);
  } 
  
  // attempt to connect to Wifi network:
  while ( status != WL_CONNECTED) { 
    Serial.print("Attempting to connect to SSID: ");
    Serial.println(ssid);
    // Connect to WPA/WPA2 network. Change this line if using open or WEP network:    
    status = WiFi.begin(ssid, pass);

    // wait 10 seconds for connection:
    delay(10000);
  } 
  // you're connected now, so print out the status:
  printWifiStatus();
}
void loop() {
  // if there's incoming data from the net connection.
  // send it out the serial port.  This is for debugging
  // purposes only:
    if(isUP) {
    --motorValue;
    
    if(motorValue<30) {
      isUP = !isUP;
    }
    delay(10);
  }else {
    ++motorValue;
    
    if(motorValue>60) {
      isUP = !isUP;
    }
    delay(10);
  }
  analogWrite(MOTOR, motorValue);
  
  
  while (client.available()) {
    char c = client.read();
    Serial.write(c);
    if (c=='0') {
       digitalWrite(ledPin, HIGH); 
       motorValue = 120;
    } else {
       digitalWrite(ledPin, LOW);  
       motorValue=0;
    }
  }

  // if there's no net connection, but there was one last time
  // through the loop, then stop the client:
  if (!client.connected()) {
    Serial.println();
    Serial.println("disconnecting.");
    client.stop();
    connectWIFI();
  }

  // if you're not connected, and ten seconds have passed since
  // your last connection, then connect again and send data:
  if(!client.connected() ) {
    httpRequest();
    delay(5000);
  }
  
}

// this method makes a HTTP connection to the server:
void httpRequest() {
  // if there's a successful connection:
  if (client.connect(server, 80)) {
    Serial.println("connecting...");
    // send the HTTP PUT request:
    client.println("GET /HCI/JENKINS/index.php HTTP/1.1");
    client.println("Host: serverAddress");
    client.println("User-Agent: arduino-ethernet");
    client.println("Connection: close");
    client.println();

    // note the time that the connection was made:
    lastConnectionTime = millis();
  } 
  else {
    // if you couldn't make a connection:
    Serial.println("connection failed");
    Serial.println("disconnecting.");
    client.stop();
  }
}


void printWifiStatus() {
  // print the SSID of the network you're attached to:
  Serial.print("SSID: ");
  Serial.println(WiFi.SSID());

  // print your WiFi shield's IP address:
  IPAddress ip = WiFi.localIP();
  Serial.print("IP Address: ");
  Serial.println(ip);

  // print the received signal strength:
  long rssi = WiFi.RSSI();
  Serial.print("signal strength (RSSI):");
  Serial.print(rssi);
  Serial.println(" dBm");
}






