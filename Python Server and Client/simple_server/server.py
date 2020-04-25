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
s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
s.bind((host,port)) 
s.listen(backlog) 


while 1:
    client, address = s.accept() 
    print "Client connected."

    recv_times = 0

    while 1:

		client.send("hello")
	
		data = client.recv(1024)
		print (data)
		


		


		




    

