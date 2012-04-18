<?php
$client = new SoapClient('http://localhost:1234/Mikrokopter?wsdl');

if(isset($_GET['nick']))
{
$client->SetValue(array("_element" => "AngleNick", "_value" => $_GET['nick']));
$client->SetValue(array("_element" => "AngleRoll", "_value" => $_GET['roll']));
}
else
{
$client->SetValue(array("_element" => "AngleNick", "_value" => $_POST['nick']));
$client->SetValue(array("_element" => "AngleRoll", "_value" => $_POST['roll']));
}
?>
