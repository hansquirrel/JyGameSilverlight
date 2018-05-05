#encoding=utf8
'''
Created on 2013-12-14

@author: Administrator
'''
import time

def log(msg):
    t = time.strftime('%Y-%m-%d %H:%M:%S',time.localtime(time.time()))
    print ("[%s]%s"%(t,msg))
    
    