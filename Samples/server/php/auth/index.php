<?php

// cors headers
header("Access-Control-Allow-Origin: *"); // <-- not for prod!
header("Access-Control-Allow-Methods: POST");
header("Access-Control-Allow-Headers: clientid, clientversion, content-type");

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    // short-circuit the pre-flight
    return;
} else if ($_SERVER['REQUEST_METHOD'] === 'GET') {
    // this is handy for environment debugging
    echo phpinfo();
} else {

    require('../config.php');
    require('./encrypt.php');

    $req = json_decode(file_get_contents('php://input'));
    // do some validation here to make sure the request hasn't been tampered with
    // (eg, you dont want to authorize a $1 payment when it should be $100)

    $authInit = getRandomData();

    $req->auth->merchantId = $merchant['ID'];
    $req->auth->merchantKey = $merchant['KEY'];
    $req->auth->clientId = $client['ID'];
    $req->auth->requestId = (string)time();
    $req->auth->salt = $authInit['salt'];

    // you can override request data, if you want:
    $req->payment->totalAmount = '1.01';
    // or add new features:
    $req->custom = [ 'someNewCustomData' => 'this was added by the server during client-first auth' ];

    $authKey = encryptData(
        json_encode($req),
        $client['KEY'],
        $authInit['salt'],
        $authInit['iv']
    );

    $req->auth->authKey = $authKey;
    unset($req->auth->merchantKey); // <-- sensitive!

    header("Content-Type: application/json");
    // and return at least the new 'auth' object to the client.
    // anything else is optional, and will override client data
    echo json_encode(['auth' => $req->auth, 'payment' => $req->payment, 'custom' => $req->custom]);
}?>