<?php

function getRandomData($i = 16) {
    $iv = openssl_random_pseudo_bytes($i);
    $salt = base64_encode(bin2hex($iv));
    return [
        "iv" => $iv,
        "salt" => $salt
    ];
}

function encryptData($toBeHashed, $password, $salt, $iv) {
    $encryptHash = hash_pbkdf2("sha1", $password, $salt, 1500, 32, true);
    $encrypted = openssl_encrypt($toBeHashed, "aes-256-cbc", $encryptHash, 0, $iv);
    return $encrypted;
}

?>