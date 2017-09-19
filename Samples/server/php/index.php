<?php
header("Content-Type: application/json");
header("Access-Control-Allow-Origin: *"); // <-- not for prod!
header("Access-Control-Allow-Methods: POST");
header("Access-Control-Allow-Headers: clientid, clientversion, content-type");

require_once('./auth.php');
require_once('./verify.php');

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS')
{
    return; // short-circuit the pre-flight
}
else if ($_SERVER['REQUEST_METHOD'] === 'GET')
{
    echo phpinfo(); // handy for environmental debugging
}
else if ($_SERVER['REQUEST_URI'] === '/auth')
{
    echo getAuthKey(file_get_contents('php://input'));
}
else if ($_SERVER['REQUEST_URI'] === '/verify')
{
    echo testHmac(file_get_contents('php://input'));
}
else
{
    echo $_SERVER['REQUEST_URI'];
}

?>