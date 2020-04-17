#!/usr/bin/env python 

""" 
A simple echo server,
to test tcp networking code
""" 

import socket 
import time

host = '' 
port = 50000 
backlog = 5 
size = 1024 
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM) 
s.bind((host,port)) 
s.listen(backlog) 

while 1:
    client, address = s.accept() 
    print "Client connected."

    recv_times = 0

    while 1: 
        client.send("msg:"+ str(recv_times) + "\n")
        

        data = client.recv(size)
        print("data:" + data)

    

