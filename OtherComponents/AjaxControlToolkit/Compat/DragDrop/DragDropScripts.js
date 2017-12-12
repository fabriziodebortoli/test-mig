Type.registerNamespace("AjaxControlToolkit");AjaxControlToolkit.IDragSource=function(){};AjaxControlToolkit.IDragSource.prototype={get_dragDataType:function(){throw Error.notImplemented()},getDragData:function(){throw Error.notImplemented()},get_dragMode:function(){throw Error.notImplemented()},onDragStart:function(){throw Error.notImplemented()},onDrag:function(){throw Error.notImplemented()},onDragEnd:function(){throw Error.notImplemented()}};AjaxControlToolkit.IDragSource.registerInterface("AjaxControlToolkit.IDragSource");AjaxControlToolkit.IDropTarget=function(){};AjaxControlToolkit.IDropTarget.prototype={get_dropTargetElement:function(){throw Error.notImplemented()},canDrop:function(){throw Error.notImplemented()},drop:function(){throw Error.notImplemented()},onDragEnterTarget:function(){throw Error.notImplemented()},onDragLeaveTarget:function(){throw Error.notImplemented()},onDragInTarget:function(){throw Error.notImplemented()}};AjaxControlToolkit.IDropTarget.registerInterface("AjaxControlToolkit.IDropTarget");AjaxControlToolkit.DragMode=function(){throw Error.invalidOperation()};AjaxControlToolkit.DragMode.prototype={Copy:0,Move:1};AjaxControlToolkit.DragMode.registerEnum("AjaxControlToolkit.DragMode");AjaxControlToolkit.DragDropEventArgs=function(c,a,b){this._dragMode=c;this._dataType=a;this._data=b};AjaxControlToolkit.DragDropEventArgs.prototype={get_dragMode:function(){return this._dragMode||null},get_dragDataType:function(){return this._dataType||null},get_dragData:function(){return this._data||null}};AjaxControlToolkit.DragDropEventArgs.registerClass("AjaxControlToolkit.DragDropEventArgs");AjaxControlToolkit._DragDropManager=function(){this._instance=null;this._events=null};AjaxControlToolkit._DragDropManager.prototype={add_dragStart:function(a){this.get_events().addHandler("dragStart",a)},remove_dragStart:function(a){this.get_events().removeHandler("dragStart",a)},get_events:function(){if(!this._events)this._events=new Sys.EventHandlerList;return this._events},add_dragStop:function(a){this.get_events().addHandler("dragStop",a)},remove_dragStop:function(a){this.get_events().removeHandler("dragStop",a)},_getInstance:function(){var a=this;if(!a._instance){if(Sys.Browser.agent===Sys.Browser.InternetExplorer)a._instance=new AjaxControlToolkit.IEDragDropManager;else a._instance=new AjaxControlToolkit.GenericDragDropManager;a._instance.initialize();a._instance.add_dragStart(Function.createDelegate(a,a._raiseDragStart));a._instance.add_dragStop(Function.createDelegate(a,a._raiseDragStop))}return a._instance},startDragDrop:function(b,c,d,a){this._getInstance().startDragDrop(b,c,d,a)},registerDropTarget:function(a){this._getInstance().registerDropTarget(a)},unregisterDropTarget:function(a){this._getInstance().unregisterDropTarget(a)},dispose:function(){delete this._events;Sys.Application.unregisterDisposableObject(this);Sys.Application.removeComponent(this)},_raiseDragStart:function(c,b){var a=this.get_events().getHandler("dragStart");if(a)a(this,b)},_raiseDragStop:function(c,b){var a=this.get_events().getHandler("dragStop");if(a)a(this,b)}};AjaxControlToolkit._DragDropManager.registerClass("AjaxControlToolkit._DragDropManager");AjaxControlToolkit.DragDropManager=new AjaxControlToolkit._DragDropManager;AjaxControlToolkit.IEDragDropManager=function(){var b=null,a=this;AjaxControlToolkit.IEDragDropManager.initializeBase(a);a._dropTargets=b;a._radius=10;a._useBuiltInDragAndDropFunctions=true;a._activeDragVisual=b;a._activeContext=b;a._activeDragSource=b;a._underlyingTarget=b;a._oldOffset=b;a._potentialTarget=b;a._isDragging=false;a._mouseUpHandler=b;a._documentMouseMoveHandler=b;a._documentDragOverHandler=b;a._dragStartHandler=b;a._mouseMoveHandler=b;a._dragEnterHandler=b;a._dragLeaveHandler=b;a._dragOverHandler=b;a._dropHandler=b};AjaxControlToolkit.IEDragDropManager.prototype={add_dragStart:function(a){this.get_events().addHandler("dragStart",a)},remove_dragStart:function(a){this.get_events().removeHandler("dragStart",a)},add_dragStop:function(a){this.get_events().addHandler("dragStop",a)},remove_dragStop:function(a){this.get_events().removeHandler("dragStop",a)},initialize:function(){var a=this;AjaxControlToolkit.IEDragDropManager.callBaseMethod(a,"initialize");a._mouseUpHandler=Function.createDelegate(a,a._onMouseUp);a._documentMouseMoveHandler=Function.createDelegate(a,a._onDocumentMouseMove);a._documentDragOverHandler=Function.createDelegate(a,a._onDocumentDragOver);a._dragStartHandler=Function.createDelegate(a,a._onDragStart);a._mouseMoveHandler=Function.createDelegate(a,a._onMouseMove);a._dragEnterHandler=Function.createDelegate(a,a._onDragEnter);a._dragLeaveHandler=Function.createDelegate(a,a._onDragLeave);a._dragOverHandler=Function.createDelegate(a,a._onDragOver);a._dropHandler=Function.createDelegate(a,a._onDrop)},dispose:function(){var a=this;if(a._dropTargets){for(var b=0;b<a._dropTargets;b++)a.unregisterDropTarget(a._dropTargets[b]);a._dropTargets=null}AjaxControlToolkit.IEDragDropManager.callBaseMethod(a,"dispose")},startDragDrop:function(c,b,h,f){var a=this,j=window._event;if(a._isDragging)return;a._underlyingTarget=null;a._activeDragSource=c;a._activeDragVisual=b;a._activeContext=h;a._useBuiltInDragAndDropFunctions=typeof f!="unefined"?f:true;var g={x:j.clientX,y:j.clientY};b.originalPosition=b.style.position;b.style.position="absolute";document._lastPosition=g;b.startingPoint=g;var k=a.getScrollOffset(b,true);b.startingPoint=a.addPoints(b.startingPoint,k);var d=parseInt(b.style.left),e=parseInt(b.style.top);if(isNaN(d))d="0";if(isNaN(e))e="0";b.startingPoint=a.subtractPoints(b.startingPoint,{x:d,y:e});a._prepareForDomChanges();c.onDragStart();var l=new AjaxControlToolkit.DragDropEventArgs(c.get_dragMode(),c.get_dragDataType(),c.getDragData(h)),i=a.get_events().getHandler("dragStart");if(i)i(a,l);a._recoverFromDomChanges();a._wireEvents();a._drag(true)},_stopDragDrop:function(c){var b=null,a=this,e=window._event;if(a._activeDragSource!=b){a._unwireEvents();if(!c)c=a._underlyingTarget==b;if(!c&&a._underlyingTarget!=b)a._underlyingTarget.drop(a._activeDragSource.get_dragMode(),a._activeDragSource.get_dragDataType(),a._activeDragSource.getDragData(a._activeContext));a._activeDragSource.onDragEnd(c);var d=a.get_events().getHandler("dragStop");if(d)d(a,Sys.EventArgs.Empty);a._activeDragVisual.style.position=a._activeDragVisual.originalPosition;a._activeDragSource=b;a._activeContext=b;a._activeDragVisual=b;a._isDragging=false;a._potentialTarget=b;e.preventDefault()}},_drag:function(g){var b=null,a=this,f=window._event,e={x:f.clientX,y:f.clientY};document._lastPosition=e;var h=a.getScrollOffset(a._activeDragVisual,true),c=a.addPoints(a.subtractPoints(e,a._activeDragVisual.startingPoint),h);if(!g&&parseInt(a._activeDragVisual.style.left)==c.x&&parseInt(a._activeDragVisual.style.top)==c.y)return;$common.setLocation(a._activeDragVisual,c);a._prepareForDomChanges();a._activeDragSource.onDrag();a._recoverFromDomChanges();a._potentialTarget=a._findPotentialTarget(a._activeDragSource,a._activeDragVisual);var d=a._potentialTarget!=a._underlyingTarget||a._potentialTarget==b;if(d&&a._underlyingTarget!=b)a._leaveTarget(a._activeDragSource,a._underlyingTarget);if(a._potentialTarget!=b)if(d){a._underlyingTarget=a._potentialTarget;a._enterTarget(a._activeDragSource,a._underlyingTarget)}else a._moveInTarget(a._activeDragSource,a._underlyingTarget);else a._underlyingTarget=b},_wireEvents:function(){var b="mousemove",a=this;if(a._useBuiltInDragAndDropFunctions){$addHandler(document,"mouseup",a._mouseUpHandler);$addHandler(document,b,a._documentMouseMoveHandler);$addHandler(document.body,"dragover",a._documentDragOverHandler);$addHandler(a._activeDragVisual,"dragstart",a._dragStartHandler);$addHandler(a._activeDragVisual,"dragend",a._mouseUpHandler);$addHandler(a._activeDragVisual,"drag",a._mouseMoveHandler)}else{$addHandler(document,"mouseup",a._mouseUpHandler);$addHandler(document,b,a._mouseMoveHandler)}},_unwireEvents:function(){var b="mousemove",a=this;if(a._useBuiltInDragAndDropFunctions){$removeHandler(a._activeDragVisual,"drag",a._mouseMoveHandler);$removeHandler(a._activeDragVisual,"dragend",a._mouseUpHandler);$removeHandler(a._activeDragVisual,"dragstart",a._dragStartHandler);$removeHandler(document.body,"dragover",a._documentDragOverHandler);$removeHandler(document,b,a._documentMouseMoveHandler);$removeHandler(document,"mouseup",a._mouseUpHandler)}else{$removeHandler(document,b,a._mouseMoveHandler);$removeHandler(document,"mouseup",a._mouseUpHandler)}},registerDropTarget:function(b){var a=this;if(a._dropTargets==null)a._dropTargets=[];Array.add(a._dropTargets,b);a._wireDropTargetEvents(b)},unregisterDropTarget:function(a){this._unwireDropTargetEvents(a);if(this._dropTargets)Array.remove(this._dropTargets,a)},_wireDropTargetEvents:function(c){var b=this,a=c.get_dropTargetElement();a._dropTarget=c;$addHandler(a,"dragenter",b._dragEnterHandler);$addHandler(a,"dragleave",b._dragLeaveHandler);$addHandler(a,"dragover",b._dragOverHandler);$addHandler(a,"drop",b._dropHandler)},_unwireDropTargetEvents:function(c){var b=this,a=c.get_dropTargetElement();if(a._dropTarget){a._dropTarget=null;$removeHandler(a,"dragenter",b._dragEnterHandler);$removeHandler(a,"dragleave",b._dragLeaveHandler);$removeHandler(a,"dragover",b._dragOverHandler);$removeHandler(a,"drop",b._dropHandler)}},_onDragStart:function(d){window._event=d;document.selection.empty();var c=d.dataTransfer;if(!c&&d.rawEvent)c=d.rawEvent.dataTransfer;var b=this._activeDragSource.get_dragDataType().toLowerCase(),a=this._activeDragSource.getDragData(this._activeContext);if(a){if(b!="text"&&b!="url"){b="text";if(a.innerHTML!=null)a=a.innerHTML}c.effectAllowed="move";c.setData(b,a.toString())}},_onMouseUp:function(a){window._event=a;this._stopDragDrop(false)},_onDocumentMouseMove:function(a){window._event=a;this._dragDrop()},_onDocumentDragOver:function(a){window._event=a;if(this._potentialTarget)a.preventDefault()},_onMouseMove:function(a){window._event=a;this._drag()},_onDragEnter:function(c){window._event=c;if(this._isDragging)c.preventDefault();else{var b=AjaxControlToolkit.IEDragDropManager._getDataObjectsForDropTarget(this._getDropTarget(c.target));for(var a=0;a<b.length;a++)this._dropTarget.onDragEnterTarget(AjaxControlToolkit.DragMode.Copy,b[a].type,b[a].value)}},_onDragLeave:function(c){window._event=c;if(this._isDragging)c.preventDefault();else{var b=AjaxControlToolkit.IEDragDropManager._getDataObjectsForDropTarget(this._getDropTarget(c.target));for(var a=0;a<b.length;a++)this._dropTarget.onDragLeaveTarget(AjaxControlToolkit.DragMode.Copy,b[a].type,b[a].value)}},_onDragOver:function(c){window._event=c;if(this._isDragging)c.preventDefault();else{var b=AjaxControlToolkit.IEDragDropManager._getDataObjectsForDropTarget(this._getDropTarget(c.target));for(var a=0;a<b.length;a++)this._dropTarget.onDragInTarget(AjaxControlToolkit.DragMode.Copy,b[a].type,b[a].value)}},_onDrop:function(c){window._event=c;if(!this._isDragging){var b=AjaxControlToolkit.IEDragDropManager._getDataObjectsForDropTarget(this._getDropTarget(c.target));for(var a=0;a<b.length;a++)this._dropTarget.drop(AjaxControlToolkit.DragMode.Copy,b[a].type,b[a].value)}c.preventDefault()},_getDropTarget:function(a){while(a){if(a._dropTarget!=null)return a._dropTarget;a=a.parentNode}return null},_dragDrop:function(){if(this._isDragging)return;this._isDragging=true;this._activeDragVisual.dragDrop();document.selection.empty()},_moveInTarget:function(a,b){this._prepareForDomChanges();b.onDragInTarget(a.get_dragMode(),a.get_dragDataType(),a.getDragData(this._activeContext));this._recoverFromDomChanges()},_enterTarget:function(a,b){this._prepareForDomChanges();b.onDragEnterTarget(a.get_dragMode(),a.get_dragDataType(),a.getDragData(this._activeContext));this._recoverFromDomChanges()},_leaveTarget:function(a,b){this._prepareForDomChanges();b.onDragLeaveTarget(a.get_dragMode(),a.get_dragDataType(),a.getDragData(this._activeContext));this._recoverFromDomChanges()},_findPotentialTarget:function(c){var a=this,f=window._event;if(a._dropTargets==null)return null;var j=c.get_dragDataType(),i=c.get_dragMode(),h=c.getDragData(a._activeContext),d=a.getScrollOffset(document.body,true),k=f.clientX+d.x,l=f.clientY+d.y,g={x:k-a._radius,y:l-a._radius,width:a._radius*2,height:a._radius*2},e;for(var b=0;b<a._dropTargets.length;b++){e=$common.getBounds(a._dropTargets[b].get_dropTargetElement());if($common.overlaps(g,e)&&a._dropTargets[b].canDrop(i,j,h))return a._dropTargets[b]}return null},_prepareForDomChanges:function(){this._oldOffset=$common.getLocation(this._activeDragVisual)},_recoverFromDomChanges:function(){var a=this,b=$common.getLocation(a._activeDragVisual);if(a._oldOffset.x!=b.x||a._oldOffset.y!=b.y){a._activeDragVisual.startingPoint=a.subtractPoints(a._activeDragVisual.startingPoint,a.subtractPoints(a._oldOffset,b));scrollOffset=a.getScrollOffset(a._activeDragVisual,true);var c=a.addPoints(a.subtractPoints(document._lastPosition,a._activeDragVisual.startingPoint),scrollOffset);$common.setLocation(a._activeDragVisual,c)}},addPoints:function(a,b){return {x:a.x+b.x,y:a.y+b.y}},subtractPoints:function(a,b){return {x:a.x-b.x,y:a.y-b.y}},getScrollOffset:function(b,e){var c=b.scrollLeft,d=b.scrollTop;if(e){var a=b.parentNode;while(a!=null&&a.scrollLeft!=null){c+=a.scrollLeft;d+=a.scrollTop;if(a==document.body&&(c!=0&&d!=0))break;a=a.parentNode}}return {x:c,y:d}},getBrowserRectangle:function(){var b=window.innerWidth,a=window.innerHeight;if(b==null)b=document.documentElement.clientWidth;if(a==null)a=document.documentElement.clientHeight;return {x:0,y:0,width:b,height:a}},getNextSibling:function(a){for(a=a.nextSibling;a!=null;a=a.nextSibling)if(a.innerHTML!=null)return a;return null},hasParent:function(a){return a.parentNode!=null&&a.parentNode.tagName!=null}};AjaxControlToolkit.IEDragDropManager.registerClass("AjaxControlToolkit.IEDragDropManager",Sys.Component);AjaxControlToolkit.IEDragDropManager._getDataObjectsForDropTarget=function(g){if(g==null)return [];var e=window._event,f=[],b=["URL","Text"],c;for(var a=0;a<b.length;a++){var d=e.dataTransfer;if(!d&&e.rawEvent)d=e.rawEvent.dataTransfer;c=d.getData(b[a]);if(g.canDrop(AjaxControlToolkit.DragMode.Copy,b[a],c))if(c)Array.add(f,{type:b[a],value:c})}return f};AjaxControlToolkit.GenericDragDropManager=function(){var b=null,a=this;AjaxControlToolkit.GenericDragDropManager.initializeBase(a);a._dropTargets=b;a._scrollEdgeConst=40;a._scrollByConst=10;a._scroller=b;a._scrollDeltaX=0;a._scrollDeltaY=0;a._activeDragVisual=b;a._activeContext=b;a._activeDragSource=b;a._oldOffset=b;a._potentialTarget=b;a._mouseUpHandler=b;a._mouseMoveHandler=b;a._keyPressHandler=b;a._scrollerTickHandler=b};AjaxControlToolkit.GenericDragDropManager.prototype={initialize:function(){var a=this;AjaxControlToolkit.GenericDragDropManager.callBaseMethod(a,"initialize");a._mouseUpHandler=Function.createDelegate(a,a._onMouseUp);a._mouseMoveHandler=Function.createDelegate(a,a._onMouseMove);a._keyPressHandler=Function.createDelegate(a,a._onKeyPress);a._scrollerTickHandler=Function.createDelegate(a,a._onScrollerTick);if(Sys.Browser.agent===Sys.Browser.Safari)AjaxControlToolkit.GenericDragDropManager.__loadSafariCompatLayer(a);a._scroller=new Sys.Timer;a._scroller.set_interval(10);a._scroller.add_tick(a._scrollerTickHandler)},startDragDrop:function(b,c,d){var a=this;a._activeDragSource=b;a._activeDragVisual=c;a._activeContext=d;AjaxControlToolkit.GenericDragDropManager.callBaseMethod(a,"startDragDrop",[b,c,d])},_stopDragDrop:function(a){this._scroller.set_enabled(false);AjaxControlToolkit.GenericDragDropManager.callBaseMethod(this,"_stopDragDrop",[a])},_drag:function(a){AjaxControlToolkit.GenericDragDropManager.callBaseMethod(this,"_drag",[a]);this._autoScroll()},_wireEvents:function(){$addHandler(document,"mouseup",this._mouseUpHandler);$addHandler(document,"mousemove",this._mouseMoveHandler);$addHandler(document,"keypress",this._keyPressHandler)},_unwireEvents:function(){$removeHandler(document,"keypress",this._keyPressHandler);$removeHandler(document,"mousemove",this._mouseMoveHandler);$removeHandler(document,"mouseup",this._mouseUpHandler)},_wireDropTargetEvents:function(){},_unwireDropTargetEvents:function(){},_onMouseUp:function(a){window._event=a;this._stopDragDrop(false)},_onMouseMove:function(a){window._event=a;this._drag()},_onKeyPress:function(a){window._event=a;var b=a.keyCode?a.keyCode:a.rawEvent.keyCode;if(b==27)this._stopDragDrop(true)},_autoScroll:function(){var a=this,c=window._event,b=a.getBrowserRectangle();if(b.width>0){a._scrollDeltaX=a._scrollDeltaY=0;if(c.clientX<b.x+a._scrollEdgeConst)a._scrollDeltaX=-a._scrollByConst;else if(c.clientX>b.width-a._scrollEdgeConst)a._scrollDeltaX=a._scrollByConst;if(c.clientY<b.y+a._scrollEdgeConst)a._scrollDeltaY=-a._scrollByConst;else if(c.clientY>b.height-a._scrollEdgeConst)a._scrollDeltaY=a._scrollByConst;if(a._scrollDeltaX!=0||a._scrollDeltaY!=0)a._scroller.set_enabled(true);else a._scroller.set_enabled(false)}},_onScrollerTick:function(){var d=document.body.scrollLeft,f=document.body.scrollTop;window.scrollBy(this._scrollDeltaX,this._scrollDeltaY);var c=document.body.scrollLeft,e=document.body.scrollTop,a=this._activeDragVisual,b={x:parseInt(a.style.left)+(c-d),y:parseInt(a.style.top)+(e-f)};$common.setLocation(a,b)}};AjaxControlToolkit.GenericDragDropManager.registerClass("AjaxControlToolkit.GenericDragDropManager",AjaxControlToolkit.IEDragDropManager);if(Sys.Browser.agent===Sys.Browser.Safari)AjaxControlToolkit.GenericDragDropManager.__loadSafariCompatLayer=function(a){a._getScrollOffset=a.getScrollOffset;a.getScrollOffset=function(){return {x:0,y:0}};a._getBrowserRectangle=a.getBrowserRectangle;a.getBrowserRectangle=function(){var b=a._getBrowserRectangle(),c=a._getScrollOffset(document.body,true);return {x:b.x+c.x,y:b.y+c.y,width:b.width+c.x,height:b.height+c.y}}};