var webdb = { };

webdb._db = null;

webdb.check = function () {
    var ls = window.localStorage;
    if (!ls) {
        alert('local storage unavailable :(');
    }
};

webdb.add = function (url, json) {
    var ls = window.localStorage;
    if (ls) {
        try {
            ls.setItem(url, json);
        } catch (e) {
            alert(e);
        }
    }
};

webdb.get = function (url) {
    var ls = window.localStorage;
    return ls ? ls.getItem(url) : null;
};

webdb.clear = function () {
    var ls = window.localStorage;
    if (ls) {
        ls.clear();
    }
};