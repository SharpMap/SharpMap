(function () {
    var events = OpenLayers.Events;
    var root;
    var global = this;
    if (typeof ProvideCustomRxRootObject == "undefined") {
        root = global.Rx;
    }
    else {
        root = ProvideCustomRxRootObject();
    }

    var observable = root.Observable;
    var observableCreate = observable.Create;

    events.prototype.toObservable = function (eventType) {
        var parent = this;
        var scope = parent.object;
        return observableCreate(function (observer) {
            var handler = function (obj) {
                try {
                    observer.OnNext(obj);
                }
                catch (err) {
                    observer.OnError(err);
                }
            }
            parent.register(eventType, scope, handler);
            return function () {
                parent.unregister(eventType, scope, handler);
            };
        });
    };
})();