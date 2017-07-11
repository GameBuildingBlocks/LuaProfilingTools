#ifndef _SD_QUEUE_H_00138F8F2E70_200806260949
#define _SD_QUEUE_H_00138F8F2E70_200806260949
#include <assert.h>

#ifdef __cplusplus
extern "C"
{
#endif

#define MAX_QUEUE_VALUE (0x7fffffff)
#define MAX_QUEUE_SIZE  (MAX_QUEUE_VALUE)
#define SUCCESS (0)
#define sd_assert(a) do{ \
	assert(a); \
	if(!(a))exit(0);\
	}while(0);
#define sd_memset memset
#define CHECK_VALUE(a) assert((a) == 0)

#define MIN_QUEUE_CAPACITY (2)

#define QUEUE_NO_ROOM (-1)

    typedef struct
    {
        volatile unsigned int _add_ref;
        volatile unsigned int _sub_ref;
    } QUEUE_INT;

#define QINT_ADD(int_data, addend)  (int_data._add_ref += addend)
#define QINT_SUB(int_data, subtrahend)  (int_data._sub_ref += subtrahend)
#define QINT_VALUE(int_data)    (signed int)(int_data._add_ref - int_data._sub_ref)
    /* call in write thread(push) */
#define QINT_SET_VALUE_1(int_data, value)   (int_data._add_ref = int_data._sub_ref + value)
    /* call in read thread(pop) */
#define QINT_SET_VALUE_2(int_data, value)   (int_data._sub_ref = int_data._add_ref - value)


    typedef struct t_queue_node
    {
        void *_data;
        struct t_queue_node *_nxt_node;
    } QUEUE_NODE, *pQUEUE_NODE;

    typedef struct
    {
        /* its next node is the first node can be used*/
        QUEUE_NODE *_queue_head;
        /* its next node is the first free node*/
        QUEUE_NODE *_queue_tail;
        QUEUE_INT _queue_size;
        QUEUE_INT _queue_actual_capacity;
        QUEUE_INT _queue_capacity;

        /* optimation for alloc node, need not too accurate */
        unsigned int _empty_count;
        unsigned int _full_count;
    } QUEUE;

    /**/
#define QUEUE_AJUST_THRESHOLD   (10)
    /* must > 1 */
#define QUEUE_REDUCE_TIMES      (2)
    /* do not use parenthess, must > 1 */
#define QUEUE_ENLARGE_TIMES     3 / 2



    int queue_alloctor_init(void);
    int queue_alloctor_uninit(void);


    int queue_init(QUEUE *queue, unsigned int capacity);

    int queue_uninit(QUEUE *queue);


    /* @Simple Function@
     * Return : the size of queue
     */
    unsigned int queue_size(QUEUE *queue);


    int queue_push(QUEUE *queue, void *data);

    /* get the last data-ptr of queue, can used for change queue-data directly in some special case.
       it should be used with caution. */
    int queue_get_tail_ptr(QUEUE *queue, void **tail_ptr);

    /* peek head, not popped */
    int queue_peek(QUEUE *queue, void **data);

    /*
     * Return immediately, even if there were not any node popped.
     * If no node popped, the *data will be NULL.
     */
    int queue_pop(QUEUE *queue, void **data);


    /* functions below are prepared for no-lock malloc */

    /* when the queue full, return immediately with QUEUE_NO_ROOM */
    int queue_push_without_alloc(QUEUE *queue, void *data);

    /**/
    int queue_pop_without_dealloc(QUEUE *queue, void **data);


    /* recycle node up to capacity or queue_size.
     * it be used to recycle free-node as queue_pop_without_dealloc() can not do.
     */
    int queue_recycle(QUEUE *queue);

    /* alloc new node to keep capacity
     * used to alloc new node as queue_push_without_alloc() can not do it.
     */
    int queue_reserved(QUEUE *queue, unsigned int capacity);

    /* check queue for optimazation of memory */
    int queue_check_full(QUEUE *queue);
    int queue_check_empty(QUEUE *queue);

#ifdef __cplusplus
}
#endif

#endif
