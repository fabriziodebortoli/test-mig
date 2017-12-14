Type.registerNamespace("AjaxControlToolkit");AjaxControlToolkit.BehaviorBase=function(c){var b=null,a=this;AjaxControlToolkit.BehaviorBase.initializeBase(a,[c]);a._clientStateFieldID=b;a._pageRequestManager=b;a._partialUpdateBeginRequestHandler=b;a._partialUpdateEndRequestHandler=b};AjaxControlToolkit.BehaviorBase.prototype={initialize:function(){AjaxControlToolkit.BehaviorBase.callBaseMethod(this,"initialize")},dispose:function(){var a=this;AjaxControlToolkit.BehaviorBase.callBaseMethod(a,"dispose");if(a._pageRequestManager){if(a._partialUpdateBeginRequestHandler){a._pageRequestManager.remove_beginRequest(a._partialUpdateBeginRequestHandler);a._partialUpdateBeginRequestHandler=null}if(a._partialUpdateEndRequestHandler){a._pageRequestManager.remove_endRequest(a._partialUpdateEndRequestHandler);a._partialUpdateEndRequestHandler=null}a._pageRequestManager=null}},get_ClientStateFieldID:function(){return this._clientStateFieldID},set_ClientStateFieldID:function(a){if(this._clientStateFieldID!=a){this._clientStateFieldID=a;this.raisePropertyChanged("ClientStateFieldID")}},get_ClientState:function(){if(this._clientStateFieldID){var a=document.getElementById(this._clientStateFieldID);if(a)return a.value}return null},set_ClientState:function(b){if(this._clientStateFieldID){var a=document.getElementById(this._clientStateFieldID);if(a)a.value=b}},registerPartialUpdateEvents:function(){var a=this;if(Sys&&Sys.WebForms&&Sys.WebForms.PageRequestManager){a._pageRequestManager=Sys.WebForms.PageRequestManager.getInstance();if(a._pageRequestManager){a._partialUpdateBeginRequestHandler=Function.createDelegate(a,a._partialUpdateBeginRequest);a._pageRequestManager.add_beginRequest(a._partialUpdateBeginRequestHandler);a._partialUpdateEndRequestHandler=Function.createDelegate(a,a._partialUpdateEndRequest);a._pageRequestManager.add_endRequest(a._partialUpdateEndRequestHandler)}}},_partialUpdateBeginRequest:function(){},_partialUpdateEndRequest:function(){}};AjaxControlToolkit.BehaviorBase.registerClass("AjaxControlToolkit.BehaviorBase",Sys.UI.Behavior);AjaxControlToolkit.DynamicPopulateBehaviorBase=function(c){var b=null,a=this;AjaxControlToolkit.DynamicPopulateBehaviorBase.initializeBase(a,[c]);a._DynamicControlID=b;a._DynamicContextKey=b;a._DynamicServicePath=b;a._DynamicServiceMethod=b;a._cacheDynamicResults=false;a._dynamicPopulateBehavior=b;a._populatingHandler=b;a._populatedHandler=b};AjaxControlToolkit.DynamicPopulateBehaviorBase.prototype={initialize:function(){var a=this;AjaxControlToolkit.DynamicPopulateBehaviorBase.callBaseMethod(a,"initialize");a._populatingHandler=Function.createDelegate(a,a._onPopulating);a._populatedHandler=Function.createDelegate(a,a._onPopulated)},dispose:function(){var a=this;if(a._populatedHandler){if(a._dynamicPopulateBehavior)a._dynamicPopulateBehavior.remove_populated(a._populatedHandler);a._populatedHandler=null}if(a._populatingHandler){if(a._dynamicPopulateBehavior)a._dynamicPopulateBehavior.remove_populating(a._populatingHandler);a._populatingHandler=null}if(a._dynamicPopulateBehavior){a._dynamicPopulateBehavior.dispose();a._dynamicPopulateBehavior=null}AjaxControlToolkit.DynamicPopulateBehaviorBase.callBaseMethod(a,"dispose")},populate:function(b){var a=this;if(a._dynamicPopulateBehavior&&a._dynamicPopulateBehavior.get_element()!=$get(a._DynamicControlID)){a._dynamicPopulateBehavior.dispose();a._dynamicPopulateBehavior=null}if(!a._dynamicPopulateBehavior&&a._DynamicControlID&&a._DynamicServiceMethod){a._dynamicPopulateBehavior=$create(AjaxControlToolkit.DynamicPopulateBehavior,{id:a.get_id()+"_DynamicPopulateBehavior",ContextKey:a._DynamicContextKey,ServicePath:a._DynamicServicePath,ServiceMethod:a._DynamicServiceMethod,cacheDynamicResults:a._cacheDynamicResults},null,null,$get(a._DynamicControlID));a._dynamicPopulateBehavior.add_populating(a._populatingHandler);a._dynamicPopulateBehavior.add_populated(a._populatedHandler)}if(a._dynamicPopulateBehavior)a._dynamicPopulateBehavior.populate(b?b:a._DynamicContextKey)},_onPopulating:function(b,a){this.raisePopulating(a)},_onPopulated:function(b,a){this.raisePopulated(a)},get_dynamicControlID:function(){return this._DynamicControlID},get_DynamicControlID:this.get_dynamicControlID,set_dynamicControlID:function(b){var a=this;if(a._DynamicControlID!=b){a._DynamicControlID=b;a.raisePropertyChanged("dynamicControlID");a.raisePropertyChanged("DynamicControlID")}},set_DynamicControlID:this.set_dynamicControlID,get_dynamicContextKey:function(){return this._DynamicContextKey},get_DynamicContextKey:this.get_dynamicContextKey,set_dynamicContextKey:function(b){var a=this;if(a._DynamicContextKey!=b){a._DynamicContextKey=b;a.raisePropertyChanged("dynamicContextKey");a.raisePropertyChanged("DynamicContextKey")}},set_DynamicContextKey:this.set_dynamicContextKey,get_dynamicServicePath:function(){return this._DynamicServicePath},get_DynamicServicePath:this.get_dynamicServicePath,set_dynamicServicePath:function(b){var a=this;if(a._DynamicServicePath!=b){a._DynamicServicePath=b;a.raisePropertyChanged("dynamicServicePath");a.raisePropertyChanged("DynamicServicePath")}},set_DynamicServicePath:this.set_dynamicServicePath,get_dynamicServiceMethod:function(){return this._DynamicServiceMethod},get_DynamicServiceMethod:this.get_dynamicServiceMethod,set_dynamicServiceMethod:function(b){var a=this;if(a._DynamicServiceMethod!=b){a._DynamicServiceMethod=b;a.raisePropertyChanged("dynamicServiceMethod");a.raisePropertyChanged("DynamicServiceMethod")}},set_DynamicServiceMethod:this.set_dynamicServiceMethod,get_cacheDynamicResults:function(){return this._cacheDynamicResults},set_cacheDynamicResults:function(a){if(this._cacheDynamicResults!=a){this._cacheDynamicResults=a;this.raisePropertyChanged("cacheDynamicResults")}},add_populated:function(a){this.get_events().addHandler("populated",a)},remove_populated:function(a){this.get_events().removeHandler("populated",a)},raisePopulated:function(b){var a=this.get_events().getHandler("populated");if(a)a(this,b)},add_populating:function(a){this.get_events().addHandler("populating",a)},remove_populating:function(a){this.get_events().removeHandler("populating",a)},raisePopulating:function(b){var a=this.get_events().getHandler("populating");if(a)a(this,b)}};AjaxControlToolkit.DynamicPopulateBehaviorBase.registerClass("AjaxControlToolkit.DynamicPopulateBehaviorBase",AjaxControlToolkit.BehaviorBase);AjaxControlToolkit.ControlBase=function(b){var a=this;AjaxControlToolkit.ControlBase.initializeBase(a,[b]);a._clientStateField=null;a._callbackTarget=null;a._onsubmit$delegate=Function.createDelegate(a,a._onsubmit);a._oncomplete$delegate=Function.createDelegate(a,a._oncomplete);a._onerror$delegate=Function.createDelegate(a,a._onerror)};AjaxControlToolkit.ControlBase.__doPostBack=function(c,b){if(!Sys.WebForms.PageRequestManager.getInstance().get_isInAsyncPostBack())for(var a=0;a<AjaxControlToolkit.ControlBase.onsubmitCollection.length;a++)AjaxControlToolkit.ControlBase.onsubmitCollection[a]();Function.createDelegate(window,AjaxControlToolkit.ControlBase.__doPostBackSaved)(c,b)};AjaxControlToolkit.ControlBase.prototype={initialize:function(){var b="undefined",a=this;AjaxControlToolkit.ControlBase.callBaseMethod(a,"initialize");if(a._clientStateField)a.loadClientState(a._clientStateField.value);if(typeof Sys.WebForms!==b&&typeof Sys.WebForms.PageRequestManager!==b){Array.add(Sys.WebForms.PageRequestManager.getInstance()._onSubmitStatements,a._onsubmit$delegate);if(AjaxControlToolkit.ControlBase.__doPostBackSaved==null||typeof AjaxControlToolkit.ControlBase.__doPostBackSaved==b){AjaxControlToolkit.ControlBase.__doPostBackSaved=window.__doPostBack;window.__doPostBack=AjaxControlToolkit.ControlBase.__doPostBack;AjaxControlToolkit.ControlBase.onsubmitCollection=[]}Array.add(AjaxControlToolkit.ControlBase.onsubmitCollection,a._onsubmit$delegate)}else $addHandler(document.forms[0],"submit",a._onsubmit$delegate)},dispose:function(){var b="undefined",a=this;if(typeof Sys.WebForms!==b&&typeof Sys.WebForms.PageRequestManager!==b){Array.remove(AjaxControlToolkit.ControlBase.onsubmitCollection,a._onsubmit$delegate);Array.remove(Sys.WebForms.PageRequestManager.getInstance()._onSubmitStatements,a._onsubmit$delegate)}else $removeHandler(document.forms[0],"submit",a._onsubmit$delegate);AjaxControlToolkit.ControlBase.callBaseMethod(a,"dispose")},findElement:function(a){return $get(this.get_id()+"_"+a.split(":").join("_"))},get_clientStateField:function(){return this._clientStateField},set_clientStateField:function(b){var a=this;if(a.get_isInitialized())throw Error.invalidOperation(AjaxControlToolkit.Resources.ExtenderBase_CannotSetClientStateField);if(a._clientStateField!=b){a._clientStateField=b;a.raisePropertyChanged("clientStateField")}},loadClientState:function(){},saveClientState:function(){return null},_invoke:function(g,d,h){var a=this;if(!a._callbackTarget)throw Error.invalidOperation(AjaxControlToolkit.Resources.ExtenderBase_ControlNotRegisteredForCallbacks);if(typeof WebForm_DoCallback==="undefined")throw Error.invalidOperation(AjaxControlToolkit.Resources.ExtenderBase_PageNotRegisteredForCallbacks);var e=[];for(var b=0;b<d.length;b++)e[b]=d[b];var c=a.saveClientState();if(c!=null&&!String.isInstanceOfType(c))throw Error.invalidOperation(AjaxControlToolkit.Resources.ExtenderBase_InvalidClientStateType);var f=Sys.Serialization.JavaScriptSerializer.serialize({name:g,args:e,state:a.saveClientState()});WebForm_DoCallback(a._callbackTarget,f,a._oncomplete$delegate,h,a._onerror$delegate,true)},_oncomplete:function(a,b){a=Sys.Serialization.JavaScriptSerializer.deserialize(a);if(a.error)throw Error.create(a.error);this.loadClientState(a.state);b(a.result)},_onerror:function(a){throw Error.create(a)},_onsubmit:function(){if(this._clientStateField)this._clientStateField.value=this.saveClientState();return true}};AjaxControlToolkit.ControlBase.registerClass("AjaxControlToolkit.ControlBase",Sys.UI.Control);