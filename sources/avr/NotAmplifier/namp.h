/*
 * namp.h
 *
 * Created: 2017/04/15 22:44:37
 *  Author: Jackhammer
 */ 


#ifndef NAMP_H_
#define NAMP_H_

#include <stdint.h>			// types


//! �V���A�����痬��Ă��郁�b�Z�[�W�Q
#define MSG_OP_NOP				"nop"					//!< no operation
#define MSG_OP_OK				"ok_"					//!< ok
#define MSG_OP_ERR				"err"					//!< err
#define MSG_OP_WAY				"way"					//!< who are you
#define MSG_OP_IAM				"iam"					//!< i am
#define MSG_OP_VER				"ver"					//!< version
#define MSG_OP_MIC				"mic"					//!< mic
#define MSG_OP_MPW				"mpw"					//!< mic power
#define MSG_OP_VOL				"vol"					//!< volume
#define MSG_OP_PKM				"pkm"					//!< peak meter
#define MSG_OP_PIN				"pin"					//!< ping
#define APP_IDENTITY			"fkad"

//! message�̖���ID�ꗗ
enum {
	MSG_OP_ID_NOP,					//!< no operation
	MSG_OP_ID_OK,					//!< ok
	MSG_OP_ID_ERR,					//!< error
	MSG_OP_ID_WAY,					//!< who are you
	MSG_OP_ID_IAM,					//!< i am
	MSG_OP_ID_VER,					//!< version
	MSG_OP_ID_MIC,					//!< mic
	MSG_OP_ID_MPW,					//!< mic power
	MSG_OP_ID_VOL,					//!< volume
	MSG_OP_ID_PKM,					//!< peak meter
	MSG_OP_ID_PIN,					//!< ping
	NUM_OF_MSG_OP_ID
} message_op;




//! ���b�Z�[�W�̋��ʃt�H�[�}�b�g��`
typedef struct
{
	char		op[3];					//!< 0-2:operator
	union
	{
		char val_c[4];					//!< 3-6:chars value
		struct
		{
			int16_t val_i_a;			//!< 3-4:int16 value
			int8_t	val_i_b;			//!< 5  :int8  value
			int8_t	val_i_c;			//!< 6  :int8  value
		};
	};
	char		reserve[1];				//!< 7  :reserve
} namp_message;

//! �����̏�Ԃ��i�[����\����
typedef struct
{
	uint16_t	vol;					//!< ���ʃ{�����[��(10bit)
	uint16_t	mic;					//!< �}�C�N�{�����[��(10bit)
	uint8_t		mpw;					//!< �}�C�N�X�C�b�`���
	uint16_t	pkm;					//!< �s�[�N���[�^�[

} namp_status;


char namp_msg_make_response(namp_message* pmsg, char opid, namp_status* pstats);
void namp_msg_set_op(namp_message *pmsg, char opid);
char namp_msg_get(namp_message *pmsg, const char *pdata);

#endif /* NAMP_H_ */