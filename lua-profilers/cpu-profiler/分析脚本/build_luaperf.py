#coding: utf8
# usage: python build_luaperf.py --file sample_input\luaprofiler.json --mongo "mongodb://10.20.79.217:27017,10.20.79.218:27017/?replicaSet=testplus" --db performance_analyse --notdebugging
import sys
import logging
import os
import traceback
import argparse
import ubox
import json
import uuid
import re
import gzip
import tarfile
import shutil
import requests
from functools import partial
from pprint import pprint
from pymongo import MongoClient


DEBUG = True
FORMAT="%(asctime)s - %(name)s - %(levelname)s - %(message)s"

MONGO = None
DEFAULT_MONGO = "mongodb://10.20.79.217:27017,10.20.79.218:27017/?replicaSet=testplus"
DEFAULT_DB = "performance_analyse"
PERF_NAMES = 'perf_names.log'
PERF_STATS = 'perf_stats.log'
PERF_TIME = 'perf_time.log'
#UPLOAD_TO = "http://10.20.87.227/uploadrawdata"
UPLOAD_TO = "http://10.20.79.215/uploadrawdata"
KEYWORD = None

def perf_dir(dirname):
  assert os.path.isdir(dirname), "%s is not a dir" % dirname
  path = os.path.join(dirname, PERF_NAMES)
  assert os.path.isfile(path), "%s is not a file" % path
  path = os.path.join(dirname, PERF_STATS)
  assert os.path.isfile(path), "%s is not a file" % path
  path = os.path.join(dirname, PERF_TIME)
  assert os.path.isfile(path), "%s is not a file" % path
  return dirname

def perf_src(src):
  assert os.path.exists(src)
  return src

parser = argparse.ArgumentParser()
parser.add_argument('--notdebugging', action='store_true')
parser.add_argument('--file', required = True)
parser.add_argument('--uuid', default = uuid.uuid4().hex)
parser.add_argument('--mongo', default = DEFAULT_MONGO)
parser.add_argument('--db', default = DEFAULT_DB)
parser.add_argument('--logfile')
parser.add_argument('--keyword')
    
def load_fnmap(fp):
  fnmap = {}
  for line in fp:
    if not line.strip(): continue # empty
    tokens = line.strip().split(',')
    if len(tokens) != 3:
      logging.error("invalid line: %r", line)
      continue
    fid, fname, fparent = map(lambda x: x.split('=')[-1].strip(), tokens[:3])
    fnmap[fid] = dict(name=fname, parent=fparent)
  return fnmap

def cache_build(func):
  cache = {}
  def wrap(fnmap, fid):
    if fid in cache: return cache[fid]
    ret = func(fnmap, fid)
    cache[fid] = ret
    return ret
  return wrap

@cache_build
def build_call_stack(fnmap, fid):
  ret = []
  cur = fid
  while True:
    info = fnmap.get(cur)
    if not info: break
    ret.insert(0, info['name'])
    cur = info['parent']
  return ret

def dump_function_data(meta, function_data):
  ret = []
  func_map = meta['function']
  for item in function_data:
    n = int(item.split('/')[0])
    ret.append("%s: %s" % (func_map[n], item))
  return '\n'.join(ret)

pattern = re.compile(r'(?P<frame>\d+): sys_mem=[^(]+\((?P<sys_mem>[0-9.]+)%\), video_mem=[^(]+\((?P<video_mem>[0-9.]+)%\), app_mem=(?P<app_mem>[0-9.]+)MB, app_peak=(?P<app_peak>[0-9.]+)MB, app_g3d=(?P<app_g3d>[0-9.]+)MB')
def parse_stats_line(line):
  """
    input: 360: sys_mem=31.93GB/24.63GB(22%), video_mem=8192.00MB/6789.29MB(82.9%), app_mem=753.75MB, app_peak=753.75MB, app_g3d=76.69MB
    output: True, 360, {sys_mem=22, video_mem=82.9, app_mem=753.75, app_peak=753.75, app_g3d=76.69}
  """
  matched = pattern.match(line)
  if matched:
    d = matched.groupdict()
    frame = d.pop('frame')
    for k in d:
      d[k] = float(d[k])
    return True, int(frame), d
  return False, None, None


def iter_build_stats(fnmap, fp, reportid):
  perf = ubox.FTTStatData(reportid = reportid)
  for line in fp:
    ok, frame, info = parse_stats_line(line)
    if ok:
      perf.append(stat_frame = frame, **info)
      pass
  for bucket in perf.get_buckets():
    bucket['type'] = TYPE
    yield dict(collection = 'frame_data', document=bucket, selector=dict(uuid=bucket['uuid'], index=bucket['index']))
    pprint(bucket)

def iter_build_frame_data(reportid, frame_stat):
  perf = ubox.FrameData(ubox.LuaFrame, appkey='jx3', reportid = opts.uuid, extra=dict(type='luaprof'))
  for f in frame_stat:
    perf.append(frame = f, fi = frame_stat.get(f, 100))
  for bucket in perf.get_buckets():
    bucket['type'] = TYPE
    yield dict(collection="frame_data", document=bucket)
    pass

def process_function_data(perf, current_frame, item, stack):
  #if item['fn'].startswith('called from'):
  #  fn = ('%s;;;%s;;;ln:%s' % (item['info'], item['mod'], item.get('ln', ''))).replace('/', '|')
  #else:
  fn = ('%s\n%s\n%s' % (item['info'], item['mod'], item['fn'])).replace('/', '|') 
  stack.append(fn)
  perf.append(current_frame, stack, item['cs'])
  for subitem in item['sub']:
    process_function_data(perf, current_frame, subitem, stack)
  stack.pop()
  pass

DUMP_TO = 'Test/dump'

def flush(perf, frame, cache, dumpto):
  if not cache:
    return
  if dumpto:
    with dumpto.open(str(frame) + '.json.gz', 'wb') as fp:
      fp.write(json.dumps(cache))
  for item in cache:
    process_function_data(perf, frame, item, [])

def check_interested(item):
  if not KEYWORD: return True
  if item['mod'].lower().find(KEYWORD) != -1: return True
  if item['fn'].lower().find(KEYWORD) != -1: return True
  for subitem in item['sub']:
    if check_interested(subitem):
      return True

def wholefilereader(fp):
  for item in json.loads('[' + fp.read()[:-1] + ']'):
    yield item

def linereader(fp):
  for line in fp:
    try:
      yield json.loads(line.decode('gbk').rstrip(u', \r\n\t'))
    except:
      raise StopIteration

sample_reader = wholefilereader if os.getenv("WHOLEFILE") else linereader

def iter_build_perfs(fp, reportid, dumpto):
  perf = ubox.Jx3HdPerfData(reportid, appkey='jx3', fixer=1.0, extra=dict(type='luaprof',tid='lua'), bucket_size=300)
  frameperf = ubox.FrameData(ubox.LuaFrame, appkey='jx3', reportid = opts.uuid, extra=dict(type='luaprof',tid='lua'))
  cnt = 0
  cache = []
  interested = False
  current_frame = 0
  if sample_reader == linereader:
    logging.info("Using LINE READER")
  else:
    logging.info("Using WHOLE FILE READER")
  for item in sample_reader(fp):
    if item.get('stFlag') is not None:
      if item.get('stFlag') is False:
        if interested:
          frameperf.append(current_frame, lc = item['lc'], fc = item['fc'], fi = item['fi'], ft = item['ft'], fut = item['fut'])
      cnt += 1
      if cnt > 1000:
        print "current_frame: ", current_frame
        cnt = 0
      if interested:
        flush(perf, current_frame, cache, dumpto)
        interested = False
      cache = []
      current_frame = item.get('fid')
    elif item.get('lv'):
      #process_function_data(perf, current_frame, item, [])
      cache.append(item)
      if not interested:
        interested = check_interested(item)
        #if interested: print item

  if interested and cache:
    flush(perf, current_frame, cache, dumpto)
    interested = False
    cache = []
  meta = perf.get_meta()
  yield dict(collection="function_data", document=meta)
  yield dict(collection="function_frame_data", document=meta)
  
  for bucket in perf.get_frames():
    yield dict(collection="function_data", document=bucket)
    #pprint(bucket)
    fd = bucket['frame_data']
    for f in fd:
      for item in fd[f]:
        n = int(item.split('/')[0])
        assert float(item.split('/')[3]) >= 0, "frame: %s, item: %s, what: %s" % (f, item, meta['function'][n])
  for bucket in perf.get_func_frames():
    yield dict(collection="function_frame_data", document=bucket)
  overview = perf.get_overview()
  yield dict(collection="function_consume", document=overview)

  #ret = {}
  #for f in perf.frames:
  #  t = perf.frames[f]
  #  ret[f] = t.rectified_child_time
  #yield dict(collection="frame_stat", document=ret)
  
  for bucket in frameperf.get_buckets():
    yield dict(collection="frame_data", document=bucket)

def iter_process(reportid, files, dumpto=None):
  if 'JSON' not in files:
    logging.warn("skip process lua JSON")
    raise StopIteration
  for report in iter_build_perfs(files["JSON"], reportid, dumpto):
      yield report

def read_zip(filename):
  with zipfile.ZipFile(filename, 'r') as fz:      
    name_list = filter(lambda x:x.find('/') == -1, fz.namelist())
    files = {}
    for file in name_list:
      files[file] = fz.open(file)
  return files

def read_dir(dirname):
  files = {}
  for f in os.listdir(dirname):
    fn = os.path.join(dirname, f)
    if os.path.isfile(fn):
      files[f] = open(fn, 'rb')
  return files

def upload(reports):
  for report in reports:
    collection = report['collection']
    document = report['document']
    logging.debug("collection: %s, document: %s", collection, document.keys())
    if not DEBUG:
      MONGO[collection].insert_one(document)
    #pprint(document)

def maybe_mkdir(dirname):
  try:
    os.makedirs(dirname)
  except:
    assert os.path.isdir(dirname), "Can't make dir: %s" % dirname

def http_upload(dst, src):
  logging.info("uploading %s->%s", src, dst)
  r = requests.post(dst, files={'file': open(src, 'rb')})
  logging.info("respond: %s", r.text)

class DumpTo:
  def __init__(self, reportid):
    self.dirname = os.path.join('tmp', reportid)
    maybe_mkdir(self.dirname)
    self.commited = False
    pass

  def open(self, filename, mode):
    assert self.commited is False, "try openning after commited"
    return gzip.open(os.path.join(self.dirname, filename), mode)

  def commit(self):
    filename = self.dirname + '.tar.bz2'
    logging.info("packing files info %s", filename)
    with tarfile.open(self.dirname + '.tar.bz2', 'w:bz2') as fp:
      name = self.dirname
      fp.add(name = name, arcname=name.replace('tmp/',''))
    logging.info("%s is ready to upload", filename)
    http_upload(UPLOAD_TO, filename)
    logging.info("%s is uploaded", filename)
    self.commit = True
    logging.info("deleting %s", self.dirname)
    shutil.rmtree(self.dirname)
    logging.info('deleting %s', filename)
    os.unlink(filename)
    pass

def main(opts):
  fp = open(opts.file, 'rb')
  logging.info("process_jx3perf begins")
  DUMP_TO = DumpTo(opts.uuid)
  try:
    upload(list(iter_process(opts.uuid, {'JSON': fp}, dumpto=DUMP_TO)))
  finally:
    fp.close()
  DUMP_TO.commit()
  logging.info("process_jx3perf ends")

if __name__ == '__main__':
  opts = parser.parse_args()
  try:
    if opts.notdebugging:
      DEBUG = False
      assert opts.mongo and opts.db, "requires --mongo and --db"
      MONGO = MongoClient(opts.mongo)[opts.db]
    if opts.logfile:
      logging.basicConfig(
        filename = opts.logfile,
        level = logging.DEBUG,
        format = FORMAT
      )
    else:
      logging.basicConfig(
        level = logging.DEBUG,
        format = FORMAT
      )
    if opts.keyword: KEYWORD = opts.keyword.lower()
    main(opts)
    logging.info("uuid: %s", opts.uuid)
  except:
    logging.error(traceback.format_exc())
    sys.exit(1)
