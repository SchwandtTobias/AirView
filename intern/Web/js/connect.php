<?php
$client = new SoapClient('http://localhost:1234/Mikrokopter?wsdl');
$client->ConnectWithPort(array("_port" => "COM7"));
?>
