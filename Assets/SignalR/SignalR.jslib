var SignalRLib = {

    $vars: {
        connection: null,
        lastConnectionId: '',
        connectedCallback: null,
        disconnectedCallback: null,
        handlerCallback: null,
        responseCallback: null,
        UTF8ToString: function (arg) {
            return (typeof Pointer_stringify === 'undefined') ? UTF8ToString(arg) : Pointer_stringify(arg);
        },
        invokeCallback: function (args, callback) {
            var sig = 'v';
            var messages = [];
            for (var i = 0; i < args.length; i++) {
                var message = args[i];
                var bufferSize = lengthBytesUTF8(message) + 1;
                var buffer = _malloc(bufferSize);
                stringToUTF8(message, buffer, bufferSize);
                messages.push(buffer);
                sig += 'i';
            }
            if (typeof Runtime === 'undefined') {
                dynCall(sig, callback, messages);
            } else {
                Runtime.dynCall(sig, callback, messages);
            }
        }
    },

    InitJs: function (url, accessToken) {
        url = vars.UTF8ToString(url);
        accessToken = vars.UTF8ToString(accessToken);
        vars.connection = new signalR.HubConnectionBuilder()
            .withUrl(url, {
                accessTokenFactory: () => accessToken,
                transport: signalR.HttpTransportType.LongPolling,
                logger: signalR.LogLevel.Debug
            })
            .build();
    },

    ConnectJs: function (connectedCallback, disconnectedCallback) {
        vars.connectedCallback = connectedCallback;
        vars.disconnectedCallback = disconnectedCallback;
        vars.connection.start()
            .then(function () {
                vars.lastConnectionId = vars.connection.connectionId;
                vars.connection.onclose(function (err) {
                    if (err) {
                        console.error('Connection closed due to error: "' + err.toString() + '".');
                    }
                    vars.invokeCallback([vars.lastConnectionId], vars.disconnectedCallback);
                });
                vars.connection.onreconnecting(function (err) {
                    console.log('Connection lost due to error: "' + err.toString() + '". Reconnecting.');
                });
                vars.connection.onreconnected(function (connectionId) {
                    console.log('Connection reestablished. Connected with connectionId: "' + connectionId + '".');
                    vars.lastConnectionId = connectionId;
                    vars.invokeCallback([vars.lastConnectionId], vars.connectedCallback);
                });
                vars.invokeCallback([vars.lastConnectionId], vars.connectedCallback);
            }).catch(function (err) {
            return console.error(err.toString());
        });
    },

    StopJs: function () {
        if (vars.connection) {
            vars.connection.stop()
                .catch(function (err) {
                    return console.error(err.toString());
                });
        }
    },

    StartOrRecoverSessionJs: function (traceParent, responseCallback) {
        vars.responseCallback = responseCallback;
        traceParent = vars.UTF8ToString(traceParent);
        vars.connection.invoke("StartOrRecoverSession", {"traceParent": traceParent})
            .then(value => {
                value = JSON.stringify(value);
                console.log(value);
                vars.invokeCallback([value], vars.responseCallback);
            }).catch(error => console.log(error));
    },

    InvokeJs: function (methodName, arg1) {
        methodName = vars.UTF8ToString(methodName);

        arg1 = vars.UTF8ToString(arg1);
        console.log("Invoking " + methodName + " with " + arg1);
        vars.connection.invoke(methodName, arg1)
            .catch(error => console.error(error));
    },

    OnJs: function (methodName, argCount, callback) {
        methodName = vars.UTF8ToString(methodName);

        vars.handlerCallback = callback;
        vars.connection.on(methodName, function (arg) {
            arg = JSON.stringify(arg);
            console.log(arg);
            vars.invokeCallback([methodName, arg], vars.handlerCallback);
        });
    }
};

autoAddDeps(SignalRLib, '$vars');
mergeInto(LibraryManager.library, SignalRLib);