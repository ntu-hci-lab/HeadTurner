#include "Simple_Function.h"
#include "Constant_Definition.h"

// #include <Encoder.h>

/*== Timer Variables ==*/
Constant_Timer Timer_Encoder ;
Constant_Timer Timer_USB ;
Constant_Timer Timer_Control ;

/*== Motor Variables ==*/
volatile signed long slEncoder_Counter = 0 ;
volatile signed long slSet_Point = 0 ;

static signed long slSet_Wave_Center = 0 ;
static float fWave_Amp = AMPLITUDE_DEFAULT ;
static float fWave_Period = PERIOD_DEFAULT_MS ;
static float fWave_Interval = 6.28 * TIME_CONTROL_MS / fWave_Period ;
static float fWaveCounter = 0 ;

static int iSpeed = 0 ;

static char cControlMode = 0 ;

// Encoder myEnc(PIN_ENCODER_A, PIN_ENCODER_B ) ;
/*Speed and Acceleration Measurement*/
int prevPos = 0;
float prevSpeed = 0;
float currSpeed;
float maxSpeed = 0;
float currAcc;
float maxAcc = 0;

void setup() {
  Hardware_Setup() ;
  Software_Setup() ;
}

void loop() {
  USB_CMD_Task() ;
  USB_Plot_Task() ;
  Control_Task() ;
}

/*========= Function =========*/
/*==== Setup Function ====*/
void Hardware_Setup(void ) {
  /*== LED Setup Function ==*/
  // config IO direction of LED
  pinMode(PIN_LED_ONBOARD, OUTPUT ) ;

  /*== USB Setup Function ==*/
  // config USB UART baud rate
  Serial.begin(BAUDRATE_USB ) ;

  /*== Motor Setup Function ==*/
  pinMode(PIN_INA, OUTPUT ) ;
  pinMode(PIN_INB, OUTPUT ) ;
  pinMode(PIN_ENABLE, OUTPUT ) ;

  pinMode(PIN_ENCODER_A, INPUT ) ;
  pinMode(PIN_ENCODER_B, INPUT ) ;

  attachInterrupt(digitalPinToInterrupt(PIN_ENCODER_A ), Encoder_Task_Simple, CHANGE ) ;
  attachInterrupt(digitalPinToInterrupt(PIN_ENCODER_B ), Encoder_Task_Simple, CHANGE ) ;

  // attachInterrupt(digitalPinToInterrupt(PIN_ENCODER_A ), Encoder_Task, CHANGE ) ;
  // attachInterrupt(digitalPinToInterrupt(PIN_ENCODER_B ), Encoder_Task, CHANGE ) ;

  Encoder_Task_Simple() ;
}

void Software_Setup(void ) {
  /*== Timer Setup Function ==*/
  Timer_USB.Timer_Task(0 ) ;
  Timer_Encoder.Timer_Task(0 ) ;
  Timer_Control.Timer_Task(0 ) ;

  /*== Motor Control Setup Function ==*/
  slEncoder_Counter = 0 ;
  slSet_Point = 0 ;
  iSpeed = 0 ;
  cControlMode = MODE_SPEED ;
  MOTOR_STOP() ;

  /*== Setup Finished!! ==*/
  Serial.print("SetPoint" ) ;
  Serial.print(" ") ;
  Serial.println("Encoder" ) ;
}

/*==== Basic Function ====*/

/*== USB Function ==*/
void USB_CMD_Task(void ) {
  static signed long _slWave = 0 ;
  static signed long _slData0 = 0 ;
  static int _iData0 = 0 ;
  static char _cCommand, _cCommand2, _cTemp ;

  static String _sStringIn = "" ;
  static char _cDataIn[BUFFER_SIZE_USB] ;
  static bool _bData_Complete ;

  static int _iTemp =  0 ;

  // Read buffer
  while ((_cTemp = Serial.read() ) != -1 )
  {
    if (_cTemp == '\n') {
      _bData_Complete = true ;
      break ;
    }
    else _sStringIn += _cTemp ;
  }

  if (_bData_Complete ) {
    _sStringIn.toCharArray(_cDataIn, BUFFER_SIZE_USB ) ;
    // check case
    switch (_cDataIn[0] )
    {
      case CASE_SPEED:
        cControlMode = MODE_SPEED ;
        _iTemp = sscanf(_cDataIn, "s%d", &_iData0 ) ;
        if (_iTemp > 0 ) {
          if (abs(_iData0 ) > MAX_DUTY ) ;
          else iSpeed = _iData0 ;
        }
        break ;
      case CASE_POSITION_STEP:
        _iTemp = sscanf(_cDataIn, "p%ld", &_slData0 ) ;
        if (_iTemp > 0 ) {
          if (abs(_slData0 ) > MOTOR_ENCODER_PPR ) ;
          else {
            cControlMode = MODE_POSITION_STEP ;
            slSet_Point = _slData0 ;
          }
        }
        break ;
      case CASE_WAVE_SIN:
        // check sub-case
        switch (_cDataIn[1] )
        {
          case SUB_CASE_WAVE_AMP:
            _iTemp = sscanf(_cDataIn, "wa%ld", &_slData0 ) ;
            if (_iTemp > 0 ) {
              fWave_Amp = abs(_slData0 ) ;
            }
            break ;
          case SUB_CASE_WAVE_PERIOD:
            _iTemp = sscanf(_cDataIn, "wf%ld", &_slData0 ) ;
            if (_iTemp > 0 ) {
              _slData0 = abs(_slData0 ) ;
              fWaveCounter = fWaveCounter * (float)_slData0 / fWave_Period ;
              fWave_Period = (float)_slData0 ;
              fWave_Interval = 6.28 * TIME_CONTROL_MS / fWave_Period ;
            }
            break ;
          default:
            break ;
        }
        if (cControlMode != MODE_POSITION_WAVE ) {
          slSet_Wave_Center = slSet_Point ;
        }
        cControlMode = MODE_POSITION_WAVE ;
        break ;
      default:
        break ;
    }
    _sStringIn = "" ;
    _bData_Complete = false ;
  }
}

void USB_Plot_Task(void ) {
  if (Serial.availableForWrite() >= BUFFER_SIZE_USB) {
    if (Timer_USB.Timer_Task(TIME_USB_MS_PLOT ) ) {
      Serial.print(slSet_Point ) ;
      Serial.print(" ") ;
      Serial.println(slEncoder_Counter ) ;
//      Measurement_Task(slEncoder_Counter);
    }
  }
}

void Measurement_Task(int currPos) {
  currSpeed = (float)(currPos - prevPos) / MOTOR_ENCODER_PPR * 0.2 / (TIME_USB_MS_PLOT) * 1000;
  currAcc = (currSpeed - prevSpeed) / (TIME_USB_MS_PLOT) * 1000;
  if (maxSpeed < abs(currSpeed)) {
    maxSpeed = abs(currSpeed);
  }
  if (maxAcc < abs(currAcc)) {
    maxAcc = abs(currAcc);
  }
  prevSpeed = currSpeed;
  prevPos = currPos;
  Serial.println("MaxSpeed:" + String(maxSpeed));
  Serial.println("MaxAcc:" + String(maxAcc));
}

/*== Motor Function ==*/
void Encoder_Task(void ) {
  /*== Encoder Conunter ==*/
  static uint8_t _pin_data = 0x00 ;
  static uint8_t _pin_data_last = 0x00 ;
  static uint8_t _pin_temp_1 = 0x00 ;
  static uint8_t _pin_temp_2 = 0x00 ;

  _pin_data = DATA_ENCODER ;
  _pin_temp_1 = _pin_data ^ _pin_data_last ;
  _pin_temp_2 = (_pin_data >> 1 )^_pin_data_last ;

  if (_pin_temp_1 & MASK_ENCODER ) {
    if (_pin_temp_2 & MASK_ENCODER_PLUS ) slEncoder_Counter++ ;
    else slEncoder_Counter-- ;
  }

  _pin_data_last = _pin_data ;
}

void Encoder_Task_Simple(void ) {
  /*== Encoder Conunter ==*/
  static uint8_t _pin_data_A = 0x00 ;
  static uint8_t _pin_data_A_last = 0x00 ;

  _pin_data_A = _pin_data_A_last & 0b00000011 ;
  if (digitalRead(PIN_ENCODER_A ) ) _pin_data_A |= 0b00000100 ;
  if (digitalRead(PIN_ENCODER_B ) ) _pin_data_A |= 0b00001000 ;
  switch (_pin_data_A ) {
    case 0: case 5: case 10: case 15:
      break;
    case 1: case 7: case 8: case 14:
      slEncoder_Counter++; break;
    case 2: case 4: case 11: case 13:
      slEncoder_Counter--; break;
    case 3: case 12:
      slEncoder_Counter += 2; break;
    default:
      slEncoder_Counter -= 2; break;
  }
  _pin_data_A_last = (_pin_data_A >> 2 ) ;
}

void Control_Task(void ) {
  static int _iAbs_duty ;
  static signed long _slPos_err ;
  static signed long _slDelta_err ;
  static signed long _slIntegral_err ;
  static signed long _slPrev_err ;
  static signed long _slOutput_temp ;
  static bool _bSign_output = true ;
  static float _fTemp1, _fTemp2 = 0.0 ;

  // slEncoder_Counter = myEnc.read() ;

  if (cControlMode == MODE_SPEED ) {
    // Reset Position Control variables
    _slPrev_err = 0 ;
    _slIntegral_err = 0 ;
    slSet_Point = slEncoder_Counter ;
    fWaveCounter = 0 ;
    // Motor Output
    if (iSpeed > 0 ) MOTOR_CW(abs(iSpeed ) ) ;
    else MOTOR_CCW(abs(iSpeed ) ) ;
  }
  else if (cControlMode == MODE_POSITION_STEP | cControlMode == MODE_POSITION_WAVE ) {
    if (Timer_Control.Timer_Task(TIME_CONTROL_MS ) ) {
      if (cControlMode == MODE_POSITION_WAVE ) {
        // sin wave generator
        _fTemp1 = fWave_Amp * sin(fWaveCounter * fWave_Interval ) ;
        slSet_Point = slSet_Wave_Center + (signed long )_fTemp1 ;
        fWaveCounter += 1 ;
      }
      else fWaveCounter = 0 ;

      // PID controller
      _slPos_err = slSet_Point - slEncoder_Counter ;
      _slDelta_err = _slPos_err - _slPrev_err ;
      _slIntegral_err += _slPos_err ;
      // Clamp Integral to avoid windup problem
      if (_slIntegral_err > POSITION_CONTROL_I_Max ) _slIntegral_err = POSITION_CONTROL_I_Max ;
      else if (_slIntegral_err < -POSITION_CONTROL_I_Max ) _slIntegral_err = -POSITION_CONTROL_I_Max ;

      _slOutput_temp = ((_slPos_err * POSITION_CONTROL_P +
                         _slIntegral_err * POSITION_CONTROL_I +
                         _slDelta_err * POSITION_CONTROL_D ) >> 8 ) ;
      _slPrev_err = _slPos_err ;

      if (_slOutput_temp >= 0 ) _bSign_output = true ;
      else _bSign_output = false ;

      _slOutput_temp = abs(_slOutput_temp ) ;

      if (_slOutput_temp > MAX_DUTY ) {
        LED_ONBOARD_ON() ;
        _slOutput_temp = MAX_DUTY ;
      }
      else LED_ONBOARD_OFF() ;

      if (abs(_slPos_err) > DEAD_ZONE) {
        if (_bSign_output ) {
          MOTOR_CW(_slOutput_temp ) ;
          iSpeed = _slOutput_temp ;
        }
        else {
          MOTOR_CCW(_slOutput_temp ) ;
          iSpeed = -_slOutput_temp ;
        }
      }
      else {
        MOTOR_STOP();
        iSpeed = 0 ;
      }
    }
  }
}
