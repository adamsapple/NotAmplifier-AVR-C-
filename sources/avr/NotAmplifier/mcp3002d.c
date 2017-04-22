/*
 * mcp3002d.c
 *
 * Created: 2017/03/14 9:27:24
 *  Author: 287
 */ 

#include "mcp3002d.h"

#include <stdint.h>			// types
#include <avr/io.h>			// ports
#include <util/delay.h>		// _delay_xx()


// SPI用コマンド群
#define START			0b01000000							//
#define MODE_SNGL		0b00100000							//
#define MODE_DIFF		0b00000000							//
#define CH0				0b00000000							//
#define CH1				0b00010000							//
#define MSBF_MSB		0b00001000							//
#define MSBF_LSB		0b00000000							//
#define CMD_BASE		((START)|(MODE_SNGL)|(MSBF_MSB))	// 
#define MAKE_CMD(ch)	(CMD_BASE|((ch)<<4))				// 
#define CMD_CH0			MAKE_CMD(CH0)
#define CMD_CH1			MAKE_CMD(CH1)

// MCPのタイミングwait(nano seconds)関係
#define T_HI_MIN		(140)
#define	T_LO_MIN		(140)
#define	T_SUCS_MIN		(100)
#define T_DIS_MAX		(100)
#define T_CSH_MIN		(3100)

// MCP3002のspecに関するもの
#define BIT_WIDTH		10									// 10bit長幅のデータ
#define MAX_VALUE		((1<<BIT_WIDTH)-1)					// 最大値
#define NUM_CHANNELS	2									// チャンネル数

// 
#define SET_BIT(p)				PORT_SPI |= _BV(p)				// Bit-On用汎用マクロ
#define CLR_BIT(p)				PORT_SPI &= ~_BV(p)				// Bit-Off用汎用マクロ
#define REV_BIT(p)				PORT_SPI ^= _BV(p)
#define	IS_BIT_SET(pin,bit)		bit_is_set(pin, bit)
#define GET_PINBIT(pin,offs)	((pin>>offs)&1)

// wait処理を有効にするかどうか
#if defined USE_SPI_MIN_WAIT
#	define	_SPI_MIN_WAIT(x)	_delay_us(x)
#else
#	define	_SPI_MIN_WAIT(x)
#endif


// CS制御
#define CSBIT_HI()		SET_BIT(SPI_SS0)
#define CSBIT_LO()		CLR_BIT(SPI_SS0)

// SCK制御
#define CLKBIT_HI()		SET_BIT(SPI_USCK)
#define CLKBIT_LO()		CLR_BIT(SPI_USCK)
#define CLKBIT_REV()	REV_BIT(SPI_USCK)

// MOSI制御
#define MOSI_HI()		SET_BIT(SPI_MOSI)
#define MOSI_LO()		CLR_BIT(SPI_MOSI)

// MISO読取
#define MISO_GET()		GET_PINBIT(PIN_SPI,SPI_MISO)

typedef uint8_t (*PFUNC_spi_transfer8bit)(uint8_t);


static inline uint8_t spi_transfer8bit_mode00(uint8_t send)
{
	uint8_t val = 0;
	int8_t  i	= 8;
	
	while(i--)
	{
		//
		// MCU latches data from A/D Converter on rising edges of SCLK
		// MCU Received Data. (Aligned with rising edge of clock)
		//
		CLKBIT_HI();				// Shift

		if(send & 0x80)
		{
			MOSI_HI();
		}else{
			MOSI_LO();
		}

		send <<= 1;
		_SPI_MIN_WAIT(T_HI_MIN);	// Shift最低時間待ち

		//
		// Data is clocked out of A/D Converter on falling edges.
		// MCU Transmitted Data. (Aligned with falling edge of clock)
		//
		CLKBIT_LO();				// Latch(Mode 0,0: SCLK idles low)
		_SPI_MIN_WAIT(T_LO_MIN);	// Latch最低時間待ち
		
		val	<<= 1;
		val |= MISO_GET();
	}

	return val;
}

static inline uint8_t spi_transfer8bit_mode11(uint8_t send)
{
	uint8_t val = 0;
	int8_t  i	= 8;
	
	while(i--)
	{
		//
		// Data is clocked out of A/D Converter on falling edges.
		// MCU Transmitted Data. (Aligned with falling edge of clock)
		//
		CLKBIT_LO();				// Shift
		
		if(send & 0x80)
		{
			MOSI_HI();
		}else{
			MOSI_LO();
		}
		send <<= 1;
		_SPI_MIN_WAIT(T_LO_MIN);	// Shift最低時間待ち

		//
		// MCU latches data from A/D Converter on rising edges of SCLK
		// MCU Received Data. (Aligned with rising edge of clock)
		//
		CLKBIT_HI();				// Latch(Mode 1,1: SCLK idles high)
		_SPI_MIN_WAIT(T_HI_MIN);	// Latch最低時間待ち

		val	<<= 1;
		val |= MISO_GET();
	}

	return val;
}

void mcp3002d_init(void)
{
	// Set MOSI, CS and SCK output, all others input
	DDR_SPI  |= _BV(SPI_MOSI)|_BV(SPI_USCK);	// as output.
	DDR_SPI  |= _BV(SPI_SS0);					// as output.
	DDR_SPI  &= ~_BV(SPI_MISO);					// as input.
	PORT_SPI |= _BV(SPI_MISO);					// as pull-up.

	CSBIT_HI();
}

uint16_t mcp3002d_get( uint8_t ch )
{
	uint16_t	val = 0;
	PFUNC_spi_transfer8bit spi_transfer = spi_transfer8bit_mode00;

	if( ch >= NUM_CHANNELS )
	{
		return 0;
	}

	CSBIT_LO();
	CLKBIT_LO();
	_SPI_MIN_WAIT(T_SUCS_MIN);

	val =  spi_transfer( MAKE_CMD(ch) )<<8;
	val |= spi_transfer(0);

	//CLKBIT_LO();
	//_SPI_MIN_WAIT(T_LO_MIN);
	CSBIT_HI();
	_SPI_MIN_WAIT(T_CSH_MIN);

	return val&((1<<BIT_WIDTH)-1);		// Masking Bit-width. (10 bits)
}
