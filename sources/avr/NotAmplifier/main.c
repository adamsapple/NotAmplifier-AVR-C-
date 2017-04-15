/*
 * NotAmplifier.c
 *
 * Created: 2017/04/15 21:53:36
 * Author : Jackhammer
 */ 

//==============================================================================
// 
//  
//    テスト版 (ATtine2313)
// 
// http://tsukulog555.blog14.fc2.com/blog-entry-73.html
// http://www.natural-science.or.jp/article/20101215012553.php
// http://d.hatena.ne.jp/hijouguchi/?of=59
// http://www.letstryit.net/2011/06/avr-rotaryencoder.html
// http://www.geocities.jp/kuman2600/k5rot-enc.html 2と3を入れ替える
// http://elm-chan.org/docs/tec/te01.html チャタリング対策
// http://startelc.com/H8/H8_51Encder1.html 回転判別式
// http://www.chibiegg.net/elec/avr-elec/generate_pwm.htm sinテーブル
// http://avrwiki.osdn.jp/cgi-bin/wiki.cgi?page=Timer0#p14
// http://usicolog.nomaki.jp/engineering/avr/avrInterrupt.html#howToUse うしこblog
// やねうらお AVRシリアル通信のリングバッファ
// http://d.hatena.ne.jp/yaneurao/20080710
// http://d.hatena.ne.jp/yaneurao/20080713
//==============================================================================

//======================================
// system configuration.
//======================================
#include "config.h"

#include <stdint.h>				// types
#include <stdlib.h>
#include <string.h>

//#include <avr/iotn2313.h>
#include <avr/io.h>				// ピン名等
#include <avr/sfr_defs.h>		// _BV等
#include <avr/interrupt.h>		// 割込処理等
#include <avr/wdt.h>			// WDT

#include <util/delay.h>			// _delay_ms等
#include <util/setbaud.h>		// F_CPU、BAUD宣言のチェック

#include "util.h"
#include "usart.h"
#include "mcp3002d.h"
#include "namp.h"


#define	PRESCALE_1				0b001;
#define	PRESCALE_8				0b010;
#define	PRESCALE_64				0b011;
#define	PRESCALE_256			0b100;
#define	PRESCALE_1024			0b101;

#define PRESCALE_SEL			PRESCALE_256
#define COUNTER_1S				(31<<2)
#define COUNTER_500MS			(COUNTER_1S >>1)
#define COUNTER_LED				COUNTER_500MS
#define COUNTER_LED_OP			(249)


#ifdef DEBUG
#	undef  COUNTER_LED
#	define COUNTER_LED			1
#endif



volatile int timer				= COUNTER_LED;
typedef void (*PFUNC_usart_transmit)(const void*, unsigned char);


//=============================================================================
/**
 * @brief ﾀｲﾏ/ｶｳﾝﾀ0溢れ割り込み
 */
//ISR(TIMER0_OVF_vect)
//{
//	PORT_LED ^=  _BV(PORT_LED_BIT);
//}

//=============================================================================
/**
 * @brief ﾀｲﾏ/ｶｳﾝﾀ0比較B割り込み
 */
ISR(TIMER0_COMPB_vect)
{
	if(--timer > 0) return;
	timer = COUNTER_LED;

	OCR0B = (OCR0B-(256-COUNTER_LED_OP)+256)&0xFF;	// 比較値を減算していくことで、COUNTER_LED_OPのカウンタの代用にする

	PORT_LED ^= _BV(PORT_LED_BIT);
}


//=============================================================================
/**
 * 初期化
 * @brief	初期化
 * @return	none
 */
static inline void initialize()
{
	/*
		NOTAMPで使用するPORT検討

		SPI_USCK	(OUT)	PB7
		SPI_MISO	(IN)	PB6
		SPI_MOSI	(OUT)	PB5
		SPI_SS0		(OUT)	PB4
		MIC_PWR_IN	(IN)	PB3(PCINT3)
		PMT_PWM		(--)	PB2(OC0A)
		RELAY_SWT	(OUT)	PB1
		RESET		(--)	PA2
		USART_RX	(--)	PD0
		USART_TX	(--)	PD1
		LED_LISTEN	(OUT)	PD2
		LED_CONNECT	(OUT)	PD3
		LED_PING	(OUT)	PD5
		RTS			(IN)	PA1
		CTS			(OUT)	PA0
		
		(未使用)
			PD4,PD6
			PB0
	*/

	//======================================
	// port configuration.
	//======================================
	DDR_LED	 |= _BV(PORT_LED_BIT);				// LED制御ポートの該当BITを出力設定に。
	PORT_LED &= ~_BV(PORT_LED_BIT);				// PORT_LED_BITをLOに設定

	DDR_PWM	 |= _BV(PORT_PWM_BIT);				// PWM出力ポートの該当BITを出力設定に。
	PORT_PWM &= ~_BV(PORT_PWM_BIT);				// PORT_PWM_BITをLOに設定

	DDR_MPW	 &= ~_BV(PORT_MPW_BIT);				// Mic Powerポートを入力に設定
	PORT_MPW |= _BV(PORT_MPW_BIT);				// PORT_PWM_BITをプルアップ

	DDR_RELAY_SWT	|= _BV(PORT_RELAY_SWT_BIT);		// Relay Switchポートを出力に設定
	PORT_RELAY_SWT	&= ~_BV(PORT_RELAY_SWT_BIT);	// PORT_RELAY_SWT_BITをLOに設定

#if defined USE_RTSCTS
	DDR_CTSRTS	|= _BV(CTS_OUT);				// CTSを出力に設定
	DDR_CTSRTS	&= ~_BV(RTS_IN);				// RTSを入力に設定
	PORT_CTSRTS |= _BV(RTS_IN);					// RTS_INをプルアップ
#endif

	DDR_MPW	 &= ~_BV(PORT_MPW_BIT);				// mic_powerを入力に設定
	PORT_MPW |= _BV(PORT_MPW_BIT);				// mic_powerをプルアップ


	//======================================
	// timer configuration.
	//======================================
	// TCCR0xレジスタで設定する項目
	// TCCR0A COM0A1 COM0A0 COM0B1 COM0B0   -     -  WGM01 WGM00
	// TCCR0B FOC0A  FOC0B    -      -    WGM02 CS02 CS01  CS00
	
	// WGM(Wave Generation Mode)
	// WGM01=0,WGM00=0 => 標準動作		 : 
	// WGM01=1,WGM00=0 => CTC動作		 : 比較一致が起きた際、TCNTが0リセットされる
	// WGM01=0,WGM00=1 => 位相基準PWM動作: TCNT0が0～255で振幅する.
	// WGM01=1,WGM00=1 => 高速PWM動作	 : TCNT0が0⇒255で回転する

	// COmpare Match 
	// COM0x1=0,COM0x0=0 => PWM出力無し
	// COM0x1=0,COM0x0=1 => 比較一致でトグル
	// COM0x1=1,COM0x0=0 => 比較一致でLO、BOTTMでHI　　※一般的
	// COM0x1=1,COM0x0=1 => 上記反転

	TCNT0	= 0;						// タイマ0カウンタの初期化
	TCCR0A	= 0b00000011;				// 高速PWM(PWM出力はOC0A(PB2))
	TCCR0B	= PRESCALE_SEL;				// クロックを1024分周
	
	OCR0A	= 0;						// Timer0のA比較ﾚｼﾞｽﾀ(PWMのduty比)
	OCR0B	= COUNTER_LED_OP;			// Timer0のB比較ﾚｼﾞｽﾀ

	TIMSK	= _BV(OCIE0B);				// [TCNT0のB比較]を有効化
	//TIMSK	= _BV(OCIE0B)|_BV(TOIE0);	// [TCNT0のB比較][TCNT0のOVF]を有効化
	//======================================
	// usart configuration.
	//======================================
	usart_init();

	UCSRA	= 0;						// (USART Control and Status Register A):
	UCSRB	= _BV(RXEN)|_BV(TXEN);		// (USART Control and Status Register B):受信許可,送信許可(=>0b00011000)
	UCSRC	= 0b00000110;				// (USART Control and Status Register C):非同期、8bit長データ、パリティ無し
	
	sei();
}

//=============================================================================
/**
 * DEBUG:messageをシリアルに出力する
 * @brief	messageをシリアルに出力
 * @param	(msg) 出力対象のmessage
 * @return	none
 */
void status_update(namp_status* pstats){
	pstats->mic = 1;//(pstats->mic+1) & MIC_MASK;
	pstats->vol = 2;//(pstats->vol+2) & VOL_MASK;
	pstats->mpw = GET_MPW();
	//pstats->pkm = 0;
}

void msg_make_and_send_response(namp_message *pmsg, char opid, namp_status *pstats){
	opid = namp_msg_make_response(pmsg, opid, pstats);				//!< 受信データを元に返信データを作成

	//! 返信が必要であれば、返信する
	if(opid != MSG_OP_ID_NOP)
	{
		usart_transmit_bytes(pmsg, (unsigned char)sizeof(*pmsg));	//!< 返信データを送信
	}
}

//=============================================================================
//
//! main.
//
//=============================================================================
int main()
{
	char	buf[sizeof(namp_message)];
	char    opid;
	namp_message	msg;
	namp_status		stats	 = {0};
	namp_status		stats_plv= {-1,-1,-1,-1};
	
	initialize();									//!< 初期化
	mcp3002d_init();								//!< MCP3002の利用ポートを初期化
	
	_delay_ms(DELAY_RELAY_SWITCH_ON);
	PORT_RELAY_SWT	|= _BV(PORT_RELAY_SWT_BIT);		//!< Relay SwitchをHIに。

	while(1)
	{
		usart_recieve_bytes(buf, sizeof(buf));		//!< 受信
		opid = namp_msg_get(&msg, buf);				//!< message解析
		
		//! msgが無効な場合はやり直し
		if(opid == MSG_OP_ID_NOP)
		{
			continue;
		}

		status_update(&stats);										//!< ステータス更新
		
		msg_make_and_send_response(&msg, opid, &stats);
		
		if(stats.mic != stats_plv.mic){
			msg_make_and_send_response(&msg, MSG_OP_ID_MIC, &stats);
		}
		if(stats.vol != stats_plv.vol){
			msg_make_and_send_response(&msg, MSG_OP_ID_VOL, &stats);
		}
		if(stats.mpw != stats_plv.mpw){
			msg_make_and_send_response(&msg, MSG_OP_ID_MPW, &stats);
		}

		memcpy(&stats_plv, &stats, sizeof(stats));

		//msg_put_debug(&msg);
	}

	return 0;
}
