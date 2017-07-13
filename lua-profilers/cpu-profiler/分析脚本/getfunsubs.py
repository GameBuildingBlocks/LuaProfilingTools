import json


import os



import sys


total_call_cnt = 0

total_cs = 0



filename=sys.argv[1]


def getts(obj):
	if 'cs' in obj:
		print obj['cs']

def conv(x):
	if x[0] == 'sub':
		return " "
	return '"' +  unicode(x[0]).encode('gbk') + '"' + ':"' + unicode(x[1]).encode('gbk') + '"'

def subfind(obj):

	global total_call_cnt

 	global total_cs

	if 'sub' not in obj:
		return


	if 'fn' not in obj:
		return

	

	

	if unicode(obj['fn']).encode('gbk') != sys.argv[2]:
		for subjson in obj['sub']:
			subfind(subjson)
		return

	total_call_cnt +=  1

	total_cs += obj['cs']
	
	
	for subjson in obj['sub']:
		print ','.join(map(conv, subjson.items()))	


f = open(filename, 'r');

for line in f.readlines():

	str1 = line.rstrip(',\n\r').decode('gbk')

	obj = json.loads(str1);
	
	subfind(obj)
	#getts(obj)


print  " total_cs:" + str(total_cs) + " total_call_cnt:" +  str(total_call_cnt)
