#encoding=utf8
import MySQLdb
import ConfigParser
from log import log

def getConnect():
    cf = ConfigParser.ConfigParser()  
    cf.read("config.ini")
    db_ip = cf.get("db", "ip")
    db_port = int(cf.get("db", "port"))
    db_user = cf.get("db","user")
    db_password = cf.get("db","password")
    db_db = cf.get("db","db")
    
    log("connect to database %s:%d" % (db_ip, db_port))
    
    conn=MySQLdb.connect(host=db_ip, user=db_user, passwd=db_password, db=db_db, port=db_port, charset='utf8')
    log("connect success!")
    return conn

def testConnect(conn):
    if(conn==None):
        conn = getConnect()
    else:
        try:
            cur = conn.cursor()
            cur.execute("select id from jy_user limit 1")
            cur.close()
        except:
            conn = getConnect()
        

def getUser(name, conn):
    try:
        cur = conn.cursor()
        cur.execute("select id,name,password,score,createtime,updatetime from jy_user where name='%s'"%name)
        return cur.fetchone()
    except:
        return None
    
def updateScore(name, score, conn):
    try:
        cur = conn.cursor()
        cur.execute("update jy_user set score=%d where name='%s'"%(score,name))
        cur.close()
    except:
        return None

def getSave(name, index, conn):
    try:
        cur = conn.cursor()
        cur.execute("SELECT jy_save.content,jy_save.createtime,jy_save.updatetime FROM jy_save ,jy_user WHERE jy_save.userid =  jy_user.id AND jy_user.name =  '%s' AND jy_save.`index` =  '%d'"%(name, index))
        return cur.fetchone()
    except:
        return None

def getSaves(name, conn):
    try:
        cur = conn.cursor()
        cur.execute("SELECT jy_save.index,jy_save.content,jy_save.createtime,jy_save.updatetime FROM jy_save ,jy_user WHERE jy_save.userid =  jy_user.id AND jy_user.name =  '%s' ORDER BY jy_save.index ASC"%(name))
        return cur.fetchall()
    except:
        return None
    
def save(name, index, content, conn):
    try:
        cur = conn.cursor()
        userid = getUser(name,conn)[0]
        msg = "UPDATE jy_save SET content = '%s' where userid=%s and `index`=%s"%(content, userid, index)
        cur.execute(msg)
        print msg
        return True
    except:
        return False
    
if __name__ == "__main__":
    con = getConnect()
    print getUser("admin", con)
    print getSave("admin", 1 , con)
    print getSaves("admin", con)
    con.close()