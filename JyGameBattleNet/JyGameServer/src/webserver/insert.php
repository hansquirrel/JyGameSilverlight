<?php session_start();
$conn=mysqli_connect("localhost","root","KOFcgwin123","jygame");
$conn->query('set names utf-8');
if(empty($conn))
{
   die("数据库连接失败");
}
$username=$_POST['username'];
$password=$_POST['password'];
$str="select * from jy_user where name='".$username."'";
   $result1=$conn->query($str);
   $row=$result1->fetch_row();
if($row)
{ 
$temp="已有人注册此名，请重新选择名字!"; 
echo $temp;
echo"<a href=regist.php>返回</a>";
} 
else {
if(true)
{
   $sql="INSERT INTO jy_user VALUES(null,'$username', '$password',1000,null,null,0,0)";
   $result=$conn->query($sql);
   if($result==true)
      {
      echo "注册成功";
      }
      else {echo "注册失败".mysql_error();}
} 
}
?>
