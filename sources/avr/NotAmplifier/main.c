/*
 * NotAmplifier.c
 *
 * Created: 2017/04/15 21:53:36
 * Author : Jackhammer
 */ 

//==============================================================================
// 
//  
//    �e�X�g�� (ATtine2313)
// 
// http://tsukulog555.blog14.fc2.com/blog-entry-73.html
// http://www.natural-science.or.jp/article/20101215012553.php
// http://d.hatena.ne.jp/hijouguchi/?of=59
// http://www.letstryit.net/2011/06/avr-rotaryencoder.html
// http://www.geocities.jp/kuman2600/k5rot-enc.html 2��3�����ւ���
// http://elm-chan.org/docs/tec/te01.html �`���^�����O�΍�
// http://startelc.com/H8/H8_51Encder1.html ��]���ʎ�
// http://www.chibiegg.net/elec/avr-elec/generate_pwm.htm sin�e�[�u��
// http://avrwiki.osdn.jp/cgi-bin/wiki.cgi?page=Timer0#p14
// http://usicolog.nomaki.jp/engineering/avr/avrInterrupt.html#howToUse ������blog
// ��˂��炨 AVR�V���A���ʐM�̃����O�o�b�t�@
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
#include <avr/io.h>				// �s������
#include <avr/sfr_defs.h>		// _BV��
#include <avr/interrupt.h>		// ����������
#include <avr/wdt.h>			// WDT

#include <util/delay.h>			// _delay_ms��
#include <util/setbaud.h>		// F_CPU�ABAUD�錾�̃`�F�b�N

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
 * @brief ���/����0��ꊄ�荞��
 */
//ISR(TIMER0_OVF_vect)
//{
//	PORT_LED ^=  _BV(PORT_LED_BIT);
//}

//=============================================================================
/**
 * @brief ���/����0��rB���荞��
 */
ISR(TIMER0_COMPB_vect)
{
	if(--timer > 0) return;
	timer = COUNTER_LED;

	OCR0B = (OCR0B-(256-COUNTER_LED_OP)+256)&0xFF;	// ��r�l�����Z���Ă������ƂŁACOUNTER_LED_OP�̃J�E���^�̑�p�ɂ���

	PORT_LED ^= _BV(PORT_LED_BIT);
}


//=============================================================================
/**
 * ������
 * @brief	������
 * @return	none
 */
static inline void initialize()
{
	/*
		NOTAMP�Ŏg�p����PORT����

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
		
		(���g�p)
			PD4,PD6
			PB0
	*/

	//======================================
	// port configuration.
	//======================================
	DDR_LED	 |= _BV(PORT_LED_BIT);				// LED����|�[�g�̊Y��BIT���o�͐ݒ�ɁB
	PORT_LED &= ~_BV(PORT_LED_BIT);				// PORT_LED_BIT��LO�ɐݒ�

	DDR_PWM	 |= _BV(PORT_PWM_BIT);				// PWM�o�̓|�[�g�̊Y��BIT���o�͐ݒ�ɁB
	PORT_PWM &= ~_BV(PORT_PWM_BIT);				// PORT_PWM_BIT��LO�ɐݒ�

	DDR_MPW	 &= ~_BV(PORT_MPW_BIT);				// Mic Power�|�[�g����͂ɐݒ�
	PORT_MPW |= _BV(PORT_MPW_BIT);				// PORT_PWM_BIT���v���A�b�v

	DDR_RELAY_SWT	|= _BV(PORT_RELAY_SWT_BIT);		// Relay Switch�|�[�g���o�͂ɐݒ�
	PORT_RELAY_SWT	&= ~_BV(PORT_RELAY_SWT_BIT);	// PORT_RELAY_SWT_BIT��LO�ɐݒ�

#if defined USE_RTSCTS
	DDR_CTSRTS	|= _BV(CTS_OUT);				// CTS���o�͂ɐݒ�
	DDR_CTSRTS	&= ~_BV(RTS_IN);				// RTS����͂ɐݒ�
	PORT_CTSRTS |= _BV(RTS_IN);					// RTS_IN���v���A�b�v
#endif

	DDR_MPW	 &= ~_BV(PORT_MPW_BIT);				// mic_power����͂ɐݒ�
	PORT_MPW |= _BV(PORT_MPW_BIT);				// mic_power���v���A�b�v


	//======================================
	// timer configuration.
	//======================================
	// TCCR0x���W�X�^�Őݒ肷�鍀��
	// TCCR0A COM0A1 COM0A0 COM0B1 COM0B0   -     -  WGM01 WGM00
	// TCCR0B FOC0A  FOC0B    -      -    WGM02 CS02 CS01  CS00
	
	// WGM(Wave Generation Mode)
	// WGM01=0,WGM00=0 => �W������		 : 
	// WGM01=1,WGM00=0 => CTC����		 : ��r��v���N�����ہATCNT��0���Z�b�g�����
	// WGM01=0,WGM00=1 => �ʑ��PWM����: TCNT0��0�`255�ŐU������.
	// WGM01=1,WGM00=1 => ����PWM����	 : TCNT0��0��255�ŉ�]����

	// COmpare Match 
	// COM0x1=0,COM0x0=0 => PWM�o�͖���
	// COM0x1=0,COM0x0=1 => ��r��v�Ńg�O��
	// COM0x1=1,COM0x0=0 => ��r��v��LO�ABOTTM��HI�@�@����ʓI
	// COM0x1=1,COM0x0=1 => ��L���]

	TCNT0	= 0;						// �^�C�}0�J�E���^�̏�����
	TCCR0A	= 0b00000011;				// ����PWM(PWM�o�͂�OC0A(PB2))
	TCCR0B	= PRESCALE_SEL;				// �N���b�N��1024����
	
	OCR0A	= 0;						// Timer0��A��rڼ޽�(PWM��duty��)
	OCR0B	= COUNTER_LED_OP;			// Timer0��B��rڼ޽�

	TIMSK	= _BV(OCIE0B);				// [TCNT0��B��r]��L����
	//TIMSK	= _BV(OCIE0B)|_BV(TOIE0);	// [TCNT0��B��r][TCNT0��OVF]��L����
	//======================================
	// usart configuration.
	//======================================
	usart_init();

	UCSRA	= 0;						// (USART Control and Status Register A):
	UCSRB	= _BV(RXEN)|_BV(TXEN);		// (USART Control and Status Register B):��M����,���M����(=>0b00011000)
	UCSRC	= 0b00000110;				// (USART Control and Status Register C):�񓯊��A8bit���f�[�^�A�p���e�B����
	
	sei();
}

//=============================================================================
/**
 * DEBUG:message���V���A���ɏo�͂���
 * @brief	message���V���A���ɏo��
 * @param	(msg) �o�͑Ώۂ�message
 * @return	none
 */
void status_update(namp_status* pstats){
	pstats->mic = 1;//(pstats->mic+1) & MIC_MASK;
	pstats->vol = 2;//(pstats->vol+2) & VOL_MASK;
	pstats->mpw = GET_MPW();
	//pstats->pkm = 0;
}

void msg_make_and_send_response(namp_message *pmsg, char opid, namp_status *pstats){
	opid = namp_msg_make_response(pmsg, opid, pstats);				//!< ��M�f�[�^�����ɕԐM�f�[�^���쐬

	//! �ԐM���K�v�ł���΁A�ԐM����
	if(opid != MSG_OP_ID_NOP)
	{
		usart_transmit_bytes(pmsg, (unsigned char)sizeof(*pmsg));	//!< �ԐM�f�[�^�𑗐M
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
	
	initialize();									//!< ������
	mcp3002d_init();								//!< MCP3002�̗��p�|�[�g��������
	
	_delay_ms(DELAY_RELAY_SWITCH_ON);
	PORT_RELAY_SWT	|= _BV(PORT_RELAY_SWT_BIT);		//!< Relay Switch��HI�ɁB

	while(1)
	{
		usart_recieve_bytes(buf, sizeof(buf));		//!< ��M
		opid = namp_msg_get(&msg, buf);				//!< message���
		
		//! msg�������ȏꍇ�͂�蒼��
		if(opid == MSG_OP_ID_NOP)
		{
			continue;
		}

		status_update(&stats);										//!< �X�e�[�^�X�X�V
		
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
