var sharpmap = {};

sharpmap.queue = (function () {
    var queued = [], active = 0, size = 6;

    function init() {
        webdb.check();
    }

    function process() {
        if ((active >= size) || !queued.length)
            return;

        active++;
        queued.pop()();
    }

    function dequeue(send) {
        for (var i = 0; i < queued.length; i++) {
            if (queued[i] == send) {
                queued.splice(i, 1);
                return true;
            }
        }
        return false;
    }

    function request(url, callback, mimeType) {
        var req;

        function send() {
            req = new XMLHttpRequest();
            if (mimeType && req.overrideMimeType) {
                req.overrideMimeType(mimeType);
            }
            req.open("GET", url, true);
            req.onreadystatechange = function () {
                if (req.readyState == 4) {
                    active--;
                    if (req.status < 300)
                        callback(req);
                    process();
                }
            };
            req.send(null);
        }

        function abort(hard) {
            if (dequeue(send))
                return true;

            if (hard && req) {
                req.abort();
                return true;
            }
            return false;
        }

        queued.push(send);
        process();
        return { abort: abort };
    }

    function json(url, callback) {
        var data = webdb.get(url), parse;
        if (data) {
            parse = JSON.parse(data);
            console.log('retrieved from webdb ' + url);
            return callback(parse);
        }

        return request(url, function (req) {
            var text;
            if (req.responseText) {
                text = req.responseText;
                webdb.add(url, text);
                data = JSON.parse(text);
                console.log('added to webdb ' + url);
                callback(data);
            }
        }, "application/json");
    }

    return { init: init, json: json };
})();
