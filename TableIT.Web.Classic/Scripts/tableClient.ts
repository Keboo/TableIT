import * as signalR from "@microsoft/signalr";

connection: signalR.HubConnection;

function generateJwt() {

    // the header typically consists of two parts:
    // the type of the token, which is jwt, and the signing algorithm being used,
    // such as hmac sha256 or rsa.
    const header = {
        "alg": "none",
        "typ": "jwt",
    }
    var encodedheaders = btoa(JSON.stringify(header))

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
    const claims = {
        "tableid": "@model.tableid",
        "nbf": iat,
        "exp": iat + (60 * 60 * 1),
        "iat": iat
    }
    var encodedplayload = btoa(JSON.stringify(claims))
    // remove padding equal characters
    encodedplayload = encodedplayload.replace(/=+$/, '');

    // replace characters according to base64url specifications
    encodedplayload = encodedplayload.replace(/\+/g, '-');
    encodedplayload = encodedplayload.replace(/\//g, '_');

    const jwt = `${encodedheaders}.${encodedplayload}.`
    console.log(jwt)

    return jwt
}

async function startAsync(endpoint: string, tableId: string) {
    this.connection = new signalR.HubConnectionBuilder()
        .withUrl(endpoint, {
        accessTokenFactory: function () {
            return generateJwt()
        }
    })
    .configureLogging(signalR.LogLevel.Information)
    .build();

    try {
        return this.connection.start();
    } catch (err) {
        console.error(err)
        setTimeout(startAsync, 5000);
    }
}



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