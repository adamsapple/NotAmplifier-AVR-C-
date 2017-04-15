/*
 * mcp3002d.h
 *
 * Created: 2017/03/14 9:27:36
 *  Author: 287
 * 
 * MCP3002 SPI-MODE [0,0] and [1,1]
 *
 * see:
 https://passing.breeze.cc/mt/archives/2011/01/arduino-attiny2313-spi.html
 http://playground.arduino.cc/Code/USI-SPI
 http://www.atmel.com/Images/Atmel-2582-Using-the-USI-Module-for-SPI-Communication-on-tinyAVR-and-megaAVR-Devices_ApplicationNote_AVR319.pdf
 SPIの説明 http://wazalabo.com/pic-spi.html
 割込み http://tsukulog555.blog14.fc2.com/blog-entry-73.html
 */ 

#ifndef MCP3002D_H_
#define MCP3002D_H_

#include "config.h"

#include <stdint.h>		// types
#include <avr/io.h>		// ports


#if !defined SPI_USCK
#	error "MCP3002D define error. Must define SPI_USCK."
#endif
#if !defined SPI_MISO
#	error "MCP3002D define error. Must define SPI_MISO."
#endif
#if !defined SPI_MOSI
#	error "MCP3002D define error. Must define SPI_MOSI."
#endif
#if !defined SPI_SS0
#	error "MCP3002D define error. Must define SPI_SS0."
#endif
#if !defined DDR_SPI
#	error "MCP3002D define error. Must define DDR_SPI."
#endif
#if !defined PORT_SPI
#	error "MCP3002D define error. Must define PORT_SPI."
#endif
#if !defined PIN_SPI
#	error "MCP3002D define error. Must define PIN_SPI."
#endif

//#define SPI_USCK		PB7				// Universal Serial ClocK
//#define SPI_MISO		PB6				// Master In Slave Out
//#define SPI_MOSI		PB5				// Master Out Slave In
//#define SPI_SS0		PB4				// Slave Select(CS:Chip Select)
//#define DDR_SPI		DDRB
//#define PORT_SPI		PORTB
//#define PIN_SPI		PINB

//#define	USE_SPI_MIN_WAIT

//=====================================
// spiに使用するポートの初期化
void mcp3002d_init(void);

//=====================================
// 値取得
// ch: Channel (0 to 1)
uint16_t mcp3002d_get( uint8_t ch );


#endif /* MCP3002D_H_ */
