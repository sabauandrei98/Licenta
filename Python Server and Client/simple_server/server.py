#!/usr/bin/env python 

""" 
A simple echo server,
to test tcp networking code
""" 

import socket 

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
        client.send("ana are mere"+"\n")
        data = client.recv(size)

        recv_times = recv_times + 1
        print(recv_times)
        if recv_times == 100:
            break

