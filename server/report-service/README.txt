
#
#
#		  JSON template of message

{
	"commantType": ""  ,
	"message":"",
	"response":""
}

where commandType is enumerator   CommandType { OK, NAMESPACE, DATA, TEMPLATE, ASK, TEST, GUID, ERROR, PAGE, PDF, RUN, PAUSE, STOP }
 in the MessageBuilder class (RS-Message.cs)

EXAMPLE:

{
	"commantType": "1"  ,  //NAMESPACE
	"message":"erp.company.titles",
	"response":""
}

