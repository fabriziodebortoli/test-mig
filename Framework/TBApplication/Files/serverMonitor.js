var authTokenString = "authtoken";
var cookies;
function readCookie(name) {
    var c, C, i;
    if (cookies) { 
        return cookies[name]; 
    }

    c = document.cookie.split('; ');
    cookies = {};

    for (i = c.length - 1; i >= 0; i--) {
        C = c[i].split('=');
        cookies[C[0]] = C[1];
    }

    return cookies[name];
}

var authToken = readCookie(authTokenString)

$.get("getLoginActiveThreads/", { authTokenString: authToken }, function (jsonResponse) {
    var outputPanel = $("#panel1");
    outputPanel.append(json2html(jsonResponse));
});



function json2html(json) {
    var i, ret = "";
    ret += "<ul>";
    for (i in json) {
        ret += "<li>" + i + ": ";
        if (typeof json[i] === "object")
            ret += json2html(json[i]);
        else ret += json[i];
        ret += "</li>";
    }
    ret += "</ul>";
    return ret;
}
