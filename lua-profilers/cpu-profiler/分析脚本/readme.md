# 将数据提交到WEB端分析

* python /home/lijianjun/build_luaperf.py --file luaprofiler.txt --notdebugging
* 结束后会出现如下界面


    2017-07-13 17:40:26,532 - root - INFO - tmp/d9f45ec891b0428495837d23a9c71f2e.tar.bz2 is uploaded
    2017-07-13 17:40:26,532 - root - INFO - deleting tmp/d9f45ec891b0428495837d23a9c71f2e
    2017-07-13 17:40:26,532 - root - INFO - deleting tmp/d9f45ec891b0428495837d23a9c71f2e.tar.bz2
    2017-07-13 17:40:26,532 - root - INFO - process_jx3perf ends
    2017-07-13 17:40:26,533 - root - INFO - uuid: d9f45ec891b0428495837d23a9c71f2e
    
* 访问连接 http://jx3.testplus.cn/project/jx3/luaprof/d9f45ec891b0428495837d23a9c71f2e 可看到WEB端结果


# 数据分析相关脚本

执行 ./getall.sh luaprofiler.txt会生成三个文件，用来统计函数调用时间消耗