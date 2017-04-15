/*
 * namp.c
 *
 * Created: 2017/04/15 22:46:22
 *  Author: Jackhammer
 */ 

 #include "config.h"
  
 #include <stdlib.h>
 #include <string.h>

 #include <avr/io.h>			// �s������
 
 #include "namp.h"
 #include "util.h"
 
 const char *OPERATIONS[] = {
	 MSG_OP_NOP,
	 MSG_OP_OK,
	 MSG_OP_ERR,
	 MSG_OP_WAY,
	 MSG_OP_IAM,
	 MSG_OP_VER,
	 MSG_OP_MIC,
	 MSG_OP_MPW,
	 MSG_OP_VOL,
	 MSG_OP_PKM,
	 MSG_OP_PIN
 };

 //=============================================================================
/**
 * message��op��]����opid��ԋp
 * @brief	message��op��]����opid��ԋp
 * @param	(pmsg) �]���Ώۂ�message
 * @return	operation id
 */
 static inline char namp_msg_get_opid(const namp_message *pmsg){
	char			opid = MSG_OP_ID_NOP;
	const char			*pop;
	unsigned char	i;

	if(pmsg == NULL)
	{
		return opid;
	}

	pop = pmsg->op;

	for(i=NUM_OF_MSG_OP_ID; i-->0;)
	{
		if( memcmp(pop, OPERATIONS[i], sizeof(pmsg->op)) )
		{
			continue;
		}
			
		opid = i;
		break;
	}

	return opid;
}


//=============================================================================
/**
 * message��op���A�w�肵��opid���瓾����op�ŏ㏑��
 * @brief	message��op��opid���瓾����op�ŏ㏑��
 * @param	(pmsg) �]���Ώۂ�message
 * @param	(opid) operation id
 * @return	none
 */
void namp_msg_set_op(namp_message *pmsg, char opid)
{
	memcpy(pmsg->op, OPERATIONS[(unsigned char)opid], sizeof(pmsg->op));
}

//=============================================================================
/**
 * �������񂩂�message���擾���A�ԋp����
 * @brief	����������message�擾
 * @param	(msg) �ԋp����message
 * @param	(pstr) ��ƂȂ镶����
 * @return	operation id
 */
char namp_msg_get(namp_message *pmsg, const char *pdata)
{
	if(pmsg == NULL)
	{
		return MSG_OP_ID_NOP;
	}

	// msg��������
	memcpy(pmsg, pdata, sizeof(*pmsg));
	
	// op������op-index�𓾂�
	return namp_msg_get_opid(pmsg);
}


//=============================================================================
/**
 * DEBUG:message���V���A���ɏo�͂���
 * @brief	message���V���A���ɏo��
 * @param	(msg) �o�͑Ώۂ�message
 * @return	none
 */
void namp_msg_put_debug(const namp_message* pmsg){
	//usart_puts("op :");
	//usart_transmit_bytes(pmsg->op, sizeof(pmsg->op));
	//usart_puts("\r\n");
	//usart_puts("val:");
	////usart_puts(itoa(msg->, buf, 10));
	//usart_puts("\r\n");
}

char namp_msg_make_response(namp_message* pmsg, char opid, namp_status* pstats)
{
	switch(opid)
	{
		case MSG_OP_ID_WAY:
			opid = MSG_OP_ID_IAM;
			memcpy(pmsg->val_c, APP_IDENTITY, sizeof(pmsg->val_c));
			break;
		case MSG_OP_ID_VER:
			pmsg->val_i_a = 1;
			//pmsg->val_i_b = 0;
			//pmsg->val_i_c = 0;
			break;
		case MSG_OP_ID_MIC:
			pmsg->val_i_a = MIN(MAX(0, pstats->mic), MIC_MAX);
			//pmsg->val_i_b = 0;
			//pmsg->val_i_c = 0;
			break;
		case MSG_OP_ID_MPW:
			pmsg->val_i_a = MIN(MAX(0, pstats->mpw), 1);
			//pmsg->val_i_b = 0;
			//pmsg->val_i_c = 0;
			break;
		case MSG_OP_ID_VOL:
			pmsg->val_i_a = MIN(MAX(0, pstats->vol), VOL_MAX);
			//pmsg->val_i_b = 0;
			//pmsg->val_i_c = 0;
			break;
		case MSG_OP_ID_PKM:
			pstats->pkm = MIN(MAX(0, pmsg->val_i_a), PKM_MAX);
			
			if(pstats->pkm > 0)
			{
				PWM0A_ON(pstats->pkm>>(PKM_BIT_WIDTH-PWM_DUTY_BIT_WIDTH));
			}else{
				PWM0A_OFF();
			}
			
			opid = MSG_OP_ID_OK;
			memcpy(pmsg->val_c, APP_IDENTITY, sizeof(pmsg->val_c));
			break;
		default:
			opid = MSG_OP_ID_NOP;
	}
	
	namp_msg_set_op(pmsg, opid);


	return opid;
}


