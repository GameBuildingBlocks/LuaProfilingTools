import json


import os



import sys



filename=sys.argv[1]


def getts(obj):
	if 'cs' in obj:
		print obj['cs']

def conv(x):
	return '"' +  unicode(x[0]).encode('gbk') + '"' + ':"' + unicode(x[1]).encode('gbk') + '"'

g_cur_frame = 0

g_frame_cs = 0

g_frame_file = None


g_frame_stat_file = open(sys.argv[2] + "stat.txt", "w")

def jsonfind(obj):
	global g_cur_frame
	global g_frame_cs
	global g_frame_file
	
	if 'fid' in obj:
		if g_frame_file != None:
			g_frame_stat_file.write( str(g_cur_frame + 1) + " " + str(g_frame_cs) + " " + str(obj['hook_ts'])  + "\r\n" )
		g_cur_frame = obj['fid']
		g_frame_cs = 0
		
		if g_frame_file != None:
			g_frame_file.close()
		g_frame_file = open(sys.argv[2] + str(g_cur_frame),"w")
		return


	
		

	if 'fn' in obj:
		g_frame_cs += obj['cs']	




f = open(filename, 'r');





for line in f.readlines():

	#str = line.rstrip(',\n\r').decode('gbk')
	str1 = line.rstrip(',\n\r').decode('gbk')

	obj = json.loads(str1);

		
	
	jsonfind(obj)

	g_frame_file.write(line)
	#getts(obj)

