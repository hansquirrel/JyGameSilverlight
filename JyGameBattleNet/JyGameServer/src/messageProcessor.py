#encoding=utf8
import db
import time
MESSAGE_SPLIT_TAG = "#TAG#"
MESSAGE_END_TAG = "#ENDTAG#"

DATA_BUFFER = {}

def send(socket,msg):
    msg += MESSAGE_END_TAG
    socket.send(msg.encode("utf-8"))

def process_buffer(data , conn, socket, runtimeData):
    if not(socket in runtimeData["buffer"].keys()):
        runtimeData["buffer"][socket] = ""
    
    runtimeData["buffer"][socket] += data
    tt = runtimeData["buffer"][socket].split(MESSAGE_END_TAG)
    tobekicked = []
    
    if(len(tt)>1):
        for i in range(0,len(tt)-1):
            t = tt[i]
            kicklist = process_msg(t , conn, socket, runtimeData)
            for k in kicklist:
                if k not in tobekicked:
                    tobekicked.append(k)
        runtimeData["buffer"][socket] = tt[len(tt)-1]
    return tobekicked

def process_msg(msg , conn, socket, runtimeData):
    paras = msg.split(MESSAGE_SPLIT_TAG)
    msg_head = paras[0]
    tobekicked = []
    
    if(msg_head == "LOGIN"):
        user = paras[1]
        password = paras[2]
        u = db.getUser(user, conn)
        if(u != None and u[2] == password):
            send(socket,"LOGIN" + MESSAGE_SPLIT_TAG + "true")
            runtimeData["online"][socket] = u
            #踢掉当前在线的
            for u in runtimeData["online"]:
                username = runtimeData["online"][u][1]
                if(username==user and u != socket):
                    tobekicked.append(u)
        else:
            send(socket,"LOGIN" + MESSAGE_SPLIT_TAG + "false")
            tobekicked.append(socket)
            
    if(msg_head == "LOGOUT"):
        runtimeData["online"].remove(socket)
        tobekicked.append(socket)
    
    if(msg_head == "JOIN_CHANNEL"):
        channels = paras[1].split("#")
        runtimeData["channel"][socket] = channels
        send(socket, "JOIN_CHANNEL" + MESSAGE_SPLIT_TAG + "true")
    
    if(msg_head == "GET_ONLINE_USERS"):
        ret = "GET_ONLINE_USERS"
        for u in runtimeData["online"]:
            username = runtimeData["online"][u][1]
            score = int(runtimeData["online"][u][3])
            ret = ret + MESSAGE_SPLIT_TAG +  username + "," + str(score)
        send(socket,ret)
        
    if(msg_head == "CHAT"):
        channel = paras[1]
        info = paras[2]
        me_username = runtimeData["online"][socket][1]
        me_score = int(runtimeData["online"][socket][3])
        msg = "CHAT" + MESSAGE_SPLIT_TAG + me_username + "," + str(me_score) + MESSAGE_SPLIT_TAG + channel + MESSAGE_SPLIT_TAG + info.decode("utf-8")
        for u in runtimeData["channel"]:
            try:
                if channel in runtimeData["channel"][u] and u!=socket:
                    send(u, msg)
            except:
                pass
            
    if(msg_head == "GET_SAVES"):
        me_username = runtimeData["online"][socket][1]
        saves = []
        for s in db.getSaves(me_username, conn):
            if(s[1] != None):
                saves.append(s[1])
            else:
                saves.append("Empty")
        msg = "GET_SAVES"
        for s in saves:
            msg = msg + MESSAGE_SPLIT_TAG + s
        send(socket,msg)
        
    if(msg_head == "SAVE"):
        me_username = runtimeData["online"][socket][1]
        index = paras[1]
        content = paras[2]
        ret = db.save(me_username, index, content, conn)
        if ret :
            send(socket,"SAVE" + MESSAGE_SPLIT_TAG + "true")
        else:
            send(socket,"SAVE" + MESSAGE_SPLIT_TAG + "false")
            
    if(msg_head == "BATTLE_RESULT"):
        channel = paras[1]
        winner = paras[2]
        loser = paras[3]
        
        uwin = db.getUser(winner, conn)
        ulose = db.getUser(loser, conn)
        
        winner_score = (int)(uwin[3])
        loser_score = (int)(ulose[3])
        
        #积分规则
        if(winner_score - loser_score > 100): #差距100以上，忽略
            pass
        elif(winner_score >= loser_score):
            delta = winner_score - loser_score
            winner_score += 10 - delta / 10
            loser_score -= 10 - delta / 20
        elif(loser_score > winner_score):
            delta = loser_score - winner_score
            winner_score += 10
            loser_score -= 10
        
        db.updateScore(winner, winner_score, conn)
        db.updateScore(loser, loser_score, conn)
        
        for u in runtimeData["online"]:
            username = runtimeData["online"][u][1]
            if username == winner:
                runtimeData["online"][u] = db.getUser(winner, conn)
            if username == loser:
                runtimeData["online"][u] = db.getUser(loser, conn)
        
        send(socket, "BATTLE_RESULT" + MESSAGE_SPLIT_TAG + "true")        
    
    return tobekicked

if __name__ == "__main__":
    process_msg("CHAT#TAG#wo#TAG#channelname#TAG#hello")