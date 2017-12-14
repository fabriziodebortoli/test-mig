Type.registerNamespace("AjaxControlToolkit");AjaxControlToolkit.SlideShowBehavior=function(d){var c=false,b=null,a=this;AjaxControlToolkit.SlideShowBehavior.initializeBase(a,[d]);a._nextButtonID=b;a._previousButtonID=b;a._imageDescriptionLabelID=b;a._imageTitleLabelID=b;a._playButtonID=b;a._playButtonValue="||>";a._stopButtonValue="[]";a._slideShowServicePath=b;a._slideShowServiceMethod=b;a._contextKey=b;a._useContextKey=c;a._playInterval=3e3;a._tickHandler=b;a._loop=c;a._autoPlay=c;a._inPlayMode=c;a._elementImage=b;a._bNext=b;a._bPrevious=b;a._currentIndex=-1;a._currentValue=b;a._imageDescriptionLabel=b;a._imageTitleLabel=b;a._bPlay=b;a._slides=b;a._timer=b;a._currentImageElement=b;a._images=b;a._cachedImageIndex=-1;a._clickNextHandler=b;a._clickPreviousHandler=b;a._clickPlayHandler=b;a._tickHandler=b;a._imageLoadedHandler=b};AjaxControlToolkit.SlideShowBehavior.prototype={initialize:function(){var c="click",a=this;AjaxControlToolkit.SlideShowBehavior.callBaseMethod(a,"initialize");var b=a.get_element();a._elementImage=b;a._currentImageElement=document.createElement("IMG");a._currentImageElement.style.display="none";document.body.appendChild(a._currentImageElement);var d=document.createElement("DIV");b.parentNode.insertBefore(d,b);b.parentNode.removeChild(b);d.appendChild(b);d.align="center";a.controlsSetup();if(a._bNext){a._clickNextHandler=Function.createDelegate(a,a._onClickNext);$addHandler(a._bNext,c,a._clickNextHandler)}if(a._bPrevious){a._clickPreviousHandler=Function.createDelegate(a,a._onClickPrevious);$addHandler(a._bPrevious,c,a._clickPreviousHandler)}if(a._bPlay){a._clickPlayHandler=Function.createDelegate(a,a._onClickPlay);$addHandler(a._bPlay,c,a._clickPlayHandler)}a._imageLoadedHandler=Function.createDelegate(a,a._onImageLoaded);$addHandler(a._currentImageElement,"load",a._imageLoadedHandler);a._slideShowInit()},dispose:function(){var b=null,c="click",a=this;if(a._clickNextHandler){$removeHandler(a._bNext,c,a._clickNextHandler);a._clickNextHandler=b}if(a._clickPreviousHandler){$removeHandler(a._bPrevious,c,a._clickPreviousHandler);a._clickPreviousHandler=b}if(a._clickPlayHandler){$removeHandler(a._bPlay,c,a._clickPlayHandler);a._clickPlayHandler=b}if(a._imageLoadedHandler){$removeHandler(a._currentImageElement,"load",a._imageLoadedHandler);a._imageLoadedHandler=b}if(a._timer){a._timer.dispose();a._timer=b}AjaxControlToolkit.SlideShowBehavior.callBaseMethod(a,"dispose")},add_slideChanged:function(a){this.get_events().addHandler("slideChanged",a)},remove_slideChanged:function(a){this.get_events().removeHandler("slideChanged",a)},raiseSlideChanged:function(a){var b=this.get_events().getHandler("slideChanged");if(b){if(!a)a=Sys.EventArgs.Empty;b(this,a)}},add_slideChanging:function(a){this.get_events().addHandler("slideChanging",a)},remove_slideChanging:function(a){this.get_events().removeHandler("slideChanging",a)},raiseSlideChanging:function(c,d){var b=this.get_events().getHandler("slideChanging");if(b){var a=new AjaxControlToolkit.SlideShowEventArgs(c,d,this._currentIndex);b(this,a);return a.get_cancel()}return false},get_contextKey:function(){return this._contextKey},set_contextKey:function(b){var a=this;if(a._contextKey!=b){a._contextKey=b;a.set_useContextKey(true);if(a._elementImage)a._slideShowInit();a.raisePropertyChanged("contextKey")}},get_useContextKey:function(){return this._useContextKey},set_useContextKey:function(a){if(this._useContextKey!=a){this._useContextKey=a;this.raisePropertyChanged("useContextKey")}},get_enableCaching:function(){return this._enableCaching},set_enableCaching:function(a){this._enableCaching=a},get_completionListElementID:function(){return this._completionListElementID},set_completionListElementID:function(a){this._completionListElementID=a},get_completionListCssClass:function(){this._completionListCssClass},set_completionListCssClass:function(a){if(this._completionListCssClass!=a){this._completionListCssClass=a;this.raisePropertyChanged("completionListCssClass")}},get_completionListItemCssClass:function(){this._completionListItemCssClass},set_completionListItemCssClass:function(a){if(this._completionListItemCssClass!=a){this._completionListItemCssClass=a;this.raisePropertyChanged("completionListItemCssClass")}},get_highlightedItemCssClass:function(){this._highlightedItemCssClass},set_highlightedItemCssClass:function(a){if(this._highlightedItemCssClass!=a){this._highlightedItemCssClass=a;this.raisePropertyChanged("highlightedItemCssClass")}},get_delimiterCharacters:function(){return this._delimiterCharacters},set_delimiterCharacters:function(a){this._delimiterCharacters=a},controlsSetup:function(){var a=this;if(a._previousButtonID)a._bPrevious=document.getElementById(a._previousButtonID);if(a._imageDescriptionLabelID)a._imageDescriptionLabel=document.getElementById(a._imageDescriptionLabelID);if(a._imageTitleLabelID)a._imageTitleLabel=document.getElementById(a._imageTitleLabelID);if(a._nextButtonID)a._bNext=document.getElementById(a._nextButtonID);if(a._playButtonID){a._bPlay=document.getElementById(a._playButtonID);a._bPlay.value=a._playButtonValue}},resetButtons:function(){var c=false,b=true,a=this;if(!a._loop){if(a._slides.length<=a._currentIndex+1){if(a._bNext)a._bNext.disabled=b;if(a._bPlay)a._bPlay.disabled=b;if(a._bPrevious)a._bPrevious.disabled=c;a._inPlayMode=c;if(a._timer)a._timer.set_enabled(c);if(a._bPlay)a._bPlay.value=a._playButtonValue}else{if(a._bNext)a._bNext.disabled=c;if(a._bPlay)a._bPlay.disabled=c}if(a._currentIndex<=0){if(a._bPrevious)a._bPrevious.disabled=b}else if(a._bPrevious)a._bPrevious.disabled=c}else if(a._slides.length==0){if(a._bPrevious)a._bPrevious.disabled=b;if(a._bNext)a._bNext.disabled=b;if(a._bPlay)a._bPlay.disabled=b}if(a._inPlayMode){a._timer.set_enabled(c);a._timer.set_enabled(b)}},resetSlideShowButtonState:function(){var a=this;if(a._inPlayMode){if(a._bPlay)a._bPlay.value=a._stopButtonValue}else{a.resetButtons();if(a._bPlay)a._bPlay.value=a._playButtonValue}},setCurrentImage:function(){var a=this;if(a._slides[a._currentIndex])a._currentImageElement.src=a._slides[a._currentIndex].ImagePath;else a._currentImageElement.src="";if(Sys.Browser.agent==Sys.Browser.Opera)a._onImageLoaded(true)},updateImage:function(b){var a=this;if(b){if(a.raiseSlideChanging(a._currentValue,b))return;a._currentValue=b;a._elementImage.src=b.ImagePath;a._elementImage.alt=b.Name;if(a._imageDescriptionLabel)a._imageDescriptionLabel.innerHTML=b.Description?b.Description:"";if(a._imageTitleLabel)a._imageTitleLabel.innerHTML=b.Name?b.Name:"";a.raiseSlideChanged(b);a.resetButtons()}},get_imageDescriptionLabelID:function(){return this._imageDescriptionLabelID},set_imageDescriptionLabelID:function(a){if(this._imageDescriptionLabelID!=a){this._imageDescriptionLabelID=a;this.raisePropertyChanged("imageDescriptionLabelID")}},get_imageTitleLabelID:function(){return this._imageTitleLabelID},set_imageTitleLabelID:function(a){if(this._imageTitleLabelID!=a){this._imageTitleLabelID=a;this.raisePropertyChanged("imageTitleLabelID")}},get_nextButtonID:function(){return this._nextButtonID},set_nextButtonID:function(a){if(this._nextButtonID!=a){this._nextButtonID=a;this.raisePropertyChanged("nextButtonID")}},get_playButtonID:function(){return this._playButtonID},set_playButtonID:function(a){if(this._playButtonID!=a){this._playButtonID=a;this.raisePropertyChanged("playButtonID")}},get_playButtonText:function(){return this._playButtonValue},set_playButtonText:function(a){if(this._playButtonValue!=a){this._playButtonValue=a;this.raisePropertyChanged("playButtonText")}},get_stopButtonText:function(){return this._stopButtonValue},set_stopButtonText:function(a){if(this._stopButtonValue!=a){this._stopButtonValue=a;this.raisePropertyChanged("stopButtonText")}},get_playInterval:function(){return this._playInterval},set_playInterval:function(a){if(this._playInterval!=a){this._playInterval=a;this.raisePropertyChanged("playInterval")}},get_previousButtonID:function(){return this._previousButtonID},set_previousButtonID:function(a){if(this._previousButtonID!=a){this._previousButtonID=a;this.raisePropertyChanged("previousButtonID")}},get_slideShowServicePath:function(){return this._slideShowServicePath},set_slideShowServicePath:function(a){if(this._slideShowServicePath!=a){this._slideShowServicePath=a;this.raisePropertyChanged("slideShowServicePath")}},get_slideShowServiceMethod:function(){return this._slideShowServiceMethod},set_slideShowServiceMethod:function(a){if(this._slideShowServiceMethod!=a){this._slideShowServiceMethod=a;this.raisePropertyChanged("slideShowServiceMethod")}},get_loop:function(){return this._loop},set_loop:function(a){if(this._loop!=a){this._loop=a;this.raisePropertyChanged("loop")}},get_autoPlay:function(){return this._autoPlay},set_autoPlay:function(a){if(this._autoPlay!=a){this._autoPlay=a;this.raisePropertyChanged("autoPlay")}},_onClickNext:function(a){a.preventDefault();a.stopPropagation();this._clickNext()},_onImageLoaded:function(){var a=this;a.updateImage(a._slides[a._currentIndex]);a.resetButtons();a._cacheImages()},_clickNext:function(){var a=this;if(a._slides){if(a._currentIndex+1<a._slides.length)++a._currentIndex;else if(a._loop)a._currentIndex=0;else return false;a.setCurrentImage();return true}return false},_onClickPrevious:function(a){a.preventDefault();a.stopPropagation();this._clickPrevious()},_clickPrevious:function(){var a=this;if(a._slides){if(a._currentIndex-1>=0)--a._currentIndex;else if(a._loop)a._currentIndex=a._slides.length-1;else return false;a.setCurrentImage();return true}return false},_onClickPlay:function(a){a.preventDefault();a.stopPropagation();this._play()},_play:function(){var a=this;if(a._inPlayMode){a._inPlayMode=false;a._timer.set_enabled(false);a.resetSlideShowButtonState()}else{a._inPlayMode=true;if(!a._timer){a._timer=new Sys.Timer;a._timer.set_interval(a._playInterval);a._tickHandler=Function.createDelegate(a,a._onPlay);a._timer.add_tick(a._tickHandler)}a.resetSlideShowButtonState();a._timer.set_enabled(true)}},_onPlay:function(){var a=this;if(a._slides)if(a._currentIndex+1<a._slides.length){++a._currentIndex;a.setCurrentImage();return true}else if(a._loop){a._currentIndex=0;a.setCurrentImage();return true}else{a._inPlayMode=false;a.resetSlideShowButtonState()}return false},_slideShowInit:function(){var b=null,a=this;a._currentIndex=-1;a._cachedImageIndex=-1;a._inPlayMode=false;a._currentValue=b;a._images=b;var c=b;if(a._useContextKey)c={contextKey:a._contextKey};Sys.Net.WebServiceProxy.invoke(a._slideShowServicePath,a._slideShowServiceMethod,false,c,Function.createDelegate(a,a._initSlides),b,b)},_initSlides:function(b){var a=this;a._slides=b;if(a._slides){a._images=[];a._clickNext();if(a._autoPlay)a._play()}},_cacheImages:function(){var a=this;if(a._currentIndex%3==0){var c=a._cachedImageIndex;for(var b=a._cachedImageIndex+1;b<a._slides.length;b++)if(a._slides[b]){a._images[b]=new Image;a._images[b].src=a._slides[b].ImagePath;a._cachedImageIndex=b;if(c+4<=b)break}}}};AjaxControlToolkit.SlideShowBehavior.registerClass("AjaxControlToolkit.SlideShowBehavior",AjaxControlToolkit.BehaviorBase);AjaxControlToolkit.SlideShowEventArgs=function(b,d,c){var a=this;AjaxControlToolkit.SlideShowEventArgs.initializeBase(a);a._previousSlide=b;a._nextSlide=d;a._slideIndex=c};AjaxControlToolkit.SlideShowEventArgs.prototype={get_previousSlide:function(){return this._previousSlide},get_nextSlide:function(){return this._nextSlide},get_slideIndex:function(){return this._slideIndex}};AjaxControlToolkit.SlideShowEventArgs.registerClass("AjaxControlToolkit.SlideShowEventArgs",Sys.CancelEventArgs);