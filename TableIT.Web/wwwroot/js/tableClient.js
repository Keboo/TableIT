var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
(function (factory) {
    if (typeof module === "object" && typeof module.exports === "object") {
        var v = factory(require, exports);
        if (v !== undefined) module.exports = v;
    }
    else if (typeof define === "function" && define.amd) {
        define(["require", "exports", "@microsoft/signalr"], factory);
    }
})(function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var signalR = require("@microsoft/signalr");
    connection: signalR.HubConnection;
    function generateJwt() {
        // the header typically consists of two parts:
        // the type of the token, which is jwt, and the signing algorithm being used,
        // such as hmac sha256 or rsa.
        var header = {
            "alg": "none",
            "typ": "jwt",
        };
        var encodedheaders = btoa(JSON.stringify(header));
        // remove padding equal characters
        encodedheaders = encodedheaders.replace(/=+$/, '');
        // replace characters according to base64url specifications
        encodedheaders = encodedheaders.replace(/\+/g, '-');
        encodedheaders = encodedheaders.replace(/\//g, '_');
        var iat = Math.floor(Date.now() / 1000);
        // the second part of the token is the payload, which contains the claims.
        // claims are statements about an entity (typically, the user) and
        // additional data. there are three types of claims:
        // registered, public, and private claims.
        var claims = {
            "tableid": "@model.tableid",
            "nbf": iat,
            "exp": iat + (60 * 60 * 1),
            "iat": iat
        };
        var encodedplayload = btoa(JSON.stringify(claims));
        // remove padding equal characters
        encodedplayload = encodedplayload.replace(/=+$/, '');
        // replace characters according to base64url specifications
        encodedplayload = encodedplayload.replace(/\+/g, '-');
        encodedplayload = encodedplayload.replace(/\//g, '_');
        var jwt = "".concat(encodedheaders, ".").concat(encodedplayload, ".");
        console.log(jwt);
        return jwt;
    }
    function startAsync(endpoint, tableId) {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                this.connection = new signalR.HubConnectionBuilder()
                    .withUrl(endpoint, {
                    accessTokenFactory: function () {
                        return generateJwt();
                    }
                })
                    .configureLogging(signalR.LogLevel.Information)
                    .build();
                try {
                    return [2 /*return*/, this.connection.start()];
                }
                catch (err) {
                    console.error(err);
                    setTimeout(startAsync, 5000);
                }
                return [2 /*return*/];
            });
        });
    }
});
//    const endpoint = "https://tableitfunctions.azurewebsites.net/api";
//        var configMessageId = createGuid();
//const connection = new signalR.HubConnectionBuilder()
//    .withUrl(endpoint, {
//        accessTokenFactory: function () {
//            return generateJwt()
//        }
//    })
//    .configureLogging(signalR.LogLevel.Information)
//    .build();
//async function start() {
//    try {
//        setStatus("Connecting...");
//        await connection.start();
//        setStatus("Connected. Getting info...");
//        //Get the current config
//        connection.invoke('requestmessage', JSON.stringify({
//            "GroupId": configMessageId,
//            "Data": "{}",
//            "DataType": "TableIT.Core.Messages.TableConfigurationRequest"
//        }));
//    } catch (err) {
//        console.log(err);
//        setTimeout(start, 5000);
//    }
//};
//function setStatus(message) {
//    var status = document.getElementById("status");
//    status.innerText = message;
//}
//connection.onclose(async () => {
//    await start();
//});
//connection.on('responsemessage', (msg) => {
//    console.log("Got message");
//    console.log(msg);
//    var message = JSON.parse(msg);
//    if (message.GroupId == configMessageId) {
//        var payload = JSON.parse(message.Data);
//        if (payload.Config.CurrentResourceId) {
//            setStatus("Loading image");
//            var image = document.getElementById("resourceImage");
//            image.src = `${endpoint}/resources/${payload.Config.CurrentResourceId}`
//        } else {
//            setStatus("Failed to get image");
//        }
//        configMessageId = null;
//    }
//});
//function createGuid() {
//    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
//        var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
//        return v.toString(16);
//    });
//}
//# sourceMappingURL=tableClient.js.map