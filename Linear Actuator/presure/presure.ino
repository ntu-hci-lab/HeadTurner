//VALUE
int presure_valueU=0;
int presure_valueD=0;
int presure_valueL=0;
int presure_valueR=0;
//PIN
int presure_pinU=A1;
int presure_pinD=A2;
int presure_pinL=A3;
int presure_pinR=A4;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(115200);
  pinMode(A1, INPUT);
  pinMode(A2, INPUT);
  pinMode(A3, INPUT);
  pinMode(A4, INPUT);
}

void loop() {
  // put your main code here, to run repeatedly:
   presure_valueU = analogRead(presure_pinU);
   presure_valueD = analogRead(presure_pinD);
   presure_valueL = analogRead(presure_pinL);
   presure_valueR = analogRead(presure_pinR);
//   String data = " UP:"+String(presure_valueU)+" DOWN:"+String(presure_valueD)+" LEFT:"+String(presure_valueL)+" RIGHT:"+String(presure_valueR); 
   String data = String(presure_valueU)+","+String(presure_valueD)+","+String(presure_valueL)+","+String(presure_valueR);    
   Serial.println(data);
   //Serial.println("test");
}
