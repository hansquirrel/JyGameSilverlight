<?php session_start();

function ban($info)
{
  $banlist = array("`","'","\"","#","阴道","强奸","鸡巴","妓女","一夜情","SM","裸","广告","操","做爱","政府","共产党","湿润","小穴","乳","强暴","性","xing","zuo","外挂","作弊","修改","激情","交友","cao","打炮","干炮","淫");
  foreach($banlist as $banword){
        if(strstr($info,$banword))
        {
                return true;
        } 
  }
  return false;
}

$conn=mysqli_connect("localhost","root","KOFcgwin123","jygame");
$conn->query('set names utf8');
if(empty($conn))
{
   die("");
}

$danmu=urldecode($_GET['danmu']);
if($danmu!="" && !ban($danmu)){
  $sql="INSERT INTO jy_danmu VALUES(null,'$danmu', CURRENT_TIMESTAMP)";
}
$result=$conn->query($sql);
$conn->close();
?>
