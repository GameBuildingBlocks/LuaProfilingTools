max_table_size = 60000000

printf= function(s, ...)  
    return io.write(s:format(...))  
end

function generator_string(v, n)
  local s = ""
  while string.len(s) < n do
    s = s .. tostring(v)
    s = s.. s
  end
  return string.sub( s, 0, n )
end

function generator_string_shared(n)
  local c = "111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111"
  local s = c
  while string.len(s) < n do
    s = s .. c
  end
  return string.sub(s, 0, n)
end

function align_block_test(max_size)
  local blocks = {}

  printf("8b align!\r\n")
  align_size_8b = 8
  count_8b = max_size / align_size_8b 
  for i = 0, count_8b do
      str = generator_string(i, align_size_8b)
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}   
      end
  end
  blocks = {}

  printf("16b align!\r\n")
  align_size_16b = 16
  count_16b = max_size / align_size_16b 
  for i = 0, align_size_16b do
      str = generator_string(i, align_size_16b)
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}     
      end
  end
  blocks = {}

  printf("32b align!\r\n")
  align_size_32b = 32
  count_32b = max_size / align_size_32b
  for i = 0, count_32b do
      str = generator_string(i, align_size_32b)
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}       
      end
  end
  blocks = {}

  printf("64b align!\r\n")
  align_size_64b = 64
  count_64b = max_size / align_size_64b 
  for i = 0, count_64b do
      str = generator_string(i, align_size_64b)
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}      
      end
  end
  blocks = {}

  printf("128b align!\r\n")
  align_size_128b = 128
  count_128b = max_size / align_size_128b
  for i = 0, align_size_128b do
      str = generator_string(i, align_size_128b)
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {} 
      end
  end
  blocks = {}

  printf("512b align!\r\n")
  align_size_512b = 512
  count_512b = max_size / align_size_512b
  for i = 0, count_512b do
      str = generator_string(i, align_size_512b)
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}      
      end
  end
  blocks = {}

  printf("1k align!\r\n")
  align_size_1k = 1 * 1024
  count_1k = max_size / align_size_1k 
  for i = 0, count_1k do
      str = generator_string(i, align_size_1k)
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}     
      end
  end
  blocks = {}

  printf("2k align!\r\n")
  align_size_2k = 2 * 1024
  count_2k = max_size / align_size_2k
  for i = 0, count_2k do
      str = generator_string(i, align_size_2k)
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}       
      end
  end
  blocks = {}

  printf("4k align!\r\n")
  align_size_4k = 4 * 1024
  count_4k = max_size / align_size_4k
  for i = 0, count_4k do
      str = generator_string(i, align_size_4k)
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}      
      end
  end
  blocks = {}

  printf("8k align!\r\n")
  align_size_8k = 8 * 1024
  count_8k = max_size / align_size_8k
  for i = 0, count_8k do
      str = generator_string(i, align_size_8k)
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}     
      end
  end
  blocks = {}

  printf("16k align!\r\n")
  align_size_16k = 16 * 1024
  count_16k = max_size / align_size_16k 
  for i = 0, count_16k do
      str = generator_string(i, align_size_16k)
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}       
      end
  end
  blocks = {}

  printf("32k align!\r\n")
  align_size_32k = 32 * 1024 
  count_32k = max_size / align_size_32k
  for i = 0, count_32k do
      str = generator_string(i, align_size_32k)
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}      
      end
  end
  blocks = {}

  printf("64k align!\r\n")
  align_size_64k = 64 * 1024
  count_64k = max_size / align_size_64k
  for i = 0, count_64k do
    str = generator_string(i, align_size_64k)
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}        
    end
  end
  blocks = {}

  printf("128k align!\r\n")
  align_size_128k = 128 * 1024
  count_128k = max_size / align_size_128k
  for i = 0, count_128k do
      str = generator_string(i, align_size_128k)
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}     
      end
  end
  blocks = {}

  printf("512k align!\r\n")
  align_size_512k = 512 * 1024
  count_512k = max_size / align_size_512k
  for i = 0, count_512k do
      str = generator_string(i, align_size_512k)
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}     
      end
  end
  blocks = {}

  printf("1024k align!\r\n")
  align_size_1024k = 1024 * 1024
  count_1024k = max_size / align_size_1024k
  for i = 0, count_1024k do
      str = generator_string(i, align_size_1024k)
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}    
      end
  end
  blocks = {}

end

function noalign_block_test(max_size)
    local blocks = {}
  
    math.randomseed(os.time())
    min_size = 1
    printf("8b noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_8b = math.max(math.random(8), min_size)
        str = generator_string(i, align_size_8b)
        blocks[i] = str
        i = i + align_size_8b
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("16b noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_16b = math.max(math.random(16), min_size)
        str = generator_string(i, align_size_16b)
        blocks[i] = str
        i = i + align_size_16b
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("32b noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_32b = math.max(math.random(32), min_size)
        str = generator_string(i, align_size_32b)
        blocks[i] = str
        i = i + align_size_32b
        if (max_table_size < table.getn(blocks)) then
            blocks = {}        
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("64b noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_64b = math.max(math.random(64), min_size)
        str = generator_string(i, align_size_64b)
        blocks[i] = str
        i = i + align_size_64b
        if (max_table_size < table.getn(blocks)) then
            blocks = {}      
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("128b noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_128b = math.max(math.random(128), min_size)
        str = generator_string(i, align_size_128b)
        blocks[i] = str
        i = i + align_size_128b
        if (max_table_size < table.getn(blocks)) then
            blocks = {}    
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("256b noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_256b = math.max(math.random(256), min_size)
        str = generator_string(i, align_size_256b)
        blocks[i] = str
        i = i + align_size_256b
        if (max_table_size < table.getn(blocks)) then
            blocks = {}   
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("512b noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_512b = math.max(math.random(512), min_size)
        str = generator_string(i, align_size_512b)
        blocks[i] = str
        i = i + align_size_512b
        if (max_table_size < table.getn(blocks)) then
            blocks = {} 
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("1k noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_1k = math.max(math.random(1 * 1024), min_size)
        str = generator_string(i, align_size_1k)
        blocks[i] = str
        i = i + align_size_1k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("2k noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_2k = math.max(math.random(2 * 1024), min_size)
        str = generator_string(i, align_size_2k)
        blocks[i] = str
        i = i + align_size_2k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("4k noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_4k = math.max(math.random(4 * 1024), min_size)
        str = generator_string(i, align_size_4k)
        blocks[i] = str
        i = i + align_size_4k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("8k noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_8k = math.max(math.random(8 * 1024), min_size)
        str = generator_string(i, align_size_8k)
        blocks[i] = str
        i = i + align_size_8k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}        
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("16k noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_16k = math.max(math.random(16 * 1024), min_size)
        str = generator_string(i, align_size_16k)
        blocks[i] = str
        i = i + align_size_16k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}         
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("32k noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_32k = math.max(math.random(32 * 1024), min_size)
        str = generator_string(i, align_size_32k)
        blocks[i] = str
        i = i + align_size_32k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}        
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("64k noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_64k = math.max(math.random(64 * 1024), min_size)
        str = generator_string(i, align_size_64k)
        blocks[i] = str
        i = i + align_size_64k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}    
        end
    end
    blocks = {}


    math.randomseed(os.time())
    printf("128k noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_128k = math.max(math.random(128 * 1024), min_size)
        str = generator_string(i, align_size_128k)
        blocks[i] = str
        i = i + align_size_128k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}   
        end
    end
    blocks = {}

    math.randomseed(os.time())
    printf("512k noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_512k = math.max(math.random(512 * 1024), min_size)
        str = generator_string(i, align_size_512k)
        blocks[i] = str
        i = i + align_size_512k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}       
        end
    end
    blocks = {}

    math.randomseed(os.time())
    printf("1024k noalign!\r\n")
    i = 0
    while i < max_size do
        align_size_1024k = math.max(math.random(1024 * 1024), min_size)
        str = generator_string(i, align_size_1024k)
        blocks[i] = str
        i = i + align_size_1024k
        if (max_table_size < table.getn(blocks)) then
            blocks = {} 
        end
    end
    blocks = {}

end

function align_block_test_shared(max_size)
  local blocks = {}

 printf("8b align shared!\r\n")
  align_size_8b = 8
  count_8b = max_size / align_size_8b
  str = generator_string_shared(align_size_8b)
  for i = 0, count_8b do
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}  
      end
  end
  blocks = {}

  printf("16b align shared!\r\n")
  align_size_16b = 16
  count_16b = max_size / align_size_16b
  str = generator_string_shared(align_size_16b)
  for i = 0, count_16b do
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}
      end
  end
  blocks = {}

  printf("32b align shared!\r\n")
  align_size_32b = 32
  count_32b = max_size / align_size_32b
  str = generator_string_shared(align_size_32b)
  for i = 0, count_32b do
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}
      end
  end
  blocks = {}

  printf("64b align shared!\r\n")
  align_size_64b = 64
  count_64b = max_size / align_size_64b
  str = generator_string_shared(align_size_64b)
  for i = 0, count_64b do
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}
      end
  end
  blocks = {}

  printf("128b align shared!\r\n")
  align_size_128b = 128
  count_128b = max_size / align_size_128b
  str = generator_string_shared(align_size_128b)
  for i = 0, align_size_128b do
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}
      end
  end
  blocks = {}

  printf("512b align shared!\r\n")
  align_size_512b = 512
  count_512b = max_size / align_size_512b
  str = generator_string_shared(align_size_512b)
  for i = 0, count_512b do
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}
      end
  end
  blocks = {}

  printf("1k align shared!\r\n")
  align_size_1k = 1 * 1024
  count_1k = max_size / align_size_1k
  str = generator_string_shared(align_size_1k)
  for i = 0, count_1k do
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}
      end
  end
  blocks = {}

  printf("2k align shared!\r\n")
  align_size_2k = 2 * 1024
  count_2k = max_size / align_size_2k
  str = generator_string_shared(align_size_2k)
  for i = 0, count_2k do
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}
      end
  end
  blocks = {}

  printf("4k align shared!\r\n")
  align_size_4k = 4 * 1024
  count_4k = max_size / align_size_4k
  str = generator_string_shared(align_size_4k)
  for i = 0, count_4k do
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}
      end
  end
  blocks = {}

  printf("8k align shared!\r\n")
  align_size_8k = 8 * 1024
  count_8k = max_size / align_size_8k
  str = generator_string_shared(align_size_8k)
  for i = 0, count_8k do
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}
      end
  end
  blocks = {}

  printf("16k align shared!\r\n")
  align_size_16k = 16 * 1024
  count_16k = max_size / align_size_16k
  str = generator_string_shared(align_size_16k)
  for i = 0, count_16k do
      blocks[i] = str
      if (max_table_size < table.getn(blocks)) then
        blocks = {}
      end
  end
  blocks = {}

  printf("32k align shared!\r\n")
  align_size_32k = 32 * 1024
  count_32k = max_size / align_size_32k
  str = generator_string_shared(align_size_32k)
  for i = 0, count_32k do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}   
    end
  end
  blocks = {}

  printf("64k align shared!\r\n")
  align_size_64k = 64 * 1024
  count_64k = max_size / align_size_64k
  for i = 0, count_64k do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}  
    end
  end
  blocks = {}

  printf("128k align shared!\r\n")
  align_size_128k = 128 * 1024
  count_128k = max_size / align_size_128k
  str = generator_string_shared(align_size_128k)
  for i = 0, count_128k do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end
  blocks = {}

  printf("512k align shared!\r\n")
  align_size_512k = 512 * 1024
  count_512k = max_size / align_size_512k
  str = generator_string_shared(align_size_512k)
  for i = 0, count_512k do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}  
    end
  end
  blocks = {}

  printf("1024k align shared!\r\n")
  align_size_1024k = 1024 * 1024
  count_1024k = max_size / align_size_1024k
  str = generator_string_shared(align_size_1024k)
  for i = 0, count_1024k do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end

end

function align_block_test_shared(max_size)
  local blocks = {}

 printf("8b align shared!\r\n")
  align_size_8b = 8
  count_8b = max_size / align_size_8b
  str = generator_string_shared(8)
  
  for i = 0, count_8b do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end
  blocks = {}

  printf("16b align shared!\r\n")
  align_size_16b = 16
  count_16b = max_size / align_size_16b
  str = generator_string_shared(align_size_16b)
  
  for i = 0, align_size_16b do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end
  blocks = {}

  printf("32b align shared!\r\n")
  align_size_32b = 32
  count_32b = max_size / align_size_32b
  str = generator_string_shared(align_size_32b)
  for i = 0, count_32b do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end
  blocks = {}

  printf("64b align shared!\r\n")
  align_size_64b = 64
  count_64b = max_size / align_size_64b
  str = generator_string_shared(align_size_64b)
  for i = 0, count_64b do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end
  blocks = {}

  printf("128b align shared!\r\n")
  align_size_128b = 128
  count_128b = max_size / align_size_128b
  str = generator_string_shared(align_size_128b)
  for i = 0, align_size_128b do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end
  blocks = {}

  printf("512b align shared!\r\n")
  align_size_512b = 512
  count_512b = max_size / align_size_512b
  str = generator_string_shared(align_size_512b)
  for i = 0, count_512b do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end
  blocks = {}

  printf("1k align shared!\r\n")
  align_size_1k = 1 * 1024
  count_1k = max_size / align_size_1k
  str = generator_string_shared(align_size_1k)
  for i = 0, count_1k do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end
  blocks = {}

  printf("2k align shared!\r\n")
  align_size_2k = 2 * 1024
  count_2k = max_size / align_size_2k
  str = generator_string_shared(align_size_2k)
  for i = 0, count_2k do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end
  blocks = {}

  printf("4k align shared!\r\n")
  align_size_4k = 4 * 1024
  count_4k = max_size / align_size_4k
  str = generator_string_shared(align_size_4k)
  for i = 0, count_4k do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end
  blocks = {}

  printf("8k align shared!\r\n")
  align_size_8k = 8 * 1024
  count_8k = max_size / align_size_8k
  str = generator_string_shared(align_size_8k)
  for i = 0, count_8k do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {} 
    end
  end
  blocks = {}

  printf("16k align shared!\r\n")
  align_size_16k = 16 * 1024
  count_16k = max_size / align_size_16k
  str = generator_string_shared(align_size_16k)
  for i = 0, count_16k do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end
  blocks = {}

  printf("32k align shared!\r\n")
  align_size_32k = 32 * 1024
  count_32k = max_size / align_size_32k
  str = generator_string_shared(align_size_32k)
  for i = 0, count_32k do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end
  blocks = {}

  printf("64k align shared!\r\n")
  align_size_64k = 64 * 1024
  count_64k = max_size / align_size_64k
  str = generator_string_shared(align_size_64k)
  
  for i = 0, count_64k do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end
  blocks = {}

  printf("128k align shared!\r\n")
  align_size_128k = 128 * 1024
  count_128k = max_size / align_size_128k
  str = generator_string_shared(align_size_128k)
  
  for i = 0, count_128k do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end
  blocks = {}

  printf("512k align shared!\r\n")
  align_size_512k = 512 * 1024
  count_512k = max_size / align_size_512k
  str = generator_string_shared(align_size_512k)
  for i = 0, count_512k do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end
  blocks = {}

  printf("1024k align shared!\r\n")
  align_size_1024k = 1024 * 1024
  count_1024k = max_size / align_size_1024k
  str = generator_string_shared(align_size_1024k)
  
  for i = 0, count_1024k do
    blocks[i] = str
    if (max_table_size < table.getn(blocks)) then
        blocks = {}
    end
  end

end

function noalign_block_test_shared(max_size)
    local blocks = {}
  
    math.randomseed(os.time())
    min_size = 1
    printf("8b noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_8b = math.max(math.random(8), min_size)
        str = generator_string_shared(align_size_8b)
        blocks[i] = str
        i = i + align_size_8b
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("16b noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_16b = math.max(math.random(16), min_size)
        str = generator_string_shared(align_size_16b)
        blocks[i] = str
        i = i + align_size_16b
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("32b noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_32b = math.max(math.random(32), min_size)
        str = generator_string_shared(align_size_32b)
        blocks[i] = str
        i = i + align_size_32b
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("64b noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_64b = math.max(math.random(64), min_size)
        str = generator_string_shared(align_size_64b)
        blocks[i] = str
        i = i + align_size_64b
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("128b noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_128b = math.max(math.random(128), min_size)
        str = generator_string_shared(align_size_128b)
        blocks[i] = str
        i = i + align_size_128b
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("256b noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_256b = math.max(math.random(256), min_size)
        str = generator_string_shared(align_size_256b)
        blocks[i] = str
        i = i + align_size_256b
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("512b noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_512b = math.max(math.random(512), min_size)
        str = generator_string(i, align_size_512b)
        blocks[i] = str
        i = i + align_size_512b
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("1k noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_1k = math.max(math.random(1 * 1024), min_size)
        str = generator_string_shared(align_size_1k)
        blocks[i] = str
        i = i + align_size_1k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("2k noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_2k = math.max(math.random(2 * 1024), min_size)
        str = generator_string_shared(align_size_2k)
        blocks[i] = str
        i = i + align_size_2k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("4k noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_4k = math.max(math.random(4 * 1024), min_size)
        str = generator_string_shared(align_size_4k)
        blocks[i] = str
        i = i + align_size_4k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("8k noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_8k = math.max(math.random(8 * 1024), min_size)
        str = generator_string_shared(align_size_8k)
        blocks[i] = str
        i = i + align_size_8k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("16k noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_16k = math.max(math.random(16 * 1024), min_size)
        str = generator_string_shared(align_size_16k)
        blocks[i] = str
        i = i + align_size_16k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("32k noalign shared!\r\n")
    i = 0
    while i < max_size do
       align_size_32k = math.max(math.random(32 * 1024), min_size)
        str = generator_string_shared(align_size_32k)
        blocks[i] = str
        i = i + align_size_32k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    min_size = 1
    printf("64k noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_64k = math.max(math.random(64 * 1024), min_size)
        str = generator_string_shared(align_size_64k)
        blocks[i] = str
        i = i + align_size_64k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    printf("128k noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_128k = math.max(math.random(128 * 1024), min_size)
        str = generator_string_shared(align_size_128k)
        blocks[i] = str
        i = i + align_size_128k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    printf("512k noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_512k = math.max(math.random(512 * 1024), min_size)
        str = generator_string_shared(align_size_512k)
        blocks[i] = str
        i = i + align_size_512k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

    math.randomseed(os.time())
    printf("1024k noalign shared!\r\n")
    i = 0
    while i < max_size do
        align_size_1024k = math.max(math.random(1024 * 1024), min_size)
        str = generator_string_shared(align_size_1024k)
        blocks[i] = str
        i = i + align_size_1024k
        if (max_table_size < table.getn(blocks)) then
            blocks = {}
        end
    end
    blocks = {}

end

local unit_mb = 1024 * 1024
local max_mb = 900
local start = 1
local  step = 1
    
for i = start, max_mb, step do
  local size = i * unit_mb
  printf("<----------align shared %d m----------->\r\n", i)
  align_block_test_shared(size)
  printf("<----------noalign shared %d m----------->\r\n", i)
  noalign_block_test_shared(size)
  printf("<----------align %d m----------->\r\n", i)
  align_block_test(size)
  printf("<----------noalign %d m----------->\r\n", i)
  noalign_block_test(size)
end