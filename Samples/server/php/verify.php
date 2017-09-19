<?php

function testHmac($response) {
    require_once('./config.php');
    $resp = json_decode($response);
    $hmac = base64_encode(hash_hmac('sha512', $resp->data, $client['KEY'], true));
    return json_encode([ 'data' => $resp->data, 'received' => $resp->hash, 'calculated' => $hmac, 'isMatch' => ($resp->hash === $hmac) ]);
}

?>