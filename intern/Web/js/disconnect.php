<?php
$client = new SoapClient('http://localhost:1234/Mikrokopter?wsdl');
$client->Disconnect();
?>
