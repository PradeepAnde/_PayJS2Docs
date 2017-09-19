<?php

function getAuthKey($str = '') {
    
    require_once('./config.php');
    require_once('./encrypt.php');

    $req = json_decode($str);
    // do some validation here to make sure the request hasn't been tampered with
    // (eg, you dont want to authorize a $1 payment when it should be $100)
    
    $authInit = getRandomData();
    
    $req->auth->merchantId = $merchant['ID'];
    $req->auth->merchantKey = $merchant['KEY'];
    $req->auth->clientId = $client['ID'];
    $req->auth->requestId = (string)time();
    $req->auth->salt = $authInit['salt'];
    
    // you can override request data, if you want:
    // $req->payment->totalAmount = '1.01';

    // or add new features:
    // $req->custom = [ 'someNewCustomData' => 'this was added by the server during client-first auth' ];
    
    $authKey = encryptData(
        json_encode($req),
        $client['KEY'],
        $authInit['salt'],
        $authInit['iv']
    );
    
    $req->auth->authKey = $authKey;
    unset($req->auth->merchantKey); // <-- sensitive!
    
    // and return at least the new 'auth' object to the client.
    // anything else is optional, and will override client data
    return json_encode([ 'auth' => $req->auth ]);
    //return json_encode([ 'auth' => $req->auth, 'payment' => $req->payment, 'custom' => $req->custom ]);

}

?>