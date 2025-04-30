#ifndef CONSTANT_DEFINITION_H
#define CONSTANT_DEFINITION_H

#include <Arduino.h>

/*======== Hardware Definition ========*/
/*==== Pin Definition ====*/
/*== USB Baud Rate ==*/
#define BAUDRATE_USB					115200

/*== LED Control ==*/
#define PIN_LED_ONBOARD					13

/*== Motor ==*/
/*= Encoder =*/
#define PIN_ENCODER_A					2	// PD2
#define PIN_ENCODER_B					3	// PD3

#define MASK_ENCODER					0x0C
#define MASK_ENCODER_PLUS				0x04

#define DATA_ENCODER					PIND

/*= Driver L298 =*/
#define PIN_INA							6
#define PIN_INB							5
#define PIN_ENABLE						4

/*= Control Debug=*/
#define PIN_DEBUG						2

/*== Hardware Setup Time ==*/
#define TIME_SETUP_DELAY_MS				500

/*======== Software Definition ========*/
/*==== USB Constant ====*/
#define TIME_USB_MS						5000

#define BUFFER_SIZE_USB					20

#define CASE_SPEED						's'
#define CASE_POSITION_STEP				'p'
#define CASE_WAVE_SIN					'w'

#define SUB_CASE_WAVE_AMP				'a'
#define SUB_CASE_WAVE_PERIOD			'f'

/*==== USB Plot Constant ====*/
#define TIME_USB_MS_PLOT				20

/*==== LED Constant ====*/
#define TIME_LED_FLASH_MS				25

#define LED_ONBOARD_ON()				digitalWrite(PIN_LED_ONBOARD, HIGH )
#define LED_ONBOARD_OFF()				digitalWrite(PIN_LED_ONBOARD, LOW )

/*==== Motor Constant ====*/
#define TIME_ENCODER_MS					1
#define TIME_CONTROL_MS					1

#define MODE_SPEED						0
#define MODE_POSITION_STEP				1
#define MODE_POSITION_WAVE				2

#define MOTOR_CW(_PWM )					analogWrite(PIN_INA, 0 ), analogWrite(PIN_INB, _PWM ), digitalWrite(PIN_ENABLE, HIGH )
#define MOTOR_CCW(_PWM )				analogWrite(PIN_INA, _PWM ), analogWrite(PIN_INB, 0 ), digitalWrite(PIN_ENABLE, HIGH )
#define MOTOR_STOP()					analogWrite(PIN_INA, 0 ), analogWrite(PIN_INB, 0 ), digitalWrite(PIN_ENABLE, LOW )
#define MAX_DUTY						250

#define DEAD_ZONE						20 // 50 for slower motor

#define MOTOR_ENCODER_PPR				680 // 16(gear ratio)*500(encoder ppr)*4

#define POSITION_CONTROL_P				2000 // 610, 52, 70, 185 for slower motor 2000,160,40,185 for fasterr motor
#define POSITION_CONTROL_I  			160
#define POSITION_CONTROL_D  			40
#define POSITION_CONTROL_I_Max			185 

#define AMPLITUDE_DEFAULT				200
#define PERIOD_DEFAULT_MS				5000.0

#endif
