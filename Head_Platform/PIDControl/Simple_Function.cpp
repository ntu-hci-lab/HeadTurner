#include "Simple_Function.h"

boolean Constant_Timer::Timer_Task(unsigned long _time_zone){
	if((millis() - _last_time) >= _time_zone ){
		_last_time = millis() ;
		return true ;
	}
	return false ;
}