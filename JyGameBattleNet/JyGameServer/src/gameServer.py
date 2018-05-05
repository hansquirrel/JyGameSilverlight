#encoding=utf8
import select
import socket
import ConfigParser  
import messageProcessor
import db
from log import log
import traceback

MAX_CLIENTS = 1000
SELECT_TIME_OUT = 60 * 5
SERVER_VERSION = "0.0.0.1"
RECIEVE_BUFFER_SIZE = 1024 * 1024 * 2

runtimeData = {}
runtimeData["online"] = {}
runtimeData["channel"] = {}
runtimeData["buffer"] = {}

def removeClient(socket, inputs):
    if(socket in runtimeData["online"].keys()):
        del runtimeData["online"][socket]
    if(socket in runtimeData["channel"].keys()):
        del runtimeData["channel"][socket]
    if(socket in runtimeData["buffer"].keys()):
        del runtimeData["buffer"][socket]
    if socket in inputs:
        inputs.remove(socket)
    try:
        socket.close()
        log("removing socket%s"%str(socket.getpeername()))
    except:
        pass

if __name__ == "__main__":
    cf = ConfigParser.ConfigParser()  
    cf.read("config.ini")
    
    server = socket.socket(socket.AF_INET,socket.SOCK_STREAM)
    server.setblocking(False)
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR  , 1)
    
    host_ip = cf.get("server", "ip")
    host_port = int(cf.get("server","port"))
    server_address = (host_ip, host_port)
    server.bind(server_address)
    server.listen(MAX_CLIENTS)
    inputs = [server]
    timeout = SELECT_TIME_OUT
    log("jygame server v" + SERVER_VERSION)
    log("server startup on %s:%d ...."%(host_ip, host_port))
    
    dbconn = db.getConnect()
    while inputs:
        #print "waiting for next event"
        readable , writable , exceptional = select.select(inputs, [], [], timeout)
        db.testConnect(dbconn)
        kicklist = []
        if not (readable or writable or exceptional) :
            log( "I'm living..")
            
        for s in readable :
            if s is server:
                connection, client_address = s.accept()
                log("connection from %s"%str(client_address))
                connection.setblocking(False)
                inputs.append(connection)
            else:
                try:
                    data = s.recv(RECIEVE_BUFFER_SIZE)
                    if data :
                        log("received %s from %s"%(data,str(s.getpeername())))
                        kicklist = messageProcessor.process_buffer(data , dbconn, s, runtimeData)
                    else:
                        removeClient(s, inputs)
                except Exception,e:
                    print traceback.format_exc()
                    print "exception :" + str(e)
                    removeClient(s, inputs)
                    
        for s in kicklist:
            removeClient(s,inputs)
