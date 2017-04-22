/*
 * config.h
 *
 * Created: 2017/03/26 17:33:12
 * Author : Jackhammer
 */ 


#ifndef CONFIG_H_
#define CONFIG_H_

#ifndef F_CPU
#	define F_CPU			8000000UL		// CPU-Frequency=8MHz
#	define BAUD				9600			// シリアル通信のボーレート()
//#	define BAUD				19200			// シリアル通信のボーレート()
#endif


//=====================================
//! SPI_PORTS
//=====================================
#define DDR_SPI				DDRB
#define PORT_SPI			PORTB
#define PIN_SPI				PINB
#define SPI_USCK			PB7				// Universal Serial ClocK
#define SPI_MISO			PB6				// Master In Slave Out
#define SPI_MOSI			PB5				// Master Out Slave In
#define SPI_SS0				PB4				// Slave Select(CS:Chip Select)

//=====================================
//! USART PORTS
//=====================================
#define USE_RTSCTS
#define	USE_SPI_MIN_WAIT
#define	DDR_CTSRTS			DDRA
#define	PORT_CTSRTS			PORTA
#define	PIN_CTSRTS			PINA
#define RTS_IN				PA1
#define CTS_OUT				PA0

//=====================================
//! LED PORT
//=====================================
#define DDR_LED					DDRB
#define PORT_LED				PORTB
#define PORT_LED_BIT			PB0

//=====================================
//! PWM(VU-Meter) PORT
//=====================================

#define DDR_PWM					DDRB
#define PORT_PWM				PORTB
#define PORT_PWM_BIT			PB2

//=====================================
//! Mic Power PORT
//=====================================
#define	DDR_MPW					DDRB
#define	PORT_MPW				PORTB
#define	PORT_MPW_BIT			PB3
#define	PIN_MPW					PINB

//=====================================
//! Relay Switch PORT
//=====================================
#define	DDR_RELAY_SWT			DDRB
#define	PORT_RELAY_SWT			PORTB
#define	PORT_RELAY_SWT_BIT		PB1


#define PWM0A_ON(x)				{TCCR0A |= _BV(COM0A1);OCR0A = (x);}
#define PWM0A_OFF()				{TCCR0A &= ~_BV(COM0A1);OCR0A = 0;}
#define GET_MPW()				((~(PIN_MPW>>PORT_MPW_BIT))&1)

//! 定数関係
#define MIC_BIT_WIDTH			10
#define MIC_MAX					((1<<MIC_BIT_WIDTH)-1)
#define MIC_MASK				MIC_MAX

#define VOL_BIT_WIDTH			10
#define VOL_MAX					((1<<VOL_BIT_WIDTH)-1)
#define VOL_MASK				VOL_MAX

#define PKM_BIT_WIDTH			10
#define PKM_MAX					((1<<PKM_BIT_WIDTH)-1)
#define PKM_MASK				PKM_MAX

#define PWM_DUTY_BIT_WIDTH		8
#define PWM_DUTY_MAX			((1<<PWM_DUTY_BIT_WIDTH)-1)
#define PWM_DUTY_MASK			PWM_DUTY_MAX

#define DELAY_RELAY_SWITCH_ON	2500


#endif /* CONFIG_H_ */
