python getccallstat.py $1 | sort -r -k2 -n > ccall.$1
python getallcallstat.py $1 | sort -r -k2 -n > allcall.$1
python getluacallstat.py $1 | sort -r -k2 -n > luacall.$1
