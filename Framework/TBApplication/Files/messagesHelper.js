Ext.define('MessagesHelper', {

    singleton: true,

    //mixins: {
    //    observable: 'Ext.mixin.Observable'
    //},
    messagesPanel: undefined,
   
    setMessagePanel: function(oPanel){
        this.messagesPanel = oPanel;
    },

    addMessage: function (sMessage) {
        if (this.messagesPanel) {
            var sMessages = this.messagesPanel.html ? this.messagesPanel.html : "" ;
            this.messagesPanel.update(sMessages + "<p>" + sMessage + "</p>");
        }
    },

    clearMessages: function () {
        if (this.messagesPanel) {
            this.messagesPanel.update("");
        }
    }
  
});