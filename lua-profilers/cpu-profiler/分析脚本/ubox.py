import uuid
import collections

def floatpoint_fix(x):
  return int(x * 100) / 100.0

class UboxPerfData:
  def __init__(self, appkey='jx3hd', bucket_size = 300, reportid=None):
    self.call_stack_map = dict()
    self.buckets = dict()
    self.bucket_size = bucket_size
    self.appkey = appkey
    self.uuid = reportid or uuid.uuid4().hex
    self.bigbucket = {}
    self.totaltime = 0
    
  def _get_bucket(self, frame):
    return int(frame / self.bucket_size) + 1
  
  def append(self, frame, call_stack, time):
    is_root = len(call_stack) == 1
    if type(call_stack) is list:
      call_stack = '/'.join(call_stack)
    if call_stack not in self.call_stack_map:
      self.call_stack_map[call_stack] = len(self.call_stack_map)
    sid = self.call_stack_map[call_stack]
    bid = self._get_bucket(frame)
    if bid not in self.buckets:
      self.buckets[bid] = {}
    bucket = self.buckets[bid]
    if frame not in bucket:
      bucket[frame] = {}
    stats = bucket[frame]
    if sid not in stats:
      stats[sid] = [0,0] 
    stats[sid][0] += time
    stats[sid][1] += 1
    
    if is_root: # we only need the first layer to calculate the total time
      self.totaltime += time
      #print "total: ", self.totaltime
      
    if sid not in self.bigbucket:
      self.bigbucket[sid] = [0,0,0,0,0] # time, count, peaktime, avgtime, persent
    self.bigbucket[sid][0] += time
    self.bigbucket[sid][1] += 1
    self.bigbucket[sid][2] = max(self.bigbucket[sid][2], time)
    self.bigbucket[sid][3] = floatpoint_fix(self.bigbucket[sid][0] * 1.0 / self.bigbucket[sid][1])
    
    
    
  def get_meta(self):
    ret = dict(index=0, uuid=self.uuid, appkey=self.appkey, frame_data={})
    ret['function'] = [''] * len(self.call_stack_map)
    for k,v in self.call_stack_map.items():
      ret['function'][v] = k
    return ret
    
  def get_frames(self):
    for bid in self.buckets:
      ret = dict(index=bid, uuid=self.uuid, appkey=self.appkey, frame_data={}, function=[])
      bucket = self.buckets[bid]     
      frame_data = ret['frame_data']
      for frame in bucket:
        single_frame_stat = []
        frame_data[str(frame)] = single_frame_stat
        stats = bucket[frame]
        for sid in stats:
          single_frame_stat.append('%d/%.2f/%d/0' % (sid, stats[sid][0] / 100.0, stats[sid][1]))
      yield ret

  def get_overview(self):
    meta = self.get_meta()
    ret = dict(index=0, uuid=self.uuid, appkey=self.appkey, function_consume=[])
    l = ret['function_consume']
    for sid in self.bigbucket:
      self.bigbucket[sid][4] = floatpoint_fix(self.bigbucket[sid][0] * 100.0 / self.totaltime)
      #print self.bigbucket[sid][4]
      l.append(dict(
        function = meta['function'][sid], 
        data = dict(
          TotalTime = floatpoint_fix(self.bigbucket[sid][0] / 100.0), 
          Calls = self.bigbucket[sid][1],
          PeakTime = floatpoint_fix(self.bigbucket[sid][2] / 100.0),
          AvgTime = floatpoint_fix(self.bigbucket[sid][3] / 100.0),
          Persent = self.bigbucket[sid][4]
          )
        ))
    return ret

class StatNode:
  def __init__(self, name):
    self.name = name
    self.kids = {}
    self.time = 0
    self.count = 0
    self.child_time = 0
    self.max = 0
    # init by parent
    self.parent = None
    self.fullname = ''
    self.level = 0
    
    #set in rectify
    self.rectified_time = 0
    self.rectified_count = 0
    self.rectified_child_time = 0
    self.rectified_max = 0
    self.rectified_nodes = 0

  def get_child(self, call_frame):
    ret = self.kids.get(call_frame)
    if not ret:
      ret = StatNode(call_frame)
      ret.parent = self
      ret.fullname = self.fullname + ('/' if ret.parent.fullname else '') + call_frame
      ret.level = self.level + 1
      self.kids[call_frame] = ret
    return ret

  def add_stack(self, call_stack, time):
    cur = self
    for call_frame in call_stack:
      node = cur.get_child(call_frame)
      cur = node
    node.max = max(node.max, time)
    node.time += time
    node.count += 1
    node.parent.child_time += time
    pass
  
  def rectify(self):
    # make sure parent_time >= sum(child_time)
    rectified_time = 0
    rectified_max = 0
    rectified_nodes = 1
    for node in self.kids.values():
      node.rectify()
      rectified_time += node.rectified_time
      rectified_max = max(node.rectified_max, rectified_max)
      rectified_nodes += node.rectified_nodes
      
    self.rectified_time = max(self.time, rectified_time)
    self.rectified_child_time = rectified_time
    self.rectified_max = max(rectified_max, self.max)
    self.rectified_nodes = rectified_nodes

  def walk(self):
    for name in self.kids:
      child = self.kids[name]
      yield child
      for x in child.walk():
        yield x
      

class Jx3HdPerfData:
  def __init__(self, reportid, appkey='jx3hd', bucket_size = 300, fixer = 1/100.0, extra={}):
    self.extra = extra
    self.fixer = fixer
    self.bucket_size = bucket_size
    self.appkey = appkey
    self.uuid = reportid
    self.overall_tree = StatNode("#")
    self.frames = {}
    self.function = None
    self.functionid = None
    self.buckets = {}
    self.rectified = False
    pass
    
  def append(self, frame, call_stack, time):
    if frame not in self.frames:
      self.frames[frame] = StatNode(str(frame))
    frame_tree = self.frames[frame]
    frame_tree.add_stack(call_stack, time)
    self.overall_tree.add_stack(call_stack, time)
    pass
    
  def _get_bucket(self, frame):
    return int(frame / self.bucket_size) + 1
    
  def build_function_map(self):
    if self.function is not None: return
    self.function = []
    self.fullname2idx = {}
    for idx, node in enumerate(self.overall_tree.walk()):
      self.function.append(node.fullname)
      self.fullname2idx[node.fullname] = idx
    pass
  
  def build_buckets(self):
    if self.buckets: return
    for frame in self.frames:
      bid = self._get_bucket(frame)
      if bid not in self.buckets:
        self.buckets[bid] = []
      self.buckets[bid].append(frame)
      
  def rectify(self):
    if self.rectified: return
    self.overall_tree.rectify()
    for node in self.frames.values():
      node.rectify()
    self.rectified = True

  def get_meta(self):
    ret = dict(index=0, uuid=self.uuid, appkey=self.appkey, frame_data={}, bucket=self.bucket_size)
    self.build_function_map()
    ret['function'] = self.function
    ret.update(self.extra)
    return ret
  
  def get_frames(self):
    self.build_buckets()
    self.rectify()
    for bid in self.buckets:
      ret = dict(index=bid, uuid=self.uuid, appkey=self.appkey, frame_data={}, function=[])
      frame_data = ret['frame_data']
      for frame in sorted(self.buckets[bid]):
        total_time = self.frames[frame].rectified_child_time
        single_frame_stat = []
        frame_data[str(frame)] = single_frame_stat
        for node in self.frames[frame].walk():
          #single_frame_stat.append('%d/%.2f/%d/%.2f' % (self.fullname2idx[node.fullname], node.rectified_time * self.fixer, node.count, (node.rectified_time - node.rectified_child_time) * self.fixer))
          single_frame_stat.append('%(Frame)d/%(TotalTime).2f/%(Calls)d/%(Self).2f/%(AvgTime).2f/%(PeakTime).2f/%(RootTime).2f' % dict(
            Frame = self.fullname2idx[node.fullname],
            TotalTime = floatpoint_fix(node.rectified_time * self.fixer), 
            Calls = node.count,
            PeakTime = floatpoint_fix(node.rectified_max * self.fixer),
            AvgTime = floatpoint_fix(node.rectified_time * self.fixer / node.count),
            RootTime = floatpoint_fix(total_time),
            Self = floatpoint_fix((node.rectified_time - node.rectified_child_time) * self.fixer)
          ))
      ret.update(self.extra)
      yield ret

  def get_overview(self):
    self.rectify()
    total_time = self.overall_tree.rectified_child_time
    ret = dict(index=0, uuid=self.uuid, appkey=self.appkey, function_consume=[])
    l = ret['function_consume']
    for node in self.overall_tree.walk():
      l.append(dict(
        function = node.fullname, 
        data = dict(
          TotalTime = floatpoint_fix(node.rectified_time * self.fixer), 
          Calls = node.count,
          PeakTime = floatpoint_fix(node.rectified_max * self.fixer),
          AvgTime = floatpoint_fix(node.rectified_time * self.fixer / node.count),
          Persent = floatpoint_fix(node.rectified_time * 100.0 / total_time),
          Self = floatpoint_fix((node.rectified_time - node.rectified_child_time) * self.fixer)
          )
        ))
    ret.update(self.extra)
    return ret
  
  def get_func_mata(self):
    self.build_function_map()
    ret = dict(index=0, uuid=self.uuid, appkey=self.appkey, data={}, function=[], bucket=self.bucket_size)
    ret['function'] = self.function
    ret.update(self.extra)
    return ret
  
  def get_func_frames(self):
    self.rectify()
    self.build_buckets()
    for bid in self.buckets:
      ret = dict(index=bid, uuid=self.uuid, appkey=self.appkey, data={}, function=[])
      data = ret['data']
      for frame in sorted(self.buckets[bid]):
        for node in self.frames[frame].walk():
          sid = str(self.fullname2idx[node.fullname])
          if sid not in data: data[sid] = []
          data[sid].append("%d/%.2f/%d/%.2f" % (frame, node.rectified_time * self.fixer, node.count, (node.rectified_time - node.rectified_child_time) * self.fixer))
      ret.update(self.extra)
      yield ret

class FTTStatsData:
  def __init__(self, reportid, appkey='jx3hd', bucket_size = 300):
    self.appkey = appkey
    self.uuid = reportid
    self.frames = {}
    self.bucket_size = bucket_size
    self.buckets = {}

  def append(self, frame, **attrs):
    self.frames[frame] = attrs

  def _get_bucket(self, frame):
    return int(frame / self.bucket_size) + 1
    
  def build_buckets(self):
    if self.buckets: return
    for frame in self.frames:
      bid = self._get_bucket(frame)
      if bid not in self.buckets:
        self.buckets[bid] = []
      self.buckets[bid].append(frame)
      
  def get_buckets(self):
    self.build_buckets()
    for bid in self.buckets:
      ret = dict(index=bid, uuid=self.uuid, appkey=self.appkey, data={})
      data = ret['data']
      for frame in sorted(self.buckets[bid]):
        data[str(frame)] = self.frames[frame]
      yield ret

FTTFrame = collections.namedtuple('FTTFrame', 'frame fi')
class FTTFrameData:
  def __init__(self, appkey="jx3hd", bucket_size = 10000, reportid = None):
    self.appkey = appkey
    self.bucket_size = bucket_size
    self.reportid = reportid
    self.buckets = dict()
    self.uuid = reportid or uuid.uuid4().hex
    
  def _get_bucket(self, frame):
    return frame/self.bucket_size + 1
  
  def append(self, frame, **info):
    bid = self._get_bucket(frame)
    if bid not in self.buckets:
      self.buckets[bid] = {"data":[]}
    bucket = self.buckets[bid]
    bucket['data'].append(FTTFrame(frame = frame, **info))

  def get_buckets(self):
    for bid in self.buckets:
      ret = dict(index=bid, uuid=self.uuid, appkey=self.appkey, data={})
      src = self.buckets[bid]['data']
      dst = ret
      for field in FTTFrame._fields:
        dst[field] = []
        for i in src:
          val = getattr(i, field)
          if type(val) is float:
            val = floatpoint_fix(val)
          dst[field].append(val)
      yield ret

FTTStat = collections.namedtuple('FTTStat', 'stat_frame sys_mem video_mem app_mem app_peak app_g3d')
class FTTStatData:
  def __init__(self, appkey="jx3hd", bucket_size = 10000, reportid = None):
    self.appkey = appkey
    self.bucket_size = bucket_size
    self.reportid = reportid
    self.buckets = dict()
    self.uuid = reportid or uuid.uuid4().hex
    
  def _get_bucket(self, frame):
    return frame/self.bucket_size + 1
  
  def append(self, stat_frame, **info):
    bid = self._get_bucket(stat_frame)
    if bid not in self.buckets:
      self.buckets[bid] = {"data":[]}
    bucket = self.buckets[bid]
    bucket['data'].append(FTTStat(stat_frame = stat_frame, **info))

  def get_buckets(self):
    for bid in self.buckets:
      ret = dict(index=bid, uuid=self.uuid, appkey=self.appkey, data={})
      src = self.buckets[bid]['data']
      dst = ret
      for field in FTTStat._fields:
        dst[field] = []
        for i in src:
          val = getattr(i, field)
          if type(val) is float:
            val = floatpoint_fix(val)
          dst[field].append(val)
      yield ret

      
LuaFrame = collections.namedtuple('LuaFrame', 'frame fc lc fi ft fut')
class FrameData:
  def __init__(self, clazz, appkey="jx3", bucket_size = 10000, reportid = None, extra={}):
    self.extra = extra
    self.clazz = clazz
    self.appkey = appkey
    self.bucket_size = bucket_size
    self.reportid = reportid
    self.buckets = dict()
    self.uuid = reportid or uuid.uuid4().hex
    
  def _get_bucket(self, frame):
    return frame/self.bucket_size + 1
  
  def append(self, frame, **info):
    bid = self._get_bucket(frame)
    if bid not in self.buckets:
      self.buckets[bid] = {"data":[]}
    bucket = self.buckets[bid]
    bucket['data'].append(self.clazz(frame = frame, **info))

  def get_buckets(self):
    for bid in self.buckets:
      ret = dict(index=bid, uuid=self.uuid, appkey=self.appkey, data={})
      src = self.buckets[bid]['data']
      dst = ret
      for field in self.clazz._fields:
        dst[field] = []
        for i in src:
          val = getattr(i, field)
          if type(val) is float:
            val = floatpoint_fix(val)
          dst[field].append(val)
      ret.update(self.extra)
      yield ret
      
class LuaPerfData:
  def __init__(self, appkey='jx3hd', bucket_size = 300, reportid=None):
    self.fn_map = dict()
    self.buckets = dict()
    self.bucket_size = bucket_size
    self.appkey = appkey
    self.uuid = reportid or uuid.uuid4().hex

  def _get_bucket(self, frame):
    return int(frame / self.bucket_size) + 1
  
  def append(self, frame, fn, cs, st, et, lv):
    if fn not in self.fn_map:
      self.fn_map[fn] = len(self.fn_map)
    fid = self.fn_map[fn]
    bid = self._get_bucket(frame)
    if bid not in self.buckets:
      self.buckets[bid] = {}
    bucket = self.buckets[bid]
    if frame not in bucket:
      bucket[frame] = []
    stats = bucket[frame]
    stats.append('%d/%f/%f/%f/%d' % (fid, cs, st, et, lv))
    
  def get_meta(self):
    ret = dict(index=0, uuid=self.uuid, appkey=self.appkey, frame_data={})
    ret['function'] = [''] * len(self.fn_map)
    for k,v in self.fn_map.items():
      ret['function'][v] = k
    return ret
    
  def get_frames(self):
    for bid in self.buckets:
      ret = dict(index=bid, uuid=self.uuid, appkey=self.appkey, frame_data={}, funtion=[])
      bucket = self.buckets[bid]     
      frame_data = ret['frame_data']
      for frame in bucket:
        single_frame_stat = []
        frame_data[str(frame)] = single_frame_stat
        stats = bucket[frame]
        for stat in stats:
          single_frame_stat.append(stat)
      yield ret

class LuaPerfOverData:
  def __init__(self, appkey="jx3hd", bucket_size = 10000, reportid = None):
    self.appkey = appkey
    self.bucket_size = bucket_size
    self.reportid = reportid
    self.buckets = dict()
    self.uuid = reportid or uuid.uuid4().hex
    
  def _get_bucket(self, frame):
    return frame/self.bucket_size + 1
  
  def append(self, frame, lc, fc, fi, ft, fut):
    bid = self._get_bucket(frame)
    if bid not in self.buckets:
      self.buckets[bid] = {"data":[]}
    bucket = self.buckets[bid]
    bucket['data'].append('%(frame)s/%(lc)f/%(fc)f/%(fi)f/%(ft)f/%(fut)f' % locals())
  
  def get_buckets(self):
    for bid in self.buckets:
      ret = dict(index=bid, uuid=self.uuid, appkey=self.appkey, data=self.buckets[bid]['data'])
      yield ret

JX3EngineFrame = collections.namedtuple('JX3EngineFrame', 'frame fi')
class Jx3EngineOverAllData:
  def __init__(self, appkey="jx3hd", bucket_size = 10000, reportid = None):
    self.appkey = appkey
    self.bucket_size = bucket_size
    self.reportid = reportid
    self.buckets = dict()
    self.uuid = reportid or uuid.uuid4().hex
    
  def _get_bucket(self, frame):
    return frame/self.bucket_size + 1
  
  def append(self, frame, fi, scene=None):
    bid = self._get_bucket(frame)
    if bid not in self.buckets:
      self.buckets[bid] = {"data":[]}
    bucket = self.buckets[bid]
    bucket['data'].append(JX3EngineFrame(frame = frame, fi = fi))

  def get_buckets(self):
    for bid in self.buckets:
      ret = dict(index=bid, uuid=self.uuid, appkey=self.appkey, data={})
      src = self.buckets[bid]['data']
      dst = ret
      field = 'frame'
      dst[field] = []
      for i in src:
        dst[field].append(getattr(i, field))
      field = 'fi'
      dst[field] = []
      for i in src:
        dst[field].append(floatpoint_fix(getattr(i, field) / 100.0))
      yield ret

class LuaPerfData2:
  def __init__(self, appkey='jx3', bucket_size = 300, reportid=None):
    self.fn_map = dict()
    self.buckets = dict()
    self.bucket_size = bucket_size
    self.appkey = appkey
    self.uuid = reportid or uuid.uuid4().hex

  def _get_bucket(self, frame):
    return int(frame / self.bucket_size) + 1
  
  def append(self, frame, fn, cs, st, et, lv):
    if fn not in self.fn_map:
      self.fn_map[fn] = len(self.fn_map)
    fid = self.fn_map[fn]
    bid = self._get_bucket(frame)
    if bid not in self.buckets:
      self.buckets[bid] = {}
    bucket = self.buckets[bid]
    if frame not in bucket:
      bucket[frame] = []
    stats = bucket[frame]
    stats.append('%d/%f/%f/%f/%d' % (fid, cs, st, et, lv))
if __name__ == '__main__':
  t = StatNode('#')
  t.add_stack("adaoi", 100)
  t.add_stack("adboi", 100)
  t.add_stack("adaci", 100)
  for a in t.walk():
    print a.fullname, a.time, a.child_time
