Type.registerNamespace("AjaxControlToolkit.HTMLEditor");AjaxControlToolkit.HTMLEditor.ActiveModeChangedArgs=function(d,c,b){var a=this;if(arguments.length!=3)throw Error.parameterCount();AjaxControlToolkit.HTMLEditor.ActiveModeChangedArgs.initializeBase(a);a._oldMode=d;a._newMode=c;a._editPanel=b};AjaxControlToolkit.HTMLEditor.ActiveModeChangedArgs.prototype={get_oldMode:function(){return this._oldMode},get_newMode:function(){return this._newMode},get_editPanel:function(){return this._editPanel}};AjaxControlToolkit.HTMLEditor.ActiveModeChangedArgs.registerClass("AjaxControlToolkit.HTMLEditor.ActiveModeChangedArgs",Sys.EventArgs);