<?php
$data = $_GET['status'];
$fp = fopen("./check.txt", 'w');
flock($fp, LOCK_EX);
fwrite($fp, $data);
flock($fp, LOCK_UN);
fclose($fp);

echo $data."-ok";
?>