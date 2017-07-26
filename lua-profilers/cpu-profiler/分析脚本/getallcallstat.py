import json


import os



import sys



filename=sys.argv[1]


g_fun_stat = {}


def getts(obj):
	if 'cs' in obj:
		print obj['cs']

def conv(x):
	return '"' +  unicode(x[0]).encode('gbk') + '"' + ':"' + unicode(x[1]).encode('gbk') + '"'

def subfind(obj):

	global g_fun_stat

	if 'fn' in obj:
		funname = obj['fn']
		if funname not in g_fun_stat:
			g_fun_stat[funname] = {"cs":0, "cnt":0}

		g_fun_stat[funname]['cs'] += obj['cs']
	
		g_fun_stat[funname]['cnt'] += 1


	if 'sub' not in obj:
		return

	for subjson in obj['sub']:
		subfind(subjson)


f = open(filename, 'r');

for line in f.readlines():

	str1 = line.rstrip(',\n\r').decode('gbk')

	obj = json.loads(str1);
	
	subfind(obj)
	#getts(obj)



for k in g_fun_stat:
	cs = g_fun_stat[k]['cs']

	cnt = g_fun_stat[k]['cnt']

	print str(cs/cnt) + " " + str(cs) + " " + str(cnt) + " " + unicode(k).encode('gbk')
