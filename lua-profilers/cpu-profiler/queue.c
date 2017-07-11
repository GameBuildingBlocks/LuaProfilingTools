#include "lp.h"
#include "queue.h"
#include "stdlib.h"
#include <stdio.h>
#include <string.h>




int queue_alloctor_init(void)
{
    int ret_val = SUCCESS;
    return ret_val;
}

int queue_alloctor_uninit(void)
{
    int ret_val = SUCCESS;
    return ret_val;
}

int queue_init(QUEUE *queue, unsigned int capacity)
{
    int ret_val = SUCCESS;
    QUEUE_NODE *free_node = NULL;

    if(capacity < MIN_QUEUE_CAPACITY)
        capacity = MIN_QUEUE_CAPACITY;

    memset(queue, 0, sizeof(QUEUE));
    QINT_SET_VALUE_1(queue->_queue_capacity, capacity);

    /* pre-alloc two node */


	free_node = (QUEUE_NODE*)malloc(sizeof(QUEUE_NODE));

  
    queue->_queue_head = free_node;

	free_node = (QUEUE_NODE*)malloc(sizeof(QUEUE_NODE));

    queue->_queue_tail = free_node;

    queue->_queue_head->_nxt_node = queue->_queue_tail;
    queue->_queue_tail->_nxt_node = queue->_queue_head;


    queue->_empty_count = queue->_full_count = 0;

    return ret_val;
}


unsigned int queue_size(QUEUE *queue)
{
    return QINT_VALUE(queue->_queue_size);
}

int queue_push(QUEUE *queue, void *node)
{
    int ret_val = SUCCESS;
    QUEUE_NODE *free_node = NULL;

    sd_assert(queue != NULL);

    if(QINT_VALUE(queue->_queue_size) >= QINT_VALUE(queue->_queue_actual_capacity))
    {
        /* allocate new node */
		free_node = (QUEUE_NODE*)malloc(sizeof(QUEUE_NODE));


        /* init node */
        sd_memset(free_node, 0, sizeof(QUEUE_NODE));

        free_node->_data = node;
        free_node->_nxt_node = queue->_queue_tail->_nxt_node;
        queue->_queue_tail->_nxt_node = free_node;
        QINT_ADD(queue->_queue_actual_capacity, 1);
    }

    queue->_queue_tail->_nxt_node->_data = node;
    queue->_queue_tail = queue->_queue_tail->_nxt_node;

    QINT_ADD(queue->_queue_size, 1);

    return ret_val;
}

int queue_get_tail_ptr(QUEUE *queue, void **tail_ptr)
{
    int ret_val = SUCCESS;
    *tail_ptr = (void*)(&queue->_queue_tail->_data);
    return ret_val;
}

int queue_peek(QUEUE *queue, void **data)
{
    int ret_val = SUCCESS;
    *data = NULL;

    if(QINT_VALUE(queue->_queue_size) > 0)
        *data = (void*)queue->_queue_head->_nxt_node->_nxt_node->_data; /* skip a SENTINEL node */

    return ret_val;
}

int queue_pop(QUEUE *queue, void **node)
{
    int ret_val = SUCCESS;
    QUEUE_NODE *free_node = NULL;
    *node = NULL;

    if(QINT_VALUE(queue->_queue_size) > 0)
    {
        free_node = (QUEUE_NODE*)queue->_queue_head->_nxt_node;
        *node = (void*)free_node->_nxt_node->_data; /* skip a SENTINEL node */
        free_node->_nxt_node->_data = NULL;
        if(QINT_VALUE(queue->_queue_size) > QINT_VALUE(queue->_queue_capacity)
           || QINT_VALUE(queue->_queue_actual_capacity) > QINT_VALUE(queue->_queue_capacity))
        {
            queue->_queue_head->_nxt_node = free_node->_nxt_node;

            /* free the extra node */
			free(free_node);

            QINT_SUB(queue->_queue_actual_capacity, 1);
        }
        else
        {
            queue->_queue_head = free_node;
        }

        QINT_SUB(queue->_queue_size, 1);
    }

    return ret_val;
}

int queue_push_without_alloc(QUEUE *queue, void *data)
{
    int ret_val = SUCCESS;
	static FILE* s_file = NULL;

    sd_assert(queue != NULL);

	 

    while(QINT_VALUE(queue->_queue_size) >= QINT_VALUE(queue->_queue_actual_capacity))
    {
		SLEEP_SHORT_TIME();

		if(s_file == NULL){
			char tmpbuf[256];
			sprintf(tmpbuf, "luaprofiler.busy.%d.txt", (int)GETPID());
			s_file = fopen(tmpbuf, "w");
		}

		fprintf(s_file, "nolock_queue size too big usleep 10us..\n");
		fflush(s_file);
    }

    queue->_queue_tail->_nxt_node->_data = data;
    queue->_queue_tail = queue->_queue_tail->_nxt_node;

    QINT_ADD(queue->_queue_size, 1);

    return ret_val;
}

int queue_pop_without_dealloc(QUEUE *queue, void **data)
{
    int ret_val = SUCCESS;
    QUEUE_NODE *free_node = NULL;
    *data = NULL;

    if(QINT_VALUE(queue->_queue_size) > 0)
    {
        free_node = (QUEUE_NODE*)queue->_queue_head->_nxt_node;
        *data = (void*)free_node->_nxt_node->_data; /* skip a SENTINEL node */
        free_node->_nxt_node->_data = NULL;
        queue->_queue_head = free_node;
        QINT_SUB(queue->_queue_size, 1);
    }



    return ret_val;
}

int queue_recycle(QUEUE *queue)
{
	#define MAX(n1, n2)  ((n1) > (n2) ? (n1) : (n2))

    int ret_val = SUCCESS;
    int blow = MAX(QINT_VALUE(queue->_queue_size), QINT_VALUE(queue->_queue_capacity));
    int uppper = QINT_VALUE(queue->_queue_actual_capacity);
    QUEUE_NODE *free_node = NULL;

    for(; blow < uppper; blow++)
    {
        free_node = (QUEUE_NODE*)queue->_queue_tail->_nxt_node;
        queue->_queue_tail->_nxt_node = free_node->_nxt_node;

        /* free this node */
		free(free_node);

        QINT_SUB(queue->_queue_actual_capacity, 1);
    }

    return ret_val;
}

/* alloc new node to keep capacity */
int queue_reserved(QUEUE *queue, unsigned int capacity)
{
    int ret_val = SUCCESS;
    unsigned int cur_size = QINT_VALUE(queue->_queue_actual_capacity);
    QUEUE_NODE *free_node = NULL;

    if(capacity < MIN_QUEUE_CAPACITY)
        capacity = MIN_QUEUE_CAPACITY;

	QUEUE_NODE* tmp_buf_array = (QUEUE_NODE*)malloc(sizeof(QUEUE_NODE) * capacity);

	int index = 0;

    for(; cur_size < capacity; cur_size++)
    {
        /* allocate new node */
		free_node = (QUEUE_NODE*)&tmp_buf_array[index++];


        /* init node */
        sd_memset(free_node, 0, sizeof(QUEUE_NODE));

        free_node->_nxt_node = queue->_queue_head->_nxt_node;
        queue->_queue_head->_nxt_node = free_node;

        queue->_queue_head = free_node;

        QINT_ADD(queue->_queue_actual_capacity, 1);
    }

    QINT_SET_VALUE_1(queue->_queue_capacity, capacity);

    return ret_val;
}

int queue_check_full(QUEUE *queue)
{
    int ret_val = SUCCESS;
    unsigned int new_size = 0;

    if(QINT_VALUE(queue->_queue_actual_capacity) == 0 || QINT_VALUE(queue->_queue_size) >= QINT_VALUE(queue->_queue_actual_capacity) - 1)
    {
        queue->_empty_count = 0;

        if(queue->_full_count++ > QUEUE_AJUST_THRESHOLD)
        {
            new_size = QINT_VALUE(queue->_queue_actual_capacity) * QUEUE_ENLARGE_TIMES;
            if(new_size <= QINT_VALUE(queue->_queue_actual_capacity))
                new_size = QINT_VALUE(queue->_queue_actual_capacity) + 1;

            ret_val = queue_reserved(queue, new_size);
			
            CHECK_VALUE(ret_val);

            queue->_full_count = 0;
        }
    }
    else if(QINT_VALUE(queue->_queue_size) * QUEUE_REDUCE_TIMES < QINT_VALUE(queue->_queue_actual_capacity))
    {
        /* avoid the capacity always increased */
        queue->_full_count = 0;

        if(queue->_empty_count++ > QUEUE_AJUST_THRESHOLD)
        {
            new_size = QINT_VALUE(queue->_queue_actual_capacity) / QUEUE_REDUCE_TIMES;
            if(new_size < MIN_QUEUE_CAPACITY)
                new_size = MIN_QUEUE_CAPACITY;

            /* operation in read thread(pop) */
            QINT_SET_VALUE_2(queue->_queue_capacity, new_size);
            queue->_empty_count = 0;
        }
    }
    else
    {
        queue->_full_count = 0;
        queue->_empty_count = 0;
    }

    return ret_val;
}

int queue_check_empty(QUEUE *queue)
{
    int ret_val = SUCCESS;
    unsigned int new_size = 0;

    if(QINT_VALUE(queue->_queue_size) * QUEUE_REDUCE_TIMES < QINT_VALUE(queue->_queue_actual_capacity))
    {
        if(queue->_empty_count++ > QUEUE_AJUST_THRESHOLD)
        {
            new_size = QINT_VALUE(queue->_queue_actual_capacity) / QUEUE_REDUCE_TIMES;

            if(new_size < MIN_QUEUE_CAPACITY)
                new_size = MIN_QUEUE_CAPACITY;

            /* operate in write thread(push) */
            QINT_SET_VALUE_1(queue->_queue_capacity, new_size);
            ret_val = queue_recycle(queue);
            CHECK_VALUE(ret_val);

            queue->_empty_count = 0;
        }
    }
    else
    {
        queue->_empty_count = 0;
    }

    return ret_val;
}
