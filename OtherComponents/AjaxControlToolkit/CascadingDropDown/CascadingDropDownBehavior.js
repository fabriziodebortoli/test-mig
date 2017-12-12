Type.registerNamespace("AjaxControlToolkit");AjaxControlToolkit.CascadingDropDownSelectionChangedEventArgs=function(b,a){AjaxControlToolkit.CascadingDropDownSelectionChangedEventArgs.initializeBase(this);this._oldValue=b;this._newValue=a};AjaxControlToolkit.CascadingDropDownSelectionChangedEventArgs.prototype={get_oldValue:function(){return this._oldValue},get_newValue:function(){return this._newValue}};AjaxControlToolkit.CascadingDropDownSelectionChangedEventArgs.registerClass("AjaxControlToolkit.CascadingDropDownSelectionChangedEventArgs",Sys.EventArgs);AjaxControlToolkit.CascadingDropDownBehavior=function(c){var b=null,a=this;AjaxControlToolkit.CascadingDropDownBehavior.initializeBase(a,[c]);a._parentControlID=b;a._category=b;a._promptText=b;a._loadingText=b;a._promptValue=b;a._emptyValue=b;a._emptyText=b;a._servicePath=b;a._serviceMethod=b;a._contextKey=b;a._useContextKey=false;a._parentElement=b;a._changeHandler=b;a._parentChangeHandler=b;a._lastParentValues=b;a._selectedValue=b};AjaxControlToolkit.CascadingDropDownBehavior.prototype={initialize:function(){var a=this;AjaxControlToolkit.CascadingDropDownBehavior.callBaseMethod(a,"initialize");$common.prepareHiddenElementForATDeviceUpdate();var b=a.get_element();a._clearItems();b.CascadingDropDownCategory=a._category;a._changeHandler=Function.createDelegate(a,a._onChange);$addHandler(b,"change",a._changeHandler);if(a._parentControlID){a._parentElement=$get(a._parentControlID);Sys.Debug.assert(a._parentElement!=null,String.format(AjaxControlToolkit.Resources.CascadingDropDown_NoParentElement,a._parentControlID));if(a._parentElement){b.CascadingDropDownParentControlID=a._parentControlID;a._parentChangeHandler=Function.createDelegate(a,a._onParentChange);$addHandler(a._parentElement,"change",a._parentChangeHandler);if(!a._parentElement.childDropDown)a._parentElement.childDropDown=[];a._parentElement.childDropDown.push(a)}}a._onParentChange(null,true)},dispose:function(){var a=this,b=a.get_element();if(a._changeHandler){$removeHandler(b,"change",a._changeHandler);a._changeHandler=null}if(a._parentChangeHandler){if(a._parentElement)$removeHandler(a._parentElement,"change",a._parentChangeHandler);a._parentChangeHandler=null}AjaxControlToolkit.CascadingDropDownBehavior.callBaseMethod(a,"dispose")},_clearItems:function(){var a=this.get_element();while(0<a.options.length)a.remove(0)},_isPopulated:function(){var a=this.get_element().options.length;if(this._promptText)return a>1;else return a>0},_setOptions:function(c,m,f){var a=this;if(!a.get_isInitialized())return;var b=a.get_element();a._clearItems();var g,h="";if(f&&a._loadingText){g=a._loadingText;if(a._selectedValue)h=a._selectedValue}else if(!f&&c&&0==c.length&&null!=a._emptyText){g=a._emptyText;if(a._emptyValue)h=a._emptyValue}else if(a._promptText){g=a._promptText;if(a._promptValue)h=a._promptValue}if(g){var j=new Option(g,h);b.options[b.options.length]=j}var d=null,e=-1;if(c){for(i=0;i<c.length;i++){var n=c[i].name,k=c[i].value;if(c[i].isDefaultValue){e=i;if(a._promptText)e++}var j=new Option(n,k);if(k==a._selectedValue)d=j;b.options[b.options.length]=j}if(d)d.selected=true}if(d)a.set_SelectedValue(b.options[b.selectedIndex].value,b.options[b.selectedIndex].text);else if(!d&&e!=-1){b.options[e].selected=true;a.set_SelectedValue(b.options[e].value,b.options[e].text)}else if(!m&&!d&&!f&&!a._promptText&&b.options.length>0)a.set_SelectedValue(b.options[0].value,b.options[0].text);else if(!m&&!d&&!f)a.set_SelectedValue("","");if(b.childDropDown&&!f)for(i=0;i<b.childDropDown.length;i++)b.childDropDown[i]._onParentChange();else if(c&&Sys.Browser.agent!==Sys.Browser.Safari&&Sys.Browser.agent!==Sys.Browser.Opera)if(document.createEvent){var l=document.createEvent("HTMLEvents");l.initEvent("change",true,false);a.get_element().dispatchEvent(l)}else if(document.createEventObject)a.get_element().fireEvent("onchange");if(a._loadingText||a._promptText||a._emptyText)b.disabled=!c||0==c.length;a.raisePopulated(Sys.EventArgs.Empty)},_onChange:function(){var b=this;if(!b._isPopulated())return;var a=b.get_element();if(-1!=a.selectedIndex&&!(b._promptText&&0==a.selectedIndex))b.set_SelectedValue(a.options[a.selectedIndex].value,a.options[a.selectedIndex].text);else b.set_SelectedValue("","")},_onParentChange:function(i,g){var a=this,j=a.get_element(),b="",d=a._parentControlID;while(d){var c=$get(d);if(c&&-1!=c.selectedIndex){var e=c.options[c.selectedIndex].value;if(e&&e!=""){b=c.CascadingDropDownCategory+":"+e+";"+b;d=c.CascadingDropDownParentControlID;continue}}break}if(b!=""&&a._lastParentValues==b)return;a._lastParentValues=b;if(b==""&&a._parentControlID){a._setOptions(null,g);return}a._setOptions(null,g,true);if(a._servicePath&&a._serviceMethod){var f=new Sys.CancelEventArgs;a.raisePopulating(f);if(f.get_cancel())return;var h={knownCategoryValues:b,category:a._category};if(a._useContextKey)h.contextKey=a._contextKey;Sys.Net.WebServiceProxy.invoke(a._servicePath,a._serviceMethod,false,h,Function.createDelegate(a,a._onMethodComplete),Function.createDelegate(a,a._onMethodError));$common.updateFormToRefreshATDeviceBuffer()}},_onMethodComplete:function(a){this._setOptions(a)},_onMethodError:function(b){var a=this;if(b.get_timedOut())a._setOptions([a._makeNameValueObject(AjaxControlToolkit.Resources.CascadingDropDown_MethodTimeout)]);else a._setOptions([a._makeNameValueObject(String.format(AjaxControlToolkit.Resources.CascadingDropDown_MethodError,b.get_statusCode()))])},_makeNameValueObject:function(a){return {name:a,value:a}},get_ParentControlID:function(){return this._parentControlID},set_ParentControlID:function(a){if(this._parentControlID!=a){this._parentControlID=a;this.raisePropertyChanged("ParentControlID")}},get_Category:function(){return this._category},set_Category:function(a){if(this._category!=a){this._category=a;this.raisePropertyChanged("Category")}},get_PromptText:function(){return this._promptText},set_PromptText:function(a){if(this._promptText!=a){this._promptText=a;this.raisePropertyChanged("PromptText")}},get_PromptValue:function(){return this._promptValue},set_PromptValue:function(a){if(this._promptValue!=a){this._promptValue=a;this.raisePropertyChanged("PromptValue")}},get_EmptyText:function(){return this._emptyText},set_EmptyText:function(a){if(this._emptyText!=a){this._emptyText=a;this.raisePropertyChanged("EmptyText")}},get_EmptyValue:function(){return this._emptyValue},set_EmptyValue:function(a){if(this._emptyValue!=a){this._emptyValue=a;this.raisePropertyChanged("EmptyValue")}},get_LoadingText:function(){return this._loadingText},set_LoadingText:function(a){if(this._loadingText!=a){this._loadingText=a;this.raisePropertyChanged("LoadingText")}},get_SelectedValue:function(){return this._selectedValue},set_SelectedValue:function(b,c){var a=this;if(a._selectedValue!=b){if(!c){var d=b.indexOf(":::");if(-1!=d){c=b.slice(d+3);b=b.slice(0,d)}}var e=a._selectedValue;a._selectedValue=b;a.raisePropertyChanged("SelectedValue");a.raiseSelectionChanged(new AjaxControlToolkit.CascadingDropDownSelectionChangedEventArgs(e,b))}AjaxControlToolkit.CascadingDropDownBehavior.callBaseMethod(a,"set_ClientState",[a._selectedValue+":::"+c])},get_ServicePath:function(){return this._servicePath},set_ServicePath:function(a){if(this._servicePath!=a){this._servicePath=a;this.raisePropertyChanged("ServicePath")}},get_ServiceMethod:function(){return this._serviceMethod},set_ServiceMethod:function(a){if(this._serviceMethod!=a){this._serviceMethod=a;this.raisePropertyChanged("ServiceMethod")}},get_contextKey:function(){return this._contextKey},set_contextKey:function(b){var a=this;if(a._contextKey!=b){a._contextKey=b;a.set_useContextKey(true);a.raisePropertyChanged("contextKey")}},get_useContextKey:function(){return this._useContextKey},set_useContextKey:function(a){if(this._useContextKey!=a){this._useContextKey=a;this.raisePropertyChanged("useContextKey")}},add_selectionChanged:function(a){this.get_events().addHandler("selectionChanged",a)},remove_selectionChanged:function(a){this.get_events().removeHandler("selectionChanged",a)},raiseSelectionChanged:function(b){var a=this.get_events().getHandler("selectionChanged");if(a)a(this,b)},add_populating:function(a){this.get_events().addHandler("populating",a)},remove_populating:function(a){this.get_events().removeHandler("populating",a)},raisePopulating:function(b){var a=this.get_events().getHandler("populating");if(a)a(this,b)},add_populated:function(a){this.get_events().addHandler("populated",a)},remove_populated:function(a){this.get_events().removeHandler("populated",a)},raisePopulated:function(b){var a=this.get_events().getHandler("populated");if(a)a(this,b)}};AjaxControlToolkit.CascadingDropDownBehavior.registerClass("AjaxControlToolkit.CascadingDropDownBehavior",AjaxControlToolkit.BehaviorBase);