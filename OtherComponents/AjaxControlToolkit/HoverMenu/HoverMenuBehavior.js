Type.registerNamespace("AjaxControlToolkit");AjaxControlToolkit.HoverMenuBehavior=function(c){var b=null,a=this;AjaxControlToolkit.HoverMenuBehavior.initializeBase(a,[c]);a._hoverBehavior=b;a._popupBehavior=b;a._mouseEnterHandler=b;a._mouseLeaveHandler=b;a._unhoverHandler=b;a._hoverHandler=b;a._inHover=b;a._oldClass=b;a._popupElement=b;a._onShowJson=b;a._onHideJson=b;a._popupElement=b;a._hoverCssClass=b;a._offsetX=0;a._offsetY=0;a._popDelay=100;a._hoverDelay=0;a._popupPosition=b};AjaxControlToolkit.HoverMenuBehavior.prototype={initialize:function(){var b=null,a=this;AjaxControlToolkit.HoverMenuBehavior.callBaseMethod(a,"initialize");a._hoverHandler=Function.createDelegate(a,a._onHover);a._unhoverHandler=Function.createDelegate(a,a._onUnhover);a._mouseEnterHandler=Function.createDelegate(a,a._onmouseover);a._mouseLeaveHandler=Function.createDelegate(a,a._onmouseout);var c=a.get_element();$addHandler(c,"mouseover",a._mouseEnterHandler);$addHandler(c,"mouseout",a._mouseLeaveHandler);if(a._popupElement){a._popupBehavior=$create(AjaxControlToolkit.PopupBehavior,{id:a.get_id()+"_PopupBehavior"},b,b,a._popupElement);if(a._popupPosition)a._popupBehavior.set_positioningMode(AjaxControlToolkit.HoverMenuPopupPosition.Absolute);else a._popupBehavior.set_positioningMode(AjaxControlToolkit.HoverMenuPopupPosition.Center);if(a._onShowJson)a._popupBehavior.set_onShow(a._onShowJson);if(a._onHideJson)a._popupBehavior.set_onHide(a._onHideJson);a._hoverBehavior=$create(AjaxControlToolkit.HoverBehavior,{id:a.get_id()+"_HoverBehavior",hoverDelay:a._hoverDelay,unhoverDelay:a._popDelay,hoverElement:a._popupElement},b,b,c);a._hoverBehavior.add_hover(a._hoverHandler);a._hoverBehavior.add_unhover(a._unhoverHandler)}},dispose:function(){var b=null,a=this;a._onShowJson=b;a._onHideJson=b;if(a._popupBehavior){a._popupBehavior.dispose();a._popupBehavior=b}if(a._popupElement)a._popupElement=b;if(a._mouseEnterHandler)$removeHandler(a.get_element(),"mouseover",a._mouseEnterHandler);if(a._mouseLeaveHandler)$removeHandler(a.get_element(),"mouseout",a._mouseLeaveHandler);if(a._hoverBehavior){if(a._hoverHandler){a._hoverBehavior.remove_hover(a._hoverHandler);a._hoverHandler=b}if(a._unhoverHandler){a._hoverBehavior.remove_hover(a._unhoverHandler);a._unhoverHandler=b}a._hoverBehavior.dispose();a._hoverBehavior=b}AjaxControlToolkit.HoverMenuBehavior.callBaseMethod(a,"dispose")},_getLeftOffset:function(){var a=this,c=$common.getLocation(a.get_element()).x,d=$common.getLocation(a.get_popupElement().offsetParent).x,b=0;switch(a._popupPosition){case AjaxControlToolkit.HoverMenuPopupPosition.Left:b=-1*a._popupElement.offsetWidth;break;case AjaxControlToolkit.HoverMenuPopupPosition.Right:b=a.get_element().offsetWidth}return b+c-d+a._offsetX},_getTopOffset:function(){var a=this,c=$common.getLocation(a.get_element()).y,d=$common.getLocation(a.get_popupElement().offsetParent).y,b=0;switch(a._popupPosition){case AjaxControlToolkit.HoverMenuPopupPosition.Top:b=-1*a._popupElement.offsetHeight;break;case AjaxControlToolkit.HoverMenuPopupPosition.Bottom:b=a.get_element().offsetHeight}return c-d+b+a._offsetY},_onHover:function(){var a=this;if(a._inHover)return;var b=new Sys.CancelEventArgs;a.raiseShowing(b);if(b.get_cancel())return;a._inHover=true;a.populate();a._popupBehavior.show();if($common.getCurrentStyle(a._popupElement,"display")=="none")a._popupElement.style.display="block";a._popupBehavior.set_x(a._getLeftOffset());a._popupBehavior.set_y(a._getTopOffset());a.raiseShown(Sys.EventArgs.Empty)},_onUnhover:function(){var a=this,b=new Sys.CancelEventArgs;a.raiseHiding(b);if(b.get_cancel())return;a._inHover=false;a._resetCssClass();a._popupBehavior.hide();a.raiseHidden(Sys.EventArgs.Empty)},_onmouseover:function(){var a=this,b=a.get_element();if(a._hoverCssClass&&b.className!=a._hoverCssClass){a._oldClass=b.className;b.className=a._hoverCssClass}},_onmouseout:function(){this._resetCssClass()},_resetCssClass:function(){var a=this,b=a.get_element();if(!a._inHover&&a._hoverCssClass&&b.className==a._hoverCssClass)b.className=a._oldClass},get_onShow:function(){return this._popupBehavior?this._popupBehavior.get_onShow():this._onShowJson},set_onShow:function(b){var a=this;if(a._popupBehavior)a._popupBehavior.set_onShow(b);else a._onShowJson=b;a.raisePropertyChanged("onShow")},get_onShowBehavior:function(){return this._popupBehavior?this._popupBehavior.get_onShowBehavior():null},onShow:function(){if(this._popupBehavior)this._popupBehavior.onShow()},get_onHide:function(){return this._popupBehavior?this._popupBehavior.get_onHide():this._onHideJson},set_onHide:function(b){var a=this;if(a._popupBehavior)a._popupBehavior.set_onHide(b);else a._onHideJson=b;a.raisePropertyChanged("onHide")},get_onHideBehavior:function(){return this._popupBehavior?this._popupBehavior.get_onHideBehavior():null},onHide:function(){if(this._popupBehavior)this._popupBehavior.onHide()},get_popupElement:function(){return this._popupElement},set_popupElement:function(b){var a=this;if(a._popupElement!=b){a._popupElement=b;if(a.get_isInitialized()&&a._hoverBehavior)a._hoverBehavior.set_hoverElement(a._popupElement);a.raisePropertyChanged("popupElement")}},get_HoverCssClass:function(){return this._hoverCssClass},set_HoverCssClass:function(a){if(this._hoverCssClass!=a){this._hoverCssClass=a;this.raisePropertyChanged("HoverCssClass")}},get_OffsetX:function(){return this._offsetX},set_OffsetX:function(a){if(this._offsetX!=a){this._offsetX=a;this.raisePropertyChanged("OffsetX")}},get_OffsetY:function(){return this._offsetY},set_OffsetY:function(a){if(this._offsetY!=a){this._offsetY=a;this.raisePropertyChanged("OffsetY")}},get_PopupPosition:function(){return this._popupPosition},set_PopupPosition:function(a){if(this._popupPosition!=a){this._popupPosition=a;this.raisePropertyChanged("PopupPosition")}},get_PopDelay:function(){return this._popDelay},set_PopDelay:function(a){if(this._popDelay!=a){this._popDelay=a;this.raisePropertyChanged("PopDelay")}},get_HoverDelay:function(){return this._hoverDelay},set_HoverDelay:function(a){if(this._hoverDelay!=a){this._hoverDelay=a;this.raisePropertyChanged("HoverDelay")}},add_showing:function(a){this.get_events().addHandler("showing",a)},remove_showing:function(a){this.get_events().removeHandler("showing",a)},raiseShowing:function(b){var a=this.get_events().getHandler("showing");if(a)a(this,b)},add_shown:function(a){this.get_events().addHandler("shown",a)},remove_shown:function(a){this.get_events().removeHandler("shown",a)},raiseShown:function(b){var a=this.get_events().getHandler("shown");if(a)a(this,b)},add_hiding:function(a){this.get_events().addHandler("hiding",a)},remove_hiding:function(a){this.get_events().removeHandler("hiding",a)},raiseHiding:function(b){var a=this.get_events().getHandler("hiding");if(a)a(this,b)},add_hidden:function(a){this.get_events().addHandler("hidden",a)},remove_hidden:function(a){this.get_events().removeHandler("hidden",a)},raiseHidden:function(b){var a=this.get_events().getHandler("hidden");if(a)a(this,b)}};AjaxControlToolkit.HoverMenuBehavior.registerClass("AjaxControlToolkit.HoverMenuBehavior",AjaxControlToolkit.DynamicPopulateBehaviorBase);AjaxControlToolkit.HoverMenuPopupPosition=function(){throw Error.invalidOperation()};AjaxControlToolkit.HoverMenuPopupPosition.prototype={Center:0,Top:1,Left:2,Bottom:3,Right:4};AjaxControlToolkit.HoverMenuPopupPosition.registerEnum("AjaxControlToolkit.HoverMenuPopupPosition");