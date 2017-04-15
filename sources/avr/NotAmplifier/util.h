/*
 * myutil.h
 *
 * Created: 2017/03/13 9:54:50
 *  Author: 287
 */ 

#include <compat/deprecated.h>	// for cbi(), sbi()

#ifndef MYUTIL_H_
#define MYUTIL_H_

#ifdef DEBUG
#	define _delay_ms(x)			asm("nop")
#	define _delay_us(x)			asm("nop")
#endif

//#define NOP()   asm("nop"::)
//#define NOP2()  {NOP();NOP();} // rjmp PC+1<-����ǂ����́H

#define MIN(a,b)				(((a)<(b))?(a):(b))		//!< �ŏ��l��Ԃ�
#define MAX(a,b)				(((a)>(b))?(a):(b))		//!< �ő�l��Ԃ�
#define offsetof(s,m)			(size_t)&(((s *)0)->m)

#endif /* MYUTIL_H_ */