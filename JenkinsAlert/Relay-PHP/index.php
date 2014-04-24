<?php
$fp = fopen("./check.txt", 'r');
flock($fp, LOCK_SH);
$char = fread($fp, 999);
echo $char;
flock($fp, LOCK_UN);
fclose($fp);

?>