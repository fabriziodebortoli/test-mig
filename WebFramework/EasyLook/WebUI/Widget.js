/// <reference path="Persistence.js" />

/// <reference path="jquery.min.js" />

/// <reference path="jquery-ui.min.js" />

function Widget(name, title) {
	var tb$ = jQuery.noConflict();
	var _name = name;
	var _title = title;
	var _widget;
	var _persistenceManager;
	var _self = this;
	var _content = null;
	this.create = function (parent) {
	    _widget = tb$("<div id='" + name + "' class='tbWidget' style='position:absolute;'> <table>\
						<thead>\
							<tr><th class='tbWidgetTitle'> <span class='tbWidgetTitleLabel'/><img id='toggleView' src='icon_minimize.gif' class='tbToggleView'/></th></tr>\
						</thead>\
						<tbody>\
							<tr><td><div class='tbWidgetContent'></div></td></tr>\
						</tbody>\
					</table>\
					</div>")
			.appendTo(tb$(parent))
			.draggable({
			    stop: this.save
			})
            .resizable({ handles: "all", stop: this.save })
            .resize(function () {
                _widget.css({
                    height: 'auto'
                })
            });

	    if (name == "Report") {
	        tb$('<div id="dock"><div id="scroller"><ul></ul></div></div>').hide().appendTo(tb$(parent));
	    }
	    else {
	        var docImg = tb$('<img class="dockImg" src="docking.png" alt=""/>');
	        tb$('#' + name + ' .tbWidgetTitle').append(docImg);
	        tb$('<li><a href=#' + name + '> ' + name + ' </a></li>').appendTo(tb$('#dock ul'));
	    }
	    //Changes to docking mode and out
	    var dock = tb$('#dock');
	    tb$('.dockImg', _widget).toggle(
            function () {
                alert('docked');
                //Creates tabs for all widgets except the report
                tb$('.tbWidget').each(function () {
                    var name = tb$(this).attr('id');
                    if (name != "Report") {
                        dock.append(tb$(this));
                        tb$(this)
                            .draggable("disable")
                            .css({ opacity: '1',
                                left: '20px',
                                top: 'auto'
                            });
                    }
                    else {
                        tb$(this).css({
                            left: '200px',
                            top: '0px'
                        })
                    }
                });
                dock.addClass('ui-layout-west').tabs();
                var myLayout = tb$(parent).layout({
                    size: "300",
                    minSize: "300",
                    applyDefaultStyles: true
                })
                //Resizing inner content with resizer
                tb$('.ui-layout-toggler').remove();
                tb$('.ui-layout-resizer')
                    .draggable({
                        drag: function (event, ui) {
                            tb$('.tbWidget').each(function () {
                                var name = tb$(this).attr('id');
                                if (name != "Report") {
                                    if (tb$('.ui-draggable-dragging').position().left > 300) {
                                        tb$(this).css({
                                            width: (tb$('.ui-draggable-dragging').position().left - 100) + 'px'
                                        })
                                    }
                                }
                            })
                        }
                    });
            },
            function () {
                //there was no simple way to destroy the layout...
                alert('nodocked');
                //removes all widgets and places them back into the body
                tb$('#dock .tbWidget').each(function () {
                    var widget = tb$(this).detach().draggable("enable");
                    tb$(parent).append(tb$(widget));
                });
                //Destroy everything :)
                dock.tabs("destroy").removeClass().hide();
                tb$(parent).children().remove('span');
                tb$('#Report').css({
                    left: '0px',
                    top: '0px'
                })
            }
	   );

	    _self.setTitle(_title)
	    tb$(".tbToggleView", _widget).toggle(
			function () {
			    this.src = "icon_maximize.gif";
			    tb$(".tbWidgetContent", _widget).animate({
			        height: 'toggle'
			    }, 500, _self.save);
			},
			function () {
			    this.src = "icon_minimize.gif";
			    tb$(".tbWidgetContent", _widget).animate({
			        height: 'toggle'
			    }, 500, _self.save);
			}
		);
	}   

	this.setTitle = function (title) {
		_title = title;
		tb$(".tbWidgetTitleLabel", _widget).text(_title);
	}

	this.setPersistenceManager = function(mng){
		_persistenceManager = mng;
	}
	this.setWidth = function (width) {
	    _widget.css({
	        width: width + 'px'
	    });
	}

	this.setHeight = function (height) {
	    _widget.css({
	        height: height + 'px'
	    });
	}
	this.getName = function () { return _name; }
	this.save = function () {
		_persistenceManager.persistWidget(_self);
	}
	this.getState = function () {
		var pos = _widget.offset();
		return JSON.stringify({ "title": _title, "w": _widget.width(), "h": _widget.height(), "left": pos.left, "top":pos.top });
	}
	this.setState = function (jsonState) {
	    var state = JSON.parse(jsonState);
	    _self.setTitle(state.title);
	    _widget.css({
	        width: state.w + 'px',
	        height: state.h + 'px'
	    });
	    var pos = _widget.offset();
	    pos.left = state.left;
	    pos.top = state.top;
	    _widget.offset(pos);
	}
	this.setContent = function (content) {
		var contentArea = tb$(".tbWidgetContent", _widget);
		tb$(".tbWidgetContent", _widget).empty();
		_content = content;
		_content.create(contentArea);
	}

}