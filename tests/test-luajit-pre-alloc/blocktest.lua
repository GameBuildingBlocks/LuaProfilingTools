local switch_4b = 1
local switch_8b = 1
local switch_16b = 1
local switch_32b = 1
local switch_64b = 1
local switch_128b = 1
local switch_256b = 1
local switch_512b = 1
--[[ local switch_1k = 1
local switch_2k = 1
local switch_4k = 1
local switch_8k = 1
local switch_16k = 1
local switch_32k = 1
local switch_64k = 1
local switch_128k = 1
local switch_256k = 1
local switch_512k = 1
local switch_1024k = 1  ]] 

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

function get_max_table_size(s)
    return math.max(1, 10 * 1024 / s);
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

    if switch_4b then
        local blocks4b = {}
        local align_size_4b = 4
        local count_4b = max_size / align_size_4b
        local max_table_size = get_max_table_size(align_size_4b)
        printf("4b align!-%d\r\n",max_table_size);
        for i = 0, count_4b do
            local str = generator_string(i, align_size_4b)
            blocks4b[i] = str
            if (max_table_size < table.getn(blocks4b)) then
                blocks4b = {}
            end
        end
        blocks4b = {}
        blocks4b = nil
    end

    if switch_8b then
        local blocks8b = {}
        local align_size_8b = 8
        local count_8b = max_size / align_size_8b
        local max_table_size = get_max_table_size(align_size_8b)
        printf("8b align!-%d\r\n",max_table_size);
        for i = 0, count_8b do
            local str = generator_string(i, align_size_8b)
            blocks8b[i] = str
            if (max_table_size < table.getn(blocks8b)) then
                blocks8b = {}
            end
        end
        blocks8b = {}
        blocks8b = nil
    end

    if switch_16b then
        local blocks16b = {}
        local align_size_16b = 16
        local count_16b = max_size / align_size_16b
        local max_table_size = get_max_table_size(align_size_16b)
        printf("16b align-%d!\r\n",max_table_size)
        for i = 0, align_size_16b do
            local str = generator_string(i, align_size_16b)
            blocks16b[i] = str
            if (max_table_size < table.getn(blocks16b)) then
                blocks16b = {}
            end
        end
        blocks16b = {}
        blocks16b = nil
    end

    if switch_32b then
        local blocks32b = {}
        local align_size_32b = 32
        local count_32b = max_size / align_size_32b
        local max_table_size = get_max_table_size(align_size_32b)
        printf("32b align-%d!\r\n", max_table_size)
        for i = 0, count_32b do
            local str = generator_string(i, align_size_32b)
            blocks32b[i] = str
            if (max_table_size < table.getn(blocks32b)) then
                blocks32b = {}
            end
        end
        blocks32b = {}
        blocks32b = nil
    end

    if switch_64b then
        local blocks64b = {}
        local align_size_64b = 64
        local count_64b = max_size / align_size_64b
        local max_table_size = get_max_table_size(align_size_64b)
        printf("64b align-%d!\r\n", max_table_size)
        for i = 0, count_64b do
            local str = generator_string(i, align_size_64b)
            blocks64b[i] = str
            if (max_table_size < table.getn(blocks64b)) then
                blocks64b = {}
            end
        end
        blocks64b = {}
        blocks64b = nil
    end

    if switch_128b then
        local blocks128b = {}
        local align_size_128b = 128
        local count_128b = max_size / align_size_128b
        local max_table_size = get_max_table_size(align_size_128b)
        printf("128b align!-%d\r\n", max_table_size)
        for i = 0, align_size_128b do
            local str = generator_string(i, align_size_128b)
            blocks128b[i] = str
            if (max_table_size < table.getn(blocks128b)) then
                blocks128b = {}
            end
        end
        blocks128b = {}
        blocks128b = nil
    end

    if switch_512b then
        local blocks512b = {}
        local align_size_512b = 512
        local count_512b = max_size / align_size_512b
        local max_table_size = get_max_table_size(align_size_512b)
        printf("512b align-%d!\r\n", max_table_size)
        for i = 0, count_512b do
            local str = generator_string(i, align_size_512b)
            blocks512b[i] = str
            if (max_table_size < table.getn(blocks512b)) then
                blocks512b = {}
            end
        end
        blocks512b = {}
        blocks512b = nil
    end

    if switch_1k then
        local blocks1k = {}
        local align_size_1k = 1 * 1024
        local count_1k = max_size / align_size_1k
        local max_table_size = get_max_table_size(align_size_1k)
        printf("1k align-%d!\r\n", max_table_size)
        for i = 0, count_1k do
            local str = generator_string(i, align_size_1k)
            blocks1k[i] = str
            if (max_table_size < table.getn(blocks1k)) then
                blocks1k = {}
            end
        end
        blocks1k = {}
        blocks1k = nil
    end

    if switch_2k then
        local blocks2k = {}
        local align_size_2k = 2 * 1024
        local count_2k = max_size / align_size_2k
        local max_table_size = get_max_table_size(align_size_2k)
        printf("2k align-%d!\r\n", max_table_size)
        for i = 0, count_2k do
            local str = generator_string(i, align_size_2k)
            blocks2k[i] = str
            if (max_table_size < table.getn(blocks2k)) then
                blocks2k = {}
            end
        end
        blocks2k = {}
        blocks2k = nil
    end

    if switch_4k then
        local blocks4k = {}
        local align_size_4k = 4 * 1024
        local count_4k = max_size / align_size_4k
        local max_table_size = get_max_table_size(align_size_4k)
        printf("4k align-%d!\r\n", max_table_size)
        for i = 0, count_4k do
            local str = generator_string(i, align_size_4k)
            blocks4k[i] = str
            if (max_table_size < table.getn(blocks4k)) then
                blocks4k = {}
            end
        end
        blocks4k = {}
        blocks4k = nil
    end

    if switch_8k then
        local blocks8k = {}
        local align_size_8k = 8 * 1024
        local count_8k = max_size / align_size_8k
        local max_table_size = get_max_table_size(align_size_8k)
        printf("8k align-%d!\r\n", max_table_size)
        for i = 0, count_8k do
            local str = generator_string(i, align_size_8k)
            blocks8k[i] = str
            if (max_table_size < table.getn(blocks8k)) then
                blocks8k = {}
            end
        end
        blocks8k = {}
        blocks8k = nil
    end

    if switch_16k then
        local blocks16k = {}
        local align_size_16k = 16 * 1024
        local count_16k = max_size / align_size_16k
        local max_table_size = get_max_table_size(align_size_16k)
        printf("16k align-%d!\r\n", max_table_size)
        for i = 0, count_16k do
            local str = generator_string(i, align_size_16k)
            blocks16k[i] = str
            if (max_table_size < table.getn(blocks16k)) then
                blocks16k = {}
            end
        end
        blocks16k = {}
        blocks16k = nil
    end

    if switch_32k then
        local blocks32k = {}
        local align_size_32k = 32 * 1024
        local count_32k = max_size / align_size_32k
        local max_table_size = get_max_table_size(align_size_32k)
        printf("32k align-%d!\r\n", max_table_size)
        for i = 0, count_32k do
            local str = generator_string(i, align_size_32k)
            blocks32k[i] = str
            if (max_table_size < table.getn(blocks32k)) then
                blocks32k = {}
            end
        end
        blocks32k = {}
        blocks32k = nil
    end

    if switch_64k then
        local blocks64k = {}
        local align_size_64k = 64 * 1024
        local count_64k = max_size / align_size_64k
        local max_table_size = get_max_table_size(align_size_64k)
        printf("64k align-%d!\r\n", max_table_size)
        for i = 0, count_64k do
            local str = generator_string(i, align_size_64k)
            blocks64k[i] = str
            if (max_table_size < table.getn(blocks64k)) then
                blocks64k = {}
            end
        end
        blocks64k = {}
        blocks64k = nil
    end

    if switch_128k then
        local blocks128k = {}
        local align_size_128k = 128 * 1024
        local count_128k = max_size / align_size_128k
        local max_table_size = get_max_table_size(align_size_128k)
        printf("128k align-%d!\r\n", max_table_size)
        for i = 0, count_128k do
            local str = generator_string(i, align_size_128k)
            blocks128k[i] = str
            if (max_table_size < table.getn(blocks128k)) then
                blocks128k = {}
            end
        end
        blocks128k = {}
        blocks128k = nil
    end

    if switch_256k then
        local blocks256k = {}
        local align_size_256k = 256 * 1024
        local count_256k = max_size / align_size_256k
        local max_table_size = get_max_table_size(align_size_256k)
        printf("256k align-%d!\r\n", max_table_size)
        for i = 0, count_256k do
            local str = generator_string(i, align_size_256k)
            blocks256k[i] = str
            if (max_table_size < table.getn(blocks256k)) then
                blocks256k = {}
            end
        end
        blocks256k = {}
        blocks256k = nil
    end

    if switch_512k then
        local blocks512k = {}
        local align_size_512k = 512 * 1024
        local count_512k = max_size / align_size_512k
        local max_table_size = get_max_table_size(align_size_512k)
        printf("512k align-%d!\r\n", max_table_size)
        for i = 0, count_512k do
            local str = generator_string(i, align_size_512k)
            blocks512k[i] = str
            if (max_table_size < table.getn(blocks512k)) then
                blocks512k = {}
            end
        end
        blocks512k = {}
        blocks512k = nil
    end

    if switch_1024k then
        local blocks1024k = {}
        local align_size_1024k = 1024 * 1024
        local count_1024k = max_size / align_size_1024k
        local max_table_size = get_max_table_size(align_size_1024k)
        printf("1024k align-%d!\r\n", max_table_size)
        for i = 0, count_1024k do
            local str = generator_string(i, align_size_1024k)
            blocks1024k[i] = str
            if (max_table_size < table.getn(blocks1024k)) then
                blocks1024k = {}
            end
        end
        blocks1024k = {}
        blocks1024k = nil
  end

end

function noalign_block_test(max_size)

    if switch_4b then
        math.randomseed(os.time())
        local blocks4b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(4)
        printf("4b noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_4b = math.random(min_size, 4)
            local str = generator_string(i, align_size_4b)
            blocks4b[i] = str
            i = i + align_size_4b
            if (max_table_size < table.getn(blocks4b)) then
                blocks4b = {}
            end
        end
        blocks4b = {}
        blocks4b = nil
    end

    if switch_8b then
        math.randomseed(os.time())
        local blocks8b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(8)
        printf("8b noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_8b = math.random(min_size, 8)
            local str = generator_string(i, align_size_8b)
            blocks8b[i] = str
            i = i + align_size_8b
            if (max_table_size < table.getn(blocks8b)) then
                blocks8b = {}
            end
        end
        blocks8b = {}
        block8b = nil
    end

    if switch_16b then
        math.randomseed(os.time())
        local blocks16b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(16)
        printf("16b noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_16b = math.random(min_size, 16)
            local str = generator_string(i, align_size_16b)
            blocks16b[i] = str
            i = i + align_size_16b
            if (max_table_size < table.getn(blocks16b)) then
                blocks16b = {}
            end
        end
        blocks16b = {}
        blocks16b = nil
    end

    if switch_32b then
        math.randomseed(os.time())
        local blocks32b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(32)
        printf("32b noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_32b = math.random(min_size, 32)
            local str = generator_string(i, align_size_32b)
            blocks32b[i] = str
            i = i + align_size_32b
            if (max_table_size < table.getn(blocks32b)) then
                blocks32b = {}
            end
        end
        blocks32b = {}
        blocks32b = nil
    end

    if switch_64b then
        math.randomseed(os.time())
        local blocks64b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(64)
        printf("64b noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_64b = math.random(min_size, 64)
            local str = generator_string(i, align_size_64b)
            blocks64b[i] = str
            i = i + align_size_64b
            if (max_table_size < table.getn(blocks64b)) then
                blocks64b = {}
            end
        end
        blocks64b = {}
        blocks64b = nil
    end

    if switch_128b then
        math.randomseed(os.time())
        local blocks128b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(128)
        printf("128b noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_128b = math.random(min_size, 128)
            local str = generator_string(i, align_size_128b)
            blocks128b[i] = str
            i = i + align_size_128b
            if (max_table_size < table.getn(blocks128b)) then
                blocks128b = {}
            end
        end
        blocks128b = {}
        blocks128b = nil
    end

    if switch_256b then
        math.randomseed(os.time())
        local blocks256b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(256)
        printf("256b noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            align_size_256b = math.random(min_size, 256)
            local str = generator_string(i, align_size_256b)
            blocks256b[i] = str
            i = i + align_size_256b
            if (max_table_size < table.getn(blocks256b)) then
                blocks256b = {}
            end
        end
        blocks256b = {}
        blocks256b = nil
    end

    if switch_512b then
        math.randomseed(os.time())
        local blocks512b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(512)
        printf("512b noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_512b = math.random(min_size, 512)
            local str = generator_string(i, align_size_512b)
            blocks512b[i] = str
            i = i + align_size_512b
            if (max_table_size < table.getn(blocks512b)) then
                blocks512b = {}
            end
        end
        blocks512b = {}
        blocks512b = nil
    end

    if switch_1k then
        math.randomseed(os.time())
        local blocks1k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(1 * 1024)
        printf("1k noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_1k = math.random(min_size, 1 * 1024)
            local str = generator_string(i, align_size_1k)
            blocks1k[i] = str
            i = i + align_size_1k
            if (max_table_size < table.getn(blocks1k)) then
                blocks1k = {}
            end
        end
        blocks1k = {}
        blocks1k = nil
    end

    if switch_2k then
        math.randomseed(os.time())
        local blocks2k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(2 * 1024)
        printf("2k noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_2k = math.random(min_size, 2 * 1024)
            local str = generator_string(i, align_size_2k)
            blocks2k[i] = str
            i = i + align_size_2k
            if (max_table_size < table.getn(blocks2k)) then
                blocks2k = {}
            end
        end
        blocks2k = {}
        blocks2k = nil
    end

    if switch_4k then
        math.randomseed(os.time())
        local blocks4k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(4 * 1024)
        printf("4k noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_4k = math.random(min_size, 4 * 1024)
            local str = generator_string(i, align_size_4k)
            blocks4k[i] = str
            i = i + align_size_4k
            if (max_table_size < table.getn(blocks4k)) then
                blocks4k = {}
            end
        end
        blocks4k = {}
        blocks4k = nil
    end

    if switch_8k then
        math.randomseed(os.time())
        local blocks8k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(8 * 1024)
        printf("8k noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_8k = math.random(min_size, 8 * 1024)
            local str = generator_string(i, align_size_8k)
            blocks8k[i] = str
            i = i + align_size_8k
            if (max_table_size < table.getn(blocks8k)) then
                blocks8k = {}
            end
        end
        blocks8k = {}
        blocks8k = nil
    end

    if switch_16k then
        math.randomseed(os.time())
        local blocks16k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(16 * 1024)
        printf("16k noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_16k = math.random(min_size, 16 * 1024)
            local str = generator_string(i, align_size_16k)
            blocks16k[i] = str
            i = i + align_size_16k
            if (max_table_size < table.getn(blocks16k)) then
                blocks16k = {}
            end
        end
        blocks16k = {}
        blocks16k = nil
    end

    if switch_32k then
        math.randomseed(os.time())
        local blocks32k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(32 * 1024)
        printf("32k noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_32k = math.random(min_size, 32 * 1024)
            local str = generator_string(i, align_size_32k)
            blocks32k[i] = str
            i = i + align_size_32k
            if (max_table_size < table.getn(blocks32k)) then
                blocks32k = {}
            end
        end
        blocks32k = {}
        blocks32k = nil
    end

    if switch_64k then
        math.randomseed(os.time())
        local blocks64k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(64 * 1024)
        printf("64k noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            align_size_64k = math.random(min_size, 64 * 1024)
            local str = generator_string(i, align_size_64k)
            blocks64k[i] = str
            i = i + align_size_64k
            if (max_table_size < table.getn(blocks64k)) then
                blocks64k = {}
            end
        end
        blocks64k = {}
        blocks64k = nil
    end

    if switch_128k then
        math.randomseed(os.time())
        local blocks128k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(128 * 1024)
        printf("128k noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            align_size_128k = math.random(min_size, 128 * 1024)
            local str = generator_string(i, align_size_128k)
            blocks128k[i] = str
            i = i + align_size_128k
            if (max_table_size < table.getn(blocks128k)) then
                blocks128k = {}
            end
        end
        blocks128k = {}
        blocks128k = nil
    end

    if switch_256k then
        math.randomseed(os.time())
        local blocks256k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(256 * 1024)
        printf("256k noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_256k = math.random(min_size, 256 * 1024)
            local str = generator_string(i, align_size_256k)
            blocks256k[i] = str
            i = i + align_size_256k
            if (max_table_size < table.getn(blocks256k)) then
                blocks256k = {}
            end
        end
        blocks256k = {}
        blocks256k = nil
    end

    if switch_512k then
        math.randomseed(os.time())
        local blocks512k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(512 * 1024)
        printf("512k noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            align_size_512k = math.random(min_size, 512 * 1024)
            local str = generator_string(i, align_size_512k)
            blocks512k[i] = str
            i = i + align_size_512k
            if (max_table_size < table.getn(blocks512k)) then
                blocks512k = {}
            end
        end
        blocks512k = {}
        blocks512k = nil
    end

    if switch_1024k then
        math.randomseed(os.time())
        local blocks1024k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(1024 * 1024)
        printf("1024k noalign-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            align_size_1024k = math.random(min_size, 1024 * 1024)
            local str = generator_string(i, align_size_1024k)
            blocks1024k[i] = str
            i = i + align_size_1024k
            if (max_table_size < table.getn(blocks1024k)) then
                blocks1024k = {}
            end
        end
        blocks1024k = {}
        blocks1024k = nil
    end

end

function align_block_test_shared(max_size)

    if switch_4b then
        local blocks4b = {}
        local align_size_4b = 4
        local count_4b = max_size / align_size_4b
        local max_table_size = get_max_table_size(align_size_4b)
        printf("4b align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_4b)
        for i = 0, count_4b do
            blocks4b[i] = str
            if (max_table_size < table.getn(blocks4b)) then
                blocks4b = {}
            end
        end
        blocks4b = {}
        blocks4b = nil
    end

    if switch_8b then
        local blocks8b = {}
        local align_size_8b = 8
        local count_8b = max_size / align_size_8b
        local max_table_size = get_max_table_size(align_size_8b)
        printf("8b align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_8b)
        for i = 0, count_8b do
            blocks8b[i] = str
            if (max_table_size < table.getn(blocks8b)) then
                blocks8b = {}
            end
        end
        blocks8b = {}
        blocks8b = nil
    end

    if switch_16b then
        local blocks16b = {}
        local align_size_16b = 16
        local count_16b = max_size / align_size_16b
        local max_table_size = get_max_table_size(align_size_16b)
        printf("16b align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_16b)
        for i = 0, count_16b do
            blocks16b[i] = str
            if (max_table_size < table.getn(blocks16b)) then
                blocks16b = {}
            end
        end
        blocks16b = {}
        blocks16b = nil
    end

    if switch_32b then
        local blocks32b = {}
        local align_size_32b = 32
        local count_32b = max_size / align_size_32b
        local max_table_size = get_max_table_size(align_size_32b)
        printf("32b align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_32b)
        for i = 0, count_32b do
            blocks32b[i] = str
            if (max_table_size < table.getn(blocks32b)) then
                blocks32b = {}
            end
        end
        blocks32b = {}
        blocks32b = nil
    end

    if switch_64b then
        local blocks64b = {}
        local align_size_64b = 64
        local count_64b = max_size / align_size_64b
        local max_table_size = get_max_table_size(align_size_64b)
        printf("64b align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_64b)
        for i = 0, count_64b do
            blocks64b[i] = str
            if (max_table_size < table.getn(blocks64b)) then
                blocks64b = {}
            end
        end
        blocks64b = {}
        blocks64b = nil
    end

    if switch_128b then
        local blocks128b = {}
        local align_size_128b = 128
        local count_128b = max_size / align_size_128b
        local max_table_size = get_max_table_size(align_size_128b)
        printf("128b align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_128b)
        for i = 0, align_size_128b do
            blocks128b[i] = str
            if (max_table_size < table.getn(blocks128b)) then
                blocks128b = {}
            end
        end
        blocks128b = {}
        blocks128b = nil
    end

    if switch_512b then
        local blocks512b = {}
        local align_size_512b = 512
        local count_512b = max_size / align_size_512b
        local max_table_size = get_max_table_size(align_size_512b)
        printf("512b align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_512b)
        for i = 0, count_512b do
            blocks512b[i] = str
            if (max_table_size < table.getn(blocks512b)) then
                blocks512b = {}
            end
        end
        blocks512b = {}
        blocks512b = nil
    end

    if switch_1k then
        local blocks1k = {}
        local align_size_1k = 1 * 1024
        local count_1k = max_size / align_size_1k
        local max_table_size = get_max_table_size(align_size_1k)
        printf("1k align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_1k)
        for i = 0, count_1k do
            blocks1k[i] = str
            if (max_table_size < table.getn(blocks1k)) then
                blocks1k = {}
            end
        end
        blocks1k = {}
        blocks1k = nil
    end

    if switch_2k then
        local blocks2k = {}
        local align_size_2k = 2 * 1024
        local count_2k = max_size / align_size_2k
        local max_table_size = get_max_table_size(align_size_2k)
        printf("2k align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_2k)
        for i = 0, count_2k do
            blocks2k[i] = str
            if (max_table_size < table.getn(blocks2k)) then
                blocks2k = {}
            end
        end
        blocks2k = {}
        blocks2k = nil
    end

    if switch_4k then
        local blocks4k = {}
        local align_size_4k = 4 * 1024
        local count_4k = max_size / align_size_4k
        local max_table_size = get_max_table_size(align_size_4k)
        printf("4k align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_4k)
        for i = 0, count_4k do
            blocks4k[i] = str
            if (max_table_size < table.getn(blocks4k)) then
                blocks4k = {}
            end
        end
        blocks4k = {}
        blocks4k = nil
    end

    if switch_8k then
        local blocks8k = {}
        local align_size_8k = 8 * 1024
        local count_8k = max_size / align_size_8k
        local max_table_size = get_max_table_size(align_size_8k)
        printf("8k align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_8k)
        for i = 0, count_8k do
            blocks8k[i] = str
            if (max_table_size < table.getn(blocks8k)) then
                blocks8k = {}
            end
        end
        blocks8k = {}
        blocks8k = nil
    end

    if switch_16k then
        local blocks16k = {}
        local align_size_16k = 16 * 1024
        local count_16k = max_size / align_size_16k
        local max_table_size = get_max_table_size(align_size_16k)
        printf("16k align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_16k)
        for i = 0, count_16k do
            blocks16k[i] = str
            if (max_table_size < table.getn(blocks16k)) then
                blocks16k = {}
            end
        end
        blocks16k = {}
        blocks16k = nil
    end

    if switch_32k then
        local blocks32k = {}
        local align_size_32k = 32 * 1024
        local count_32k = max_size / align_size_32k
        local max_table_size = get_max_table_size(align_size_32k)
        printf("32k align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_32k)
        for i = 0, count_32k do
            blocks32k[i] = str
            if (max_table_size < table.getn(blocks32k)) then
                blocks32k = {}
            end
        end
        blocks32k = {}
        blocks32k = nil
    end

    if switch_64k then
        local blocks64k = {}
        local align_size_64k = 64 * 1024
        local count_64k = max_size / align_size_64k
        local max_table_size = get_max_table_size(align_size_64k)
        printf("64k align shared-%d!\r\n", max_table_size)
        for i = 0, count_64k do
            blocks64k[i] = str
            if (max_table_size < table.getn(blocks64k)) then
                blocks64k = {}
            end
        end
        blocks64k = {}
        blocks64k = nil
    end

    if switch_128k then
        local blocks128k = {}
        local align_size_128k = 128 * 1024
        local count_128k = max_size / align_size_128k
        local max_table_size = get_max_table_size(align_size_128k)
        printf("128k align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_128k)
        for i = 0, count_128k do
            blocks128k[i] = str
            if (max_table_size < table.getn(blocks128k)) then
                blocks128k = {}
            end
        end
        blocks128k = {}
        blocks128k = nil
    end

    if switch_256k then
        local blocks256k = {}
        local align_size_256k = 256 * 1024
        local count_256k = max_size / align_size_256k
        local max_table_size = get_max_table_size(align_size_256k)
        printf("256k align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_256k)
        for i = 0, count_256k do
            blocks256k[i] = str
            if (max_table_size < table.getn(blocks256k)) then
                blocks256k = {}
            end
        end
        blocks256k = {}
        blocks256k = nil
    end

    if switch_512k then
        local blocks512k = {}
        local align_size_512k = 512 * 1024
        local count_512k = max_size / align_size_512k
        local max_table_size = get_max_table_size(align_size_512k)
        printf("512k align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_512k)
        for i = 0, count_512k do
            blocks512k[i] = str
            if (max_table_size < table.getn(blocks512k)) then
                blocks512k = {}
            end
        end
        blocks512k = {}
        blocks512k = nil
    end

    if switch_1024k then
        local blocks1024k = {}
        local align_size_1024k = 1024 * 1024
        local count_1024k = max_size / align_size_1024k
        local max_table_size = get_max_table_size(align_size_1024k)
        printf("1024k align shared-%d!\r\n", max_table_size)
        local str = generator_string_shared(align_size_1024k)
        for i = 0, count_1024k do
            blocks1024k[i] = str
            if (max_table_size < table.getn(blocks1024k)) then
                blocks1024k = {}
            end
        end
        blocks1024k = {}
        blocks1024k = nil
    end

end

function noalign_block_test_shared(max_size)

    if switch_4b then
        math.randomseed(os.time())
        local blocks4b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(4)
        printf("4b noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_4b = math.random(min_size, 4)
            local str = generator_string_shared(align_size_4b)
            blocks4b[i] = str
            i = i + align_size_4b
            if (max_table_size < table.getn(blocks4b)) then
                blocks4b = {}
            end
        end
        blocks4b = {}
        blocks4b = nil
    end

    if switch_8b then
        math.randomseed(os.time())
        local blocks8b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(8)
        printf("8b noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_8b = math.random(min_size, 8)
            local str = generator_string_shared(align_size_8b)
            blocks8b[i] = str
            i = i + align_size_8b
            if (max_table_size < table.getn(blocks8b)) then
                blocks8b = {}
            end
        end
        blocks8b = {}
        blocks8b = nil
    end

    if switch_16b then
        math.randomseed(os.time())
        local blocks16b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(16)
        printf("16b noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_16b = math.random(min_size, 16)
            local str = generator_string_shared(align_size_16b)
            blocks16b[i] = str
            i = i + align_size_16b
            if (max_table_size < table.getn(blocks16b)) then
                blocks16b = {}
            end
        end
        blocks16b = {}
        blocks16b = nil
    end

    if switch_32b then
        math.randomseed(os.time())
        local blocks32b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(32)
        printf("32b noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_32b = math.random(min_size, 32)
            local str = generator_string_shared(align_size_32b)
            blocks32b[i] = str
            i = i + align_size_32b
            if (max_table_size < table.getn(blocks32b)) then
                blocks32b = {}
            end
        end
        blocks32b = {}
        blocks32b = nil
    end

    if switch_64b then
        math.randomseed(os.time())
        local blocks64b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(64)
        printf("64b noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_64b = math.random(min_size, 64)
            local str = generator_string_shared(align_size_64b)
            blocks64b[i] = str
            i = i + align_size_64b
            if (max_table_size < table.getn(blocks64b)) then
                blocks64b = {}
            end
        end
        blocks64b = {}
        blocks64b = nil
    end

    if switch_128b then
        math.randomseed(os.time())
        local blocks128b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(128)
        printf("128b noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_128b = math.random(min_size, 128)
            local str = generator_string_shared(align_size_128b)
            blocks128b[i] = str
            i = i + align_size_128b
            if (max_table_size < table.getn(blocks128b)) then
                blocks128b = {}
            end
        end
        blocks128b = {}
        blocks128b = nil
    end

    if switch_256b then
        math.randomseed(os.time())
        local blocks256b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(256)
        printf("256b noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            align_size_256b = math.random(min_size, 256)
            local str = generator_string_shared(align_size_256b)
            blocks256b[i] = str
            i = i + align_size_256b
            if (max_table_size < table.getn(blocks256b)) then
                blocks256b = {}
            end
        end
        blocks256b = {}
        blocks256b = nil
    end

    if switch_512b then
        math.randomseed(os.time())
        local blocks512b = {}
        local min_size = 1
        local max_table_size = get_max_table_size(512)
        printf("512b noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_512b = math.random(min_size, 512)
            local str = generator_string(i, align_size_512b)
            blocks512b[i] = str
            i = i + align_size_512b
            if (max_table_size < table.getn(blocks512b)) then
                blocks512b = {}
            end
        end
        blocks512b = {}
        blocks512b = nil
    end

    if switch_1k then
        math.randomseed(os.time())
        local blocks1k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(1 * 1024)
        printf("1k noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_1k = math.random(min_size, 1 * 1024)
            local str = generator_string_shared(align_size_1k)
            blocks1k[i] = str
            i = i + align_size_1k
            if (max_table_size < table.getn(blocks1k)) then
                blocks1k = {}
            end
        end
        blocks1k = {}
        blocks1k = nil
    end

    if switch_2k then
        math.randomseed(os.time())
        local blocks2k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(2 * 1024)
        printf("2k noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_2k = math.random(min_size, 2 * 1024)
            local str = generator_string_shared(align_size_2k)
            blocks2k[i] = str
            i = i + align_size_2k
            if (max_table_size < table.getn(blocks2k)) then
                blocks2k = {}
            end
        end
        blocks2k = {}
        blocks2k = nil
    end

    if switch_4k then
        math.randomseed(os.time())
        local blocks4k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(4 * 1024)
        printf("4k noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_4k = math.random(min_size, 4 * 1024)
            local str = generator_string_shared(align_size_4k)
            blocks4k[i] = str
            i = i + align_size_4k
            if (max_table_size < table.getn(blocks4k)) then
                blocks4k = {}
            end
        end
        blocks4k = {}
        blocks4k = nil
    end

    if switch_8k then
        math.randomseed(os.time())
        local blocks8k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(8 * 1024)
        printf("8k noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_8k = math.random(min_size, 8 * 1024)
            local str = generator_string_shared(align_size_8k)
            blocks8k[i] = str
            i = i + align_size_8k
            if (max_table_size < table.getn(blocks8k)) then
                blocks8k = {}
            end
        end
        blocks8k = {}
        blocks8k = nil
    end

    if switch_16k then
        math.randomseed(os.time())
        local blocks16k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(16 * 1024)
        printf("16k noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_16k = math.random(min_size, 16 * 1024)
            local str = generator_string_shared(align_size_16k)
            blocks16k[i] = str
            i = i + align_size_16k
            if (max_table_size < table.getn(blocks16k)) then
                blocks16k = {}
            end
        end
        blocks16k = {}
        blocks16k = nil
    end

    if switch_32k then
        math.randomseed(os.time())
        local blocks32k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(32 * 1024)
        printf("32k noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
        local align_size_32k = math.random(min_size, 32 * 1024)
            local str = generator_string_shared(align_size_32k)
            blocks32k[i] = str
            i = i + align_size_32k
            if (max_table_size < table.getn(blocks32k)) then
                blocks32k = {}
            end
        end
        blocks32k = {}
        blocks32k = nil
    end

    if switch_64k then
        math.randomseed(os.time())
        local blocks64k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(64 * 1024)
        printf("64k noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            align_size_64k = math.random(min_size, 64 * 1024)
            local str = generator_string_shared(align_size_64k)
            blocks64k[i] = str
            i = i + align_size_64k
            if (max_table_size < table.getn(blocks64k)) then
                blocks64k = {}
            end
        end
        blocks64k = {}
        blocks64k = nil
    end

    if switch_128k then
        math.randomseed(os.time())
        local blocks128k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(128 * 1024)
        printf("128k noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            align_size_128k = math.random(min_size, 128 * 1024)
            local str = generator_string_shared(align_size_128k)
            blocks128k[i] = str
            i = i + align_size_128k
            if (max_table_size < table.getn(blocks128k)) then
                blocks128k = {}
            end
        end
        blocks128k = {}
        blocks128k = nil
    end

    if switch_256k then
        math.randomseed(os.time())
        local blocks256k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(256 * 1024)
        printf("256k noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            local align_size_256k = math.random(min_size, 256 * 1024)
            local str = generator_string_shared(align_size_256k)
            blocks256k[i] = str
            i = i + align_size_256k
            if (max_table_size < table.getn(blocks256k)) then
                blocks256k = {}
            end
        end
        blocks256k = {}
        blocks256k = nil
    end

    if switch_512k then
        math.randomseed(os.time())
        local blocks512k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(512 * 1024)
        printf("512k noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            align_size_512k = math.random(min_size, 512 * 1024)
            local str = generator_string_shared(align_size_512k)
            blocks512k[i] = str
            i = i + align_size_512k
            if (max_table_size < table.getn(blocks512k)) then
                blocks512k = {}
            end
        end
        blocks512k = {}
        blocks512k = nil
    end

    if switch_1024k then
        math.randomseed(os.time())
        local blocks1024k = {}
        local min_size = 1
        local max_table_size = get_max_table_size(1024 * 1024)
        printf("1024k noalign shared-%d!\r\n", max_table_size)
        i = 0
        while i < max_size do
            align_size_1024k = math.random(min_size, 1024 * 1024)
            local str = generator_string_shared(align_size_1024k)
            blocks1024k[i] = str
            i = i + align_size_1024k
            if (max_table_size < table.getn(blocks1024k)) then
                blocks1024k = {}
            end
        end
        blocks1024k = {}
        blocks1024k = nil
    end

end

local start_time = os.clock()
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

printf("use time:%ds\r\n", os.clock() - start_time)