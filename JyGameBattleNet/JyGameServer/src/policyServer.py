#encoding=utf8

from SocketServer import TCPServer, BaseRequestHandler
import traceback

class MyBaseRequestHandlerr(BaseRequestHandler):
    def handle(self):
        try:
            data = self.request.recv(1024).strip()
            print "receive from (%r):%r" % (self.client_address, data)
            if(data=="<policy-file-request/>"):
                self.request.sendall(self.get_policy_file())
                #print self.get_policy_file()
        except:
            traceback.print_exc()
        
    def get_policy_file(self):
        f = open("policyfile.xml")
        content = f.read()
        f.close()
        return content

if __name__ == "__main__":
    host = ""
    port = 943
    addr = (host, port)
    server = TCPServer(addr, MyBaseRequestHandlerr)
    server.serve_forever()
