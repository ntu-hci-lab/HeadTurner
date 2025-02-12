
#ifndef SIMPLE_FUNCTION_H
#define SIMPLE_FUNCTION_H

#include <Arduino.h>

class Constant_Timer{
	public:
		boolean Timer_Task(unsigned long _time_zone ) ;
	private:
		unsigned long _last_time ;
};

#endif