function callcanvas() {
	return false;
}
function calltask() {
	return true;
}

function widgetClass(id) {
	var wid = id;

	this.webpage = function () {
		var sHtml = "";
		sHtml += '<body>';
		sHtml += '<div class="chatbox" id="' + wid + '"></div>';
		sHtml += '<div class="chatInput">';
		sHtml += '<input name="usermsg"   type="text"    id="' + wid + '_usermsg"   size="50" />';
		sHtml += '<input name="submitmsg" type="submit"  id="' + wid + '_submitmsg" value="Send" /> ';
		sHtml += '</div>';
		sHtml += '</body>';
		return sHtml;
	}

	this.start = function () {
		jq$("#" + wid + "_usermsg").keypress(function (e) {
			if (e.which == 13) {
				updateChat(wid);
			}
		});

		jq$("#" + wid + "_submitmsg").click(function () {
			updateChat(wid);
		});

		updateChat(wid);
		return true;
	}

	function updateChat() {
		jq$.post(
				"widget/Chat/chat.aspx",
				{ data: jq$("#" + wid + "_usermsg").val(),
					nChat: -1
				},
				function (data) {
				},
				"json"
			);

				jq$("#" + wid + "_usermsg").val("");
				jq$("#" + wid).animate({
				scrollTop: '+=50'
		},
	1500);
	}

	return this;
}



function widgetcanvas(id) {
	var wid = id;

	this.preCanvas = function () {

		return true;
	}

	this.postCanvas = function () {

		return true;
	}

	return this;
}

function task(id) {
	var wid = id;
	var startChatId = -1;

	function refreshChat() {
		jq$.post(
			"widget/Chat/chat.aspx",
			{ data: "",
				nChat: startChatId
			},
			function (data) {
				//console.log(startChatId);

				if (data.message != "" || startChatId == -1) {
					startChatId = data.nLine;
					jq$("#" + wid).append(data.message);
				}
			},
			"json"
		);

		jq$("#" + wid).animate({
				scrollTop: '+=50'
			},
		1500);
	}


	this.run = function () {
		refreshChat();
		return true;
	}

	return this;
}