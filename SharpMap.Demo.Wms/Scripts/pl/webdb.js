var webdb = {};

webdb._db = null;

webdb._onSuccess = function () {
};

webdb._onError = function (tx, e) {
    alert('something unexpected happened: ' + e.message);
};

webdb.open = function () {
    var dbSize = 5 * 1024 * 1024;
    webdb._db = openDatabase('sharpmap', '1.0', 'sharpmap localstorage', dbSize);
};

webdb.createTable = function () {
    webdb._db.transaction(function (tx) {
        tx.executeSql('CREATE TABLE IF NOT EXISTS sharpmap(ID INTEGER PRIMARY KEY ASC, url TEXT, json TEXT, added_on DATETIME)', []);
    });
};

webdb.dropTable = function () {
    webdb._db.transaction(function (tx) {
        tx.executeSql('DROP TABLE IF EXISTS sharpmap');
    });
};

webdb.add = function (url, json) {
    webdb._db.transaction(function (tx) {
        var addedOn = new Date();
        tx.executeSql('INSERT INTO sharpmap(url, json, added_on) VALUES (?,?,?)', [url, json, addedOn], webdb._onSuccess, webdb._onError);
    });
};

webdb.get = function (url, success) {    
    webdb._db.transaction(function (tx) {
        tx.executeSql('SELECT json FROM sharpmap WHERE url = ?', [url], success, webdb._onError);
    });
};