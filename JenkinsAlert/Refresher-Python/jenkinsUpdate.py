#!/usr/bin/python

import time
import urllib2

def getJenkins():
	try:

		isSuccess=False;

		page = urllib2.urlopen("JENKINS_ADDRESS:8080");

		html = page.read().decode("utf8");

		if html.find("images/32x32/blue.png") == -1:
		    isSuccess = False;
		else:
		    isSuccess = True;

		if isSuccess == True:
			urllib2.urlopen("SERVER_ADDRESS/insert.php?status=1");
			print("ok");
		else:
			urllib2.urlopen("SERVER_ADDRESS/insert.php?status=0");
			print("fail");
	except:
		print("ERROR");

while True:
	getJenkins();
	time.sleep(5);