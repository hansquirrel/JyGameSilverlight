<?php 
header("Content-type: text/html; charset=utf-8"); 
session_start();
$conn=mysqli_connect("localhost","root","KOFcgwin123","jygame");
$conn->query('set names utf8');
if(empty($conn))
{
   die("error");
}
//$str="select * from jy_danmu where `time`>=NOW() - interval 2 hour order by rand() limit 12";
$str="select * from jy_danmu where id>(select max(id) from jy_danmu) -1000 order by rand() limit 10";

$result1=$conn->query($str);

while($row=$result1->fetch_row())
{
  echo "$row[0]#SPLIT#$row[1]#DANMU#";
}

$str="select * from jy_danmu where `time` >= NOW() - interval 300 second order by rand() limit 5";

$result1=$conn->query($str);

while($row=$result1->fetch_row())
{
  echo "$row[0]#SPLIT#$row[1]#DANMU#";
}

$conn->close();
?>
