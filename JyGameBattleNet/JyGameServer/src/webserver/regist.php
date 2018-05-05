<html> 
<head> 
<title>申请帐号</title> 
</head>


<form method="post" action="insert.php" name="send" onSubmit="return Check()"> 
      
      <td width="210" class="p11" valign="bottom"><font color="#FF6699">*</font>为必填项</td> 
          <p>用户名(只能是英文或数字)：         
               
                    <input type="text" name="username" size="20" class="c3a">
                    <font color="#FF6699">*</font>
     </p>
          <p>密码：         
                 
                    <input type="password" name="password" size="20" class="c3a"> 
            <font color="#FF6699">*</font></p>
          <p>确认密码：
            <input name="cpassword" type="password" id="cpassword">
            <font color="#FF6699">*</font></p>
                    <p>
                      <input type="submit" value="下一步" > 
                      <input type="reset" value="重来"> 
    </p>
</form> 
<script language="javascript">
function Check()// 验证表单数据有效性的函数
{
    if (document.send.username.value=="") 
    {
        window.alert('请输入用户名!'); 
        
        return false;
    }
    if (document.send.username.value.length<4) 
    {
        window.alert('用户名长度必须大于4!'); 
        
        return false;
    }
    if (document.send.password.value=="") 
    {
        alert('请输入密码!'); 
        
        return false;
    }
    if (document.send.password.value!= document.send.cpassword.value) 
    {
        alert('确认密码与密码不一致!'); 
        return false;
    }
    return true;
}
</script>
</body> 
</html>
